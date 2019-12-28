using System;

namespace MSSqlOperator.Databases
{
    public class DatabaseStatus 
    {
        public DateTimeOffset LastUpdate { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}
