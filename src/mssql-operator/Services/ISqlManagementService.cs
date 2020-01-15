using MSSqlOperator.Credentials;
using MSSqlOperator.Databases;
using MSSqlOperator.DatabaseServers;

namespace MSSqlOperator.Services
{
    public interface ISqlManagementService
    {
        void CreateCredential(DatabaseServer server, CredentialsSpec credentials);
        void CreateDatabase(DatabaseServer serverResource, DatabaseResource item);
        void DeleteCredential(DatabaseServer server, CredentialsSpec credentials);
        void DeleteDatabase(DatabaseServer server, string databaseName);
        bool DoesCredentialExist(DatabaseServer server, string credentialName);
        bool DoesDatabaseExist(DatabaseServer server, string databaseName);
        void ExecuteScript(DatabaseServer serverResource, DatabaseResource databaseResource, string script);
        void RestoreDatabase(DatabaseServer serverResource, DatabaseResource resource);
        void UpdateCredential(DatabaseServer server, CredentialsSpec credential);
    }
}
