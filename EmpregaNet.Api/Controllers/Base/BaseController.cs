using EmpregaNet.Domain;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interface;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Cache.MemoryService;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Base
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity> : ControllerBase where TEntity : BaseEntity, IAggregateRoot
    {
        protected readonly IMemoryService _memoryService;
        protected readonly IBaseRepository<TEntity> _repository;
        private readonly string _entityName;

        protected BaseController(IBaseRepository<TEntity> repository, IMemoryService memoryService)
        {
            _repository = repository;
            _entityName = typeof(TEntity).Name;
            _memoryService = memoryService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? orderBy = null)
        {
            var cacheKey = $"{_entityName}_GetAll_{page}_{size}_{orderBy}";
            var cachedData = await _memoryService.GetValueAsync<ListDataPagination<TEntity>>(cacheKey)!;
            var expiration = TimeSpan.FromMinutes(60);

            if (cachedData != null) return Ok(cachedData);

            var data = await _repository.GetAllAsync(page, size, orderBy);
            await _memoryService.SetValueAsync(cacheKey, data, expiration);

            return Ok(data);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById([FromRoute] long id)
        {
            var cacheKey = $"{_entityName}_GetById_{id}";
            var cachedData = await _memoryService.GetValueAsync<TEntity>(cacheKey)!;
            var expiration = TimeSpan.FromMinutes(60);

            if (cachedData != null) return Ok(cachedData);

            var data = await _repository.GetByIdAsync(id);
            await _memoryService.SetValueAsync(cacheKey, data, expiration);

            return Ok(data);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
        {
            var result = await _repository.CreateAsync(entity);
            await InvalidateCacheForEntity();

            return CreatedAtAction(nameof(GetById), new { id = GetEntityId(result) }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromRoute] long id, [FromBody] TEntity entity)
        {
            var result = await _repository.UpdateAsync(entity);
            await InvalidateCacheForEntity(id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public virtual async Task<IActionResult> Delete([FromRoute] long id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return NotFound("Não foi possível encontrar o registro.");

            await _repository.DeleteAsync(entity);
            await InvalidateCacheForEntity(id);

            return NoContent();
        }

        protected virtual Task InvalidateCacheForEntity(long id = default)
        {
            var cacheKey = $"{_entityName}_GetById_{id}";
            _memoryService.Remove(cacheKey);

            var allCacheKey = $"{_entityName}_GetAll_";
            _memoryService.Remove(allCacheKey);

            return Task.CompletedTask;
        }

        protected abstract long GetEntityId(TEntity entity);
    }
}