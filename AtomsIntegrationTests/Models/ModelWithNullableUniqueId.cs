using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class ModelWithNullableUniqueId
	{
		[UniqueId]
		public int? Id { get; set; }
	}
}
