using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class ModelWithPropertyNotCompatibleWithUniqueId
	{
		[UniqueId]
		public List<int> InvalidId { get; set; }
		[UniqueId]
		public long Id { get; set; }
	}
}
