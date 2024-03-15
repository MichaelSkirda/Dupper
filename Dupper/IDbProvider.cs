using System;
using System.Data;

namespace Dupper
{
	public interface IDbProvider<out T>
		where T : class, IDbConnection
	{
		T Connect();
		T Connect(string connectionString);
		IDbTransaction BeginTransaction();
		IDbTransaction BeginTransaction(IsolationLevel il);
	}

	public interface IDbProvider : IDbProvider<IDbConnection> { }
}