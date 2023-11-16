using System.Data;

namespace Dupper
{
	public interface IConnectionProvider
	{
		IDbConnection Connect();
		IDbConnection Connect(string connectionString);

		Task<int> ExecuteAsync(string sql, object? param = null);
		Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null);
		Task<T> QueryFirstAsync<T>(string sql, object? param = null);
		Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);

		Task<IEnumerable<TOne>> OneToManyAsync<TKey, TOne, TMany>
			(string sql, Func<TOne, TKey> key, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null)
			where TKey : notnull;

		Task<TOne?> OneToManyFirstAsync<TOne, TMany>
			(string sql, Action<TOne, TMany> addMany, string splitOn = "Id", object? param = null);
	}
}