using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;

namespace RenameMe.Api.FilterAndMiddlewares
{
    public class HandleTimezoneResultFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
        public void OnResultExecuting(ResultExecutingContext context)
        {
            var timezone = context.HttpContext.Request.Headers["x-tz"].FirstOrDefault();
            if (timezone != null && context.Result != null && context.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                var newResult = ConvertToSpecificTimezone(objectResult.Value, timezone);
                if (newResult != null)
                {
                    objectResult.Value = newResult;
                }
            }
        }

        public object? ConvertToSpecificTimezone(object value, string timezone)
        {
            if (int.TryParse(timezone, out int timezoneOffset))
            {
                var name = $"utc{string.Format("{0:+#;-#;0}", timezoneOffset / 60.0)}";
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(name, TimeSpan.FromMinutes(timezoneOffset), name, name);
                JToken jToken = JToken.FromObject(value!);
                UpdateDateTimeToSpecificTimezone(jToken, timeZoneInfo);
                return jToken.ToObject(value!.GetType());
            }
            return null;
        }

        private void UpdateDateTimeToSpecificTimezone(JToken token, TimeZoneInfo timeZoneInfo)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        var jpropertyList = token.Children();
                        foreach (var jproperty in jpropertyList)
                        {
                            UpdateDateTimeToSpecificTimezone(jproperty, timeZoneInfo);
                        }
                        break;
                    }
                case JTokenType.Array:
                    {
                        var jpropertyList = token.Children();
                        foreach (var item in jpropertyList)
                        {
                            UpdateDateTimeToSpecificTimezone(item, timeZoneInfo);
                        }
                        break;
                    }
                case JTokenType.Property:
                    {
                        if (token is JProperty prop)
                        {
                            UpdateDateTimeToSpecificTimezone(prop.Value, timeZoneInfo);
                        }
                        break;
                    }
                case JTokenType.Date:
                    {
                        if (token is JValue tokenValue)
                        {
                            tokenValue.Value = TimeZoneInfo.ConvertTime(token.ToObject<DateTimeOffset>(), timeZoneInfo);
                        }
                        break;
                    }
            }
        }
    }
}
