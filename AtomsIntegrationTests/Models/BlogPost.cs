using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class BlogPost
	{
		public enum BlogPostGenre
		{
			Horror,
			Thriller,
			Scifi,
			Fantasy
		}

		[UniqueId]
		public long PostId { get; set; }
		[UniqueId]
		public BlogPostGenre Genre { get; set; }
		[MaxLength(30)]
		public string Title { get; set; } = "";
		public string Content { get; set; } = "";
		public List<BlogComment>? BlogComments { get; set; }
	}
}
