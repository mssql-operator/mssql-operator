using k8s.Models;
using MSSqlOperator.Models;

namespace MSSqlOperator.Services
{
    public interface IKubernetesService
    {
        DatabaseServerResource GetDatabaseServer(string namespaceProperty, V1LabelSelector selector);
        V1Secret GetSecret(string namespaceProperty, string name);
        V1Service GetService(string namespaceProperty, V1LabelSelector selector);
    }
}
