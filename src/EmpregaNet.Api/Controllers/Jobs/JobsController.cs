using EmpregaNet.Api.Controllers.Core;
using EmpregaNet.Application.Jobs.Commands;
using EmpregaNet.Application.Jobs.ViewModel;
using EmpregaNet.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpregaNet.Api.Controllers.Jobs
{
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : MainController<CreateJobCommand, UpdateJobCommand, JobViewModel>
    {

        public JobsController(IMemoryService cacheService) : base(cacheService)
        {
        }

    }

}