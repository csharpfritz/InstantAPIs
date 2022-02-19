using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Fritz.InstantAPIs.Generators.Builders
{
	internal static class WebApplicationExtensionsBuilder
	{
		internal static void Build(IndentedTextWriter indentWriter, INamedTypeSymbol type, List<TableData> tables)
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
				var tableVariableName = $"table{table.Name}";
				indentWriter.WriteLine($"var {tableVariableName} = config[{type.Name}Tables.{table.Name}];");
				indentWriter.WriteLine();

				indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.Get))");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
				indentWriter.WriteLine($"app.MapGet({tableVariableName}.Route.Invoke({tableVariableName}.Name), ([FromServices] {type.Name} db) =>");
				indentWriter.Indent++;
				indentWriter.WriteLine($"db.Set<{table.PropertyType.Name}>());");
				indentWriter.Indent--;
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
				indentWriter.WriteLine();

				indentWriter.WriteLine($"if ({tableVariableName}.APIs.HasFlag(ApisToGenerate.GetById))");
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
				indentWriter.WriteLine($"app.MapGet({tableVariableName}.RouteById.Invoke({tableVariableName}.Name), async ([FromServices] {type.Name} db, [FromRoute] string id) =>");
				indentWriter.Indent++;

				var idValue = "id";

				if (table.IdType!.SpecialType == SpecialType.System_Int32)
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
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
				indentWriter.WriteLine();
			}

			indentWriter.WriteLine("return app;");
			indentWriter.Indent--;
			indentWriter.WriteLine("}");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}
	}
}