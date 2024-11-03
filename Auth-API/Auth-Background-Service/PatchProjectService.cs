using Microsoft.Extensions.Hosting;

namespace Auth_Background_Service
{
    public class PatchProjectService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process starting ###");
            ExecuteProcess();
            return Task.CompletedTask;
        }

        private void ExecuteProcess()
        {
            Console.WriteLine("### Process executing ###");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("### Process stopping ###");
            return Task.CompletedTask;
        }
    }
}
