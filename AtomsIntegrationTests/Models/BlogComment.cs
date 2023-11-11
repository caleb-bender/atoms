using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class BlogComment
	{
		public string Username { get; set; }
		public string Content { get; set; }
		public DateTime CreatedAt { get; } = DateTime.Now;
	}
}
