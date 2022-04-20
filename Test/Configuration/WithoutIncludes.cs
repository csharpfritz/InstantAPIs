using InstantAPIs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

namespace Test.Configuration;

public class WithoutIncludes : BaseFixture
{

	InstantAPIsBuilder<MyContext> _Builder;

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
		Assert.Equal(2, config.Count);
		Assert.Equal(ApiMethodsToGenerate.All, config.First().ApiMethodsToGenerate);
		Assert.Equal(ApiMethodsToGenerate.All, config.Skip(1).First().ApiMethodsToGenerate);

	}

}
