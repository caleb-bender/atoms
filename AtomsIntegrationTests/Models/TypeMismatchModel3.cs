using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("TypeMismatchModels")]
	public class TypeMismatchModel3
	{
		[UniqueId]
		public long Id { get; set; }
	}
}
