using Core;
using Microsoft.Extensions.DependencyInjection;
using TCPServer.Models.Commands;
using TCPServer.Services;

namespace TCPServer
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
            serviceProvider.GetService<Server>().Run("127.0.0.1", 13000);
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            // IMPORTANT! Register our application entry point
            services.AddSingleton<IPacketFormatter, PacketFormatter>();
            services.AddSingleton<ISessionsRepository, SessionsRepository>();
            services.AddSingleton<ICommandHandler, OperationsHandler>();
            services.AddTransient<Server>();
            return services;
        }
    }

}
