using Kanyon.Kubernetes.Core.V1;

namespace MSSqlOperator.Kanyon.V1Alpha1
{
    public class DeploymentScriptSpec
    {
        public LabelSelector databaseSelector { get; set; }
        public string script { get; set; }
    }
}
