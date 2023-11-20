using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("JobPostings")]
	public class JobPostingModelEntityMismatch
	{
		[UniqueId]
		public Guid Id { get; set; }
	}
}
