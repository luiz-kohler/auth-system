using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Auth_Background_Service
{
    public class PatchProjectService : IHostedService
    {
        private readonly PatchProjectProfile _profile;

        public PatchProjectService(PatchProjectProfile profile)
        {
            _profile = profile;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process starting ###");
            ExecuteProcess();
            return Task.CompletedTask;
        }

        private void ExecuteProcess()
        {
            Console.WriteLine("### Process executing started ###");

            var endpoints = GetAllEndpoints();

            Console.WriteLine("### Process executing finished ###");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process stopping ###");
            return Task.CompletedTask;
        }

        private List<EndpointForCreateProject> GetAllEndpoints()
        {
            return _profile.Assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Select(method =>
                {
                    var controllerRoute = method.DeclaringType.GetCustomAttribute<RouteAttribute>()?.Template ?? method.DeclaringType.Name.Replace("Controller", "");
                    var httpMethodAttribute = method.GetCustomAttributes().FirstOrDefault(attr => attr is HttpMethodAttribute) as HttpMethodAttribute;
                    var actionRoute = httpMethodAttribute?.Template ?? method.Name;
                    var route = $"{controllerRoute}/{actionRoute}".Trim('/');

                    var httpMethod = httpMethodAttribute?.HttpMethods.FirstOrDefault() ?? "GET";

                    return new EndpointForCreateProject
                    {
                        Route = route,
                        HttpMethod = MapMethodToEHTTPMethod(httpMethod),
                        IsPublic = false
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
