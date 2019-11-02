using System;
using k8s.Models;
using OperatorSharp.CustomResources;
using OperatorSharp.CustomResources.Metadata;

namespace MSSqlOperator.Models
{
    [ApiVersion("mssql.techpyramid.ws/v1alpha1")]
    [Kind("DatabaseServer")]
    [PluralName("DatabaseServers")]
    [ShortName("dbms")]
    public class DatabaseServerResource : CustomResource<DatabaseServer, DatabaseServerStatus>
    {
    }

    public class DatabaseServer 
    {
        public V1LabelSelector ServiceSelector { get; set; }
        public string AdminUserName { get; set; }
        public SecretSource AdminPasswordSecret { get; set; }
        public string ServiceUrl { get; set; }
    }
}