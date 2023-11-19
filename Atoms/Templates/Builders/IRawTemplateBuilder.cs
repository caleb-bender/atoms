using Atoms.Templates.Query;

namespace Atoms.Templates.Builders
{
	/// <summary>
	/// Defines an interface for objects that are used to build
	/// IAtomicQueryTemplates by defining a raw query in the language of
	/// the database engine
	/// </summary>
	public interface IRawTemplateBuilder
	{
		/// <summary>
		/// Consumption method that returns an IAtomicQueryTemplate configured
		/// with the connection, query text, and optionally the exception handler.
		/// </summary>
		/// <typeparam name="T">The type that each result in the query is mapped to.</typeparam>
		IAtomicQueryTemplate<T> GetQueryTemplate<T>();
		SqlServerRawTemplateBuilder SetConnectionString(string connectionString);
		SqlServerRawTemplateBuilder SetExceptionHandler(Func<Exception, Task> exceptionHandler);
		SqlServerRawTemplateBuilder SetQueryText(string queryText);
	}
}