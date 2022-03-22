using Fritz.InstantAPIs.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fritz.InstantAPIs.Generators.Tests;

public static class DbContextAPIGeneratorTests
{
	[Theory]
	[InlineData("int", "int.Parse(id)")]
	[InlineData("long", "long.Parse(id)")]
	[InlineData("Guid", "Guid.Parse(id)")]
	[InlineData("string", "id")]
	public static async Task GenerateWhenDbContextExists(string idType, string idParseMethod)
	{
		var code =
$@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;
using System;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]

namespace MyApplication
{{
	public class CustomerContext : DbContext
	{{
		public DbSet<Contact> Contacts => Set<Contact>();
	}}

	public class Contact
	{{
		public {idType} Id {{ get; set; }}
		public string? Name {{ get; set; }}
	}}
}}";
		var generatedCode =
$@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{{
	public enum CustomerContextTables
	{{
		Contacts
	}}
	
	public static partial class IEndpointRouteBuilderExtensions
	{{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) {{ options(builder); }}
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{{url}}"");
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.GetById))
				{{
					var url = tableContacts.RouteGetById.Invoke(tableContacts.Name);
					app.MapGet(url, async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{{
						var outValue = await db.Contacts.FindAsync({idParseMethod});
						if (outValue is null) {{ return Results.NotFound(); }}
						return Results.Ok(outValue);
					}});
					
					logger.LogInformation($""Created API: HTTP GET\t{{url}}"");
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Insert))
				{{
					var url = tableContacts.RoutePost.Invoke(tableContacts.Name);
					app.MapPost(url, async ([FromServices] CustomerContext db, [FromBody] Contact newObj) =>
					{{
						db.Add(newObj);
						await db.SaveChangesAsync();
						var id = newObj.Id;
						return Results.Created($""{{url}}/{{id}}"", newObj);
					}});
					
					logger.LogInformation($""Created API: HTTP POST\t{{url}}"");
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					}});
					
					logger.LogInformation($""Created API: HTTP PUT\t{{url}}"");
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Delete))
				{{
					var url = tableContacts.RouteDeleteById.Invoke(tableContacts.Name);
					app.MapDelete(url, async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{{
						Contact? obj = await db.Contacts.FindAsync({idParseMethod});
						
						if (obj is null) {{ return Results.NotFound(); }}
						
						db.Contacts.Remove(obj);
						await db.SaveChangesAsync();
						return Results.NoContent();
					}});
					
					logger.LogInformation($""Created API: HTTP DELETE\t{{url}}"");
				}}
			}}
			
			return app;
		}}
	}}
}}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenMultipleDbContextsExists()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;
using System;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]
[assembly: InstantAPIsForDbContext(typeof(PersonContext))]

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}

	public class PersonContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}

	public class Contact
	{
		public string? Name { get; set; }
	}
}";
		var customerGeneratedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum CustomerContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		var personGeneratedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum PersonContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapPersonContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<PersonContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<PersonContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[PersonContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] PersonContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] PersonContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] 
			{ 
				(typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", customerGeneratedCode),
				(typeof(DbContextAPIGenerator), "PersonContext_DbContextAPIGenerator.g.cs", personGeneratedCode)
			},
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenIdentifierUsesKeyAttribute()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;
using System;
using System.ComponentModel.DataAnnotations;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}

	public class Contact
	{
		[Key]
		public int Unique { get; set; }
		public string? Name { get; set; }
	}
}";
		var generatedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum CustomerContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.GetById))
				{
					var url = tableContacts.RouteGetById.Invoke(tableContacts.Name);
					app.MapGet(url, async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{
						var outValue = await db.Contacts.FindAsync(int.Parse(id));
						if (outValue is null) { return Results.NotFound(); }
						return Results.Ok(outValue);
					});
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Insert))
				{
					var url = tableContacts.RoutePost.Invoke(tableContacts.Name);
					app.MapPost(url, async ([FromServices] CustomerContext db, [FromBody] Contact newObj) =>
					{
						db.Add(newObj);
						await db.SaveChangesAsync();
						var id = newObj.Unique;
						return Results.Created($""{url}/{id}"", newObj);
					});
					
					logger.LogInformation($""Created API: HTTP POST\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Delete))
				{
					var url = tableContacts.RouteDeleteById.Invoke(tableContacts.Name);
					app.MapDelete(url, async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{
						Contact? obj = await db.Contacts.FindAsync(int.Parse(id));
						
						if (obj is null) { return Results.NotFound(); }
						
						db.Contacts.Remove(obj);
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP DELETE\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenTableTypeNamespaceIsDifferentThanDbContextNamespace()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;
using MyTableTypes;
using System;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}
}

namespace MyTableTypes
{
	public class Contact
	{
		public string? Name { get; set; }
	}
}";
		var generatedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MyTableTypes;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum CustomerContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenDbContextIsNotMarkedByAttribute()
	{
		var code =
@"using Microsoft.EntityFrameworkCore;

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts { get; set; }
	}

	public class Contact
	{
		public string? Name { get; set; }
	}
}";

		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenTypeGivenInAttributeIsNotDbContext()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;

