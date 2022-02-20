using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Fritz.InstantAPIs.Generators.Builders
{
	internal static class WebApplicationExtensionsBuilder
	{
		internal static void Build(IndentedTextWriter indentWriter, INamedTypeSymbol type, List<TableData> tables,
			NamespaceGatherer namespaces)
		{
			indentWriter.WriteLine("public static partial class WebApplicationExtensions");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"public static WebApplication Map{type.Name}ToAPIs(this WebApplication app, InstanceAPIGeneratorConfig<{type.Name}Tables>? config = null)");

			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"if (config is null) {{ config = new InstanceAPIGeneratorConfig<{type.Name}Tables>(); }}");
			indentWriter.WriteLine();

			foreach (var table in tables)
			{
				if (!table.PropertyType.ContainingNamespace.Equals(type.ContainingNamespace, SymbolEqualityComparer.Default))
				{
					namespaces.Add(table.PropertyType.ContainingNamespace);
				}

				var tableVariableName = $"table{table.Name}";

				indentWriter.WriteLine($"var {tableVariableName} = config[{type.Name}Tables.{table.Name}];");
				indentWriter.WriteLine();

				BuildGet(indentWriter, type, table, tableVariableName);
				indentWriter.WriteLine();

				if (table.IdType is not null)
				{
					BuildGetById(indentWriter, type, table, tableVariableName);
					indentWriter.WriteLine();
				}

				BuildPost(indentWriter, type, table, tableVariableName);
				indentWriter.WriteLine();
				BuildPut(indentWriter, type, table, tableVariableName);

				if (table.IdType is not null)
				{
					indentWriter.WriteLine();
					BuildDeleteById(indentWriter, type, table, tableVariableName);
				}
			}

			indentWriter.WriteLine();
			indentWriter.WriteLine("return app;");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildGet(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Get))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine($"app.MapGet({tableVariableName}.RouteGet.Invoke({tableVariableName}.Name), ([FromServices] {type.Name} db) =>");
			indentWriter.Indent++;
			indentWriter.WriteLine($"db.Set<{table.PropertyType.Name}>());");
			indentWriter.Indent--;
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildGetById(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.GetById))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine($"app.MapGet({tableVariableName}.RouteGetById.Invoke({tableVariableName}.Name), async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
			indentWriter.Indent++;
			indentWriter.WriteLine($"await db.Set<{table.PropertyType.Name}>().FindAsync({GetIdParseCode(table.IdType!)}));");
			indentWriter.Indent--;
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildPost(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Insert))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine($"app.MapPost({tableVariableName}.RoutePost.Invoke({tableVariableName}.Name), async ([FromServices] {type.Name} db, [FromBody] {table.PropertyType.Name} newObj) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine("db.Add(newObj);");
			indentWriter.WriteLine("await db.SaveChangesAsync();");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildPut(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Update))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine($"app.MapPut({tableVariableName}.RoutePut.Invoke({tableVariableName}.Name), async ([FromServices] {type.Name} db, [FromRoute] string id, [FromBody] {table.PropertyType.Name} newObj) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"db.Set<{table.PropertyType.Name}>().Attach(newObj);");
			indentWriter.WriteLine("db.Entry(newObj).State = EntityState.Modified;");
			indentWriter.WriteLine("await db.SaveChangesAsync();");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildDeleteById(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Delete))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine($"app.MapDelete({tableVariableName}.RouteDeleteById.Invoke({tableVariableName}.Name), async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var set = db.Set<{table.PropertyType.Name}>();");
			indentWriter.WriteLine($"{table.PropertyType.Name}? obj = await set.FindAsync({GetIdParseCode(table.IdType!)});");
			indentWriter.WriteLine();
			indentWriter.WriteLine("if (obj == null) return;");
			indentWriter.WriteLine();
			indentWriter.WriteLine($"db.Set<{table.PropertyType.Name}>().Remove(obj);");
			indentWriter.WriteLine("await db.SaveChangesAsync();");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static string GetIdParseCode(INamedTypeSymbol tableType)
		{
			var idValue = "id";

			if (tableType.SpecialType == SpecialType.System_Int32)
			{
				idValue = "int.Parse(id)";
			}
			else if (tableType.SpecialType == SpecialType.System_Int64)
			{
				idValue = "long.Parse(id)";
			}
			// TODO: This is not ideal for identifying a Guid...I think...
			else if (tableType.ToDisplayString() == "System.Guid")
			{
				idValue = "Guid.Parse(id)";
			}

			return idValue;
		}
	}
}