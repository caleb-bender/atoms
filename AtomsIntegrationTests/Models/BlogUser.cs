using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("TheBlogUsers")]
	public class BlogUser
	{
		public enum BlogUserRole
		{
			Reader,
			Contributor,
			Moderator,
		};

		[UniqueId]
		public long UserId { get; set; }
		[UniqueId]
		[DbPropertyName("GroupId")]
		public string GroupName { get; set; } = "";
		public BlogUserRole UserRole { get; set; } = BlogUserRole.Reader;

	}
}
