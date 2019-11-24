using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSSqlOperator.Models;
using MSSqlOperator.Services;

namespace MSSqlOperator.DatabaseServers
{
    public static class DatabaseServerExtensions
    {
        public static DatabaseServerResource Rehydrate(this IKubernetesService service, DatabaseServerResource server)
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
                server.Spec.AdminPasswordSecret = service.Rehydrate(server.Metadata.NamespaceProperty, server.Spec.AdminPasswordSecret);
            }

            return server;
        }

        public static SecretSource Rehydrate(this IKubernetesService service, string namespaceProperty, SecretSource secretSource)
        {
            var keyRef = secretSource.SecretKeyRef;
            var secret = service.GetSecret(namespaceProperty, keyRef.Name);
            if (secret?.Data.ContainsKey(keyRef.Key) ?? false)
            {
                var data = secret.Data[keyRef.Key];
                secretSource.Value = Encoding.Default.GetString(data);
            }

            return secretSource;
        }
    }
}
