using System;
using System.Data;
using System.Threading;

namespace Dupper
{
	public class DbProvider<T> : IDbProvider<T>
		where T : class, IDbConnection
	{
		private string? ConnectionString { get; set; }
		private Func<T>? DbConnectionProvider { get; set; }
		private Func<string, T>? DbConnectionFactory { get; set; }

		public IDbTransaction? Transaction { get; private set; }
		private T? _connection;
		public T? Connection => _connection;

		private Mutex Mutex { get; set; } = new Mutex();
		private int MutexMillisecondsTimeout { get; set; } = 5000;



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
			try
			{
				WaitMutex(MutexMillisecondsTimeout);

				if (_connection != null)
					return _connection;

				if (DbConnectionProvider == null && (DbConnectionFactory == null || ConnectionString == null))
					throw new InvalidOperationException(ExceptionMessages.NitherProviderNorFactory);

				bool hasConnected = TryConnect();

				if (hasConnected && _connection != null)
					return _connection;

				throw new InvalidOperationException(ExceptionMessages.FailedToCreateConnection);
			}
			finally
			{
				Mutex.ReleaseMutex();
			}
		}

		public T Connect(string connectionString)
		{
			try
			{
				WaitMutex(MutexMillisecondsTimeout);

				if (DbConnectionFactory == null)
					throw new InvalidOperationException(ExceptionMessages.NoFactory);

				_connection = DbConnectionFactory(connectionString);

				if (_connection == null)
					throw new InvalidOperationException(ExceptionMessages.FailedToCreateConnection);

				return _connection;
			}
			finally
			{
				Mutex.ReleaseMutex();
			}
		}

		private void WaitMutex(int millisecondsTimeout)
		{
			bool getMutex = Mutex.WaitOne(millisecondsTimeout);
			if (getMutex == false)
				throw new InvalidOperationException(ExceptionMessages.FailedToGetMutex);
		}

		private bool TryConnect()
		{
			try
			{
				if (DbConnectionProvider != null)
				{
					_connection = DbConnectionProvider();
					return true;
				}
				else if (DbConnectionFactory != null && ConnectionString != null)
				{
					_connection = DbConnectionFactory(ConnectionString);
					return true;
				}
				return false;
			}
			catch
			{
				return false;
			}
		}

		public void Dispose()
		{
			Transaction?.Dispose();
			_connection?.Dispose();
		}

		public IDbTransaction BeginTransaction()
		{
			if (Transaction != null)
				throw new InvalidOperationException(ExceptionMessages.TransactionAlreadyStarted);
			IDbConnection connection = Connect();
			IDbTransaction transaction = connection.BeginTransaction();
			Transaction = transaction;
			return transaction;
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			if (Transaction != null)
				throw new InvalidOperationException(ExceptionMessages.TransactionAlreadyStarted);
			IDbConnection connection = Connect();
			IDbTransaction transaction = connection.BeginTransaction(il);
			Transaction = transaction;
			return transaction;
		}

		public void CommitTransaction()
		{
			if (Transaction == null)
				throw new InvalidOperationException(ExceptionMessages.NoStartedTransaction);
			Transaction.Commit();
			Transaction = null;
		}

		public void RollbackTransaction()
		{
			if (Transaction == null)
				throw new InvalidOperationException(ExceptionMessages.NoStartedTransaction);
			Transaction.Rollback();
			Transaction = null;
		}

		public void ClearTransaction()
		{
			if (Transaction == null)
				return;
			try { Transaction.Dispose(); }
			catch { }
			Transaction = null;
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
