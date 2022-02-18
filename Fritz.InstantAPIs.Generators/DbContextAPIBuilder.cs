using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fritz.InstantAPIs.Generators
{
	public sealed class DbContextAPIBuilder
	{
		public DbContextAPIBuilder(INamedTypeSymbol type) =>
			Text = Build(type);

		private static SourceText? Build(INamedTypeSymbol type)
		{
			var tables = new List<TableData>();
			var isIdKeyAttribute = false;

			foreach(var property in type.GetMembers().OfType<IPropertySymbol>()
				.Where(_ => !_.IsStatic && _.DeclaredAccessibility == Accessibility.Public &&
					_.Type.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore.DbSet")))
			{
				var propertyType = (INamedTypeSymbol)property.Type;
				var propertySetType = (INamedTypeSymbol)propertyType.TypeArguments.First()!;

				// TODO: KeyAttribute alternative.
				var idProperty = propertySetType.GetMembers().OfType<IPropertySymbol>()
					.FirstOrDefault(_ => string.Equals(_.Name, "id", StringComparison.OrdinalIgnoreCase) &&
						!_.IsStatic && _.DeclaredAccessibility == Accessibility.Public);

				if (idProperty is null)
				{
					idProperty = propertySetType.GetMembers().OfType<IPropertySymbol>()
						.FirstOrDefault(_ => _.GetAttributes().Any(_ => _.AttributeClass!.Name == "Key" || _.AttributeClass.Name == "KeyAttribute"));
					isIdKeyAttribute = idProperty is not null;
				}

				tables.Add(new TableData(property.Name, propertySetType, idProperty?.Type as INamedTypeSymbol));
			}

			if(tables.Count > 0)
			{
				using var writer = new StringWriter();
				using var indentWriter = new IndentedTextWriter(writer, "\t");

				var namespaces = new NamespaceGatherer();
				namespaces.Add("System");
				namespaces.Add("Microsoft.EntityFrameworkCore");
				namespaces.Add("Microsoft.AspNetCore.Builder");

				if (!type.ContainingNamespace.IsGlobalNamespace)
				{
					indentWriter.WriteLine($"namespace {type.ContainingNamespace.ToDisplayString()}");
					indentWriter.WriteLine("{");
					indentWriter.Indent++;
				}

				indentWriter.WriteLine("public static partial class WebApplicationExtensions");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;

				namespaces.Add("Microsoft.AspNetCore.Builder");
				indentWriter.WriteLine($"public static WebApplication Map{type.Name}ToAPIs(this WebApplication app)");

				indentWriter.WriteLine("{");
				indentWriter.Indent++;

				foreach(var table in tables)
				{
					if(!table.PropertyType.ContainingNamespace.Equals(type.ContainingNamespace, SymbolEqualityComparer.Default))
					{
						namespaces.Add(table.PropertyType.ContainingNamespace);
					}

					namespaces.Add("Microsoft.AspNetCore.Mvc");

					indentWriter.WriteLine($"app.MapGet(\"/api/{table.Name}\", ([FromServices] {type.Name} db) =>");
					indentWriter.Indent++;
					indentWriter.WriteLine($"db.Set<{table.PropertyType.Name}>());");
					indentWriter.Indent--;

					if(table.IdType is not null)
					{
						indentWriter.WriteLine();
						indentWriter.WriteLine($"app.MapGet(\"/api/{table.Name}/{{id}}\", async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
						indentWriter.Indent++;

						var idValue = "id";

						if(table.IdType.SpecialType == SpecialType.System_Int32)
						{
							idValue = "int.Parse(id)";
						}
						else if (table.IdType.SpecialType == SpecialType.System_Int64)
						{
							idValue = "long.Parse(id)";
						}
						// TODO: This is not ideal for identifying a Guid...I think...
						if (table.IdType.ToDisplayString() == "System.Guid")
						{
							idValue = "Guid.Parse(id)";
						}

						indentWriter.WriteLine($"await db.Set<{table.PropertyType.Name}>().FindAsync({idValue}));");
						indentWriter.Indent--;
					}
				}

				indentWriter.WriteLine();
				indentWriter.WriteLine("return app;");
				indentWriter.Indent--;
				indentWriter.WriteLine("}");

				indentWriter.Indent--;
				indentWriter.WriteLine("}");

				if (!type.ContainingNamespace.IsGlobalNamespace)
				{
					indentWriter.Indent--;
					indentWriter.WriteLine("}");
				}

				var code = namespaces.Values.Count > 0 ?
					string.Join(Environment.NewLine,
						string.Join(Environment.NewLine, namespaces.Values.Select(_ => $"using {_};")),
						string.Empty, "#nullable enable", string.Empty, writer.ToString()) :
					string.Join(Environment.NewLine, "#nullable enable", string.Empty, writer.ToString());

				return SourceText.From(code, Encoding.UTF8);
			}

			return null;
		}

		public SourceText? Text { get; private set; }
	}
}
