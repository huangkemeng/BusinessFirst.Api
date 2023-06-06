using RenameMe.Api.FilterAndMiddlewares;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RenameMe.Api.Middlewares
{
    public class AutoResolveAsyncActionFilter : IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller;
            var type = controller.GetType();
            var typeProperties = type.GetProperties();
            foreach (var typeProperty in typeProperties)
            {
                if (typeProperty.IsPubliclyWritable() && typeProperty.HasAttribute<AutoResolveAttribute>())
                {
                    if (typeProperty.GetValue(controller) == null)
                    {
                        var autoResolvePropertyValue = context.HttpContext.RequestServices.GetService(typeProperty.PropertyType);
                        if (autoResolvePropertyValue != null)
                        {
                            typeProperty.SetValue(controller, autoResolvePropertyValue);
                        }
                    }
                }
            }
            return next();
        }
    }
}
