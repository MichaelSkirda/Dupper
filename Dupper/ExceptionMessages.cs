namespace Dupper
{
	internal class ExceptionMessages
	{
		internal static string NitherProviderNorFactoryMessage =
			"Nither DbConnectionProvider nor (DbConnectionFactory with ConnectionString) not provided." +
			"Use constructor (Func<IDbConnection>)" +
			"or (string, Func<string, IDbConnection>) " +
			"or (string, Func<string, IDbConnection>, Func<IDbConnection>)";

		internal static string NoFactoryMessage =
			"DbConnectionFactory not provided. " +
			"Use constructor (string, Func<string, IDbConnection>) " +
			"or (string, Func<string, IDbConnection>, Func<IDbConnection>)" +
			"or (Func<string, IDbConnection>)";
	}
}
