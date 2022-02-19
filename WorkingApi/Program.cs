using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using WorkingApi;

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

app.MapMyContextToAPIs();
app.Run();