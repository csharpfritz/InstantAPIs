using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Fritz.InstantAPIs.Generators.Builders
{
	internal static class IEndpointRouteBuilderExtensionsBuilder
	{
		internal static void Build(IndentedTextWriter indentWriter, INamedTypeSymbol type, List<TableData> tables,
			NamespaceGatherer namespaces)
		{
			indentWriter.WriteLine("public static partial class IEndpointRouteBuilderExtensions");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"public static IEndpointRouteBuilder Map{type.Name}ToAPIs(this IEndpointRouteBuilder app, Action<InstanceAPIGeneratorConfigBuilder<{type.Name}Tables>>? options = null)");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine("ILogger logger = NullLogger.Instance;");
			indentWriter.WriteLine("if (app.ServiceProvider is not null)");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine("var loggerFactory = app.ServiceProvider.GetRequiredService<ILoggerFactory>();");
			indentWriter.WriteLine("logger = loggerFactory.CreateLogger(\"InstantAPIs\");");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
			indentWriter.WriteLine();

			indentWriter.WriteLine($"var builder = new InstanceAPIGeneratorConfigBuilder<{type.Name}Tables>();");
			indentWriter.WriteLine("if (options is not null) { options(builder); }");
			indentWriter.WriteLine("var config = builder.Build();");
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
				indentWriter.WriteLine($"if ({tableVariableName}.Included == Included.Yes)");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;

				BuildGet(indentWriter, type, table, tableVariableName);
				indentWriter.WriteLine();

				if (table.IdType is not null)
				{
					BuildGetById(indentWriter, type, table, tableVariableName);
					indentWriter.WriteLine();
					BuildPost(indentWriter, type, table, tableVariableName);
					indentWriter.WriteLine();
				}

				BuildPut(indentWriter, type, table, tableVariableName);

				if (table.IdType is not null)
				{
					indentWriter.WriteLine();
					BuildDeleteById(indentWriter, type, table, tableVariableName);
				}

				indentWriter.Indent--;
				indentWriter.WriteLine("}");
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

			indentWriter.WriteLine($"var url = {tableVariableName}.RouteGet.Invoke({tableVariableName}.Name);");
			indentWriter.WriteLine($"app.MapGet(url, ([FromServices] {type.Name} db) =>");
			indentWriter.Indent++;
			indentWriter.WriteLine($"Results.Ok(db.{table.Name}));");
			indentWriter.Indent--;
			indentWriter.WriteLine();
			indentWriter.WriteLine("logger.LogInformation($\"Created API: HTTP GET\\t{url}\");");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildGetById(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.GetById))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var url = {tableVariableName}.RouteGetById.Invoke({tableVariableName}.Name);");
			indentWriter.WriteLine($"app.MapGet(url, async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var outValue = await db.{table.Name}.FindAsync({GetIdParseCode(table.IdType!)});");
			indentWriter.WriteLine("if (outValue is null) { return Results.NotFound(); }");
			indentWriter.WriteLine("return Results.Ok(outValue);");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");

			indentWriter.WriteLine();
			indentWriter.WriteLine("logger.LogInformation($\"Created API: HTTP GET\\t{url}\");");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildPost(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Insert))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var url = {tableVariableName}.RoutePost.Invoke({tableVariableName}.Name);");
			indentWriter.WriteLine($"app.MapPost(url, async ([FromServices] {type.Name} db, [FromBody] {table.PropertyType.Name} newObj) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine("db.Add(newObj);");
			indentWriter.WriteLine("await db.SaveChangesAsync();");
			indentWriter.WriteLine($"var id = newObj.{table.IdName!};");
			// TODO: We're assuming that the "created" route is the same as POST/id,
			// and this may not be true.
			indentWriter.WriteLine($"return Results.Created($\"{{url}}/{{id}}\", newObj);");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");

			indentWriter.WriteLine();
			indentWriter.WriteLine("logger.LogInformation($\"Created API: HTTP POST\\t{url}\");");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildPut(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Update))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var url = {tableVariableName}.RoutePut.Invoke({tableVariableName}.Name);");
			indentWriter.WriteLine($"app.MapPut(url, async ([FromServices] {type.Name} db, [FromRoute] string id, [FromBody] {table.PropertyType.Name} newObj) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"db.{table.Name}.Attach(newObj);");
			indentWriter.WriteLine("db.Entry(newObj).State = EntityState.Modified;");
			indentWriter.WriteLine("await db.SaveChangesAsync();");
			indentWriter.WriteLine("return Results.NoContent();");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");

			indentWriter.WriteLine();
			indentWriter.WriteLine("logger.LogInformation($\"Created API: HTTP PUT\\t{url}\");");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

		private static void BuildDeleteById(IndentedTextWriter indentWriter, INamedTypeSymbol type, TableData table, string tableVariableName)
		{
			indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Delete))");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"var url = {tableVariableName}.RouteDeleteById.Invoke({tableVariableName}.Name);");
			indentWriter.WriteLine($"app.MapDelete(url, async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"{table.PropertyType.Name}? obj = await db.{table.Name}.FindAsync({GetIdParseCode(table.IdType!)});");
			indentWriter.WriteLine();
			indentWriter.WriteLine("if (obj is null) { return Results.NotFound(); }");
			indentWriter.WriteLine();
			indentWriter.WriteLine($"db.{table.Name}.Remove(obj);");
			indentWriter.WriteLine("await db.SaveChangesAsync();");
			indentWriter.WriteLine("return Results.NoContent();");

			indentWriter.Indent--;
			indentWriter.WriteLine("});");

			indentWriter.WriteLine();
			indentWriter.WriteLine("logger.LogInformation($\"Created API: HTTP DELETE\\t{url}\");");

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