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
using OperatorSharp;
using MSSqlOperator.DatabaseServers;
using MSSqlOperator.DeploymentScripts;
using App.Metrics;

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
                    new OperatorScope<DeploymentScriptOperator>(services)
                    // new OperatorScope<DatabaseServerOperator>(services)
                };

                var tasks = operators.Select(os => os.StartAsync("default", tokenSource.Token));
                var reporter = ReportMetrics(services, tokenSource.Token);
                await Task.WhenAll(tasks.Append(reporter));
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

        private static async Task ReportMetrics(IServiceProvider services, CancellationToken token = default)
        {
            var metrics = services.GetService<IMetricsRoot>();
            while (!token.IsCancellationRequested)
            {
                await Task.WhenAll(metrics.ReportRunner.RunAllAsync(token));
                await Task.Delay(1000);
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddHttpClient("k8s");

            services.AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(configure => configure.MinLevel = LogLevel.Debug);

            services.AddMetrics(builder => {
#if DEBUG
                builder.Report.ToConsole();
#endif
                });

            services.AddScoped<IKubernetes, Kubernetes>(provider => {
                var factory = provider.GetRequiredService<ILoggerFactory>();
                var logger = factory.CreateLogger<HttpClient>();
                var loggingHandler = new LoggingHttpMessageHandler(logger);
                var config = KubernetesClientConfiguration.IsInCluster() ? KubernetesClientConfiguration.InClusterConfig() : KubernetesClientConfiguration.BuildDefaultConfig();
                return new Kubernetes(config, loggingHandler);
            });

            services.AddScoped<IKubernetesService, KubernetesService>();
            services.AddScoped<ISqlManagementService, SqlManagementService>();
            services.AddScoped<IEventRecorder<DatabaseResource>, EventRecorder<DatabaseResource>>();
            services.AddScoped<IEventRecorder<DeploymentScriptResource>, EventRecorder<DeploymentScriptResource>>();
            services.AddScoped<DatabaseServerRehydrator>();
            services.AddScoped<DatabaseOperator>();
            services.AddScoped<DatabaseServerOperator>();
            services.AddScoped<DeploymentScriptOperator>();

            return services.BuildServiceProvider();
        }
    }
}
