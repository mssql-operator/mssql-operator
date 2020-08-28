using System;
using System.Collections.Generic;
using System.Text;
using Kapitan.Core;

namespace MSSqlOperator.Kapitan.V1Alpha1
{
    public class DeploymentScript : IManifestObject
    {
        public string apiVersion => "mssql-operator.github.io/v1alpha1";
        public string kind => "DeploymentScript";

        public DeploymentScriptSpec spec { get; set; }
    }
}
