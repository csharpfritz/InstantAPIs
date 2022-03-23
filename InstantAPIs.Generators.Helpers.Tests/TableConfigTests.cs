using Xunit;

namespace InstantAPIs.Generators.Helpers.Tests
{
	public static class TableConfigTests
	{
		[Fact]
		public static void Create()
		{
			var config = new TableConfig<Values>(Values.Three);

			Assert.Equal(Values.Three, config.Key);
			Assert.Equal("Three", config.Name);
			Assert.Equal(Included.Yes, config.Included);
			Assert.Equal(ApisToGenerate.All, config.APIs);
			Assert.Equal("/api/a/{id}", config.RouteDeleteById("a"));
			Assert.Equal("/api/a", config.RouteGet("a"));
			Assert.Equal("/api/a/{id}", config.RouteGetById("a"));
			Assert.Equal("/api/a", config.RoutePost("a"));
			Assert.Equal("/api/a/{id}", config.RoutePut("a"));
		}

		[Fact]
		public static void CreateWithCustomization()
		{
			var config = new TableConfig<Values>(Values.Three,
				Included.No, "a", ApisToGenerate.Get, 
				routeGet: value => $"get/{value}", 
				routeGetById: value => $"getById/{value}",
				routePost: value => $"post/{value}",
				routePut: value => $"put/{value}",
				routeDeleteById: value => $"delete/{value}");

			Assert.Equal(Values.Three, config.Key);
			Assert.Equal("a", config.Name);
			Assert.Equal(Included.No, config.Included);
			Assert.Equal(ApisToGenerate.Get, config.APIs);
			Assert.Equal("delete/a", config.RouteDeleteById("a"));
			Assert.Equal("get/a", config.RouteGet("a"));
			Assert.Equal("getById/a", config.RouteGetById("a"));
			Assert.Equal("post/a", config.RoutePost("a"));
			Assert.Equal("put/a", config.RoutePut("a"));
		}

		private enum Values
		{
			One, Two, Three
		}
	}
}
