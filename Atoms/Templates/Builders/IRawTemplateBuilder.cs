using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Templates.Query;
using CalebBender.Atoms.Exceptions;

namespace CalebBender.Atoms.Templates.Builders
{
	/// <summary>
	/// Defines an interface for objects that are used to build
	/// IAtomicQueryTemplates by defining a raw query in the language of
	/// the database engine
	/// </summary>
	public interface IRawTemplateBuilder
	{
		/// <summary>
		/// Consumption method that returns an IAtomicMutationTemplate configured
		/// with the connection, mutation text, and optionally an exeception handler or
		/// a cancellation token.
		/// </summary>
		/// <exception cref="ConnectionStringMissingException" />
		/// <exception cref="MutationTextMissingException" />
		IAtomicMutationTemplate GetMutationTemplate();
		/// <summary>
		/// Consumption method that returns an IAtomicQueryTemplate configured
		/// with the connection, query text, and optionally an exeception handler or
		/// a cancellation token.
		/// </summary>
		/// <typeparam name="T">The type that each result in the query is mapped to.</typeparam>
		/// <exception cref="ConnectionStringMissingException" />
		/// <exception cref="QueryTextMissingException" />
		IAtomicQueryTemplate<T> GetQueryTemplate<T>();
		IRawTemplateBuilder SetCancellationToken(CancellationToken cancellationToken);
		IRawTemplateBuilder SetConnectionString(string connectionString);
		IRawTemplateBuilder SetExceptionHandler(Func<Exception, Task> exceptionHandler);
		IRawTemplateBuilder SetQueryText(string queryText);
		IRawTemplateBuilder SetMutationText(string mutationText);
	}
}