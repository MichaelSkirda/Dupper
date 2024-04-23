using System;
using System.Data;

namespace Dupper
{
	public interface IDbProvider<out T> : IDisposable
		where T : class, IDbConnection
	{
		IDbTransaction? Transaction { get; }
		T? Connection { get; }

		T GetConnectionOrConnect(bool switchToNewConnection = false);
		T Connect(string connectionString, bool switchToNewConnection = false);
		T Connect(bool switchToNewConnection = false);
		IDbTransaction BeginTransaction();
		IDbTransaction BeginTransaction(IsolationLevel il);
		void CommitTransaction();
		void RollbackTransaction();
	}

	public interface IDbProvider : IDbProvider<IDbConnection> { }
}