using System;
using System.Data;

namespace Dupper
{
	public class DbProvider<T> : IDbProvider<T>, IDisposable
		where T : class, IDbConnection
	{
		private string? ConnectionString { get; set; }
		private Func<T>? DbConnectionProvider { get; set; }
		private Func<string, T>? DbConnectionFactory { get; set; }
		private T? _connection;
		public T Connection => Connect();
		

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
			if(_connection != null)
				return _connection;

			if (DbConnectionProvider != null)
				_connection = DbConnectionProvider();
			else if (DbConnectionFactory != null && ConnectionString != null)
				_connection = DbConnectionFactory(ConnectionString);

			if (_connection != null)
				return _connection;

			throw new InvalidOperationException(ExceptionMessages.NitherProviderNorFactory);
		}

		public T Connect(string connectionString)
		{
			if (DbConnectionFactory == null)
				throw new InvalidOperationException(ExceptionMessages.NoFactory);
			_connection = DbConnectionFactory(connectionString);
			return _connection;
		}

		public void Dispose()
		{
			_connection?.Dispose();
		}
	}

	public class DbProvider : DbProvider<IDbConnection>, IDbProvider
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
