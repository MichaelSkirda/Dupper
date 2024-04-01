using System;
using System.Data;

namespace Dupper
{
	public interface IDbProvider<out T> : IDisposable
		where T : class, IDbConnection
	{
		IDbTransaction? Transaction { get; }
		T? Connection { get; }

		T GetConnectionOrConnect();
		T GetConnectionOrConnect(string connectionString);
		IDbTransaction BeginTransaction();
		IDbTransaction BeginTransaction(IsolationLevel il);
	}

	public interface IDbProvider : IDbProvider<IDbConnection> { }
}