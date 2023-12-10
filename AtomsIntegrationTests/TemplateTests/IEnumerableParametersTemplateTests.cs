using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.Models.BlogPost;

namespace AtomsIntegrationTests.TemplateTests
{
	public abstract class IEnumerableParametersTemplateTests : IDisposable
	{
		protected readonly IAtomicRepository<BlogPost> blogPostRepo;
		protected readonly BlogPost[] blogPosts = new[]
		{
			new BlogPost { PostId = 1L, Genre = BlogPostGenre.Fantasy, Title = "1" },
			new BlogPost { PostId = 2L, Genre = BlogPostGenre.Horror, Title = "2" },
			new BlogPost { PostId = 3L, Genre = BlogPostGenre.Thriller, Title = "3" },
			new BlogPost { PostId = 4L, Genre = BlogPostGenre.Scifi, Title = "4" }
		};

		protected readonly IAtomicRepository<CustomerOrder> customerOrderRepo;
		protected readonly List<CustomerOrder> customerOrders = new List<CustomerOrder>
		{
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Unknown },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByThirdParty },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.PickupByCustomer },
			new CustomerOrder { OrderId = Guid.NewGuid(), FulfillmentType = CustomerOrder.FulfillmentTypes.Delivery }
		};

		public IEnumerableParametersTemplateTests()
		{
			blogPostRepo = GetAtomicRepository<BlogPost>();
			customerOrderRepo = GetAtomicRepository<CustomerOrder>();
		}

		protected abstract IAtomicRepository<T> GetAtomicRepository<T>() where T : class, new();

		protected abstract void Cleanup();

		public void Dispose()
		{
			Cleanup();
		}
	}
}
