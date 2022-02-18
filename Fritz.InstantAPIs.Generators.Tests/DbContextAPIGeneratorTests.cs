using Microsoft.CodeAnalysis.Testing;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fritz.InstantAPIs.Generators.Tests
{
	public static class DbContextAPIGeneratorTests
	{
		[Fact]
		public static async Task GenerateWhenDbContextExists()
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
		public int Id { get; set; }
		public string? Name { get; set; }
	}
}";
			var generatedCode =
@"using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	}
}