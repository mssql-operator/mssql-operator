using System;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.Databases
{
    public class DatabaseStatus : CustomResourceStatus
    {
        public DateTimeOffset LastUpdate { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}
