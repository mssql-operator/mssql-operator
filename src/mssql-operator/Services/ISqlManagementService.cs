using MSSqlOperator.Databases;
using MSSqlOperator.DatabaseServers;

namespace MSSqlOperator.Services
{
    public interface ISqlManagementService
    {
        void CreateDatabase(DatabaseServer serverResource, DatabaseResource item);
        void DeleteDatabase(DatabaseServer server, string databaseName);
        bool DoesDatabaseExist(DatabaseServer server, string databaseName);
        void ExecuteScript(DatabaseServer serverResource, DatabaseResource databaseResource, string script);
        void RestoreDatabase(DatabaseServer serverResource, DatabaseResource resource);
    }
}
