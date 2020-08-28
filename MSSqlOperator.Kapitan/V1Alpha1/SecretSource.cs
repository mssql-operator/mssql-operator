using System;
using System.Collections.Generic;
using System.Text;
using Kapitan.Kubernetes.Core.V1;

namespace MSSqlOperator.Kapitan.V1Alpha1
{
    public class SecretSource
    {
        public string value { get; set; }
        public SecretKeySelector secretKeyRef { get; set; }
    }
}
