using Xunit;

namespace Test.Configuration;

public class WhenIncludeSpecifiesBaseUrl : InstantAPIsConfigBuilderFixture
{

	[Fact]
	public void ShouldSpecifyThatUrl()
	{

		// arrange

		// act
		var BaseUrl = new Uri("/testapi", UriKind.Relative);
		_Builder.IncludeTable(db => db.Contacts, baseUrl: BaseUrl.ToString());
		var config = _Builder.Build();

		// assert
		Assert.Single(config);
		Assert.Equal(BaseUrl, config.First().BaseUrl);

	}
}
