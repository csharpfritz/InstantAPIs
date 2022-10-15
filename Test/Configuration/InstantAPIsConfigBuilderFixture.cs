using InstantAPIs;
using Microsoft.EntityFrameworkCore;

namespace Test.Configuration;

public abstract class InstantAPIsConfigBuilderFixture : BaseFixture
{
	internal InstantAPIsConfigBuilder<MyContext> _Builder;

	public InstantAPIsConfigBuilderFixture()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
			.UseInMemoryDatabase("TestDb")
			.Options;
		_Builder = new(new(_ContextOptions));

	}
}
