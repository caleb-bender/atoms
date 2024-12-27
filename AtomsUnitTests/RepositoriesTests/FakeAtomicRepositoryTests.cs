using CalebBender.Atoms.DataAttributes;
using CalebBender.Atoms.Exceptions;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Testing;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsUnitTests.RepositoriesTests
{
    public class FakeAtomicRepositoryTests
    {
        class PlayerStats
        {
            [UniqueId]
            public int Id { get; set; }
            [UniqueId]
            public int PlayerId { get; set; }
            public int Health { get; set; }
            public int Armor { get; set; }
            public int Damage { get; set; }
        }


        List<PlayerStats> playerStatsCollection = new();
        IAtomicRepository<PlayerStats> fakeRepo;
        PlayerStats stats = new PlayerStats { Id = 1, PlayerId = 1, Health = 100, Armor = 10, Damage = 20 };
        PlayerStats stats2 = new PlayerStats { Id = 2, PlayerId = 1, Health = 90, Armor = 5, Damage = 15 };
        PlayerStats stats3 = new PlayerStats { Id = 3, PlayerId = 2, Health = 80, Armor = 0, Damage = 10 };

        PlayerStats statsUpdated = new PlayerStats { Id = 1, PlayerId = 1, Health = 50, Armor = 5, Damage = 10 };
        PlayerStats stats2Updated = new PlayerStats { Id = 2, PlayerId = 1, Health = 40, Armor = 4, Damage = 8 };
        PlayerStats stats3Updated = new PlayerStats { Id = 3, PlayerId = 2, Health = 30, Armor = 3, Damage = 6 };

        public FakeAtomicRepositoryTests()
        {
            fakeRepo = new FakeAtomicRepository<PlayerStats>(playerStatsCollection);
        }

        [Fact]
        public void GivenANullCollection_WhenInitializingFakeRepo_ThenAnArgumentNullExceptionIsThrown()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                // Act
                var fakeRepo = new FakeAtomicRepository<PlayerStats>(null);
            });
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenEntryIsCreated_ThenCollectionContainsEntry()
        {
            // Act
            await fakeRepo.CreateOneAsync(stats);
            // Assert
            Assert.Single(playerStatsCollection);
            Assert.Contains(stats, playerStatsCollection);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenEntryIsCreatedAndRetrieved_ItIsAtomicOptionExists()
        {
            // Act
            await fakeRepo.CreateOneAsync(stats);
            var retrievedStatsResult = await fakeRepo.GetOneAsync(stats);
            // Assert
            var exists = Assert.IsType<AtomicOption<PlayerStats>.Exists>(retrievedStatsResult);
            Assert.Equal(stats, exists.Value);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenEntryIsCreatedAndRetrievedUsingDeepCopy_ThenTheEntryIsReturned()
        {
            // Act
            await fakeRepo.CreateOneAsync(stats);
            var retrievedStatsResult = await fakeRepo.GetOneAsync(statsUpdated);
            // Assert
            var exists = Assert.IsType<AtomicOption<PlayerStats>.Exists>(retrievedStatsResult);
            Assert.Equal(stats, exists.Value);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenNonexistentEntryIsRetrieved_ThenItIsAtomicOptionEmpty()
        {
            // Act
            var result = await fakeRepo.GetOneAsync(stats);
            // Assert
            Assert.IsType<AtomicOption<PlayerStats>.Empty>(result);
        }

        [Fact]
        public async Task WhenEntryIsCreatedAndDeleted_ThenCollectionIsEmpty()
        {
            // Act
            await fakeRepo.CreateOneAsync(stats);
            await fakeRepo.DeleteOneAsync(stats);
            // Assert
            Assert.Empty(playerStatsCollection);
        }

        [Fact]
        public async Task WhenEntryIsCreatedAndDeleted_ThenOneIsReturnedFromDelete()
        {
            // Act
            await fakeRepo.CreateOneAsync(stats);
            var result = await fakeRepo.DeleteOneAsync(stats);
            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenEntryIsDeleted_ThenZeroIsReturnedFromDelete()
        {
            // Act
            var result = await fakeRepo.DeleteOneAsync(stats);
            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenEntriesAreCreated_ThenTheCreatedEntriesAreReturned()
        {
            // Act
            var created = await fakeRepo.CreateManyAsync(new List<PlayerStats> { stats, stats2, stats3 });
            // Assert
            Assert.Equal(3, playerStatsCollection.Count);
            Assert.Equal(3, created.Count());
            Assert.Contains(stats, playerStatsCollection);
            Assert.Contains(stats2, playerStatsCollection);
            Assert.Contains(stats3, playerStatsCollection);
        }

        [Fact]
        public async Task GivenAnEmptyCollection_WhenAnNonexistentEntryIsUpdated_ThenZeroIsReturned()
        {
            // Act
            var result = await fakeRepo.UpdateOneAsync(stats);
            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GivenThreeCreatedEntries_WhenTwoAreUpdated_ThenTwoIsReturned()
        {
            // Arrange
            playerStatsCollection.AddRange(new List<PlayerStats> { stats, stats2, stats3 });
            // Act
            var result = await fakeRepo.UpdateManyAsync(new List<PlayerStats> { stats, stats2 });
            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task GivenSomeCreatedEntries_WhenTheEntryIsUpdatedUsingADeepCopyWithSameIds_ThenTheEntryInTheTestCollectionIsUpdated()
        {
            // Arrange
            playerStatsCollection.Add(stats);
            playerStatsCollection.Add(stats2);
            playerStatsCollection.Add(stats3);
            var updatedStats = new PlayerStats { Id = 1, PlayerId = 1, Health = 50, Armor = 5, Damage = 10 };
            // Act
            await fakeRepo.UpdateOneAsync(updatedStats);
            // Assert
            Assert.Equal(3, playerStatsCollection.Count);
            Assert.Contains(stats, playerStatsCollection);
            Assert.Equal(50, stats.Health);
            Assert.Equal(5, stats.Armor);
            Assert.Equal(10, stats.Damage);
        }

        [Fact]
        public async Task GivenThreeCreatedEntries_WhenAllAreUpdated_ThenThreeIsReturned_And_TheEntriesInTheTestCollectionAreUpdated()
        {
            // Arrange
            playerStatsCollection.AddRange(new List<PlayerStats> { stats, stats2, stats3 });
            var newStats = new List<PlayerStats>
            {
                statsUpdated, stats2Updated, stats3Updated
            };
            // Act
            var result = await fakeRepo.UpdateManyAsync(newStats);
            // Assert
            Assert.Equal(3, result);
            Assert.Equal(3, playerStatsCollection.Count);
            Assert.Contains(stats, playerStatsCollection);
            Assert.Contains(stats2, playerStatsCollection);
            Assert.Contains(stats3, playerStatsCollection);
            Assert.Equal(50, stats.Health);
            Assert.Equal(5, stats.Armor);
            Assert.Equal(10, stats.Damage);
            Assert.Equal(40, stats2.Health);
            Assert.Equal(4, stats2.Armor);
            Assert.Equal(8, stats2.Damage);
            Assert.Equal(30, stats3.Health);
            Assert.Equal(3, stats3.Armor);
            Assert.Equal(6, stats3.Damage);
        }

        [Fact]
        public async Task WhenTwoEntriesWithMatchingUniqueIdsAreCreated_ThenDuplicateUniqueIdExceptionIsThrown()
        {
            // Assert
            await Assert.ThrowsAsync<DuplicateUniqueIdException>(() =>
            {
                // Act
                return fakeRepo.CreateManyAsync(stats, statsUpdated);
            });
        }

        [Fact]
        public async Task GivenThreeCreatedEntries_WhenTwoAreDeleted_ThenTwoIsReturned_And_TheTestCollectionContainsOneEntry()
        {
            // Arrange
            await fakeRepo.CreateManyAsync(new List<PlayerStats> { stats, stats2, stats3 });
            // Act
            var result = await fakeRepo.DeleteManyAsync(new List<PlayerStats> { stats, stats2 });
            // Assert
            Assert.Equal(2, result);
            Assert.Single(playerStatsCollection);
            Assert.Contains(stats3, playerStatsCollection);
        }

        [Fact]
        public async Task WhenNullCollectionOfEntriesIsPassedToCreate_ThenNothingIsCreated()
        {
            // Act
            var created = await fakeRepo.CreateManyAsync(null);
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.NotNull(created);
            Assert.Empty(created);
        }

        [Fact]
        public async Task WhenEmptyCollectionOfEntriesIsPassedToCreate_ThenNothingIsCreated()
        {
            // Act
            var created = await fakeRepo.CreateManyAsync(new List<PlayerStats>());
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.NotNull(created);
            Assert.Empty(created);
        }


        [Fact]
        public async Task WhenNullCollectionOfEntriesIsPassedToUpdate_ThenNothingIsUpdated()
        {
            // Act
            var updated = await fakeRepo.UpdateManyAsync(null);
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.Equal(0, updated);
        }

        [Fact]
        public async Task WhenEmptyCollectionOfEntriesIsPassedToUpdate_ThenNothingIsUpdated()
        {
            // Act
            var updated = await fakeRepo.UpdateManyAsync(new List<PlayerStats>());
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.Equal(0, updated);
        }

        [Fact]
        public async Task WhenCreatingNullEntries_AnArgumentNullExceptionIsThrown()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                // Act
                return fakeRepo.CreateManyAsync(new List<PlayerStats> { null, null });
            });
        }

        [Fact]
        public async Task WhenUpdatingUsingNullEntries_AnArgumentNullExceptionIsThrown()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                // Act
                return fakeRepo.UpdateManyAsync(new List<PlayerStats> { null, null });
            });
        }

        [Fact]
        public async Task WhenNullCollectionOfEntriesIsPassedToDelete_ThenNothingIsDeleted()
        {
            // Act
            var deleted = await fakeRepo.DeleteManyAsync(null);
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.Equal(0, deleted);
        }

        [Fact]
        public async Task WhenEmptyCollectionOfEntriesIsPassedToDelete_ThenNothingIsDeleted()
        {
            // Act
            var deleted = await fakeRepo.DeleteManyAsync(new List<PlayerStats>());
            // Assert
            Assert.Empty(playerStatsCollection);
            Assert.Equal(0, deleted);
        }

        [Fact]
        public async Task WhenDeletingNullEntries_AnArgumentNullExceptionIsThrown()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                // Act
                return fakeRepo.DeleteManyAsync(new List<PlayerStats> { null, null });
            });
        }

        [Fact]
        public async Task WhenGettingAnEntryUsingNull_ThenAnArgumentNullExceptionIsThrown()
        {
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            {
                // Act
                return fakeRepo.GetOneAsync(null);
            });
        }

        [Fact]
        public async Task WhenTwoEntriesWithOneIdenticalUniqueIdAndOneDifferentUniqueIdAreCreated_ThenTestCollectionHasTwoEntries()
        {
            // Act
            await fakeRepo.CreateManyAsync(new List<PlayerStats> { stats, stats2 });
            // Assert
            Assert.Equal(2, playerStatsCollection.Count);
            Assert.Contains(stats, playerStatsCollection);
            Assert.Contains(stats2, playerStatsCollection);
        }

        [Fact]
        public void WhenFakeRepoIsCreatedWithModelWithoutUniqueIdAttributes_ThenMissingUniqueIdAttributeExceptionIsThrown()
        {
            // Assert
            Assert.Throws<MissingUniqueIdAttributeException>(() =>
            {
                // Act
                var fakeRepo = new FakeAtomicRepository<ModelWithoutUniqueId>(new List<ModelWithoutUniqueId>());
            });
        }

        [Fact]
        public void WhenFakeRepoIsCreatedWithModelWithOneUniqueIdAttribute_ThenItIsCreated()
        {
            // Act
            var fakeRepo = new FakeAtomicRepository<ModelWithOneUniqueId>(new List<ModelWithOneUniqueId>());
            // Assert
            Assert.NotNull(fakeRepo);
        }

        [Fact]
        public async Task GivenAFewCreatedModels_WhenThoseModelsAreDeletedUsingCopies_ThenTheTestCollectionIsEmpty()
        {
            // Arrange
            var statsList = new List<PlayerStats> { stats, stats2, stats3 };
            var statsListCopies = new List<PlayerStats> { statsUpdated, stats2Updated, stats3Updated };
            await fakeRepo.CreateManyAsync(statsList);
            // Act
            await fakeRepo.DeleteManyAsync(statsListCopies);
            // Assert
            Assert.Empty(playerStatsCollection);
        }

        public class ModelWithoutUniqueId
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class ModelWithOneUniqueId
        {
            [UniqueId]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
