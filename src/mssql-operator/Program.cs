using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSSqlOperator.Operators;
using System.Threading;
using MSSqlOperator.Services;
using System.Net.Http;
using Microsoft.Extensions.Http.Logging;

namespace MSSqlOperator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var tokenSource = new CancellationTokenSource();
            var ctrlc = new ManualResetEventSlim();
            try
            {
                // TODO: Would using a HostBuilder make this better?
                Console.CancelKeyPress += (_, __) => { tokenSource.Cancel(); ctrlc.Set(); };

                var operators = new List<IOperatorScope>
                {
                    new OperatorScope<DatabaseOperator>(services),
                    // new OperatorScope<DatabaseServerOperator>(services)
                };

                var tasks = operators.Select(os => os.StartAsync("default", tokenSource.Token));
                await Task.WhenAll(tasks);
                ctrlc.Wait();
            }
            finally
            {
                if (services is IDisposable disposable) {
                    disposable.Dispose();
                }
                tokenSource.Dispose();
                ctrlc.Dispose();
            }
            
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("k8s");

            services.AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(configure => configure.MinLevel = LogLevel.Debug);
            services.AddScoped<IKubernetes, Kubernetes>(provider => {
                var factory = provider.GetRequiredService<ILoggerFactory>();
                var logger = factory.CreateLogger<HttpClient>();
                var loggingHandler = new LoggingHttpMessageHandler(logger);
                var config = KubernetesClientConfiguration.IsInCluster() ? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildDefaultConfig();
                return new Kubernetes(config, loggingHandler);
            });
            services.AddScoped<IKubernetesService, KubernetesService>();
            services.AddScoped<ISqlManagementService, SqlManagementService>();
            services.AddScoped<DatabaseOperator>();
            services.AddScoped<DatabaseServerOperator>();

            return services.BuildServiceProvider();
        }
    }
}
