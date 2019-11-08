using Core;
using Microsoft.Extensions.DependencyInjection;

namespace TCPClient
{
    internal static class Program
    {
        private static void Main()
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<Client>().Run();
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