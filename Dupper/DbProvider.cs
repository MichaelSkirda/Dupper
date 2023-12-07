using System;
using System.Data;

namespace Dupper
{
	public class DbProvider<T> : IDbProvider<T>
		where T : class, IDbConnection
	{
		private string? ConnectionString { get; set; }
		private Func<T>? DbConnectionProvider { get; set; }
		private Func<string, T>? DbConnectionFactory { get; set; }

		public DbProvider(Func<T> dbConnectionProvider)
		{
			DbConnectionProvider = dbConnectionProvider;
		}

		public DbProvider(Func<string, T> dbConnectionFactory)
		{
			DbConnectionFactory = dbConnectionFactory;
		}

		public DbProvider(string connectionString, Func<string, T> dbConnectionFactory)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
		}

		public DbProvider(string connectionString, Func<string, T> dbConnectionFactory,
			Func<T> dbConnectionProvider)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
			DbConnectionProvider = dbConnectionProvider;
		}

		public T Connect()
		{
			if (DbConnectionProvider == null)
			{
				if (DbConnectionFactory == null || ConnectionString == null)
					throw new InvalidOperationException(ExceptionMessages.NitherProviderNorFactoryMessage);

				return DbConnectionFactory(ConnectionString);
			}
			return DbConnectionProvider();
		}

		public T Connect(string connectionString)
		{
			if (DbConnectionFactory == null)
				throw new InvalidOperationException(ExceptionMessages.NoFactoryMessage);
			return DbConnectionFactory(connectionString);
		}
	}

	public class DbProvider : DbProvider<IDbConnection>
	{
		public DbProvider(Func<IDbConnection> dbConnectionProvider)
			: base(dbConnectionProvider) { }

		public DbProvider(Func<string, IDbConnection> dbConnectionFactory)
			: base(dbConnectionFactory) { }

		public DbProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory)
			: base(connectionString, dbConnectionFactory) { }

		public DbProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory,
			Func<IDbConnection> dbConnectionProvider)
			: base(connectionString, dbConnectionFactory, dbConnectionProvider) { }
	}
}
