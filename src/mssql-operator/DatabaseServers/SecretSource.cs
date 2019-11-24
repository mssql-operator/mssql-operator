using k8s.Models;

namespace MSSqlOperator.Models
{
    public class SecretSource {
        public string Value { get; set; }
        public V1SecretKeySelector SecretKeyRef { get; set; }
    }
}