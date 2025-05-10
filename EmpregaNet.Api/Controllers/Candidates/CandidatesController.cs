using EmpregaNet.Infra.Cache.DistributedCache;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Candidates
{
    [Route("[controller]")]
    public class CandidatesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMemoryService _cacheRepository;
        private readonly IHub _sentryHub;

        public CandidatesController(IMediator mediator, IMemoryService cacheRepository, IHub sentryHub)

        {
            _mediator = mediator;
            _cacheRepository = cacheRepository;
            _sentryHub = sentryHub;
        }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetCandidateById(int id)
        // {
        //     var cacheKey = $"candidate_{id}";
        //     var candidate = await _cacheRepository.GetValueAsync<Candidate>(cacheKey);

        //     if (candidate == null)
        //     {
        //         candidate = await _mediator.Send(new GetCandidateByIdQuery(id));
        //         await _cacheRepository.SetValueAsync(cacheKey, candidate, _cacheRepository.GetCacheOptions());
        //     }

        //     return Ok(candidate);
        // }
    }
}