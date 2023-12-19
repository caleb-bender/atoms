using AtomsIntegrationTests.TemplateTests.QueryTemplateTests;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;
using CalebBender.Atoms.Templates.Mutation;

namespace AtomsIntegrationTests.TemplateTests.MutationTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerAtomicMutationTemplateNullTests : AtomicMutationTemplateNullTests
	{
		protected override IAtomicMutationTemplate GetCustomerAddressMutationTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("UPDATE CustomerAddresses SET Unit = @Unit")
				.GetMutationTemplate();
		}

		protected override IAtomicMutationTemplate GetPhoneNumberMutationTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetMutationText("UPDATE CustomerAddresses SET Unit = 123 WHERE Unit IN @Units OR City IN @Cities")
				.GetMutationTemplate();
		}
	}
}
