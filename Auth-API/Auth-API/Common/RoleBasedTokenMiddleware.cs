using Auth_API.Repositories;
using System.IdentityModel.Tokens.Jwt;

namespace Auth_API.Common
{
    public class RoleBasedTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public RoleBasedTokenMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();

            var projectName = context.Request.PathBase.Value?.Replace("/", "") ?? "";
            var endpointRoute = context.Request.Path.Value?.Replace($"/{projectName}", "") ?? "";

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "local" && endpointRoute.StartsWith("/swagger/"))
            {
                await _next(context);
                return;
            }

            var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();
            var endpoint = await endpointRepository.GetSingle(endpoint => endpoint.Project.Name == projectName && endpoint.Route == endpointRoute);
            if (endpoint == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Endpoint not found.");
                return;
            }

            if (endpoint.IsPublic)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = ValidateAndExtractUserId(token);

            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetSingle(u => u.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var userHasAccess = await userRepository.UserHasAccess(userId, endpoint.Id);
            if (!userHasAccess)
                throw new UnauthorizedAccessException("User does not have access to this endpoint.");

            await _next(context);
        }

        private int ValidateAndExtractUserId(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Authorization token is missing.");

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                throw new UnauthorizedAccessException("Invalid authorization token.");

            var jwtToken = handler.ReadJwtToken(token);
            var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim))
                throw new UnauthorizedAccessException("User identifier claim is missing in the token.");

            return int.Parse(nameIdentifierClaim);
        }

    }
}
