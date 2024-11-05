using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Auth_Background_Service
{
    public class UpsertProjectService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly UpsertProjectProfile _profile;
        private readonly AuthApiClient _authApiClient;

        public UpsertProjectService(UpsertProjectProfile profile, IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
            _profile = profile;
            _authApiClient = new AuthApiClient();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(() => ExecuteProcess(), cancellationToken);
            });

            return Task.CompletedTask;
        }

        private async Task ExecuteProcess()
        {
            Console.WriteLine("### Process executing started ###");

            var token = await Login();

            await UpsertProject(token);

            Console.WriteLine("### Process executing finished ###");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process stopping ###");
            return Task.CompletedTask;
        }

        private async Task<string> Login()
        {
            var loginRequest = new LoginRequest
            {
                Email = _profile.Email,
                Password = _profile.Password,
            };

            return await _authApiClient.Login(loginRequest);
        }

        private async Task UpsertProject(string token)
        {
            var request = new UpsertProjectRequest
            {
                Name = _profile.Project,
                Endpoints = GetAllEndpoints()
            };

            await _authApiClient.Upsert(request, token);
        }

        private List<EndpointForUpsertProject> GetAllEndpoints()
        {
            return _profile.Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Select(method =>
                {
                    var controllerRoute = method.DeclaringType.GetCustomAttribute<RouteAttribute>()?.Template ?? method.DeclaringType.Name.Replace("Controller", "");
                    var httpMethodAttribute = method.GetCustomAttributes().FirstOrDefault(attr => attr is HttpMethodAttribute) as HttpMethodAttribute;
                    var actionRoute = httpMethodAttribute?.Template ?? "";
                    var route = $"{controllerRoute}/{actionRoute}".Trim('/').ToLower();
                    var httpMethod = httpMethodAttribute?.HttpMethods.FirstOrDefault() ?? "GET";
                    var isPublic = method.GetCustomAttributes().Any(attr => attr is PublicAttribute);

                    return new EndpointForUpsertProject
                    {
                        Route = $"/{route}",
                        HttpMethod = MapMethodToEHTTPMethod(httpMethod),
                        IsPublic = isPublic
                    };
                })
                .OrderBy(x => x.Route)
                .ToList();
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
    }
}
