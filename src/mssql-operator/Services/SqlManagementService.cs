using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSqlOperator.Credentials;
using MSSqlOperator.Databases;
using MSSqlOperator.DatabaseServers;

namespace MSSqlOperator.Services
{
    public class SqlManagementService : ISqlManagementService
    {
        private readonly ILogger<SqlManagementService> logger;

        public SqlManagementService(ILogger<SqlManagementService> logger)
        {
            this.logger = logger;
        }

        public void DeleteDatabase(DatabaseServer server, string databaseName)
        {
            var serverConn = CreateServerConnection(server);

            logger.LogInformation("Deleting database {Database} from server {server}", databaseName, serverConn.Name);
            serverConn.Databases[databaseName].Drop();
            logger.LogInformation("Database {database} deleted", databaseName);
        }

        public bool DoesDatabaseExist(DatabaseServer server, string databaseName)
        {
            var serverConn = CreateServerConnection(server);

            return serverConn.Databases[databaseName] != null;
        }

        public bool DoesCredentialExist(DatabaseServer server, string credentialName)
        {
            var serverConn = CreateServerConnection(server);

            return serverConn.Credentials[credentialName] != null;
        }

        public void UpdateCredential(DatabaseServer server, CredentialsSpec credential)
        {
            var serverConn = CreateServerConnection(server);

            var originalCredential = serverConn.Credentials[credential.CredentialName];
            if (originalCredential == null) throw new ArgumentNullException("CredentialName");

            originalCredential.Alter(credential.Identity, credential.Secret.Value);
        }

        public void CreateCredential(DatabaseServer server, CredentialsSpec credentials)
        {
            var serverConn = CreateServerConnection(server);

            var newCredential = new Credential(serverConn, credentials.CredentialName);

            newCredential.Create(credentials.Identity, credentials.Secret.Value);
        }

        public void DeleteCredential(DatabaseServer server, CredentialsSpec credentials)
        {
            var serverConn = CreateServerConnection(server);
            var oldCredential = serverConn.Credentials[credentials.CredentialName];

            if (oldCredential == null) return;
            oldCredential.DropIfExists();
        }

        public void RestoreDatabase(DatabaseServer serverResource, DatabaseResource resource)
        {
            var serverConn = CreateServerConnection(serverResource);

            var restore = new Restore();
            var backupFile = resource.Spec.BackupFiles.FirstOrDefault();
            restore.Devices.AddDevice(backupFile.Path, DeviceType.File);
            restore.Database = resource.Spec.DatabaseName;
            restore.Action = RestoreActionType.Database;
            restore.ReplaceDatabase = true;

            var files = resource.Spec.DataFiles.SelectMany(kvp => kvp.Value).Union(resource.Spec.LogFiles);
            foreach (var file in files)
            {
                logger.LogDebug("Relocating file {fileName} to {filePath}", file.Name, file.Path);
                restore.RelocateFiles.Add(new RelocateFile(file.Name, file.Path));
            }

            logger.LogInformation("Restoring database {database} to server {server}", resource.Metadata.Name, serverConn.Name);
            restore.SqlRestore(serverConn);
        }

        public void CreateDatabase(DatabaseServer serverResource, DatabaseResource item)
        {
            logger.LogDebug("Processing create for {database}", item.Metadata.Name);
            var serverConn = CreateServerConnection(serverResource);

            var database = new Database(serverConn, item.Spec.DatabaseName) { Collation = ChooseCollation(item) };
            foreach (var groupEntry in item.Spec.DataFiles)
            {
                var fileGroupName = groupEntry.Key;
                if (fileGroupName.ToLower() == "primary")
                {
                    fileGroupName = "PRIMARY";
                }

                var fileGroup = new FileGroup(database, fileGroupName);
                database.FileGroups.Add(fileGroup);
                var fileCount = groupEntry.Value.Count;

                foreach (var file in groupEntry.Value)
                {
                    logger.LogDebug("Adding data file {FileGroup}/{FileName} ({FilePath})", groupEntry.Key, file.Name, file.Path);

                    var datafile = new DataFile(fileGroup, file.Name, file.Path);

                    if (fileGroupName == "PRIMARY")
                    {
                        // We mark the SMO file as primary if either the API object is set as primary or if there is only one file in the group
                        // If there isn't a primary file set in a primary filegroup, you will get an error
                        datafile.IsPrimaryFile = file.IsPrimaryFile || fileCount == 1;
                    }

                    fileGroup.Files.Add(datafile);
                }
            }

            foreach (var logEntry in item.Spec.LogFiles)
            {
                logger.LogDebug("Adding log file {FileName} ({FilePath})", logEntry.Name, logEntry.Path);
                database.LogFiles.Add(new LogFile(database, logEntry.Name, logEntry.Path));
            }

            logger.LogInformation("Creating database {database} on server {server}", item.Metadata.Name, serverConn.Name);
            database.Create();
        }

        private static string ChooseCollation(DatabaseResource item)
        {
            if (string.IsNullOrEmpty(item.Spec.Collation) || item.Spec.Collation.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                return "SQL_Latin1_General_CP1_CI_AS";
            }
            return item.Spec.Collation;
        }

        private Server CreateServerConnection(DatabaseServer server)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server.ServiceUrl,
                UserID = server.AdminUserName,
                Password = server.AdminPasswordSecret.Value,
                InitialCatalog = "master"
            };

            var serverConn = new Server(new ServerConnection(new SqlConnection(builder.ToString())));

            return serverConn;
        }

        public void ExecuteScript(DatabaseServer serverResource, DatabaseResource databaseResource, string script)
        {
            var connection = CreateServerConnection(serverResource);
            var database = connection.Databases[databaseResource.Metadata.Name];
            database.ExecuteNonQuery(script);
        }
    }
}
