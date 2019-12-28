using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using k8s;
using OperatorSharp;
using k8s.Models;
using MSSqlOperator.Services;
using System.Collections.Generic;
using Microsoft.Rest;
using MSSqlOperator.DatabaseServers;

namespace MSSqlOperator.Databases
{
    public class DatabaseOperator : Operator<DatabaseResource>
    {
        private readonly IKubernetesService k8sService;
        private readonly ISqlManagementService sqlService;
        private readonly IEventRecorder<DatabaseResource> eventRecorder;
        private readonly DatabaseServerRehydrator rehydrator;

        public DatabaseOperator(IKubernetes client, 
            ILogger<Operator<DatabaseResource>> logger, 
            IKubernetesService k8sService, 
            ISqlManagementService sqlService, 
            IEventRecorder<DatabaseResource> eventRecorder,
            DatabaseServerRehydrator rehydrator)
        : base(client, logger)
        {
            this.k8sService = k8sService;
            this.sqlService = sqlService;
            this.eventRecorder = eventRecorder;
            this.rehydrator = rehydrator;
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
                    eventRecorder.Record("CreateDatabase", 
                        "Failed", 
                        ex.Message, 
                        new V1ObjectReference(
                            item.ApiVersion, 
                            kind: item.Kind, 
                            name: item.Metadata.Name,
                            namespaceProperty: item.Metadata.NamespaceProperty)
                    );
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
                rehydrator.Rehydrate(server);
            }

            return servers.Items;
        }
    }
}
