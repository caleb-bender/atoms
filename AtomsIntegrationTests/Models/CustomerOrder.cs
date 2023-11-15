using Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class CustomerOrder
	{
		[StringToEnumVariantMappingRule("PC", PickupByCustomer)]
		[StringToEnumVariantMappingRule("D", Delivery)]
		[StringToEnumVariantMappingRule("PT", PickupByThirdParty)]
		[StringToEnumVariantMappingRule("NONE", Unknown)]
		[StringToEnumVariantMappingRule("", Unknown)]
		public enum FulfillmentTypes
		{
			Unknown,
			PickupByCustomer,
			Delivery,
			PickupByThirdParty
		}

		[UniqueId]
		public Guid OrderId { get; set; }
		[DbPropertyName("OrderType")]
		public FulfillmentTypes FulfillmentType { get; set; }
	}
}
