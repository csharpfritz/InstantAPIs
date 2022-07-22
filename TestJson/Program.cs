using InstantAPIs.Repositories.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInstantAPIs();
builder.Services.Configure<Context.Options>(x => x.JsonFilename = "mock.json");
builder.Services.AddSingleton<Context>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapInstantAPIs<Context>(builder =>
{
	builder
		.IncludeTable(context => context.LoadTable("products"), new InstantAPIs.InstantAPIsOptions.TableOptions<JsonObject, int>(), baseUrl: "api/someproducts");
});
//app.MapInstantAPIs<Context>();
app.Run();
