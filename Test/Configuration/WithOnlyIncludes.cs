using InstantAPIs;
using Xunit;

namespace Test.Configuration;

public class WithOnlyIncludes : InstantAPIsConfigBuilderFixture
{

	[Fact]
	public void ShouldNotIncludeAllTables()
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Contacts, new InstantAPIsOptions.TableOptions<Contact, int>());
		var config = _Builder.Build();

		// assert
		Assert.Single(config);
		Assert.Equal("Contacts", config.First().Name);

	}

	[Theory]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Get)]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Insert)]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Insert | ApiMethodsToGenerate.Update)]
	public void ShouldIncludeAndSetAPIMethodsToInclude(ApiMethodsToGenerate methodsToGenerate)
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Contacts, new InstantAPIsOptions.TableOptions<Contact, int>(), methodsToGenerate);
		var config = _Builder.Build();

		// assert
		Assert.Equal(methodsToGenerate, config.First().ApiMethodsToGenerate);

	}

}
