using System;
using Kapitan.Core;
using MSSqlOperator.Kapitan.V1Alpha1;

namespace MSSqlOperator.Kapitan
{
    public class DatabaseServer : IManifestObject
    {
        public string apiVersion => "mssql-operator.github.io/v1alpha1";
        public string kind => "DatabaseServer";

        public DatabaseServerSpec spec { get; set; }
    }
}
