using Core;
using Microsoft.Extensions.DependencyInjection;
using TCPServer.Services;

namespace TCPServer
{
    internal static class Program
    {
        private static void Main()
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Server>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IPacketFormatter, PacketFormatter>();
            services.AddSingleton<ISessionsRepository, SessionsRepository>();
            services.AddSingleton<ICommandHandler, OperationsHandler>();
            services.AddSingleton<IClientIdsRepository, ClientIdsRepository>();
            services.AddTransient<Server>();
            return services;
        }
    }

}
