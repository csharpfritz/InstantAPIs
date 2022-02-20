using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using WorkingApi;

//var context = new MyContext(new DbContextOptions<MyContext>());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// MapInstantAPIs is the Reflection-based approach.
// MapMyContextToAPIs is the source generator approach.
// Pick one or the other, but not both at the same time.

/*
app.MapInstantAPIs<MyContext>(config =>
{
	// Potential new config API
	// config.Include(ctx => ctx.Contacts)
	//	 .GenerateMethods(ApiMethodsToGenerate.All);

});
*/

app.UseSwagger();
app.UseSwaggerUI();

// If you want to play with customization,
// uncomment the lines that create MyContextInstanceAPIGeneratorConfig
// and change values, and then pass config into MapMyContextToAPIs.

//var config = new MyContextInstanceAPIGeneratorConfig();
//config[MyContextTables.Contacts].APIs = ApisToGenerate.GetById;
//config[MyContextTables.Contacts].RouteById = name => $"/api/{name}/custom/{{id}}";

app.MapMyContextToAPIs();
app.Run();