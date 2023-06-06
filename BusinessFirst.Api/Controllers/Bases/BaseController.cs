using RenameMe.Api.FilterAndMiddlewares;
using RenameMe.Api.Middlewares;
using Mediator.Net;
using Microsoft.AspNetCore.Mvc;

namespace RenameMe.Api.Controllers.Bases
{
    [ApiController]
    [TypeFilter(typeof(AutoResolveAsyncActionFilter))]
    public class BaseController : ControllerBase
    {
        [AutoResolve]
        public IMediator Mediator { get; set; }
    }
}
