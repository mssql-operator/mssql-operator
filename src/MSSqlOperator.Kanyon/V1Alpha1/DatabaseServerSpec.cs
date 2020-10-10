using Kanyon.Kubernetes.Core.V1;

namespace MSSqlOperator.Kapitan.V1Alpha1
{
    public class DatabaseServerSpec
    {
        public LabelSelector serviceSelector { get; set; }
        public string adminUserName { get; set; }
        public SecretSource adminPasswordSecret { get; set; }
        public string serviceUrl { get; set; }
    }
}
