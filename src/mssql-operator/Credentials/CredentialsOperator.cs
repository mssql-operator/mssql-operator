using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using MSSqlOperator.DatabaseServers;
using MSSqlOperator.Secrets;
using MSSqlOperator.Services;
using OperatorSharp;

namespace MSSqlOperator.Credentials
{
    public class CredentialsOperator : RepeatingQueuedOperator<CredentialsResource>
    {
        private readonly IKubernetesService k8sService;
        private readonly DatabaseServerRehydrator rehydrator;
        private readonly IEventRecorder<CredentialsResource> eventRecorder;
        private readonly ISqlManagementService sqlService;
        private readonly SecretSourceRehydrator secretSourceRehydrator;

        public CredentialsOperator(IKubernetes client, 
            ILogger<Operator<CredentialsResource>> logger,
            IKubernetesService k8sService,
            DatabaseServerRehydrator rehydrator,
            IEventRecorder<CredentialsResource> eventRecorder,
            ISqlManagementService sqlService,
            SecretSourceRehydrator secretSourceRehydrator) : base(client, logger, 1000, 100, 5)
        {
            this.k8sService = k8sService;
            this.rehydrator = rehydrator;
            this.eventRecorder = eventRecorder;
            this.sqlService = sqlService;
            this.secretSourceRehydrator = secretSourceRehydrator;
        }

        public override bool HandleDequeuedItem(WatchEventType eventType, CredentialsResource item, int previousExecutionCount)
        {
            try
            {
                if (eventType != WatchEventType.Deleted)
                {
                    int executionCount = 0;
                    secretSourceRehydrator.Rehydrate(item.Metadata.NamespaceProperty, item.Spec.Secret);

                    var databases = k8sService.GetDatabases(item.Metadata.NamespaceProperty, item.Spec.DatabaseSelector);
                    foreach (var database in databases.Items)
                    {
                        if (database.Status.Reason == "Available")
                        {
                            var servers = k8sService.GetDatabaseServer(item.Metadata.NamespaceProperty, database.Spec.DatabaseSelector);
                            foreach (var server in servers.Items)
                            {
                                rehydrator.Rehydrate(server);
                                if (sqlService.DoesCredentialExist(server.Spec, item.Spec.CredentialName))
                                {
                                    sqlService.UpdateCredential(server.Spec, item.Spec);
                                }
                                else
                                {
                                    sqlService.CreateCredential(server.Spec, item.Spec);
                                }

                                executionCount++;
                            }
                        }
                    }

                    if (executionCount > 0)
                    {
                        RecordStatus(item, "Executed", "Successfully executed");
                        return true;
                    }

                    return false;
                }
                else
                {
                    var databases = k8sService.GetDatabases(item.Metadata.NamespaceProperty, item.Spec.DatabaseSelector);
                    foreach (var database in databases.Items)
                    {
                        if (database.Status.Reason == "Available")
                        {
                            var servers = k8sService.GetDatabaseServer(item.Metadata.NamespaceProperty, database.Spec.DatabaseSelector);
                            foreach (var server in servers.Items)
                            {
                                rehydrator.Rehydrate(server);
                                sqlService.DeleteCredential(server.Spec, item.Spec);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred during message processing");

                try
                {
                    RecordStatus(item, "Failed", ex.GetType().Name);
                    eventRecorder.Record("ExecuteDeploymentScript",
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
                catch (Exception)
                {
                    throw;
                }
            }
            return true;
        }

        private void RecordStatus(CredentialsResource item, string reason, string message)
        {
            item.Status = new CredentialsStatus(DateTimeOffset.Now, reason, message);
            k8sService.UpdateStatus(item);
        }

        public override void HandleException(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
