using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using k8s;
using OperatorSharp.CustomResources.Metadata;
using Microsoft.Extensions.Logging;

namespace OperatorSharp
{
    public abstract class Operator 
    {
        public abstract Task WatchAsync(CancellationToken token, string watchedNamespace);
    }

    public abstract class Operator<TCustomResource> : Operator
        where TCustomResource: CustomResources.CustomResource
    {

        public Operator(Kubernetes client, ILogger<Operator<TCustomResource>> logger)
        {
            Client = client;
            Logger = logger;
        }

        protected Kubernetes Client { get; private set; }
        public ILogger<Operator<TCustomResource>> Logger { get; }

        public ApiVersion ApiVersion => GetAttribute<TCustomResource, ApiVersionAttribute>().ApiVersion;

        public abstract void HandleItem(WatchEventType eventType, TCustomResource item);

        public abstract void HandleException(Exception ex);

        public override async Task WatchAsync(CancellationToken token, string watchedNamespace)
        {
            Logger.LogDebug("Starting operator for {operator} operator", GetType().Name);
            string plural = GetAttribute<TCustomResource, PluralNameAttribute>().PluralName;

            Logger.LogDebug("Initiating watch for {resource}", plural);
            var result = await Client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                ApiVersion.Group, ApiVersion.Version, watchedNamespace, plural, watch: true, timeoutSeconds: 30
            );

            Logger.LogInformation("Watching {plural} resource in {namespace} namespace", plural, watchedNamespace);

            while (!token.IsCancellationRequested) {
                result.Watch<TCustomResource, object>(HandleItem, HandleException);
            }
        }

        public TAttribute GetAttribute<TResource, TAttribute>() where TAttribute: Attribute 
        {
            TAttribute attribute = Attribute.GetCustomAttribute(typeof(TResource), typeof(TAttribute)) as TAttribute;

            return attribute;
        }
    }
}