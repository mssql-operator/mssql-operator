using k8s.Models;

namespace MSSqlOperator
{
    public class SecretSource
    {
        public string Value { get; set; }
        public V1SecretKeySelector SecretKeyRef { get; set; }
    }
}
