using System;
using System.Linq;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using MSSqlOperator.DeploymentScripts;
using MSSqlOperator.Models;
using MSSqlOperator.Operators;
using Newtonsoft.Json.Linq;
using OperatorSharp;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.Services
{
    public class KubernetesService : IKubernetesService
    {
        private readonly IKubernetes client;
        private readonly ILogger<KubernetesService> logger;

        public KubernetesService(IKubernetes client, ILogger<KubernetesService> logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public CustomResourceList<DatabaseServerResource> GetDatabaseServer(string namespaceProperty, V1LabelSelector selector)
        {
            try
            {
                var serverSelector = selector.BuildSelector();
                logger.LogDebug("Loading referenced server {selector}", serverSelector);
                var plural = DatabaseServerOperator.PluralName.ToLower();
                var query = client.ListNamespacedCustomObject(DatabaseServerOperator.ApiVersion.Group, DatabaseServerOperator.ApiVersion.Version, namespaceProperty, plural, labelSelector: serverSelector);
                
                var server = ((JObject)query).ToObject<CustomResourceList<DatabaseServerResource>>();

                return server;
            }
            catch (HttpOperationException opEx) when (opEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public CustomResourceList<DatabaseResource> GetDatabases(string namespaceProperty, V1LabelSelector selector)
        {
            try
            {
                var databaseSelector = selector.BuildSelector();
                logger.LogDebug("Loading referenced server {selector}", databaseSelector);
                var plural = DatabaseOperator.PluralName.ToLower();
                var query = client.ListNamespacedCustomObject(DatabaseOperator.ApiVersion.Group, DatabaseOperator.ApiVersion.Version, namespaceProperty, plural, labelSelector: databaseSelector);

                var databases = ((JObject)query).ToObject<CustomResourceList<DatabaseResource>>();

                return databases;
            }
            catch (HttpOperationException opEx) when (opEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public V1ServiceList GetService(string namespaceProperty, V1LabelSelector selector)
        {
            try
            {
                var serviceSelector = selector.BuildSelector();
                logger.LogDebug("Loading referenced service {selector}", serviceSelector);
                var service = client.ListNamespacedService(namespaceProperty, labelSelector: serviceSelector);

                return service;
            }
            catch (HttpOperationException opEx) when (opEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public V1Secret GetSecret(string namespaceProperty, string name) 
        {
            try 
            {
                logger.LogDebug("Loading referenced secret {secret}", name);
                var secret = client.ReadNamespacedSecret(name, namespaceProperty);

                return secret;
            }
            catch (HttpOperationException opEx) when (opEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public void UpdateDatabaseStatus(DatabaseResource resource, string reason, string message, DateTimeOffset date) 
        {
            resource.Status = new DatabaseStatus {
                LastUpdate = date,
                Reason = reason,
                Message = message
            };

            UpdateStatus(resource);
        }

        public void UpdateDeploymentScriptStatus(DeploymentScriptResource resource, string reason, string message, DateTimeOffset date)
        {
            resource.Status = new DeploymentScriptStatus
            {
                LastUpdate = date,
                Reason = reason,
                Message = message
            };

            UpdateStatus(resource);
        }

        public void UpdateStatus<TSpec, TStatus>(CustomResource<TSpec, TStatus> resource)
        {
            client.ReplaceNamespacedCustomObjectStatus(resource, resource.ApiVersionMetadata.Group, resource.ApiVersionMetadata.Version, resource.Metadata.NamespaceProperty, resource.PluralName, resource.Metadata.Name);
        }

        public void EmitEvent(string action, string reason, string message, CustomResource involvedObject)
        {
            var objRef = new V1ObjectReference(involvedObject.ApiVersion, kind: involvedObject.Kind, name: involvedObject.Metadata.Name, namespaceProperty: involvedObject.Metadata.NamespaceProperty);
            V1Event ev = new V1Event(objRef, new V1ObjectMeta() { GenerateName = involvedObject.Metadata.Name }, action: action, message: message, reason: reason, firstTimestamp: DateTime.Now);

            client.CreateNamespacedEvent(ev, involvedObject.Metadata.NamespaceProperty);
        }
    }
}
