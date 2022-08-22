using InstantAPIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
builder.Services.AddInstantAPIs();

var app = builder.Build();

var sw = Stopwatch.StartNew();

app.MapInstantAPIs<MyContext>(config =>
{
	config.IncludeTable(db => db.Contacts, ApiMethodsToGenerate.All, "addressBook");
	config.PrimaryKeyMappingConvention(new List<string>() { "{ClassName}Id", "{ClassName}_Id", });
});

app.Run();
