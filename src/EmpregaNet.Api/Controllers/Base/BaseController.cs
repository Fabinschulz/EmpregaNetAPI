using EmpregaNet.Application.Common.Command;
using EmpregaNet.Domain.Common;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Infra.Cache.MemoryService;
using Microsoft.AspNetCore.Mvc;


namespace EmpregaNet.Api.Controllers.Base
{
    public abstract class BaseController<TRequest, TResponse> : ControllerBase
        where TResponse : class
        where TRequest : class
    {
        protected readonly IMediator _mediator;
        protected readonly IMemoryService _cacheService;
        private readonly IHub _sentryHub;
        private readonly string _entityName;

        protected BaseController(IMediator mediator, IMemoryService cacheService, IHub sentryHub)
        {
            _sentryHub = sentryHub;
            _mediator = mediator;
            _cacheService = cacheService;
            _entityName = typeof(TResponse).Name;

        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? orderBy = null)
        {

            var cacheKey = $"{_entityName}_GetAll_{page}_{size}";
            var cachedData = await _cacheService.GetValueAsync<ListDataPagination<TResponse>>(cacheKey)!;

            if (cachedData != null) return Ok(cachedData);

            var result = await _mediator.Send(new GetAllQuery<TResponse>(page, size, orderBy));
            await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));


            return Ok(result);
        }


        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> GetById([FromRoute] long id)
        {
            var cacheKey = $"{_entityName}_GetById_{id}";
            var cachedData = await _cacheService.GetValueAsync<TResponse>(cacheKey)!;

            if (cachedData != null) return Ok(cachedData);

            var result = await _mediator.Send(new GetByIdQuery<TResponse>(id));
            await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));


            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Create([FromBody] TRequest entity)
        {
            var result = await _mediator.Send(new CreateCommand<TRequest, TResponse>(entity));
            await InvalidateCacheForEntity();
            return CreatedAtAction(nameof(GetById), new { id = GetEntityId(result) }, result);
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Update([FromRoute] long id, [FromBody] TRequest entity)
        {
            var result = await _mediator.Send(new UpdateCommand<TRequest, TResponse>(id, entity));
            await InvalidateCacheForEntity(id);

            return Ok(result);
        }

        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Delete([FromRoute] long id)
        {
            await _mediator.Send(new DeleteCommand<TResponse>(id));
            await InvalidateCacheForEntity(id);

            return NoContent();
        }

        protected virtual Task InvalidateCacheForEntity(long id = default)
        {
            var allCacheKey = $"{_entityName}_GetAll_";
            _cacheService.Remove(allCacheKey);

            if (id != default)
            {
                var cacheKey = $"{_entityName}_GetById_{id}";
                _cacheService.Remove(cacheKey);
            }

            return Task.CompletedTask;
        }

        protected abstract long GetEntityId(TResponse entity);
    }
}