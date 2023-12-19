using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Mutation;
using CalebBender.Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	public abstract class AtomicMutationTemplateNullTests
	{

		[Fact]
		public async Task GivenIEnumerableParametersContainNull_WhenWeQueryAsync_ThenAnArgumentNullExceptionIsThrown()
		{
			// Arrange
			var phoneNumberMutationTemplate = GetPhoneNumberMutationTemplate();
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				// Act
				var Units = new int?[] { null, 5 };
				var Cities = new string?[] { "San Francisco" };
				var phoneNumbers = await phoneNumberMutationTemplate.MutateAsync(
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
			var customerAddressMutationTemplate = GetCustomerAddressMutationTemplate();
			// Assert
			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
			{
				// Act
				int? Unit = null;
				await customerAddressMutationTemplate.MutateAsync(new { Unit });
			});
		}

		protected abstract IAtomicMutationTemplate GetPhoneNumberMutationTemplate();
		protected abstract IAtomicMutationTemplate GetCustomerAddressMutationTemplate();
	}
}
