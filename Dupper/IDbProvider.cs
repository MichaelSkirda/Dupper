using System.Data;

namespace Dupper
{
	public interface IDbProvider
	{
		IDbConnection Connect();
		IDbConnection Connect(string connectionString);
	}
}