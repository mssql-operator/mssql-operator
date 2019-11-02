using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using k8s;
using MSSqlOperator.Models;
using Newtonsoft.Json.Linq;
using OperatorSharp;
using System.Data.SqlClient;
using k8s.Models;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace MSSqlOperator.Operators
{
    public class DatabaseOperator : Operator<DatabaseResource>
    {
        public DatabaseOperator(Kubernetes client, ILogger<Operator<DatabaseResource>> logger, int timeoutSeconds) : base(client, logger, timeoutSeconds)
        {
        }

        public override void HandleException(Exception ex)
        {
            Logger.LogError(ex, "An unknown error occured while processing the item");
        }

        public override void HandleItem(WatchEventType eventType, DatabaseResource item)
        {
            Logger.LogDebug("Recieved new Database object (v {ResourceVersion})", item.Metadata.ResourceVersion);
            DatabaseServer server = GetServerResources(item.Metadata.NamespaceProperty, item.Spec.DatabaseSelector);
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server.ServiceUrl,
                UserID = server.AdminUserName,
                Password = server.AdminPasswordSecret.Value,
                InitialCatalog = "master"
            };

            var serverConn = new Server(new ServerConnection(new SqlConnection(builder.ToString())));
            
            if (eventType == WatchEventType.Added) 
            {
                if (item.Spec.BackupFiles?.Any() ?? false) 
                {
                    RestoreDatabase(serverConn, item);
                }
                else
                {
                    CreateDatabase(serverConn, item);
                }

                Logger.LogInformation("Created database {database}", item.Metadata.Name);
            }
            else if (eventType == WatchEventType.Deleted) 
            {
                if (item.Spec.GCStrategy == GarbageCollectionStrategy.Delete) {
                    Logger.LogInformation("Deleting database {Database} from server {server}", item.Metadata.Name, serverConn.Name);
                    serverConn.Databases[item.Metadata.Name].Drop();
                    Logger.LogInformation("Database {database} deleted", item.Metadata.Name);
                }
            }
        }

        public void RestoreDatabase(Server server, DatabaseResource resource) 
        {
            var restore = new Restore();
            var backupFile = resource.Spec.BackupFiles.FirstOrDefault();
            restore.Devices.AddDevice(backupFile.Path, DeviceType.File);
            restore.Database = resource.Metadata.Name;
            restore.Action = RestoreActionType.Database;
            restore.ReplaceDatabase = true;

            var files = resource.Spec.DataFiles.SelectMany(kvp => kvp.Value).Union(resource.Spec.LogFiles);
            foreach (var file in files) 
            {
                Logger.LogDebug("Relocating file {fileName} to {filePath}", file.Name, file.Path);
                restore.RelocateFiles.Add(new RelocateFile(file.Name, file.Path));
            }

            Logger.LogInformation("Restoring database {database} to server {server}", resource.Metadata.Name, server.Name);
            restore.SqlRestore(server);
        }

        public void CreateDatabase(Server server, DatabaseResource item) 
        {
            Logger.LogDebug("Processing create for {database}", item.Metadata.Name);
            var database = new Database(server, item.Metadata.Name) { Collation = item.Spec.Collation };
            foreach (var groupEntry in item.Spec.DataFiles) 
            {
                var fileGroup = new FileGroup(database, groupEntry.Key);
                database.FileGroups.Add(fileGroup);
                
                foreach (var file in groupEntry.Value) {
                    var datafile = new DataFile(fileGroup, file.Name, file.Path) { IsPrimaryFile = file.IsPrimaryFile };
                    Logger.LogDebug("Adding data file {FileGroup}/{FileName} ({FilePath})", groupEntry.Key, file.Name, file.Path);
                    fileGroup.Files.Add(datafile);
                }
            }

            foreach (var logEntry in item.Spec.LogFiles) 
            {
                Logger.LogDebug("Adding log file {FileName} ({FilePath})", logEntry.Name, logEntry.Path);
                database.LogFiles.Add(new LogFile(database, logEntry.Name, logEntry.Path));
            }

            Logger.LogInformation("Creating database {database} on server {server}", item.Metadata.Name, server.Name);
            database.Create();
        }

        public DatabaseServer GetServerResources(string namespaceProperty, V1LabelSelector selector) 
        {
            var serverSelector = selector.BuildSelector();
            Logger.LogDebug("Loading referenced server {selector}", serverSelector);
            var server = (Client.ListNamespacedCustomObject(ApiVersion.Group, ApiVersion.Version, namespaceProperty, "DatabaseServers", labelSelector: serverSelector) as JObject).ToObject<DatabaseServerResource>();

            if (string.IsNullOrEmpty(server.Spec.ServiceUrl) && server.Spec.ServiceSelector != null)
            {
                var serviceSelector = server.Spec.ServiceSelector.BuildSelector();
                Logger.LogDebug("Loading referenced service {selector}", serviceSelector);
                var service = Client.ListNamespacedService(namespaceProperty, labelSelector: serviceSelector)?.Items?.FirstOrDefault();
                server.Spec.ServiceUrl = $"{service.Metadata.Name}.{service.Metadata.NamespaceProperty}.svc:{service.Spec.Ports.FirstOrDefault()?.Port}";
            }

            if (string.IsNullOrEmpty(server.Spec.AdminPasswordSecret.Value) && server.Spec.AdminPasswordSecret.SecretKeyRef != null)
            {
                server.Spec.AdminPasswordSecret.Value = GetValueForSecretReference(namespaceProperty, server.Spec.AdminPasswordSecret.SecretKeyRef);
            }

            return server.Spec;
        }

        private string GetValueForSecretReference(string namespaceProperty, V1SecretKeySelector keyRef)
        {
            Logger.LogDebug("Loading referenced secret {secret}:{key}", keyRef.Name, keyRef.Key);
            var secret = Client.ReadNamespacedSecret(keyRef.Name, namespaceProperty);
            if (secret.Data.ContainsKey(keyRef.Key))
            {
                var data = secret.Data[keyRef.Key];
                return Encoding.Default.GetString(data);
            }
            else
            {
                Logger.LogWarning("No secret could be found to satisfy adminPasswordSecret");
            }

            return null;
        }
    }
}