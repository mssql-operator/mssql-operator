using System;
using k8s;
using k8s.Models;

namespace OperatorSharp.CustomResources
{
    // Adapted from https://github.com/engineerd/kubecontroller-csharp
    public abstract class CustomResource : KubernetesObject 
    {
        public V1ObjectMeta Metadata { get; set; }
    }

    public abstract class CustomResource<TSpec, TStatus> : CustomResource {
        public TSpec Spec { get; set; }
        public TStatus Status { get; set; }
    }
}
