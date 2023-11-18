using Atoms.Repositories;
using Atoms.Repositories.Factories;
using Atoms.Templates.Query;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateTests : IDisposable
	{
		private readonly IAtomicRepository<BlogPost> blogPostRepo;
		private readonly IAtomicQueryTemplate<long> blogPostIdQueryTemplate;
		private readonly IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate;
		private BlogPost blogPost1 = new BlogPost { PostId = 1L, Genre = BlogPost.BlogPostGenre.Horror, Title = "The Cursed Baby" };
		private BlogPost blogPost2 = new BlogPost { PostId = 2L, Genre = BlogPost.BlogPostGenre.Scifi, Title = "Cosmic Quest" };
		private BlogPost blogPost3 = new BlogPost { PostId = 3L, Genre = BlogPost.BlogPostGenre.Fantasy, Title = "Kingdoms of the Hallowed" };

		protected AtomicQueryTemplateTests(
			IAtomicRepository<BlogPost> blogPostRepo,
			IAtomicQueryTemplate<long> blogPostIdQueryTemplate,
			IAtomicQueryTemplate<(long, BlogPost.BlogPostGenre, string)> blogIdGenreTitleQueryTemplate
		)
		{
			this.blogPostRepo = blogPostRepo;
			this.blogPostIdQueryTemplate = blogPostIdQueryTemplate;
			this.blogIdGenreTitleQueryTemplate = blogIdGenreTitleQueryTemplate;
		}

		[Fact]
		public async Task WhenWeLazilyQueryACollectionOfPrimitives_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			await blogPostRepo.CreateManyAsync(blogPost1, blogPost2, blogPost3);
			// Act
			var lazyBlogPostIds = blogPostIdQueryTemplate.QueryLazy();
			// Assert
			long expectedPostId = 1L;
			await foreach (var blogPostId in lazyBlogPostIds)
			{
				Assert.Equal(expectedPostId++, blogPostId);
			}
		}

		[Fact]
		public async Task WhenWeLazilyQueryACollectionOfACouplePrimitivesIntoATuple_ThenWeGetBackTheCorrectValues()
		{
			// Arrange
			var blogPosts = new[] { blogPost1, blogPost2, blogPost3 };
			await blogPostRepo.CreateManyAsync(blogPosts);
			// Act
			var lazyIdsGenresTitles = blogIdGenreTitleQueryTemplate.QueryLazy();
			// Assert
			int i = 0;
			await foreach (var (id, genre, title) in lazyIdsGenresTitles)
			{
				Assert.Equal(blogPosts[i].PostId, id);
				Assert.Equal(blogPosts[i].Genre, genre);
				Assert.Equal(blogPosts[i].Title, title);
				i++;
			}
		}

		public void Dispose()
		{
			Cleanup();
		}

		protected abstract void Cleanup();
	}
}
