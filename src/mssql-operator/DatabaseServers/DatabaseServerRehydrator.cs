using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MSSqlOperator.Services;

namespace MSSqlOperator.DatabaseServers
{
    public class DatabaseServerRehydrator
    {
        private readonly IKubernetesService service;
        private readonly ILogger<DatabaseServerRehydrator> logger;

        public DatabaseServerRehydrator(IKubernetesService service, ILogger<DatabaseServerRehydrator> logger)
        {
            this.service = service;
            this.logger = logger;
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
                server.Spec.AdminPasswordSecret = Rehydrate(server.Metadata.NamespaceProperty, server.Spec.AdminPasswordSecret);
            }

            return server;
        }

        public SecretSource Rehydrate(string namespaceProperty, SecretSource secretSource)
        {
            var keyRef = secretSource.SecretKeyRef;
            var secret = service.GetSecret(namespaceProperty, keyRef.Name);
            if (secret?.Data.ContainsKey(keyRef.Key) ?? false)
            {
                var data = secret.Data[keyRef.Key];
                secretSource.Value = Encoding.Default.GetString(data);
            }
            else
            {
                logger.LogWarning("Secret named {secret}:{key} could be found to satisfy adminPasswordSecret", keyRef.Name, keyRef.Key);
            }

            return secretSource;
        }
    }
}
