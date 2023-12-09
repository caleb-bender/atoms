using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	[Collection("SqlServerDBTests")]
	public abstract class BasicAtomicMutationTemplateTests : IDisposable
	{
		private readonly IAtomicRepository<BlogUser> blogUserRepo;

		public BasicAtomicMutationTemplateTests()
		{
			blogUserRepo = GetAtomicRepository<BlogUser>();
		}

		[Fact]
		public async Task GivenAFewBlogUsers_WhenWeExecuteAnUpdateSingleMutationTemplate_ThenTheNumberOfModifiedEntriesIsOne()
		{
			// Arrange
			var blogUser = new BlogUser { UserId = 1L, GroupName = "Group 1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group 2" };
			await blogUserRepo.CreateManyAsync(blogUser, blogUser2);
			var updateBlogUserMutationTemplate = GetUpdateSingleBlogUserMutationTemplate(blogUser.UserId, blogUser.GroupName);
			// Act
			var numberOfEntriesModified = await updateBlogUserMutationTemplate.MutateAsync();
			// Assert
			Assert.Equal(1, numberOfEntriesModified);
		}

		[Fact]
		public async Task GivenAFewBlogUsers_WhenWeExecuteAnUpdateSingleMutationTemplate_ThenTheCorrectOneIsUpdated()
		{
			// Arrange
			var blogUser = new BlogUser { UserId = 1L, GroupName = "Group 1" };
			var blogUser2 = new BlogUser { UserId = 1L, GroupName = "Group 2" };
			await blogUserRepo.CreateManyAsync(blogUser, blogUser2);
			var updateBlogUserMutationTemplate = GetUpdateSingleBlogUserMutationTemplate(blogUser.UserId, blogUser.GroupName);
			// Act
			await updateBlogUserMutationTemplate.MutateAsync();
			// Assert
			var retrievedBlogUserOption = await blogUserRepo.GetOneAsync(blogUser);
			var retrievedBlogUser = Assert.IsType<AtomicOption<BlogUser>.Exists>(retrievedBlogUserOption).Value;
			Assert.Equal(BlogUser.BlogUserRole.Moderator, retrievedBlogUser.UserRole);
		}

		protected abstract IAtomicMutationTemplate GetUpdateSingleBlogUserMutationTemplate(long userId, string groupName);
		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();
		protected abstract void Cleanup();

		public void Dispose()
		{
			Cleanup();
		}
	}
}
