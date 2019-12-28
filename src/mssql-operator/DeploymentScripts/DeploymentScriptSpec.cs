using k8s.Models;

namespace MSSqlOperator.DeploymentScripts
{
    public class DeploymentScriptSpec
    {
        public V1LabelSelector DatabaseSelector { get; set; }
        public string Script { get; set; }
    }
}
