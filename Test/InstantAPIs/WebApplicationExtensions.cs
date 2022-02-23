using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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
		Assert.Equal(10, dataSources[0].Endpoints.Count);
	}

}
