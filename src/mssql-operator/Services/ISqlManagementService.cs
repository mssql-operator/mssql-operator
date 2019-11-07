using MSSqlOperator.Models;

namespace MSSqlOperator.Services
{
    public interface ISqlManagementService
    {
        void CreateDatabase(DatabaseServer serverResource, DatabaseResource item);
        void DeleteDatabase(DatabaseServer server, string databaseName);
        bool DoesDatabaseExist(DatabaseServer server, string databaseName);
        void RestoreDatabase(DatabaseServer serverResource, DatabaseResource resource);
    }
}
