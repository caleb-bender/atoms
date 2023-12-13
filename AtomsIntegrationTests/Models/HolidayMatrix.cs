using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	[DbEntityName("HolidayMatrices")]
	public class HolidayMatrix
	{
		[StringToEnumVariantMappingRule("SUN", Sunday)]
		[StringToEnumVariantMappingRule("MON", Monday)]
		[StringToEnumVariantMappingRule("TUE", Tuesday)]
		[StringToEnumVariantMappingRule("WED", Wednesday)]
		[StringToEnumVariantMappingRule("THU", Thursday)]
		[StringToEnumVariantMappingRule("FRI", Friday)]
		[StringToEnumVariantMappingRule("SAT", Saturday)]
		public enum WeekDays
		{
			Unknown,
			Sunday,
			Monday,
			Tuesday,
			Wednesday,
			Thursday,
			Friday,
			Saturday
		};

		[UniqueId]
		public WeekDays HolidayDay { get; set; }
		[UniqueId]
		public WeekDays RouteDay { get; set; }
		public int DaysToSkip { get; set; }
	}
}
