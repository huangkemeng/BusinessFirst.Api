using System.ComponentModel;
using System.Reflection;

namespace RenameMe.Api.Realization.Bases
{
    public class BusinessException : Exception
    {
        public BusinessException(string msg, BusinessExceptionTypeEnum exceptionType = BusinessExceptionTypeEnum.NotSpecified) : base(GetFullExceptionMessage(exceptionType, msg))
        {
            Type = exceptionType;
        }
        public BusinessException(IEnumerable<string> msg, BusinessExceptionTypeEnum exceptionType = BusinessExceptionTypeEnum.NotSpecified) : base(GetFullExceptionMessage(exceptionType, msg.ToArray()))
        {
            Type = exceptionType;
        }
        public BusinessExceptionTypeEnum Type { get; set; }

        public string TypeName => GetTypeName(Type);


        private static string GetTypeName(BusinessExceptionTypeEnum type)
        {
            var businessExceptionTypeStateType = typeof(BusinessExceptionTypeEnum);
            var businessExceptionTypeStateTypeField = businessExceptionTypeStateType.GetField(type.ToString())!;
            var descriptionAttr = businessExceptionTypeStateTypeField.GetCustomAttribute(typeof(DescriptionAttribute));
            if (descriptionAttr is DescriptionAttribute description)
            {
                return description.Description;
            }
            return type.ToString();
        }

        private static string GetFullExceptionMessage(BusinessExceptionTypeEnum type, params string[] msg)
        {
            return $"{GetTypeName(type)}：{string.Join(";", msg)}";
        }
    }
    public enum BusinessExceptionTypeEnum
    {
        [Description("")] NotSpecified,
        /// <summary>
        /// 参数有误
        /// </summary>
        [Description("参数有误")] Validator,
        /// <summary>
        /// 配置有误
        /// </summary>
        [Description("配置有误")] Configuration,
        /// <summary>
        /// 身份验证
        /// </summary>
        [Description("身份异常")] UnauthorizedIdentity,
        /// <summary>
        /// 兼容性
        /// </summary>
        [Description("兼容性")] Compatibility,
        /// <summary>
        /// 数据为空
        /// </summary>
        [Description("空数据")] DataNull,
    }


}
