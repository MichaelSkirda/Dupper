using System.Data;

namespace DupperDemo
{
	public interface IDbConnectionProvider
	{
		IDbConnection Connect();
	}
}
