using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace RenameMe.Api.Engines.Swagger
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SwaggerApiGroupAttribute : Attribute, IApiDescriptionGroupNameProvider
    {
        public SwaggerApiGroupAttribute(SwaggerApiGroupNames input)
        {
            GroupName = input.ToString();
        }
        public SwaggerApiGroupAttribute(bool byRule)
        {
            if (byRule)
            {
                GroupName = "*";
            }
        }
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; }

    }
}
