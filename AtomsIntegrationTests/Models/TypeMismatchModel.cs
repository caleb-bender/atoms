using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class TypeMismatchModel
	{
		[UniqueId]
		public Guid Id { get; set; }
		public int Status { get; set; }
		public int? DateCreated { get; set; }
	}
}
