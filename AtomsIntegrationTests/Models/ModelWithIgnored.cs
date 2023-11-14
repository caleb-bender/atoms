using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("ModelsWithIgnored")]
	public class ModelWithIgnored
	{
		[UniqueId]
		public long Id { get; set; }
		[AtomsIgnore(ReadFromDatabase = true)]
		public string? PropertyReadFromButNotWrittenTo { get; set; } = "default";
		[AtomsIgnore]
		public string? PropertyNeitherReadFromNorWrittenTo { get; set; } = "default";
	}
}
