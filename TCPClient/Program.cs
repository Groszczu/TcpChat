using Core;
using Microsoft.Extensions.DependencyInjection;
using TCPClient.Services;

namespace TCPClient
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code
            serviceProvider.GetService<Client>().Run("127.0.0.1", 13000);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IPacketFormatter, PacketFormatter>();
            services.AddSingleton<ICommandHandler, OperationsHandler>();
            services.AddTransient<Client>();
            return services;
        }
    }
}