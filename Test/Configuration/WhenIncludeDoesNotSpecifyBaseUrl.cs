using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Test.Configuration;

public class WhenIncludeDoesNotSpecifyBaseUrl : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WhenIncludeDoesNotSpecifyBaseUrl()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}

	[Fact]
	public void ShouldSpecifyDefaultUrl()
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Contacts);
		var config = _Builder.Build();

		// assert
		Assert.Single(config.Tables);
		Assert.Equal(new Uri("/api/Contacts", uriKind: UriKind.Relative), config.Tables.First().BaseUrl);

	}


}


