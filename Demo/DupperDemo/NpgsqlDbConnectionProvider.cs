using Npgsql;
using System.Data;

namespace DupperDemo
{
	public class NpgsqlDbConnectionProvider : IDbConnectionProvider
	{
		private string ConnectionString { get; init; }

		public NpgsqlDbConnectionProvider(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public IDbConnection Connect() => new NpgsqlConnection(ConnectionString);
	}
}
