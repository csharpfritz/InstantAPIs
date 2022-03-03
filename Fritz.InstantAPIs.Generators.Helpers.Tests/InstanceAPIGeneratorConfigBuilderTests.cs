using System;
using Xunit;

namespace Fritz.InstantAPIs.Generators.Helpers.Tests;

public static class InstanceAPIGeneratorConfigBuilderTests
{
	[Fact]
	public static void BuildWithNoConfiguration()
	{
		var builder = new InstanceAPIGeneratorConfigBuilder<Values>();
		var config = builder.Build();

		foreach (var key in Enum.GetValues<Values>())
		{
			var tableConfig = config[key];

			Assert.Equal(key, tableConfig.Key);
			Assert.Equal(key.ToString(), tableConfig.Name);
			Assert.Equal(Included.Yes, tableConfig.Included);
			Assert.Equal(ApisToGenerate.All, tableConfig.APIs);
			Assert.Equal("/api/a/{id}", tableConfig.RouteDeleteById("a"));
			Assert.Equal("/api/a", tableConfig.RouteGet("a"));
			Assert.Equal("/api/a/{id}", tableConfig.RouteGetById("a"));
			Assert.Equal("/api/a", tableConfig.RoutePost("a"));
			Assert.Equal("/api/a/{id}", tableConfig.RoutePut("a"));
		}
	}

	[Fact]
	public static void BuildWithCustomInclude()
	{
		var builder = new InstanceAPIGeneratorConfigBuilder<Values>();
		builder.Include(Values.Two, "a", ApisToGenerate.Get,
			routeGet: value => $"get/{value}",
			routeGetById: value => $"getById/{value}",
			routePost: value => $"post/{value}",
			routePut: value => $"put/{value}",
			routeDeleteById: value => $"delete/{value}");
		var config = builder.Build();

		foreach (var key in Enum.GetValues<Values>())
		{
			var tableConfig = config[key];

			if (key != Values.Two)
			{
				Assert.Equal(key, tableConfig.Key);
				Assert.Equal(key.ToString(), tableConfig.Name);
				Assert.Equal(Included.Yes, tableConfig.Included);
				Assert.Equal(ApisToGenerate.All, tableConfig.APIs);
				Assert.Equal("/api/a/{id}", tableConfig.RouteDeleteById("a"));
				Assert.Equal("/api/a", tableConfig.RouteGet("a"));
				Assert.Equal("/api/a/{id}", tableConfig.RouteGetById("a"));
				Assert.Equal("/api/a", tableConfig.RoutePost("a"));
				Assert.Equal("/api/a/{id}", tableConfig.RoutePut("a"));
			}
			else
			{
				Assert.Equal(Values.Two, tableConfig.Key);
				Assert.Equal("a", tableConfig.Name);
				Assert.Equal(Included.Yes, tableConfig.Included);
				Assert.Equal(ApisToGenerate.Get, tableConfig.APIs);
				Assert.Equal("delete/a", tableConfig.RouteDeleteById("a"));
				Assert.Equal("get/a", tableConfig.RouteGet("a"));
				Assert.Equal("getById/a", tableConfig.RouteGetById("a"));
				Assert.Equal("post/a", tableConfig.RoutePost("a"));
				Assert.Equal("put/a", tableConfig.RoutePut("a"));
			}
		}
	}

	[Fact]
	public static void BuildWithCustomExclude()
	{
		var builder = new InstanceAPIGeneratorConfigBuilder<Values>();
		builder.Exclude(Values.Two);
		var config = builder.Build();

		foreach (var key in Enum.GetValues<Values>())
		{
			var tableConfig = config[key];

			Assert.Equal(key, tableConfig.Key);
			Assert.Equal(key.ToString(), tableConfig.Name);
			Assert.Equal(key != Values.Two ? Included.Yes : Included.No, tableConfig.Included);
			Assert.Equal(ApisToGenerate.All, tableConfig.APIs);
			Assert.Equal("/api/a/{id}", tableConfig.RouteDeleteById("a"));
			Assert.Equal("/api/a", tableConfig.RouteGet("a"));
			Assert.Equal("/api/a/{id}", tableConfig.RouteGetById("a"));
			Assert.Equal("/api/a", tableConfig.RoutePost("a"));
			Assert.Equal("/api/a/{id}", tableConfig.RoutePut("a"));
		}
	}

	private enum Values
	{
		One, Two, Three
	}
}
