using System;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.DeploymentScripts
{
    public class DeploymentScriptStatus : CustomResourceStatus
    {
        public DateTimeOffset LastUpdate { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
        public DateTimeOffset? ExecutedDate { get; set; }
    }
}
