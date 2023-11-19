using Atoms.Templates.Query;
using AtomsIntegrationTests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	public abstract class AtomicQueryTemplateMappingErrorsTests : IDisposable
	{
		IAtomicQueryTemplate<NonexistentModel> nonexistentModelTemplate;
		IAtomicQueryTemplate<(DateTime, string)> customerAddressModelDbEntityMismatchTemplate;
		IAtomicQueryTemplate<DateTime> employeeSalaryTypeMismatchTemplate;

		protected AtomicQueryTemplateMappingErrorsTests(

			IAtomicQueryTemplate<DateTime> employeeSalaryTypeMismatchTemplate,
			IAtomicQueryTemplate<NonexistentModel> nonexistentModelTemplate,
			IAtomicQueryTemplate<(DateTime, string)> customerAddressModelDbEntityMismatchTemplate)
		{
			this.employeeSalaryTypeMismatchTemplate = employeeSalaryTypeMismatchTemplate;
			this.nonexistentModelTemplate = nonexistentModelTemplate;
			this.customerAddressModelDbEntityMismatchTemplate = customerAddressModelDbEntityMismatchTemplate;
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		protected abstract void Cleanup();
	}
}
