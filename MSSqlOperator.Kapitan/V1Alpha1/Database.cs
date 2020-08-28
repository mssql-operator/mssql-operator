using System;
using System.Collections.Generic;
using System.Text;
using Kapitan.Core;
using Kapitan.Kubernetes.Core.V1;

namespace MSSqlOperator.Kapitan.V1Alpha1
{
    public class Database : IManifestObject
    {
        public string apiVersion => "mssql-operator.github.io/v1alpha1";
        public string kind => "Database";

        public DatabaseSpec spec { get; set; }
    }
}
