using Fritz.InstantAPIs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddInstantAPIs(options =>
{
    options.EnableSwagger = Fritz.InstantAPIs.EnableSwagger.DevelopmentOnly;
    options.Swagger = config =>
    {
        config.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "My Working API",
            Description = "An ASP.NET Core Web API"
        });
    };
});

var app = builder.Build();

var sw = Stopwatch.StartNew();

app.MapInstantAPIs<MyContext>(options =>
{
	options.IncludeTable(db => db.Contacts, (ApiMethodsToGenerate.GetById | ApiMethodsToGenerate.Get));
});
Console.WriteLine($"Elapsed time to build InstantAPIs: {sw.Elapsed}");

//app.UseSwagger();
//app.UseSwaggerUI();

app.Run();
