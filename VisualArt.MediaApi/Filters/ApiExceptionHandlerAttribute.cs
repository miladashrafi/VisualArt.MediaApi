using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VisualArt.MediaApi.Models;

namespace VisualArt.MediaApi.Filters
{
    public class ApiExceptionHandlerAttribute : ActionFilterAttribute, IActionFilter
    {
        private readonly ILogger<ApiExceptionHandlerAttribute> _logger;

        public ApiExceptionHandlerAttribute(ILogger<ApiExceptionHandlerAttribute> logger)
        {
            _logger = logger;
        }
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is not null)
            {
                var errorMessage = context.Exception is CustomException customException
                    ? customException.Message
                    : "Unknown error occurred!";
                context.Result = new BadRequestObjectResult(new StandardApiResponse(false, errorMessage));
                context.ExceptionHandled = true;
                _logger.LogError(errorMessage);
                return;
            }

            context.Result = new OkObjectResult(context.Result is ObjectResult objectResult
                ? new StandardApiObjectResponse<object>(true, objectResult.Value)
                : new StandardApiResponse(true));
        }
    }
}
