using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;
using App.Metrics.Counter;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using MSSqlOperator.DatabaseServers;
using MSSqlOperator.Services;
using OperatorSharp;
using OperatorSharp.Filters;

namespace MSSqlOperator.DeploymentScripts
{
    public class DeploymentScriptOperator : RepeatingQueuedOperator<DeploymentScriptResource>
    {
        private readonly IKubernetesService k8sService;
        private readonly DatabaseServerRehydrator rehydrator;
        private readonly ISqlManagementService sqlService;
        private readonly IEventRecorder<DeploymentScriptResource> eventRecorder;

        public DeploymentScriptOperator(IKubernetes client, 
            ILogger<Operator<DeploymentScriptResource>> logger, 
            IKubernetesService k8sService,
            DatabaseServerRehydrator rehydrator,
            ISqlManagementService sqlService,
            IEventRecorder<DeploymentScriptResource> eventRecorder,
            IMetrics metrics) : base(client, logger, 1000, 100, 5)
        {
            this.k8sService = k8sService;
            this.rehydrator = rehydrator;
            this.sqlService = sqlService;
            this.eventRecorder = eventRecorder;
            Metrics = metrics;

            Filters.Add(new IgnoreStatusUpdatesOperatorFilter<DeploymentScriptResource, DeploymentScriptStatus>());
        }

        public static CounterOptions options = new CounterOptions() { Name = "Script Executions" };

        public override void HandleException(Exception ex)
        {
            throw new NotImplementedException();
        }

        public override bool HandleDequeuedItem(WatchEventType eventType, DeploymentScriptResource item, int previousExecutionCount)
        {
            Metrics.Measure.Counter.Increment(options);
            if (eventType != WatchEventType.Deleted)
            {
                try
                {
                    int executionCount = 0;
                    var databases = k8sService.GetDatabases(item.Metadata.NamespaceProperty, item.Spec.DatabaseSelector);
                    foreach (var database in databases.Items)
                    {
                        if (database.Status.Reason == "Available")
                        {
                            var servers = k8sService.GetDatabaseServer(item.Metadata.NamespaceProperty, database.Spec.DatabaseServerSelector);
                            foreach (var server in servers.Items)
                            {
                                rehydrator.Rehydrate(server);

                                sqlService.ExecuteScript(server.Spec, database, item.Spec.Script);
                                executionCount++;
                            }
                        }
                    }

                    if (executionCount > 0)
                    {
                        k8sService.UpdateDeploymentScriptStatus(item, "Executed", "Successfully executed", DateTimeOffset.Now);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error occurred during message processing");

                    try
                    {
                        k8sService.UpdateDeploymentScriptStatus(item, "Failed", ex.GetType().Name, DateTimeOffset.Now);
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

                return false;
            }

            return true;
        }
    }
}
