using System;
using System.Data;

namespace Dupper
{
	public class DbProvider : IDbProvider
	{
		private string? ConnectionString { get; set; }
		private Func<IDbConnection>? DbConnectionProvider { get; set; }
		private Func<string, IDbConnection>? DbConnectionFactory { get; set; }

		public DbProvider(Func<IDbConnection> dbConnectionProvider)
		{
			DbConnectionProvider = dbConnectionProvider;
		}

		public DbProvider(Func<string, IDbConnection> dbConnectionFactory)
		{
			DbConnectionFactory = dbConnectionFactory;
		}

		public DbProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
		}

		public DbProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory,
			Func<IDbConnection> dbConnectionProvider)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
			DbConnectionProvider = dbConnectionProvider;
		}


		public IDbConnection Connect()
		{
			if (DbConnectionProvider == null)
			{
				if(DbConnectionFactory == null || ConnectionString == null)
					throw new InvalidOperationException(NitherProviderNorFactoryMessage);

				return DbConnectionFactory(ConnectionString);
			}
			return DbConnectionProvider();
		}

		public T Connect<T>()
			where T : class, IDbConnection
		{
			IDbConnection abstractConnection = Connect();
			T connection = Convert<T>(abstractConnection);
			return connection;
		}


		public IDbConnection Connect(string connectionString)
		{
			if (DbConnectionFactory == null)
				throw new InvalidOperationException(NoFactoryMessage);
			return DbConnectionFactory(connectionString);
		}

		public T Connect<T>(string connectionString)
			where T : class, IDbConnection
		{
			IDbConnection abstractConnection = Connect(connectionString);
			T connection = Convert<T>(abstractConnection);
			return connection;
		}


		private T Convert<T>(IDbConnection abstractConnection)
			where T : class, IDbConnection
		{
			T? connection = abstractConnection as T;

			if (connection == null)
				throw new InvalidOperationException($"Can not convert {abstractConnection.GetType()} to {typeof(T)}");
			return connection;
		}


		private static string NitherProviderNorFactoryMessage =
			"Nither DbConnectionProvider nor (DbConnectionFactory with ConnectionString) not provided." +
			"Use constructor (Func<IDbConnection>)" + 
			"or (string, Func<string, IDbConnection>) " + 
			"or (string, Func<string, IDbConnection>, Func<IDbConnection>)";

		private static string NoFactoryMessage =
			"DbConnectionFactory not provided. " +
			"Use constructor (string, Func<string, IDbConnection>) " +
			"or (string, Func<string, IDbConnection>, Func<IDbConnection>)" +
			"or (Func<string, IDbConnection>)";
	}
}
