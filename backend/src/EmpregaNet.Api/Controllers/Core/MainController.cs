using EmpregaNet.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Application.Common.Base;
using EmpregaNet.Domain.Interfaces;
using EmpregaNet.Api.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;

namespace EmpregaNet.Api.Controllers.Core
{
    /// <summary>
    /// Classe base para controladores com operações CRUD genéricas: leitura com Output Cache e invalidação coordenada em mutações.
    /// </summary>
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public abstract class MainController<TCreateCommand, TUpdateCommand, TViewModel> : ControllerBase
        where TCreateCommand : class
        where TUpdateCommand : class
        where TViewModel : class
    {
        private IMediator _IMediator = null!;
        protected IMediator _mediator => _IMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected readonly IOutputCacheManager _outputCache;
        private readonly string _entityName;

        protected MainController(IOutputCacheManager cacheService)
        {
            _outputCache = cacheService;
            _entityName = typeof(TViewModel).Name;
        }

        /// <summary>
        /// Endpoint para obter todos os recursos de uma entidade com paginação e ordenação.
        /// </summary>
        [HttpGet]
        [OutputCache(PolicyName = OutputCachePolicies.EntityRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListDataPagination<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int size = 100,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool? isDeleted = null,
            [FromQuery] bool? isActive = null)
        {
            var result = await _mediator.Send(new GetAllQuery<TViewModel>(page, size, orderBy, isDeleted, isActive));
            return Ok(result);
        }

        /// <summary>
        /// Endpoint para obter um recurso específico pelo ID.
        /// </summary>
        [HttpGet("{id:long}")]
        [OutputCache(PolicyName = OutputCachePolicies.EntityRead)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> GetById([FromRoute] long id)
        {
            var result = await _mediator.Send(new GetByIdQuery<TViewModel>(id));
            return Ok(result);
        }

        /// <summary>
        /// Endpoint para criar um novo recurso.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> Create([FromBody] TCreateCommand entity)
        {
            var id = await _mediator.Send(new CreateCommand<TCreateCommand>(entity));
            await InvalidateCacheForEntity(id);
            var successMessage = $"Recurso criado com sucesso. ID: {id}";

            return Created($"api/{_entityName.ToLower()}/{id}", successMessage);
        }

        /// <summary>
        /// Endpoint para atualizar um recurso existente.
        /// </summary>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> Update([FromRoute] long id, [FromBody] TUpdateCommand entity)
        {
            var result = await _mediator.Send(new UpdateCommand<TUpdateCommand, TViewModel>(id, entity));
            await InvalidateCacheForEntity(id);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint para excluir um recurso.
        /// </summary>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> Delete([FromRoute] long id)
        {
            await _mediator.Send(new DeleteCommand<TViewModel>(id));
            await InvalidateCacheForEntity(id);
            return NoContent();
        }

        /// <summary>
        /// Invalida Output Cache e entradas legadas relacionadas à entidade.
        /// </summary>
        protected virtual Task InvalidateCacheForEntity(long id = default)
            => _outputCache.InvalidateEntityAsync(_entityName, id);
    }
}
