using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Auth_Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AuthApiClient _authApiClient;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
            _authApiClient = new AuthApiClient();
        }

        public async Task Invoke(HttpContext context)
        {
            var projectName = context.Request.PathBase.Value?.Replace("/", "") ?? "";
            var endpointRoute = context.Request.Path.Value?.Replace($"/{projectName}", "") ?? "";
            var httpMethod = MapMethodToEHTTPMethod(context.Request.Method);

            if(endpointRoute.StartsWith("/swagger") || endpointRoute == "/endpoints/search-many")
            {
                await _next(context);
                return;
            }

            var getManyEndpointsRequest = new GetManyEndpointRequest
            {
                ProjectName = projectName,
                Route = endpointRoute,
                HttpMethod = httpMethod
            };

            var endpoints = await _authApiClient.GetEndpoints(getManyEndpointsRequest);

            if(!endpoints.Any())
                throw new Exception("Endpoint not found");

            if(endpoints.First().IsPublic)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!ValidateToken(token))
                throw new UnauthorizedAccessException("Invalid token");

            var verifyUserHasAccessRequest = new VerifyUserHasAccessRequest
            {
                EndpointId = endpoints.First().Id,
            };

            var userHasAccess = await _authApiClient.VerifyIfUserHasAccessToEndpoint(verifyUserHasAccessRequest, token);

            if (!userHasAccess)
                throw new UnauthorizedAccessException("User has not access to this endpoint");

            await _next(context);
        }

        private bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("iZvh55fBzMpwiyFwzcBFxzE8aUExGWEsusJhIGZmQdKdc0mvvv49Jfq3YCEjcbsG");

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidIssuer = "auth-api",
                    ValidateAudience = false,
                    ActorValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = TimeSpan.Zero
                    }
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private EHttpMethod MapMethodToEHTTPMethod(string method)
        {
            return method.ToUpper() switch
            {
                "PUT" => EHttpMethod.PUT,
                "POST" => EHttpMethod.POST,
                "GET" => EHttpMethod.GET,
                "DELETE" => EHttpMethod.DELETE,
                _ => throw new ArgumentOutOfRangeException($"Unsupported HTTP method: {method}")
            };
        }

        internal enum EHttpMethod
        {
            [Description("PUT")]
            PUT,
            [Description("POST")]
            POST,
            [Description("GET")]
            GET,
            [Description("DELETE")]
            DELETE
        }
    }
}
