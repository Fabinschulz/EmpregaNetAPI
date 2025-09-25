using EmpregaNet.Domain.Common;
using EmpregaNet.Infra.Cache.MemoryService;
using Microsoft.AspNetCore.Mvc;
using EmpregaNet.Application.Common.Base;

namespace EmpregaNet.Api.Controllers.Core
{
    /// <summary>
    /// MainController é uma classe base para controladores que gerenciam entidades com operações CRUD.
    /// Fornece endpoints para criar, ler, atualizar e excluir recursos, além de implementar
    /// </summary>
    /// <typeparam name="TCreateCommand"></typeparam>
    /// <typeparam name="TUpdateCommand"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    [ApiController]
    public abstract class MainController<TCreateCommand, TUpdateCommand, TViewModel> : ControllerBase
        where TCreateCommand : class
        where TUpdateCommand : class
        where TViewModel : class
    {
        private IMediator _IMediator = null!;
        protected IMediator _mediator => _IMediator ?? HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected readonly IMemoryService _cacheService;
        private readonly string _entityName;

        protected MainController(IMemoryService cacheService)
        {
            _cacheService = cacheService;
            _entityName = typeof(TViewModel).Name;

        }

        /// <summary>
        /// Endpoint para obter todos os recursos de uma entidade com paginação e ordenação.
        /// Utiliza cache para otimização de leitura.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 100, [FromQuery] string? orderBy = null)
        {

            var cacheKey = $"{_entityName}_GetAll_{page}_{size}_{orderBy}";
            var cachedData = await _cacheService.GetValueAsync<ListDataPagination<TViewModel>>(cacheKey);

            if (cachedData is not null) return Ok(cachedData);

            var result = await _mediator.Send(new GetAllQuery<TViewModel>(page, size, orderBy));
            await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);
        }

        /// <summary>
        /// Endpoint para obter um recurso específico pelo ID.
        /// Utiliza cache para otimização de leitura.
        /// </summary>
        /// <param name="id">O ID do recurso a ser obtido.</param>
        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> GetById([FromRoute] long id)
        {
            var cacheKey = $"{_entityName}_GetById_{id}";
            var cachedData = await _cacheService.GetValueAsync<TViewModel>(cacheKey);

            if (cachedData is not null) return Ok(cachedData);

            var result = await _mediator.Send(new GetByIdQuery<TViewModel>(id));
            await _cacheService.SetValueAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);
        }

        /// <summary>
        /// Endpoint para criar um novo recurso.
        /// Invalida o cache após a criação.
        /// </summary>
        /// <param name="entity">O objeto Command/ViewModel para criação.</param>
        /// <returns>Retorna o ID do recurso criado.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Create([FromBody] TCreateCommand entity)
        {
            var id = await _mediator.Send(new CreateCommand<TCreateCommand>(entity));
            await InvalidateCacheForEntity();
            var successMessage = $"{typeof(TCreateCommand).Name.Replace("Command", "")} registrado(a) com sucesso. ID: {id}";

            return Created($"api/{_entityName.ToLower()}/{id}", successMessage);
        }

        /// <summary>
        /// Endpoint para atualizar um recurso existente.
        /// Invalida o cache após a atualização.
        /// </summary>
        /// <param name="id">O ID do recurso a ser atualizado.</param>
        /// <param name="entity">O objeto com os dados para atualização.</param>
        [HttpPut("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<IActionResult> Update([FromRoute] long id, [FromBody] TUpdateCommand entity)
        {
            var result = await _mediator.Send(new UpdateCommand<TUpdateCommand, TViewModel>(id, entity));
            await InvalidateCacheForEntity(id);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint para excluir um recurso.
        /// Invalida o cache após a exclusão.
        /// </summary>
        /// <param name="id">O ID do recurso a ser excluído.</param>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DomainError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(DomainError))]
        public virtual async Task<IActionResult> Delete([FromRoute] long id)
        {
            await _mediator.Send(new DeleteCommand<TCreateCommand>(id));
            await InvalidateCacheForEntity(id);
            return NoContent();
        }

        /// <summary>
        /// Invalida as entradas de cache relacionadas à entidade.
        /// Remove a entrada específica pelo ID e todas as entradas "GetAll".
        /// </summary>
        /// <param name="id">O ID opcional do recurso que teve seu cache invalidado.</param>
        /// <returns>Um Task que indica a conclusão da operação.</returns>
        protected virtual async Task InvalidateCacheForEntity(long id = default)
        {
            // Invalida o cache do GetById específico, se o id for fornecido
            if (id != default)
            {
                var cacheKey = $"{_entityName}_GetById_{id}";
                _cacheService.Remove(cacheKey);
            }

            // Invalida todas as chaves de cache que começam com o prefixo do GetAll
            await InvalidateGetAllCache();
        }

        /// <summary>
        /// Invalida todas as entradas de cache do GetAll para a entidade atual.
        /// </summary>
        /// <returns>Um Task que indica a conclusão da operação.</returns>
        protected virtual Task InvalidateGetAllCache()
        {
            var pattern = $"{_entityName}_GetAll_";
            return _cacheService.RemoveByPatternAsync(pattern);
        }
    }
}