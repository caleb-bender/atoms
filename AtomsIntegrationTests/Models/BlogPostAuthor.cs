using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atoms.DataAttributes;

namespace AtomsIntegrationTests.Models
{
	public class BlogPostAuthor
	{
		[UniqueId]
		public long AuthorId { get; set; }
		public string AuthorName { get; set; } = "John Doe";
		public DateTime AuthorSinceDate { get; set; }
	}
}
