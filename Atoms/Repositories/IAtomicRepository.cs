﻿using CalebBender.Atoms.Utils;
using CalebBender.Atoms.Exceptions;

namespace CalebBender.Atoms.Repositories
{
	/// <summary>
	/// An object with immutable state used for basic CRUD operations on a data model class.
	/// </summary>
	/// <typeparam name="TModel">
	/// A data model class with at least one public property
	/// annotated with UniqueIdAttribute.
	/// </typeparam>
	public interface IAtomicRepository<TModel> where TModel : class, new()
	{
		/// <summary>
		/// Creates a batch of data class models in a single transaction.
		/// </summary>
		/// <returns>
		/// A task that when completed, will contain the created data class models (in case any unique ids were auto-generated).
		/// </returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="DuplicateUniqueIdException" />
		/// <exception cref="EnumPropertyMappingFailedException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		/// <exception cref="NoWritableModelPropertiesExistException" />
		/// <exception cref="StringPropertyValueExceedsMaxLengthException" />
		Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models);
		/// <summary>
		/// Deletes a batch of data class models in a single transaction.
		/// </summary>
		/// <returns>A task containing the number of models deleted as a 32-bit integer</returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		Task<int> DeleteManyAsync(IEnumerable<TModel> models);
		/// <summary>
		/// Gets a single data model class from the database or null, wrapped inside
		/// an AtomicOption for strict null-checking.
		/// </summary>
		/// <param name="model"></param>
		/// <returns>
		/// The task that when completed, contains the AtomicOption with either
		/// the data model class instance or null.
		/// </returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		Task<AtomicOption<TModel>> GetOneAsync(TModel model);
		/// <summary>
		/// Updates a batch of data class models in a single transaction.
		/// </summary>
		/// <returns>A task containing the number of models updated as a 32-bit integer</returns>
		/// <exception cref="ArgumentNullException" />
		/// <exception cref="DbEntityNotFoundException" />
		/// <exception cref="EnumPropertyMappingFailedException" />
		/// <exception cref="ModelDbEntityMismatchException" />
		/// <exception cref="ModelPropertyTypeMismatchException" />
		/// <exception cref="NoWritableModelPropertiesExistException" />
		/// <exception cref="StringPropertyValueExceedsMaxLengthException" />
		Task<int> UpdateManyAsync(IEnumerable<TModel> models);
	}
}
