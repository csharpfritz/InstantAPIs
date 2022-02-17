# InstantAPIs
A proof-of-concept library that generates Minimal API endpoints for an Entity Framework context.  

[![Nuget](https://img.shields.io/nuget/v/Fritz.InstantAPIs)](https://www.nuget.org/packages/Fritz.InstantAPIs/)
![GitHub branch checks state](https://img.shields.io/github/checks-status/csharpfritz/InstantAPIs/main)
![GitHub last commit](https://img.shields.io/github/last-commit/csharpfritz/InstantAPIs)
![GitHub contributors](https://img.shields.io/github/contributors/csharpfritz/InstantAPIs)


For a given Entity Framework context, MyContext

```csharp
public class MyContext : DbContext 
{
    public MyContext(DbContextOptions<MyContext> options) : base(options) {}

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Address> Addresses => Set<Address>();

}
```

We can generate all of the standard CRUD API endpoints using this syntax in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<MyContext>("Data Source=contacts.db");

var app = builder.Build();

app.MapInstantAPIs<MyContext>();

app.Run();
```

Now we can navigate to `/api/Contacts` and see all of the Contacts in the database.  We can filter for a specific Contact by navigating to `/api/Contacts/1` to get just the first contact returned.  We can also post to `/api/Contacts` and add a new Contact to the database. Since there are multiple `DbSet`, you can make the same calls to `/api/Addresses`.

TODO:

- Add OpenAPI bindings
- GraphQL?
- Add gRPC bindings
- Authorization?
