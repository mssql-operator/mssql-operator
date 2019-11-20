using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using k8s;
using MSSqlOperator.Models;
using Newtonsoft.Json.Linq;
using OperatorSharp;
using System.Data.SqlClient;
using k8s.Models;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using MSSqlOperator.Services;
using System.Collections.Generic;
using Microsoft.Rest;

namespace MSSqlOperator.Operators
{
    public class DatabaseOperator : Operator<DatabaseResource>
    {
        private readonly IKubernetesService k8sService;
        private readonly ISqlManagementService sqlService;

        public DatabaseOperator(IKubernetes client, ILogger<Operator<DatabaseResource>> logger, IKubernetesService k8sService, ISqlManagementService sqlService) : base(client, logger)
        {
            this.k8sService = k8sService;
            this.sqlService = sqlService;
        }

        public override void HandleException(Exception ex)
        {
            Logger.LogError(ex, "An unknown error occured while processing the item");
        }

        public override void HandleItem(WatchEventType eventType, DatabaseResource item)
        {
            Logger.LogDebug("Recieved new Database object (v {ResourceVersion})", item.Metadata.ResourceVersion);
            try {
                var servers = GetServerResources(item.Metadata.NamespaceProperty, item.Spec.DatabaseSelector);
                foreach (var server in servers) {
                    if (eventType == WatchEventType.Added) 
                    {
                        if (sqlService.DoesDatabaseExist(server.Spec, item.Metadata.Name)) 
                        {
                            Logger.LogInformation("Database {database} already exists on server {server}", item.Metadata.Name, server.Metadata.Name);
                            k8sService.UpdateDatabaseStatus(item, "Available", "Database already exists", DateTimeOffset.Now);
                            continue;
                        } 

                        if (item.Spec.BackupFiles?.Any() ?? false) 
                        {
                            sqlService.RestoreDatabase(server.Spec, item);
                            k8sService.UpdateDatabaseStatus(item, "Available", "Database restored", DateTimeOffset.Now);
                        }
                        else
                        {
                            sqlService.CreateDatabase(server.Spec, item);
                            k8sService.UpdateDatabaseStatus(item, "Available", "Database created", DateTimeOffset.Now);
                        }

                        Logger.LogInformation("Created database {database}", item.Metadata.Name);
                    }
                    else if (eventType == WatchEventType.Deleted) 
                    {
                        if (item.Spec.GCStrategy == GarbageCollectionStrategy.Delete) 
                        {
                            sqlService.DeleteDatabase(server.Spec, item.Metadata.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred during message processing");

                try
                {
                    k8sService.UpdateDatabaseStatus(item, "Failed", ex.GetType().Name, DateTimeOffset.Now);
                    k8sService.EmitEvent("CreateDatabase", "Failed", ex.Message, DateTime.Now, item);
                }
                catch (HttpOperationException httpEx)
                {
                    Logger.LogError(httpEx, "An error occurred logging this info to Kubernetes");
                    Logger.LogDebug(httpEx.Response.Content);
                }
                catch (Exception) {
                    throw;
                }
            }
        }

        public IEnumerable<DatabaseServerResource> GetServerResources(string namespaceProperty, V1LabelSelector selector) 
        {
            var servers = k8sService.GetDatabaseServer(namespaceProperty, selector);

            foreach (var server in servers.Items) {
                if (string.IsNullOrEmpty(server?.Spec.ServiceUrl) && server?.Spec.ServiceSelector != null)
                {
                    var services = k8sService.GetService(namespaceProperty, server.Spec.ServiceSelector);
                    var sqlService = services?.Items?.FirstOrDefault();

                    if (sqlService != null) 
                    {
                        server.Spec.ServiceUrl = $"{sqlService.Metadata.Name}.{sqlService.Metadata.NamespaceProperty}.svc,{sqlService.Spec.Ports.FirstOrDefault()?.Port}";
                    }
                }

                if (string.IsNullOrEmpty(server?.Spec.AdminPasswordSecret.Value) && server?.Spec.AdminPasswordSecret.SecretKeyRef != null)
                {
                    server.Spec.AdminPasswordSecret.Value = GetValueForSecretReference(namespaceProperty, server.Spec.AdminPasswordSecret.SecretKeyRef);
                }
            }

            return servers.Items;
        }

        private string GetValueForSecretReference(string namespaceProperty, V1SecretKeySelector keyRef)
        {
            var secret = k8sService.GetSecret(namespaceProperty, keyRef.Name);
            if (secret?.Data.ContainsKey(keyRef.Key) ?? false)
            {
                var data = secret.Data[keyRef.Key];
                return Encoding.Default.GetString(data);
            }
            else
            {
                Logger.LogWarning("Secret named {secret}:{key} could be found to satisfy adminPasswordSecret", keyRef.Name, keyRef.Key);
            }

            return null;
        }
    }
}
