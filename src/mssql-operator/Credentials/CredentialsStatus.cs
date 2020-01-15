using System;
using System.Collections.Generic;
using System.Text;

namespace MSSqlOperator.Credentials
{
    public class CredentialsStatus : OperatorSharp.CustomResources.CustomResourceStatus
    {
        public CredentialsStatus() { }
        public CredentialsStatus(DateTimeOffset lastUpdate, string reason, string message)
        {
            LastUpdate = lastUpdate;
            Reason = reason;
            Message = message;
        }

        public DateTimeOffset LastUpdate { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
    }
}
