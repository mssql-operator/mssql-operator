using System.Collections.Generic;
using Kanyon.Kubernetes.Core.V1;

namespace MSSqlOperator.Kanyon.V1Alpha1
{
    public class DatabaseSpec
    {
        public LabelSelector databaseServerSelector { get; set; }
        public string collation { get; set; }
        public List<DatabaseFile> logFiles { get; set; }
        public List<DatabaseFile> backupFiles { get; set; }
        public Dictionary<string, List<DatabaseFile>> dataFiles { get; set; }
        public GarbageCollectionStrategy gCStrategy { get; set; }
        public string databaseName { get; set; }
    }
}
