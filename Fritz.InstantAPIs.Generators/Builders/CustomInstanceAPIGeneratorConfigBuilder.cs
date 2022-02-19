using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Fritz.InstantAPIs.Generators.Builders
{
	internal static class CustomInstanceAPIGeneratorConfigBuilder
	{
		internal static void Build(IndentedTextWriter indentWriter, string name, List<TableData> tables)
		{
			indentWriter.WriteLine($"public sealed class {name}InstanceAPIGeneratorConfig");
			indentWriter.Indent++;
			indentWriter.WriteLine($": InstanceAPIGeneratorConfig<{name}Tables>");
			indentWriter.Indent--;
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			indentWriter.WriteLine($"private readonly Dictionary<{name}Tables, TableConfig<{name}Tables>> tableConfigs =");
			indentWriter.Indent++;
			indentWriter.WriteLine($"new Dictionary<{name}Tables, TableConfig<{name}Tables>>()");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;

			foreach(var table in tables)
			{
				indentWriter.WriteLine($"{{ {name}Tables.{table.Name}, new TableConfig<{name}Tables>({name}Tables.{table.Name})");
				indentWriter.Indent++;
				indentWriter.WriteLine("{");
				indentWriter.Indent++;
				indentWriter.WriteLine($"Name = \"{table.Name}\",");
				indentWriter.WriteLine("Included = Included.Yes,");
				indentWriter.WriteLine("APIs = ApisToGenerate.All");
				indentWriter.Indent--;
				indentWriter.WriteLine("}");
				indentWriter.Indent--;
				indentWriter.WriteLine("},");
			}

			indentWriter.Indent--;
			indentWriter.WriteLine("};");
			indentWriter.Indent--;

			indentWriter.WriteLine();
			indentWriter.WriteLine($"public {name}InstanceAPIGeneratorConfig()");
			indentWriter.Indent++;
			indentWriter.WriteLine(": base() { }");
			indentWriter.Indent--;

			indentWriter.WriteLine();
			indentWriter.WriteLine($"public sealed override TableConfig<{name}Tables> this[{name}Tables key] => tableConfigs[key];");

			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}

	}
}