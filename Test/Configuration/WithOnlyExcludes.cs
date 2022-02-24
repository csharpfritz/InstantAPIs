using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace Test.Configuration;

public class WithOnlyExcludes : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WithOnlyExcludes()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}

	[Fact]
	public void ShouldExcludeSpecifiedTable()
	{

		// arrange

		// act
		_Builder.ExcludeTable(db => db.Addresses);
		var config = _Builder.Build();

		// assert
		Assert.Single(config.Tables);
		Assert.Equal("Contacts", config.Tables.First().Name);

	}

	[Fact]
	public void ShouldThrowAnErrorIfAllTablesExcluded()
	{

		// arrange

		// act
		_Builder.ExcludeTable(db => db.Addresses)
						.ExcludeTable(db => db.Contacts);

		Assert.Throws<ArgumentException>(() => _Builder.Build());

	}

}

