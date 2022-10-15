using Xunit;

namespace Test.Configuration;

public class WithIncludesAndExcludes : InstantAPIsConfigBuilderFixture
{


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
