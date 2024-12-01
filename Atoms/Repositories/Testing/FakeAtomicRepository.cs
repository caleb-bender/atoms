using CalebBender.Atoms.DataAttributes;
using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalebBender.Atoms.Repositories.Testing
{
    public class FakeAtomicRepository<TModel> : IAtomicRepository<TModel> where TModel : class, new()
    {
        private readonly List<TModel> testCollection;

        public FakeAtomicRepository(List<TModel> testCollection)
        {
            if (testCollection is null)
                throw new ArgumentNullException("The test collection passed to the FakeAtomicRepository constructor cannot be null");
            var properties = typeof(TModel).GetProperties();
            if (!properties.Any(p => Attribute.IsDefined(p, typeof(UniqueIdAttribute))))
                throw new MissingUniqueIdAttributeException("The data model class must have at least one public property annotated with UniqueIdAttribute");
            this.testCollection = testCollection;
        }

        public Task<IEnumerable<TModel>> CreateManyAsync(IEnumerable<TModel> models)
        {
            if (models is null) return Task.FromResult(new List<TModel>() as IEnumerable<TModel>);
            foreach (var model in models)
            {
                if (model is null)
                    throw new ArgumentNullException("A model passed to CreateManyAsync cannot be null");
                if (GetSavedModel(model) is not null)
                    throw new DuplicateUniqueIdException("The model being created already exists in the test collection");
                testCollection.Add(model);
            }
            return Task.FromResult(models);
        }

        public Task<int> DeleteManyAsync(IEnumerable<TModel> models)
        {
            if (models is null) return Task.FromResult(0);
            int numberDeleted = 0;
            foreach (var model in models)
            {
                if (model is null)
                    throw new ArgumentNullException("A model passed to DeleteManyAsync cannot be null");
                var savedModel = GetSavedModel(model);
                if (savedModel is null) continue;
                var removed = testCollection.Remove(savedModel);
                if (removed) numberDeleted++;
            }
            return Task.FromResult(numberDeleted);
        }

        public Task<AtomicOption<TModel>> GetOneAsync(TModel model)
        {
            if (model is null)
                throw new ArgumentNullException("The model passed to GetOneAsync cannot be null");
            if (!testCollection.Contains(model))
                return Task.FromResult(new AtomicOption<TModel>.Empty() as AtomicOption<TModel>);
            return Task.FromResult(new AtomicOption<TModel>.Exists(model) as AtomicOption<TModel>);
        }

        public Task<int> UpdateManyAsync(IEnumerable<TModel> models)
        {
            if (models is null) return Task.FromResult(0);
            int numberUpdated = 0;
            foreach (var model in models)
            {
                if (model is null)
                    throw new ArgumentNullException("A model passed to UpdateManyAsync cannot be null");
                var savedModel = GetSavedModel(model);
                if (savedModel is not null)
                {
                    UpdateSavedModelPropertiesInPlace(savedModel, model);
                    numberUpdated++;
                }
            }
            return Task.FromResult(numberUpdated);
        }

        private TModel? GetSavedModel(TModel model)
        {
            return testCollection.Find(m => UniqueIdsMatch(m, model));
        }

        private void UpdateSavedModelPropertiesInPlace(TModel savedModel, TModel model)
        {
            var properties = typeof(TModel).GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.IsDefined(property, typeof(UniqueIdAttribute)))
                    continue;
                var value = property.GetValue(model);
                property.SetValue(savedModel, value);
            }
        }

        private bool UniqueIdsMatch(TModel model1, TModel model2)
        {
            var properties = typeof(TModel).GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.IsDefined(property, typeof(UniqueIdAttribute)))
                {
                    var value1 = property.GetValue(model1);
                    var value2 = property.GetValue(model2);
                    if (!value1!.Equals(value2))
                        return false;
                }
            }
            return true;
        }
    }
}
