using System;
using System.Linq;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using MSSqlOperator.Models;
using MSSqlOperator.Operators;
using Newtonsoft.Json.Linq;
using OperatorSharp;

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

        public DatabaseServerResource GetDatabaseServer(string namespaceProperty, V1LabelSelector selector)
        {
            try
            {
                var serverSelector = selector.BuildSelector();
                logger.LogDebug("Loading referenced server {selector}", serverSelector);
                var plural = DatabaseServerOperator.PluralName.ToLower();
                var query = client.ListNamespacedCustomObject(DatabaseServerOperator.ApiVersion.Group, DatabaseServerOperator.ApiVersion.Version, namespaceProperty, plural, labelSelector: serverSelector);
                
                var server = ((JObject)query).ToObject<DatabaseServerResource>();

                return server;
            }
            catch (HttpOperationException opEx) when (opEx.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public V1Service GetService(string namespaceProperty, V1LabelSelector selector)
        {
            try
            {
                var serviceSelector = selector.BuildSelector();
                logger.LogDebug("Loading referenced service {selector}", serviceSelector);
                var service = client.ListNamespacedService(namespaceProperty, labelSelector: serviceSelector)?.Items?.FirstOrDefault();

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
    }
}