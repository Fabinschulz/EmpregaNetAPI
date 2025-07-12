using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Infra.Cache.MemoryService;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Jobs
{
    [Route("api/[controller]")]
    public class JobsController : MainController<JobCommand, JobViewModel>
    {

        public JobsController(IMemoryService cacheService) : base(cacheService)
        {
        }

    }

}