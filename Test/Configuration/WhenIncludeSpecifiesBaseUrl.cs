using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Configuration;


public class WhenIncludeSpecifiesBaseUrl : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WhenIncludeSpecifiesBaseUrl()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}

	[Fact]
	public void ShouldSpecifyThatUrl()
	{

		// arrange

		// act
		var BaseUrl = new Uri("/testapi", UriKind.Relative);
		_Builder.IncludeTable(db => db.Contacts, baseUrl: BaseUrl.ToString());
		var config = _Builder.Build();

		// assert
		Assert.Single(config.Tables);
		Assert.Equal(BaseUrl, config.Tables.First().BaseUrl);

	}


}


