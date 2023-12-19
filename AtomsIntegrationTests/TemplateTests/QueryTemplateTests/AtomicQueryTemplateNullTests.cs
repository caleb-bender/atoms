using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateNullTests
	{
		[Fact]
		public async Task GivenIEnumerableParametersContainNull_WhenWeQueryAsync_ThenAnArgumentNullExceptionIsThrown()
		{
			// Arrange
			var phoneNumberQueryTemplate = GetPhoneNumberQueryTemplate();
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				// Act
				var Units = new int?[] { null, 5 };
				var Cities = new string?[] { "San Francisco" };
				var phoneNumbers = await phoneNumberQueryTemplate.QueryAsync(
					new
					{
						Units,
						Cities
					});
			});
		}

		[Fact]
		public async Task GivenANullParameter_WhenWeQueryAsync_ThenAnArgumentNullExceptionIsThrown()
		{
			// Arrange
			var customerAddressQueryTemplate = GetCustomerAddressQueryTemplate();
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				// Act
				int? Unit = null;
				await customerAddressQueryTemplate.QueryAsync(new { Unit });
			});
		}

		protected abstract IAtomicQueryTemplate<CustomerAddress> GetCustomerAddressQueryTemplate();
		protected abstract IAtomicQueryTemplate<string> GetPhoneNumberQueryTemplate();
	}
}
