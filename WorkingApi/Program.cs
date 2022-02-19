using Microsoft.EntityFrameworkCore;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");

var app = builder.Build();

app.MapInstantAPIs<MyContext>(options =>
{
	options.IncludeTable(db => db.Contacts);
});

app.Run();
