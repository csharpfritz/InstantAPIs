using Microsoft.CodeAnalysis.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fritz.InstantAPIs.Generators.Tests
{
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
$@"using Microsoft.EntityFrameworkCore;
using System;

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
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) {{ options(builder); }}
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.GetById))
				{{
					app.MapGet(tableContacts.RouteGetById.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{{
						var outValue = await db.Contacts.FindAsync({idParseMethod});
						if (outValue is null) {{ return Results.NotFound(); }}
						return Results.Ok(outValue);
					}});
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
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					}});
				}}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Delete))
				{{
					app.MapDelete(tableContacts.RouteDeleteById.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{{
						Contact? obj = await db.Contacts.FindAsync({idParseMethod});
						
						if (obj == null) {{ return Results.NotFound(); }}
						
						db.Contacts.Remove(obj);
						await db.SaveChangesAsync();
						return Results.NoContent();
					}});
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
@"using Microsoft.EntityFrameworkCore;
using System;

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
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
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
			var builder = new InstanceAPIGeneratorConfigBuilder<PersonContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[PersonContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] PersonContext db) =>
						Results.Ok(db.Contacts));
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] PersonContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
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
@"using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

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
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.GetById))
				{
					app.MapGet(tableContacts.RouteGetById.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{
						var outValue = await db.Contacts.FindAsync(int.Parse(id));
						if (outValue is null) { return Results.NotFound(); }
						return Results.Ok(outValue);
					});
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
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Delete))
				{
					app.MapDelete(tableContacts.RouteDeleteById.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id) =>
					{
						Contact? obj = await db.Contacts.FindAsync(int.Parse(id));
						
						if (obj == null) { return Results.NotFound(); }
						
						db.Contacts.Remove(obj);
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
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
@"using Microsoft.EntityFrameworkCore;
using MyTableTypes;
using System;

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
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
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
		public static async Task GenerateWhenDbContextDoesNotExist()
		{
			var code =
@"using Microsoft.EntityFrameworkCore;

namespace MyApplication
{
	public class CustomerContext
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
		public static async Task GenerateWhenDbContextExistsAndDoesNotHaveIdProperty()
		{
			var code =
@"using Microsoft.EntityFrameworkCore;

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
			var builder = new InstanceAPIGeneratorConfigBuilder<CustomerContextTables>();
			if (options is not null) { options(builder); }
			var config = builder.Build();
			
			var tableContacts = config[CustomerContextTables.Contacts];
			
			if (tableContacts.Included == Included.Yes)
			{
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Get))
				{
					app.MapGet(tableContacts.RouteGet.Invoke(tableContacts.Name), ([FromServices] CustomerContext db) =>
						Results.Ok(db.Contacts));
				}
				
				if (tableContacts.APIs.HasFlag(ApisToGenerate.Update))
				{
					app.MapPut(tableContacts.RoutePut.Invoke(tableContacts.Name), async ([FromServices] CustomerContext db, [FromRoute] string id, [FromBody] Contact newObj) =>
					{
						db.Contacts.Attach(newObj);
						db.Entry(newObj).State = EntityState.Modified;
						await db.SaveChangesAsync();
						return Results.NoContent();
					});
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
	}
}