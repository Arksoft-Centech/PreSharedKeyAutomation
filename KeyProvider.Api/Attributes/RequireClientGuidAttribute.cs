using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KeyProvider.Api.Attributes
{
    /// <summary>
    /// Author: Can DOĞU
    /// </summary>
    public class RequireClientGuidHeaderAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Client-Guid", out var clientGuidHeader) ||
                !Guid.TryParse(clientGuidHeader, out Guid parsedGuid))
            {
                context.Result = new ContentResult
                {
                    StatusCode = 400,
                    Content = "Missing or invalid X-Client-Guid"
                };
            }
            else
            {
                context.HttpContext.Items["ClientGuid"] = parsedGuid;
            }
        }
    }
}
