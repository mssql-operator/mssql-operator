using k8s.Models;
using MSSqlOperator.Models;
using OperatorSharp.CustomResources;

namespace MSSqlOperator.Services
{
    public interface IKubernetesService
    {
        CustomResourceList<DatabaseServerResource> GetDatabaseServer(string namespaceProperty, V1LabelSelector selector);
        V1Secret GetSecret(string namespaceProperty, string name);
        V1ServiceList GetService(string namespaceProperty, V1LabelSelector selector);
    }
}
