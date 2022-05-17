using InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using WorkingApi;

[assembly: InstantAPIsForDbContext(typeof(MyContext))]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MyContext>(
	options => options.UseInMemoryDatabase("Test"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	await SetupMyContextAsync(scope.ServiceProvider.GetService<MyContext>()!);
}

// This is the configured version.
/*
app.MapMyContextToAPIs(options =>
	options.Include(MyContextTables.Contacts, "Contacts", ApisToGenerate.Get)
		.Exclude(MyContextTables.Orders));
*/

// This is the simple "configure everything" version.
app.MapMyContextToAPIs();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();

static async Task SetupMyContextAsync(MyContext context)
{
	await context.Contacts.AddAsync(new Contact
	{
		Id = 1,
		Name = "Jason",
		Email = "jason@bock.com"
	});

	await context.Contacts.AddAsync(new Contact
	{
		Id = 2,
		Name = "Jeff",
		Email = "jeff@fritz.com"
	});

	await context.SaveChangesAsync();
}