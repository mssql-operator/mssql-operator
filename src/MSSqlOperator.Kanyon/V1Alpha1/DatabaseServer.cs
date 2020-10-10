using System;
using Kanyon.Core;
using MSSqlOperator.Kapitan.V1Alpha1;
using Newtonsoft.Json;

namespace MSSqlOperator.Kapitan
{
    public class DatabaseServer : IManifestObject
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion => "mssql-operator.github.io/v1alpha1";
        [JsonProperty("kind")]
        public string Kind => "DatabaseServer";

        public DatabaseServerSpec spec { get; set; }
    }
}
