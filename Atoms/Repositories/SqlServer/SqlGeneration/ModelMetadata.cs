﻿using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Atoms.Utils.Reflection.AttributeCheckerHelpers;
using static Atoms.Utils.Reflection.PropertyInfoRetrieverHelpers;

namespace Atoms.Repositories.SqlServer.SqlGeneration
{
	internal static class ModelMetadata<TModel>
		where TModel : class, new()
	{
		private static readonly IEnumerable<PropertyInfo> modelPublicProperties = GetAllPublicProperties(typeof(TModel));
		private static readonly IEnumerable<PropertyInfo> modelPublicPropertiesWithUniqueIdAttribute =
			GetPublicPropertiesThatContainAttribute<UniqueIdAttribute>(typeof(TModel));
		private static readonly Type modelType = typeof(TModel);
		private static readonly DbEntityNameAttribute? dbEntityNameAttribute = modelType.GetCustomAttribute<DbEntityNameAttribute>();
		private static readonly string databaseTableName = GetDatabaseTableName();

		public static string TableName { get => databaseTableName; }
		public static IEnumerable<PropertyInfo> PublicProperties { get => modelPublicProperties; }
		public static IEnumerable<PropertyInfo> UniqueIdPublicProperties { get => modelPublicPropertiesWithUniqueIdAttribute; }

		internal static string GetDatabaseTableName()
		{
			var tableName = modelType.Name + "s";
			if (dbEntityNameAttribute is not null)
				tableName = dbEntityNameAttribute.Name;
			return tableName;
		}

		internal static string GetDatabasePropertyName(PropertyInfo property)
		{
			var propertyName = property.Name;
			var dbPropertyNameAttribute = property.GetCustomAttribute<DbPropertyNameAttribute>();
			if (dbPropertyNameAttribute is not null) propertyName = dbPropertyNameAttribute.Name;
			return propertyName;
		}
	}
}
