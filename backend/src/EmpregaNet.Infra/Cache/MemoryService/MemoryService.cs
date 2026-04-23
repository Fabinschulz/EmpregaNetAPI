using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Newtonsoft.Json;
using EmpregaNet.Domain.Interfaces;

namespace EmpregaNet.Infra.Cache
{
    public class MemoryService : IMemoryService
    {
        private IConnectionMultiplexer? _distributed;
        private IMemoryCache _local;
        private readonly ILogger<MemoryService> _logger;
        private MemoryServiceOptions _options;
        private static Random _randon = new Random();

        public MemoryService(
            IMemoryCache local,
            ILogger<MemoryService> logger,
            IOptionsMonitor<MemoryServiceOptions> options,
            IConnectionMultiplexer? distributed = null)
        {
            _local = local;
            _logger = logger;
            _options = options.CurrentValue ?? new MemoryServiceOptions
            {
                KeyPrefix = "EMPREGANET_CACHE",
            };
            _distributed = distributed;
        }

        public async Task<T?> GetValueAsync<T>(string key)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";
            var someSeconds = TimeSpan.FromSeconds(_randon.Next(1, 60)); // Tempo aleatório para antecipar o refresh do cache local

            if (_local.TryGetValue(cacheKey, out T? value))
            {
                return value;
            }

            if (_distributed != null && _distributed.IsConnected)
            {
                try
                {
                    var json = await _distributed.GetDatabase().StringGetAsync(cacheKey);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var valueFromRedis = JsonConvert.DeserializeObject<T>(json!);
                        var expiration = await _distributed.GetDatabase().KeyTimeToLiveAsync(cacheKey);

                        if (expiration.HasValue && expiration.Value < someSeconds)
                        {
                            _logger.LogDebug($"Chave de cache {cacheKey} expirada no Redis.");
                            Remove(cacheKey);
                            return default(T);
                        }

                        if (_local is not null && expiration is not null)
                        {
                            _local.Set(cacheKey, valueFromRedis, DateTimeOffset.Now.Add(expiration.Value - someSeconds));
                            _logger.LogDebug($"Chave de cache {cacheKey} encontrada no Redis e armazenada na memória local.");
                        }

                        return valueFromRedis;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter valor do Redis para a chave: {Key}", cacheKey);
                    return default(T);
                }
            }

            return default(T);
        }

        public async Task<T?> GetValueAsync<T>(string key, Func<T, bool> isValid)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";

            if (_local.TryGetValue(cacheKey, out T? value))
            {
                if (value is not null && isValid(value))
                {
                    return value;
                }
            }

            if (_distributed != null && _distributed.IsConnected)
            {
                try
                {
                    var json = await _distributed.GetDatabase().StringGetAsync(cacheKey);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var valueFromRedis = JsonConvert.DeserializeObject<T>(json!);
                        var expiration = await _distributed.GetDatabase().KeyTimeToLiveAsync(cacheKey);

                        if (valueFromRedis is not null)
                        {
                            if (!isValid(valueFromRedis))
                            {
                                _logger.LogDebug($"Chave de cache {cacheKey} inválida.");
                                Remove(cacheKey);
                                return default(T);
                            }

                            if (_local is not null && expiration is not null)
                            {
                                _local.Set(cacheKey, valueFromRedis, DateTimeOffset.Now.Add(expiration.Value));
                                _logger.LogDebug($"Chave de cache {cacheKey} encontrada no Redis e armazenada na memória local.");
                            }

                            return valueFromRedis;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter valor do Redis (com validação) para a chave: {Key}", cacheKey);
                    return default(T);
                }
            }

            return default(T);
        }

        public async Task SetValueAsync<T>(string key, T obj, TimeSpan expiration)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";

            _local.Set(cacheKey, obj, DateTimeOffset.Now.Add(expiration));

            // Mantém um registry de chaves para poder invalidar depois
            if (_local.TryGetValue("CACHE_KEY_REGISTRY", out HashSet<string>? registry))
            {
                registry?.Add(cacheKey);
            }
            else
            {
                registry = new HashSet<string> { cacheKey };
            }

            _local.Set("CACHE_KEY_REGISTRY", registry);

            if (_distributed?.IsConnected == true)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(obj);
                    await _distributed.GetDatabase().StringSetAsync(cacheKey, json, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao salvar no Redis para a chave: {Key}", cacheKey);
                }
            }
        }


        /// <summary>
        /// Remove uma única chave de cache da memória local e do Redis.
        /// </summary>
        public void Remove(string key)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";

            if (_local is not null)
            {
                _local.Remove(cacheKey);
                _logger.LogDebug($"Chave de cache {cacheKey} removida da memória local.");
            }

            if (_distributed != null)
            {
                if (_distributed.IsConnected)
                {
                    _distributed.GetDatabase().KeyDelete(cacheKey);
                    _logger.LogDebug($"Chave de cache {cacheKey} removida do Redis.");
                }
            }
        }

        /// <summary>
        /// Remove chaves de cache do Redis e da memória local com base em um padrão.
        /// </summary>
        /// <param name="pattern">O padrão de busca para as chaves (ex: "prefixo*").</param>
        public async Task RemoveByPatternAsync(string pattern)
        {
            var cachePattern = $"{_options.KeyPrefix}:{pattern}";

            if (_local is not null)
            {
                if (_local.TryGetValue("CACHE_KEY_REGISTRY", out HashSet<string>? registry))
                {
                    var keysToRemove = registry?.Where(k => k.StartsWith(cachePattern)).ToList() ?? new List<string>();

                    foreach (var key in keysToRemove)
                    {
                        _local.Remove(key);
                        registry?.Remove(key);
                        _logger.LogDebug($"Chave de cache {key} removida da memória local por padrão.");
                    }

                    _local.Set("CACHE_KEY_REGISTRY", registry);
                }
            }

            if (_distributed is null || !_distributed.IsConnected)
            {
                _logger.LogWarning($"Redis não está conectado. Portanto será removido apenas da memória local.");
                return;
            }

            try
            {
                var server = _distributed.GetServer(_distributed.GetEndPoints().First());
                var keys = server.Keys(pattern: cachePattern + "*").ToList();

                foreach (var key in keys)
                {
                    await _distributed.GetDatabase().KeyDeleteAsync(key);
                    _logger.LogDebug($"Chave de cache {key} removida do Redis por padrão.");
                }

                _logger.LogInformation($"Removidas {keys.Count} chaves de cache do Redis e da memória local.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover chaves do Redis por padrão: {Pattern}", pattern);
            }
        }

    }
}