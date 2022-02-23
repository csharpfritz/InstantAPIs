using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Test.Configuration;

public class WithIncludesAndExcludes : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WithIncludesAndExcludes()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}

	[Fact]
	public void ShouldExcludePreviouslyIncludedTable()
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Addresses)
						.IncludeTable(db => db.Contacts)
						.ExcludeTable(db => db.Addresses);
		var config = _Builder.Build();

		// assert
		Assert.Single(config.Tables);
		Assert.Equal("Contacts", config.Tables.First().Name);

	}

}

