using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class NonexistentModel
	{
		[UniqueId]
		public int Id { get; set; }
	}
}
