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
$@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

#nullable enable

namespace MyApplication
{{
	public static partial class WebApplicationExtensions
	{{
		public static WebApplication MapCustomerContextToAPIs(this WebApplication app)
		{{
			app.MapGet(""/api/Contacts"", ([FromServices] CustomerContext db) =>
				db.Set<Contact>());
			
			app.MapGet(""/api/Contacts/{{id}}"", async ([FromServices] CustomerContext db, [FromRoute] string id) =>
				await db.Set<Contact>().FindAsync({idParseMethod}));
			
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
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

#nullable enable

namespace MyApplication
{
	public static partial class WebApplicationExtensions
	{
		public static WebApplication MapCustomerContextToAPIs(this WebApplication app)
		{
			app.MapGet(""/api/Contacts"", ([FromServices] CustomerContext db) =>
				db.Set<Contact>());
			
			return app;
		}
	}
}
";

			var personGeneratedCode =
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

#nullable enable

namespace MyApplication
{
	public static partial class WebApplicationExtensions
	{
		public static WebApplication MapPersonContextToAPIs(this WebApplication app)
		{
			app.MapGet(""/api/Contacts"", ([FromServices] PersonContext db) =>
				db.Set<Contact>());
			
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
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

#nullable enable

namespace MyApplication
{
	public static partial class WebApplicationExtensions
	{
		public static WebApplication MapCustomerContextToAPIs(this WebApplication app)
		{
			app.MapGet(""/api/Contacts"", ([FromServices] CustomerContext db) =>
				db.Set<Contact>());
			
			app.MapGet(""/api/Contacts/{id}"", async ([FromServices] CustomerContext db, [FromRoute] string id) =>
				await db.Set<Contact>().FindAsync(int.Parse(id)));
			
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
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyTableTypes;
using System;

#nullable enable

namespace MyApplication
{
	public static partial class WebApplicationExtensions
	{
		public static WebApplication MapCustomerContextToAPIs(this WebApplication app)
		{
			app.MapGet(""/api/Contacts"", ([FromServices] CustomerContext db) =>
				db.Set<Contact>());
			
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
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

#nullable enable

namespace MyApplication
{
	public static partial class WebApplicationExtensions
	{
		public static WebApplication MapCustomerContextToAPIs(this WebApplication app)
		{
			app.MapGet(""/api/Contacts"", ([FromServices] CustomerContext db) =>
				db.Set<Contact>());
			
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