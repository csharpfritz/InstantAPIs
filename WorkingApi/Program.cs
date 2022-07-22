using InstantAPIs;
using System.Diagnostics;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");
builder.Services.AddInstantAPIs();

var app = builder.Build();
app.MapInstantAPIs<MyContext>(builder =>
{
	builder.IncludeTable(db => db.Contacts, new InstantAPIsOptions.TableOptions<Contact, int>() { KeySelector = x => x.Id }, ApiMethodsToGenerate.All, "addressBook");
});

app.Run();
