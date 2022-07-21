using InstantAPIs;
using InstantAPIs.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Test.InstantAPIs;

public class WebApplicationExtensions : BaseFixture
{

	[Fact]
	public void WhenMapInstantAPIsExpectedDefaultBehaviour()
	{

		// arrange
		var serviceProviderMock = Mockery.Create<IServiceProvider>();
		var optionsMock = Mockery.Create<IOptions<InstantAPIsOptions>>();
		var app = Mockery.Create<IEndpointRouteBuilder>();
		var dataSources = new List<EndpointDataSource>();
		var contextMock = Mockery.Create<IContextHelper<MyContext>>();

		contextMock.Setup(x => x.NameTable(It.Is<Expression<Func<MyContext, DbSet<Contact>>>>(a => ((MemberExpression)a.Body).Member.Name == "Contacts"))).Returns("Contacts");
		contextMock.Setup(x => x.NameTable(It.Is<Expression<Func<MyContext, DbSet<Address>>>>(a => ((MemberExpression)a.Body).Member.Name == "Addresses"))).Returns("Addresses");
		contextMock.Setup(x => x.DiscoverFromContext(It.IsAny<Uri>()))
			.Returns(new InstantAPIsOptions.ITable[] {
				new InstantAPIsOptions.Table<MyContext, DbSet<Contact>, Contact, int>("Contacts", new Uri("Contacts", UriKind.Relative), c => c.Contacts , new InstantAPIsOptions.TableOptions<Contact, int>() { KeySelector = x => x.Id }),
				new InstantAPIsOptions.Table<MyContext, DbSet<Address>, Address, int>("Addresses", new Uri("Addresses", UriKind.Relative), c => c.Addresses, new InstantAPIsOptions.TableOptions<Address, int>() { KeySelector = x => x.Id })
			});
		app.Setup(x => x.DataSources).Returns(dataSources);
		app.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
		serviceProviderMock.Setup(x => x.GetService(typeof(IOptions<InstantAPIsOptions>))).Returns(optionsMock.Object);
		serviceProviderMock.Setup(x => x.GetService(typeof(IContextHelper<MyContext>))).Returns(contextMock.Object);
		serviceProviderMock.Setup(x => x.GetService(typeof(ILoggerFactory))).Returns(Mockery.Create<ILoggerFactory>().Object);
		optionsMock.Setup(x => x.Value).Returns(new InstantAPIsOptions());

		// act
		app.Object.MapInstantAPIs<MyContext>();

		// assert
		Assert.NotEmpty(dataSources);
		Assert.Equal(10, dataSources[0].Endpoints.Count);
	}

}
