using InstantAPIs;
using Xunit;

namespace Test.Configuration;

public class WithIncludesAndExcludes : InstantAPIsConfigBuilderFixture
{


	[Fact]
	public void ShouldExcludePreviouslyIncludedTable()
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Addresses, new InstantAPIsOptions.TableOptions<Address, int>())
						.IncludeTable(db => db.Contacts, new InstantAPIsOptions.TableOptions<Contact, int>())
						.ExcludeTable(db => db.Addresses);
		var config = _Builder.Build();

		// assert
		Assert.Single(config);
		Assert.Equal("Contacts", config.First().Name);

	}

}
