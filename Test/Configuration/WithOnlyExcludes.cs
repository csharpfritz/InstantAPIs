using Xunit;

namespace Test.Configuration;

public class WithOnlyExcludes : InstantAPIsConfigBuilderFixture
{

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
