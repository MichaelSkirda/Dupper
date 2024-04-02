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
		private T? _connection { get; set; }
		public T? Connection => _connection;

		private Mutex Mutex { get; set; } = new Mutex();
		private int MutexMillisecondsTimeout { get; set; } = 5000;
		private bool _preventDisposing = false;

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

		public T GetConnectionOrConnect(bool switchToNewConnection = false)
		{
			if (_connection != null)
				return _connection;
			return Connect(switchToNewConnection);
		}

		public T Connect(bool switchToNewConnection = false)
		{
			WaitMutex(MutexMillisecondsTimeout);

			try
			{
				if (DbConnectionProvider == null && (DbConnectionFactory == null || ConnectionString == null))
					throw new InvalidOperationException(ExceptionMessages.NitherProviderNorFactory);

				T? connection = ConnectOrDefault();

				if (connection == null)
					throw new InvalidOperationException(ExceptionMessages.FailedToCreateConnection);

				if (switchToNewConnection)
					SwitchToNewConnection(connection);

				return connection;
			}
			finally
			{
				Mutex.ReleaseMutex();
			}
		}

		public T Connect(string connectionString, bool switchToNewConnection = false)
		{
			WaitMutex(MutexMillisecondsTimeout);

			try
			{
				if (DbConnectionFactory == null)
					throw new InvalidOperationException(ExceptionMessages.NoFactory);

				T tempConn = DbConnectionFactory(connectionString);

				if (tempConn == null)
					throw new InvalidOperationException(ExceptionMessages.FailedToCreateConnection);

				if (switchToNewConnection)
					SwitchToNewConnection(tempConn);

				return tempConn;
			}
			finally
			{
				Mutex.ReleaseMutex();
			}
		}

		private T? ConnectOrDefault()
		{
			try
			{
				if (DbConnectionProvider != null)
					return DbConnectionProvider();
				else if (DbConnectionFactory != null && ConnectionString != null)
					return DbConnectionFactory(ConnectionString);
				else
					return default;
			}
			catch
			{
				return default;
			}
		}

		private void SwitchToNewConnection(T connection)
		{
			_connection?.Dispose();
			_connection = connection;
		}

		private void WaitMutex(int millisecondsTimeout)
		{
			bool getMutex = Mutex.WaitOne(millisecondsTimeout);
			if (getMutex == false)
				throw new InvalidOperationException(ExceptionMessages.FailedToGetMutex);
		}

		#region Dispose

		public void PreventDisposing()
			=> _preventDisposing = true;

		public void Dispose()
		{
			DisposeResources();
			GC.SuppressFinalize(this);
		}

		private void DisposeResources()
		{
			if (_preventDisposing)
				return;
			Transaction?.Dispose();
			_connection?.Dispose();
		}

		~DbProvider()
		{
			DisposeResources();
		}

		#endregion

		#region Transactions

		public IDbTransaction BeginTransaction()
		{
			if (Transaction != null)
				throw new InvalidOperationException(ExceptionMessages.TransactionAlreadyStarted);
			IDbConnection connection = GetConnectionOrConnect();
			IDbTransaction transaction = connection.BeginTransaction();
			Transaction = transaction;
			return transaction;
		}

		public IDbTransaction BeginTransaction(IsolationLevel il)
		{
			if (Transaction != null)
				throw new InvalidOperationException(ExceptionMessages.TransactionAlreadyStarted);
			IDbConnection connection = GetConnectionOrConnect();
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
			try { Transaction?.Dispose(); }
			catch { }
			Transaction = null;
		}

		#endregion

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
