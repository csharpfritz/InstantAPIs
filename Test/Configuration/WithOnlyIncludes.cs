using Fritz.InstantAPIs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Configuration;

public class WithOnlyIncludes : BaseFixture
{

	InstantAPIsConfigBuilder<MyContext> _Builder;

	public WithOnlyIncludes()
	{

		var _ContextOptions = new DbContextOptionsBuilder<MyContext>()
		.UseInMemoryDatabase("TestDb")
		.Options;
		_Builder = new(new(_ContextOptions));

	}

	[Fact]
	public void ShouldNotIncludeAllTables()
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Contacts);
		var config = _Builder.Build();

		// assert
		Assert.Single(config.Tables);
		Assert.Equal("Contacts", config.Tables.First().Name);

	}

	[Theory]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Get)]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Insert)]
	[InlineData(ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Insert | ApiMethodsToGenerate.Update)]
	public void ShouldIncludeAndSetAPIMethodsToInclude(ApiMethodsToGenerate methodsToGenerate)
	{

		// arrange

		// act
		_Builder.IncludeTable(db => db.Contacts, methodsToGenerate);
		var config = _Builder.Build();

		// assert
		Assert.Equal(methodsToGenerate, config.Tables.First().ApiMethodsToGenerate);

	}

}
