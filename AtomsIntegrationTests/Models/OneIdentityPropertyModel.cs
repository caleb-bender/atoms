﻿using CalebBender.Atoms.DataAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.Models
{
	public class OneIdentityPropertyModel
	{
		[UniqueId(AutoGenerated = true)]
		public long Id { get; set; }
	}
}