[assembly: InstantAPIsForDbContext(typeof(string))]";

		var diagnostic = new DiagnosticResult(NotADbContextDiagnostic.Id, DiagnosticSeverity.Error)
			.WithSpan(3, 12, 3, 51); 
		
		await TestAssistants.RunAsync(code,
			Enumerable.Empty<(Type, string, string)>(),
			new[] { diagnostic }).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenDbContextExistsAndDoesNotHaveIdProperty()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}

	public class Contact
	{
		public string? Name { get; set; }
	}
}";
		var generatedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum CustomerContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		await TestAssistants.RunAsync(code,
			new[] { (typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", generatedCode) },
			Enumerable.Empty<DiagnosticResult>()).ConfigureAwait(false);
	}

	[Fact]
	public static async Task GenerateWhenMultipleAttributeDefinitionsExist()
	{
		var code =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.EntityFrameworkCore;
using MyApplication;

[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]
[assembly: InstantAPIsForDbContext(typeof(CustomerContext))]

namespace MyApplication
{
	public class CustomerContext : DbContext
	{
		public DbSet<Contact> Contacts => Set<Contact>();
	}

	public class Contact
	{
		public string? Name { get; set; }
	}
}";
		var generatedCode =
@"using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

#nullable enable

namespace MyApplication
{
	public enum CustomerContextTables
	{
		Contacts
	}
	
	public static partial class IEndpointRouteBuilderExtensions
	{
		public static IEndpointRouteBuilder MapCustomerContextToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<CustomerContextTables>>? options = null)
		{
			ILogger logger = NullLogger.Instance;
			if (app.ServiceProvider is not null)
			{
				var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();
				logger = loggerFactory.CreateLogger(""InstantAPIs"");
			}
			
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					var url = tableContacts.RouteGet.Invoke(tableContacts.Name);
					app.MapGet(url, ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
					
					logger.LogInformation($""Created API: HTTP GET\t{url}"");
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					var url = tableContacts.RoutePut.Invoke(tableContacts.Name);
					app.MapPut(url, async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
					
					logger.LogInformation($""Created API: HTTP PUT\t{url}"");
				}
			}
			
			return app;
		}
	}
}
";

		var diagnostic = new DiagnosticResult(DuplicateDefinitionDiagnostic.Id, DiagnosticSeverity.Warning)
			.WithSpan(6, 12, 6, 60);
		await TestAssistants.RunAsync(code,
			new[] { (typeof(DbContextAPIGenerator), "CustomerContext_DbContextAPIGenerator.g.cs", generatedCode) },
			new[] { diagnostic }).ConfigureAwait(false);
	}
}
