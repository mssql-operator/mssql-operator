using System;
using k8s.Models;
using MSSqlOperator.DeploymentScripts;
using MSSqlOperator.Models;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.Services
{
    public interface IKubernetesService
    {
        void EmitEvent(string action, string reason, string message, CustomResource involvedObject);
        CustomResourceList<DatabaseResource> GetDatabases(string namespaceProperty, V1LabelSelector selector);
        CustomResourceList<DatabaseServerResource> GetDatabaseServer(string namespaceProperty, V1LabelSelector selector);
        V1Secret GetSecret(string namespaceProperty, string name);
        V1ServiceList GetService(string namespaceProperty, V1LabelSelector selector);
        void UpdateDatabaseStatus(DatabaseResource resource, string reason, string message, System.DateTimeOffset date);
        void UpdateDeploymentScriptStatus(DeploymentScriptResource resource, string reason, string message, DateTimeOffset date);
        void UpdateStatus<TSpec, TStatus>(CustomResource<TSpec, TStatus> resource);
    }
}
