using Microsoft.EntityFrameworkCore;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapInstantAPIs<MyContext>(options =>
{
	options.IncludeTable(db => db.Contacts);
});

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
