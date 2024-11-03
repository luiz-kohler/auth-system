using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Auth_Background_Service
{
    public class PatchProjectService : IHostedService
    {
        private readonly Assembly _assembly;

        public PatchProjectService(Assembly assembly)
        {
            _assembly = assembly;
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

            var result = _assembly.GetTypes()
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

                    return new
                    {
                        Route = route,
                        Action = MapMethodToEHTTPMethod(httpMethod),
                    };
                })
                .OrderBy(x => x.Route)
                .ToList();

            Console.WriteLine("### Process executing finished ###");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process stopping ###");
            return Task.CompletedTask;
        }

        private int MapMethodToEHTTPMethod(string method)
        {
            return method.ToUpper() switch
            {
                "PUT" => 0,
                "POST" => 1,
                "GET" => 2,
                "DELETE" => 3,
                _ => throw new ArgumentOutOfRangeException($"Unsupported HTTP method: {method}")
            };
        }
    }
}
