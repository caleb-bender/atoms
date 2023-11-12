using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class JobPosting
	{
		[UniqueId]
		public long PostingId { get; set; }
		[UniqueId]
		public long EmployerId { get; set; }
		public string? Description { get; set; }
	}
}
