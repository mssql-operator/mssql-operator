using System;
using System.Collections.Generic;
using System.Text;
using Kanyon.Kubernetes.Core.V1;

namespace MSSqlOperator.Kanyon.V1Alpha1
{
    public class SecretSource
    {
        public string value { get; set; }
        public SecretKeySelector secretKeyRef { get; set; }
    }
}
