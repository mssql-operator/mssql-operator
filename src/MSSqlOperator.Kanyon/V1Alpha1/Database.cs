using System;
using System.Collections.Generic;
using System.Text;
using Kanyon.Core;
using Kanyon.Kubernetes.Core.V1;
using Newtonsoft.Json;

namespace MSSqlOperator.Kanyon.V1Alpha1
{
    public class Database : IManifestObject
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion => "mssql-operator.github.io/v1alpha1";
        [JsonProperty("kind")]
        public string Kind => "Database";

        public ObjectMeta metadata { get; set; }

        public DatabaseSpec spec { get; set; }
    }
}
