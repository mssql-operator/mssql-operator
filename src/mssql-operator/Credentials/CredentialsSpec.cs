using System;
using System.Collections.Generic;
using System.Text;
using k8s.Models;

namespace MSSqlOperator.Credentials
{
    public class CredentialsSpec
    {
        public V1LabelSelector DatabaseSelector { get; set; }

        public SecretSource Secret { get; set; }

        public string CredentialName { get; set; }

        public string Identity { get; set; }
    }
}
