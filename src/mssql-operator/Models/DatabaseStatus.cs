using System;

namespace MSSqlOperator
{
    public class DatabaseStatus 
    {
        public DateTimeOffset StartTime { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}