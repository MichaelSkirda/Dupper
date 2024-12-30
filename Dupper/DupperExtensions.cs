using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dupper
{
	public static class DupperExtensions
	{
		private static bool _switchToNewConnectino = true;

		public static async Task<int> ExecuteAsync(this IDbProvider<IDbConnection> db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await connection.ExecuteAsync(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<T?> ExecuteScalarAsync<T>(this IDbProvider<IDbConnection> db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await connection.ExecuteScalarAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<T> QuerySingleAsync<T>(this IDbProvider<IDbConnection> db, string sql, object? param = null,
			IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await connection.QuerySingleAsync<T>(
				sql: sql,
				param: param,
				transaction: transaction,
				commandTimeout: commandTimeout,
				commandType: commandType);
		}

		public static async Task<T> QueryFirstAsync<T>(this IDbProvider<IDbConnection> db, string sql, object? param = null,
			IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await connection.QueryFirstAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<T>> QueryAsync<T>(this IDbProvider<IDbConnection> db, string sql,
			object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
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
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			var result = new List<TReturn>();
			return await connection.QueryAsync(
				sql,
				map,
				param: param,
				splitOn: splitOn,
				transaction: transaction);
		}

		public static async Task<T?> QueryFirstOrDefaultAsync<T>
			(this IDbProvider<IDbConnection> db, string sql, object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await connection.QueryFirstOrDefaultAsync<T>(
				sql,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TOne>> OneToManyAsync<TOne, TMany>
			(this IDbProvider<IDbConnection> db, string sql, Func<TOne, object> getKey, Action<TOne, TMany> addMany,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await OneToManyAsync<TOne, TMany>(
				connection,
				sql,
				getKey,
				addMany,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TOne>> OneToManyAsync<TOne, TMany>
			(this IDbConnection connection, string sql, Func<TOne, object> getKey, Action<TOne, TMany> addMany,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			var rows = new Dictionary<object, TOne>();

			await connection.QueryAsync<TOne, TMany, TOne>(sql,
			(oneRow, manyRow) =>
			{
				object key = getKey(oneRow);
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


		public static async Task<IEnumerable<TReturn>> OneToManySelectAsync<TOne, TMany, TReturn>
			(this IDbProvider<IDbConnection> db, string sql, Func<TOne, object> getKey,
			Func<TOne, IEnumerable<TMany>, TReturn> select,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await OneToManySelectAsync<TOne, TMany, TReturn>(
				connection,
				sql,
				getKey,
				select,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TReturn>> OneToManySelectAsync<TOne, TMany, TReturn>
			(this IDbConnection connection, string sql, Func<TOne, object> getKey,
			Func<TOne, IEnumerable<TMany>, TReturn> select,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			var rows = new Dictionary<object, (TOne, ICollection<TMany>)>();

			await connection.QueryAsync<TOne, TMany, TOne>(sql,
			(oneRow, manyRow) =>
			{
				object key = getKey(oneRow);

				(TOne, ICollection<TMany>) tuple = default;
				rows.TryGetValue(key, out tuple);

				TOne? one = tuple.Item1;
				ICollection<TMany> many = tuple.Item2;

				if (one == null)
				{
					one = oneRow;
					many = new List<TMany>();
					rows[key] = (one, many);
				}
				many.Add(manyRow);

				return oneRow;
			},
			splitOn: splitOn,
			param: param,
			transaction: transaction);

			return rows.Select(x => select(x.Value.Item1, x.Value.Item2));
		}

		public static async Task<IEnumerable<TReturn>> OneToManySelectAsync<TOne, KOne, TMany, TReturn>
			(this IDbProvider<IDbConnection> db, string sql, Func<TOne, KOne, object> getKey,
			Func<TOne, KOne, IEnumerable<TMany>, TReturn> select,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await OneToManySelectAsync<TOne, KOne, TMany, TReturn>(
				connection,
				sql,
				getKey,
				select,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<IEnumerable<TReturn>> OneToManySelectAsync<TOne, KOne, TMany, TReturn>
			(this IDbConnection connection, string sql, Func<TOne, KOne, object> getKey,
			Func<TOne, KOne, IEnumerable<TMany>, TReturn> select,
			string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			var rows = new Dictionary<object, (TOne, KOne, ICollection<TMany>)>();

			await connection.QueryAsync<TOne, KOne, TMany, TOne>(sql,
			(oneRow, oneRow2, manyRow) =>
			{
				object key = getKey(oneRow, oneRow2);

				(TOne, KOne, ICollection<TMany>) tuple = default;
				rows.TryGetValue(key, out tuple);

				TOne? one = tuple.Item1;
				KOne? one2 = tuple.Item2;
				ICollection<TMany> many = tuple.Item3;

				if (one == null)
				{
					one = oneRow;
					one2 = oneRow2;
					many = new List<TMany>();
					rows[key] = (one, one2, many);
				}
				many.Add(manyRow);

				return oneRow;
			},
			splitOn: splitOn,
			param: param,
			transaction: transaction);

			return rows.Select(x => select(x.Value.Item1, x.Value.Item2, x.Value.Item3));
		}

		public static async Task<TOne?> OneToManyFirstOrDefaultAsync<TOne, TMany>
			(this IDbProvider<IDbConnection> db, string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await OneToManyFirstOrDefaultAsync(
				connection,
				sql,
				addMany,
				splitOn: splitOn,
				param: param,
				transaction: transaction);
		}

		public static async Task<TOne?> OneToManyFirstOrDefaultAsync<TOne, TMany>
			(this IDbConnection connection, string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null,
			IDbTransaction? transaction = null)
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

		public static async Task<IEnumerable<TParent>> QueryOneToOneAsync<TParent, TChild>(this IDbProvider<IDbConnection> db,
			string sql, Action<TParent, TChild> addChild, string splitOn = "Id", object? param = null,
			IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await QueryOneToOneAsync<TParent, TChild>(
				connection,
				sql,
				addChild,
				splitOn,
				param,
				transaction);
		}

		public static async Task<IEnumerable<TParent>> QueryOneToOneAsync<TParent, TChild>(this IDbConnection connection, string sql,
			Action<TParent, TChild> addChild, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			return await connection.QueryAsync<TParent, TChild, TParent>(sql,
			(parent, child) =>
			{
				addChild(parent, child);
				return parent;
			},
			splitOn: splitOn,
			param: param,
			transaction: transaction);
		}

		public static async Task<IEnumerable<Level1>> QueryOneToManyNested<Level1, Level2, Level3>(this IDbProvider<IDbConnection> db,
			string sql, Action<Level1, Level2> addLevel2, Action<Level2, Level3> addLevel3, Func<Level1, object> getKeyLevel1,
			Func<Level2, object> getKeyLevel2, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			IDbConnection connection = db.GetConnectionOrConnect(_switchToNewConnectino);
			return await QueryOneToManyNested<Level1, Level2, Level3>(
				connection,
				sql,
				addLevel2,
				addLevel3,
				getKeyLevel1,
				getKeyLevel2,
				splitOn,
				param,
				transaction
				);
		}

		public static async Task<IEnumerable<Level1>> QueryOneToManyNested<Level1, Level2, Level3>(this IDbConnection connection,
			string sql, Action<Level1, Level2> addLevel2, Action<Level2, Level3> addLevel3, Func<Level1, object> getKeyLevel1,
			Func<Level2, object> getKeyLevel2, string splitOn = "Id", object? param = null, IDbTransaction? transaction = null)
		{
			var rows = new Dictionary<object, Level1>();
			var rowsLevel2 = new Dictionary<(object, object), Level2>();

			await connection.QueryAsync<Level1, Level2, Level3, Level1>(sql,
			(rowLevel1, rowLevel2, rowLevel3) =>
			{
				object keyLevel1 = getKeyLevel1(rowLevel1);
				Level1? level1 = default;

				rows.TryGetValue(keyLevel1, out level1);

				if (level1 == null)
				{
					level1 = rowLevel1;
					rows[keyLevel1] = level1;
				}

				object keyLevel2 = getKeyLevel2(rowLevel2);
				Level2? level2 = default;

				rowsLevel2.TryGetValue((keyLevel1, keyLevel2), out level2);

				if (level2 == null)
				{
					level2 = rowLevel2;
					rowsLevel2[(keyLevel1, keyLevel2)] = level2;
				}

				addLevel3(level2, rowLevel3);

				return rowLevel1;
			},
			splitOn: splitOn,
			param: param,
			transaction: transaction);

			foreach (KeyValuePair<(object, object), Level2> kv in rowsLevel2)
			{
				object keyLevel1 = kv.Key.Item1;
				Level2 level2 = kv.Value;

				bool hasParent = rows.TryGetValue(keyLevel1, out Level1? level1);

				if (hasParent && level1 != null)
					addLevel2(level1, level2);
			}

			return rows.Select(x => x.Value);
		}

	}
}
