using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Xunit;

namespace Test.InstantAPIs;

public class WebApplicationExtensions : BaseFixture
{

	[Fact]
	public void WhenMapInstantAPIsExpectedDefaultBehaviour()
	{

		// arrange
		var app = Mockery.Create<IEndpointRouteBuilder>();
		var dataSources = new List<EndpointDataSource>();
		app.Setup(x => x.DataSources).Returns(dataSources);

		// act
		app.Object.MapInstantAPIs<MyContext>();

		// assert
		Assert.NotEmpty(dataSources);
	}


	private class MyContext : DbContext
	{

		public MyContext(DbContextOptions<MyContext> options) : base(options) { }

		public DbSet<Contact> Contacts => Set<Contact>();

	}

	private class Contact
	{

		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Email { get; set; }

	}

}
