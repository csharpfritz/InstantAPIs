using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Test.Configuration;

public abstract class InstantAPIsConfigBuilderFixture : BaseFixture
{
	internal InstantAPIsBuilder<MyContext> _Builder;

	public InstantAPIsConfigBuilderFixture()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
			.UseInMemoryDatabase("TestDb")
			.Options;
		_Builder = new(new(_ContextOptions));

	}
}
