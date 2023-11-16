using Dapper;
using System.Data;

namespace Dupper
{
	internal class DupperConnectionProvider
	{
		private string? ConnectionString { get; set; }
		private Func<string, IDbConnection> DbConnectionFactory { get; set; }

		public DupperConnectionProvider(string connectionString, Func<string, IDbConnection> dbConnectionFactory)
		{
			ConnectionString = connectionString;
			DbConnectionFactory = dbConnectionFactory;
		}

		public DupperConnectionProvider(Func<string, IDbConnection> dbConnectionFactory)
		{
			ConnectionString = null;
			DbConnectionFactory = dbConnectionFactory;
		}

		public IDbConnection Connect()
		{
			if (ConnectionString == null)
				throw new InvalidOperationException("Connection string not provided in constructor!");
			return DbConnectionFactory(ConnectionString);
		}

		public IDbConnection Connect(string connectionString)
			=> DbConnectionFactory(connectionString);


		public async Task<int> ExecuteAsync(string sql, object? param = null)
		{
			using IDbConnection connection = Connect();
			return await connection.ExecuteAsync(sql, param: param);
		}

		public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null)
		{
			using IDbConnection connection = Connect();
			return await connection.ExecuteScalarAsync<T>(sql, param: param);
		}

		public async Task<T> QueryFirstAsync<T>(string sql, object? param = null)
		{
			using IDbConnection connection = Connect();
			return await connection.QueryFirstAsync<T>(sql, param: param);
		}

		public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
		{
			using IDbConnection connection = Connect();
			IEnumerable<T> result = await connection.QueryAsync<T>(sql, param: param);
			return result;
		}

		public async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>
			(string sql, Func<TFirst, TSecond, TReturn> map)
		{
			using IDbConnection connection = Connect();
			var result = new List<TReturn>();
			return await connection.QueryAsync(sql, map);
		}

		public async Task<IEnumerable<TOne>> OneToManyAsync<TKey, TOne, TMany>
			(string sql, Func<TOne, TKey> getKey, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null)
			where TKey : notnull
		{
			using IDbConnection connection = Connect();


			var rows = new Dictionary<TKey, TOne>();

			await connection.QueryAsync<TOne, TMany, TOne>(sql,
			(oneRow, manyRow) =>
			{
				TKey key = getKey(oneRow);
				TOne? one = default;

				rows.TryGetValue(key, out one);
				if (one == null)
				{
					one = oneRow;
					rows[key] = one;
				}
				addMany(one, manyRow);

				return oneRow;
			}, splitOn: splitOn, param: param);

			return rows.Select(x => x.Value);
		}

		public async Task<TOne?> OneToManyFirstAsync<TOne, TMany>
			(string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null)
		{
			using IDbConnection connection = Connect();

			TOne? one = default;

			await connection.QueryAsync<TOne, TMany, TOne>(sql,
			(oneRow, manyRow) =>
			{
				if (one == null)
					one = oneRow;

				addMany(one, manyRow);

				return oneRow;
			},
			splitOn: splitOn,
			param: param);

			return one;
		}
	}
}
