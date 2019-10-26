using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Smo;
using OperatorSharp.CustomResources;
using OperatorSharp.CustomResources.Metadata;

namespace MSSqlOperator
{
    [ApiVersion("mssql.techpyramid.ws/v1alpha1")]
    [Kind("Database")]
    [PluralName("databases")]
    [ResourceScope(ResourceScopes.Namespaced)]
    [ShortName("db")]
    public class DatabaseResource : CustomResource<DatabaseSpec, DatabaseStatus> 
    {
    }

    public class DatabaseSpec 
    {
        public string Collation { get; set; }
        public List<DatabaseFile> LogFiles { get; set; }
        public List<DatabaseFile> BackupFiles { get; set; }
        public Dictionary<string, List<DatabaseFile>> DataFiles { get; set; }
        public GarbageCollectionStrategy GCStrategy { get; set; }
    }
}