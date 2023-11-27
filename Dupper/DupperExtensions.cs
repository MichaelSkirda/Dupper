using Dapper;
using System.Data;

namespace Dupper
{
	public static class DupperExtensions
	{
		public static async Task<int> ExecuteAsync(this IDbProvider db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			return await connection.ExecuteAsync(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<T?> ExecuteScalarAsync<T>(this IDbProvider db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			return await connection.ExecuteScalarAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<T> QueryFirstAsync<T>(this IDbProvider db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			return await connection.QueryFirstAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbProvider db, string sql,
			object? param = null, IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			IEnumerable<T> result = await connection.QueryAsync<T>(
				sql,
				param: param,
				transaction: transaction);
			return result;
		}

		public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>
			(this IDbProvider db, string sql, Func<TFirst, TSecond, TReturn> map, object? param,
			IDbTransaction? transaction = null, string splitOn = "Id")
		{
			using IDbConnection connection = db.Connect();
			var result = new List<TReturn>();
			return await connection.QueryAsync(
				sql,
				map,
				param: param,
				splitOn: splitOn,
				transaction: transaction);
		}

		public static async Task<T?> QueryFirstOrDefaultAsync<T>
			(this IDbProvider db, string sql, object? param = null, IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			return await connection.QueryFirstOrDefaultAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TOne>> OneToManyAsync<TKey, TOne, TMany>
			(this IDbProvider db, string sql, Func<TOne, TKey> getKey, Action<TOne, TMany> addMany,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
			where TKey : notnull
		{
			using IDbConnection connection = db.Connect();
			return await OneToManyAsync<TKey, TOne, TMany>(
				connection,
				sql,
				getKey,
				addMany,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TOne>> OneToManyAsync<TKey, TOne, TMany>
			(this IDbConnection connection, string sql, Func<TOne, TKey> getKey, Action<TOne, TMany> addMany,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
			where TKey : notnull
		{
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
			},
			splitOn: splitOn,
			param: param,
			transaction: transaction);

			return rows.Select(x => x.Value);
		}

		public static async Task<TOne?> OneToManyFirstAsync<TOne, TMany>
			(this IDbProvider db, string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			using IDbConnection connection = db.Connect();
			return await OneToManyFirstAsync(
				connection,
				sql,
				addMany,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<TOne?> OneToManyFirstAsync<TOne, TMany>
			(this IDbConnection connection, string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
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
			param: param,
			transaction: transaction);

			return one;
		}

	}
}
