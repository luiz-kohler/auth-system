using Auth_API.Repositories;
using Auth_API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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

        //TODO: CREATE A CUSTOM EXCEPTION FOR NO AUTH
        public async Task Invoke(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();

            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
            var endpointRepository = scope.ServiceProvider.GetRequiredService<IEndpointRepository>();

            var projectName = context.Request.PathBase.Value.Replace("/", "");
            var project = await projectRepository.GetSingle(project => project.Name == projectName);

            var endpointRoute = context.Request.Path.Value.Replace($"/{projectName}", "");
            var endpoint = await endpointRepository.GetSingle(endpoint => endpoint.ProjectId == project.Id && endpoint.Route == endpointRoute);

            if (project == null || endpoint == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("Endpoint not found.");
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
                throw new InvalidOperationException();

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
                throw new InvalidOperationException();

            //TODO: USE SOME ENUM FOR THIS
            var jwtToken = handler.ReadJwtToken(token);
            var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            if (string.IsNullOrEmpty(nameIdentifierClaim))
                throw new InvalidOperationException();

            int userId = int.Parse(nameIdentifierClaim);
            var user = await userRepository.GetSingle(user => user.Id == userId);
            if (user == null)
                throw new InvalidOperationException();

            var userHasAccess = await userRepository.UserHasAccess(user.Id, endpoint.Id);
            if (!userHasAccess)
                throw new InvalidOperationException();

            await _next(context);
        }
    }
}
