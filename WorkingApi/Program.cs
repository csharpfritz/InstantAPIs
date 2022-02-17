using Microsoft.EntityFrameworkCore;
using WorkingApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyContext>(options => options.UseSqlite("Data Source=contacts.db"));

var app = builder.Build();

app.MapInstantAPIs<MyContext>();

app.Run();
