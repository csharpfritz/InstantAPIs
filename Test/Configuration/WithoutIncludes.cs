using Fritz.InstantAPIs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace Test.Configuration;

public class WithoutIncludes : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WithoutIncludes()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}


	[Fact]
	public void ShouldIncludeAllTables()
	{

		// arrange

		// act
		var config = _Builder.Build();

		// assert
		Assert.Equal(2, config.Tables.Count);
		Assert.Equal(ApiMethodsToGenerate.All, config.Tables.First().ApiMethodsToGenerate);
		Assert.Equal(ApiMethodsToGenerate.All, config.Tables.Skip(1).First().ApiMethodsToGenerate);

	}

}

