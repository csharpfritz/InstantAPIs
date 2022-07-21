using InstantAPIs;
using Xunit;

namespace Test.Configuration;

public class WithoutIncludes : InstantAPIsConfigBuilderFixture
{
	[Fact]
	public void ShouldIncludeAllTables()
	{

		// arrange

		// act
		var config = _Builder.Build();

		// assert
		Assert.Equal(2, config.Count());
		Assert.Equal(ApiMethodsToGenerate.All, config.First().ApiMethodsToGenerate);
		Assert.Equal(ApiMethodsToGenerate.All, config.Skip(1).First().ApiMethodsToGenerate);

	}
}
