using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace EmpregaNet.Infra.Cache.MemoryService
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
            IServiceProvider serviceProvider,
            IOptionsMonitor<MemoryServiceOptions> options)
        {
            _local = local;
            _logger = logger;
            _options = options.CurrentValue ?? new MemoryServiceOptions
            {
                KeyPrefix = "EMPREGANET_",
            };

            if (serviceProvider != null)
            {
                _distributed = (IConnectionMultiplexer)serviceProvider.GetService(typeof(IConnectionMultiplexer))!;
            }
        }

        public Task<T>? GetValueAsync<T>(string key)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";
            var someSeconds = TimeSpan.FromSeconds(_randon.Next(1, 60));

            if (_local.TryGetValue(cacheKey, out T? value))
            {
                return Task.FromResult(value)!;
            }

            if (_distributed != null)
            {
                if (_distributed.IsConnected)
                {
                    var json = _distributed.GetDatabase().StringGet(cacheKey);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var valueFromRedis = JsonSerializer.Deserialize<T>(json!);
                        var expiration = _distributed.GetDatabase().KeyTimeToLive(cacheKey);

                        if (expiration.HasValue && expiration.Value < someSeconds)
                        {
                            _logger.LogDebug($"Chave de cache {cacheKey} expirada no Redis.");
                            Remove(cacheKey);
                            return default;
                        }

                        if (_local is not null && expiration is not null)
                        {
                            _local.Set(cacheKey, valueFromRedis, DateTime.Now.Add(expiration.Value - someSeconds));
                            _logger.LogDebug($"Chave de cache {cacheKey} encontrada no Redis e armazenada na memória local.");
                        }

                        return Task.FromResult(valueFromRedis)!;
                    }

                }
                else
                {
                    _logger.LogDebug($"Chave de cache {cacheKey} não encontrada no Redis.");
                }

            }

            return default;
        }

        public Task<T>? GetValueAsync<T>(string key, Func<T, bool> isValid)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";

            if (_local is not null)
            {
                var value = _local.Get(cacheKey);
                if (value is not null && isValid((T)value))
                {
                    return Task.FromResult((T)value);
                }
            }

            if (_distributed != null)
            {
                if (_distributed.IsConnected)
                {
                    var json = _distributed.GetDatabase().StringGet(cacheKey);

                    if (!string.IsNullOrEmpty(json))
                    {
                        var valueFromRedis = JsonSerializer.Deserialize<T>(json!);
                        var expiration = _distributed.GetDatabase().KeyTimeToLive(cacheKey);

                        if (valueFromRedis is not null)
                        {

                            if (!isValid(valueFromRedis))
                            {
                                _logger.LogDebug($"Chave de cache {cacheKey} inválida.");
                                Remove(cacheKey);
                                return default;
                            }

                            if (_local is not null && expiration is not null)
                            {
                                _local.Set(cacheKey, valueFromRedis, DateTime.Now.Add(expiration.Value));
                                _logger.LogDebug($"Chave de cache {cacheKey} encontrada no Redis e armazenada na memória local.");
                            }


                            return Task.FromResult(valueFromRedis);
                        }

                    }
                }
            }

            return default;
        }

        public async Task SetValueAsync<T>(string key, T obj, TimeSpan expiration)
        {
            var cacheKey = $"{_options.KeyPrefix}:{key}";

            _local.Set(cacheKey, obj, DateTime.Now.Add(expiration));

            if (_distributed?.IsConnected == true)
            {
                try
                {
                    var json = JsonSerializer.Serialize(obj);
                    await _distributed.GetDatabase().StringSetAsync(cacheKey, json, expiration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao salvar no Redis");
                }
            }
        }

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
    }
}