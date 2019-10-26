using System;
using k8s.Models;

namespace MSSqlOperator.Models
{
    public class DatabaseServer 
    {
        public V1LabelSelector DatabaseSelector { get; set; }
        public V1LabelSelector ServiceSelector { get; set; }
        public string AdminUserName { get; set; }
        public SecretSource AdminPasswordSecret { get; set; }
        public string ServiceUrl { get; set; }
    }
}