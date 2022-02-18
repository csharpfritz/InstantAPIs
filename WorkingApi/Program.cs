using Microsoft.EntityFrameworkCore;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");

var app = builder.Build();

/*
app.MapInstantAPIs<MyContext>(config =>
{
	// Potential new config API
	// config.Include(ctx => ctx.Contacts)
	//	 .GenerateMethods(ApiMethodsToGenerate.All);

});
*/

app.MapMyContextToAPIs();
app.Run();
