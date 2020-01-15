using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using MSSqlOperator.Services;

namespace MSSqlOperator.Secrets
{
    public class SecretSourceRehydrator
    {
        private readonly IKubernetesService service;
        private readonly ILogger<SecretSourceRehydrator> logger;

        public SecretSourceRehydrator(IKubernetesService service, ILogger<SecretSourceRehydrator> logger)
        {
            this.service = service;
            this.logger = logger;
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
                logger.LogWarning("Secret named {secret}:{key} could be found", keyRef.Name, keyRef.Key);
            }

            return secretSource;
        }
    }
}
