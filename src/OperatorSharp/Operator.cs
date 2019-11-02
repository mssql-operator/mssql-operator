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
    public abstract class Operator<TCustomResource> where TCustomResource: CustomResources.CustomResource
    {
        private readonly int timeoutSeconds;

        public Operator(Kubernetes client, ILogger<Operator<TCustomResource>> logger, int timeoutSeconds = 30)
        {
            Client = client;
            Logger = logger;
            this.timeoutSeconds = timeoutSeconds;
        }

        protected Kubernetes Client { get; private set; }
        public ILogger<Operator<TCustomResource>> Logger { get; }

        public ApiVersion ApiVersion => GetAttribute<ApiVersionAttribute>().ApiVersion;

        public abstract void HandleItem(WatchEventType eventType, TCustomResource item);

        public abstract void HandleException(Exception ex);

        public async Task WatchAsync(CancellationToken token, string watchedNamespace)
        {
            string plural = GetAttribute<PluralNameAttribute>().PluralName;
            
            var result = await Client.ListNamespacedCustomObjectWithHttpMessagesAsync(
                ApiVersion.Group, ApiVersion.Version, watchedNamespace,  plural, watch: true, timeoutSeconds: timeoutSeconds
            );

            while (!token.IsCancellationRequested) {
                result.Watch<TCustomResource, object>(HandleItem, HandleException);
            }
        }

        public T GetAttribute<T>() where T: Attribute 
        {
            T attribute = Attribute.GetCustomAttribute(this.GetType(), typeof(T)) as T;

            return attribute;
        }
    }
}