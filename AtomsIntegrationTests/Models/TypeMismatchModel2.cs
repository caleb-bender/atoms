using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class TypeMismatchModel2
	{
		[UniqueId]
		public long TheId { get; set; }
		public int DateCreated { get; set; }
	}
}
