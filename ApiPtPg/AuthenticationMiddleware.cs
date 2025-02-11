namespace ApiPtPg
{
    // AuthenticationMiddleware.cs

    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;

    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the user is authenticated
            if (!context.User.Identity.IsAuthenticated)
            {
                // Handle unauthorized access
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // If authenticated, proceed to the next middleware
            await _next(context);
        }
    }

}
