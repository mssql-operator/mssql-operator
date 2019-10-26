using System;
using k8s;
using k8s.Models;

namespace OperatorSharp.CustomResources
{
    // Adapted from https://github.com/engineerd/kubecontroller-csharp
    public abstract class CustomResource : KubernetesObject 
    {
        public V1ObjectMeta Metadata { get; set; }

        public T GetAttribute<T>() where T: Attribute {
            T attribute = Attribute.GetCustomAttribute(this.GetType(), typeof(T)) as T;

            return attribute;
        }
    }

    public abstract class CustomResource<TSpec, TStatus> : CustomResource {
        public TSpec Spec { get; set; }
        public TStatus Status { get; set; }
    }
}
