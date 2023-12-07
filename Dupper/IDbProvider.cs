using System.Data;

namespace Dupper
{
	public interface IDbProvider<T>
		where T : class, IDbConnection
	{
		T Connect();
		T Connect(string connectionString);
	}

	public interface IDbProvider: IDbProvider<IDbConnection> { }
}