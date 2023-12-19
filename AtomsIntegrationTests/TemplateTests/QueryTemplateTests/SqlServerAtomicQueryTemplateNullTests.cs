using AtomsIntegrationTests.Models;
using CalebBender.Atoms.Repositories;
using CalebBender.Atoms.Repositories.Factories;
using CalebBender.Atoms.Templates.Builders;
using CalebBender.Atoms.Templates.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AtomsIntegrationTests.DatabaseConfig.SqlServer.SqlServerConnection;

namespace AtomsIntegrationTests.TemplateTests.QueryTemplateTests
{
	[Collection("SqlServerDBTests")]
	public class SqlServerAtomicQueryTemplateNullTests : AtomicQueryTemplateNullTests
	{
		protected override IAtomicQueryTemplate<CustomerAddress> GetCustomerAddressQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT * FROM CustomerAddresses WHERE Unit = @Unit")
				.GetQueryTemplate<CustomerAddress>();
		}

		protected override IAtomicQueryTemplate<string> GetPhoneNumberQueryTemplate()
		{
			return new SqlServerRawTemplateBuilder()
				.SetConnectionString(GetConnectionString())
				.SetQueryText("SELECT Phone FROM CustomerAddresses WHERE Unit IN @Units OR City IN @Cities")
				.GetQueryTemplate<string>();
		}


	}
}
