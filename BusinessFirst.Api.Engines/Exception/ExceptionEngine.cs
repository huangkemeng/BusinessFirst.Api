using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Realization.Bases;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace RenameMe.Api.Engines.Exception
{
    public class UseExceptionHandler : IAppEngine
    {
        private readonly WebApplication app;

        public UseExceptionHandler(WebApplication app)
        {
            this.app = app;
        }
        public void Run()
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception != null && exception is BusinessException businessException)
                    {
                        if (businessException.Type == BusinessExceptionTypeEnum.UnauthorizedIdentity)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        }
                        context.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new
                        {
                            error = businessException.TypeName,
                            message = businessException.Message
                        });
                        await context.Response.WriteAsync(result, context.RequestAborted);
                    }
                });
            });
        }
    }
}
