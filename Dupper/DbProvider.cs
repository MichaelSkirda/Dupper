using System.Data;

namespace Dupper
{
	public class DbProvider : IDbProvider
	{
		private string? ConnectionString { get; set; }
		private Func<string, IDbConnection> DbConnectionFactory { get; set; }

		public DbProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
		}

		public DbProvider(Func<string, IDbConnection> dbConnectionFactory)
		{
			ConnectionString = null;
			DbConnectionFactory = dbConnectionFactory;
		}

		public IDbConnection Connect()
		{
			if (ConnectionString == null)
				throw new InvalidOperationException("Connection string not provided in constructor!");
			return DbConnectionFactory(ConnectionString);
		}

		public IDbConnection Connect(string connectionString)
			=> DbConnectionFactory(connectionString);
	}
}
