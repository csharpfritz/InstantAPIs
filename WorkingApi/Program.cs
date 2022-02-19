using Microsoft.EntityFrameworkCore;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapInstantAPIs<MyContext>(config =>
{
	// Potential new config API
	// config.Include(ctx => ctx.Contacts)
	//	 .GenerateMethods(ApiMethodsToGenerate.All);

});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
