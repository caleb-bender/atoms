using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("CustomerAddresses")]
	public class CustomerAddress
	{
		[UniqueId]
		[DbPropertyName("Phone")]
		public string PhoneNumber { get; set; }
		[DbPropertyName("Unit")]
		public int? UnitNumber { get; set; }
		public string? StreetNumber { get; set; }
		public string? Street { get; set; }
		public string? PostalCode { get; set; }
		public string? City { get; set; }
		public string? Province { get; set; }
		public string? Country { get; set; }
	}
}
