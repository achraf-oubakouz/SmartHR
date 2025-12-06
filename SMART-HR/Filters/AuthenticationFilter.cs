using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace SmartHR.Filters
{
    public class AuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Get controller and action names
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // Allow access to these pages without authentication
            var allowedRoutes = new[]
            {
                ("Home", "Index"),
                ("Home", "Login"),
                ("Home", "Register"),
                ("Home", "Logout"),
                ("Home", "Error"),
                ("Home", "Privacy")
            };

            // Check if current route is allowed
            foreach (var (allowedController, allowedAction) in allowedRoutes)
            {
                if (controller?.Equals(allowedController, System.StringComparison.OrdinalIgnoreCase) == true &&
                    action?.Equals(allowedAction, System.StringComparison.OrdinalIgnoreCase) == true)
                {
                    return; // Allow access
                }
            }

            // Check if user is logged in
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Redirect to login page with return URL
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Home", new { returnUrl });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action executes
        }
    }
}

