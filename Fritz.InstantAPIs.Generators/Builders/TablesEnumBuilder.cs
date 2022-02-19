using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Fritz.InstantAPIs.Generators.Builders
{
	internal sealed class TablesEnumBuilder
	{
		internal static void Build(IndentedTextWriter indentWriter, string name, List<TableData> tables)
		{
			indentWriter.WriteLine($"public enum {name}Tables");
			indentWriter.WriteLine("{");
			indentWriter.Indent++;
			indentWriter.WriteLine(string.Join(", ", tables.Select(_ => _.Name)));
			indentWriter.Indent--;
			indentWriter.WriteLine("}");
		}
	}
}