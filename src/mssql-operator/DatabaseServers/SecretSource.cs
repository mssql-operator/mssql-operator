using k8s.Models;

namespace MSSqlOperator.DatabaseServers
{
    public class SecretSource {
        public string Value { get; set; }
        public V1SecretKeySelector SecretKeyRef { get; set; }
    }
}
