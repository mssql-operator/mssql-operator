using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;

namespace MSSqlOperator
{
	public static class ConnectionStringUtilities
	{
		public static Server GetServer(string connectionString, string database = null)
		{
			SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
			if (!string.IsNullOrWhiteSpace(database))
			{
				sqlConnectionStringBuilder.InitialCatalog = database;
			}
			return new Server(new ServerConnection(new SqlConnection(sqlConnectionStringBuilder.ToString())));
		}

		public static string GetDatabaseName(string connectionString)
		{
			return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
		}
	}
}
