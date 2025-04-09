using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTrackerAPI
{
    public class ValidateUserIdHeaderAttribute : ActionFilterAttribute
    {
        private const string UserIdKey = "UserId";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("userId", out var userId) ||
                string.IsNullOrWhiteSpace(userId))
            {
                context.Result = new BadRequestObjectResult("Missing or invalid userId in headers.");
                return;
            }

            context.HttpContext.Items[UserIdKey] = userId.ToString();
        }
    }
}
