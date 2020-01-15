using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MSSqlOperator.Secrets;
using MSSqlOperator.Services;

namespace MSSqlOperator.DatabaseServers
{
    public class DatabaseServerRehydrator
    {
        private readonly IKubernetesService service;
        private readonly ILogger<DatabaseServerRehydrator> logger;
        private readonly SecretSourceRehydrator secretRehydrator;

        public DatabaseServerRehydrator(IKubernetesService service, ILogger<DatabaseServerRehydrator> logger, SecretSourceRehydrator secretRehydrator)
        {
            this.service = service;
            this.logger = logger;
            this.secretRehydrator = secretRehydrator;
        }

        public DatabaseServerResource Rehydrate(DatabaseServerResource server)
        {
            if (string.IsNullOrEmpty(server?.Spec.ServiceUrl) && server?.Spec.ServiceSelector != null)
            {
                var services = service.GetService(server.Metadata.NamespaceProperty, server.Spec.ServiceSelector);
                var sqlService = services?.Items?.FirstOrDefault();

                if (sqlService != null)
                {
                    server.Spec.ServiceUrl = $"{sqlService.Metadata.Name}.{sqlService.Metadata.NamespaceProperty}.svc,{sqlService.Spec.Ports.FirstOrDefault()?.Port}";
                }
            }

            if (string.IsNullOrEmpty(server?.Spec.AdminPasswordSecret.Value) && server?.Spec.AdminPasswordSecret.SecretKeyRef != null)
            {
                server.Spec.AdminPasswordSecret = secretRehydrator.Rehydrate(server.Metadata.NamespaceProperty, server.Spec.AdminPasswordSecret);
            }

            return server;
        }
    }
}
