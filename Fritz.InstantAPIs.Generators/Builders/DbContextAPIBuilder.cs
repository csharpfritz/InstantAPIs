using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fritz.InstantAPIs.Generators.Builders
{
	public static class DbContextAPIBuilder
	{
		public static SourceText? Build(INamedTypeSymbol type)
		{
			var tables = new List<TableData>();

			foreach(var property in type.GetMembers().OfType<IPropertySymbol>()
				.Where(_ => !_.IsStatic && _.DeclaredAccessibility == Accessibility.Public &&
					_.Type.ToDisplayString().StartsWith("Microsoft.EntityFrameworkCore.DbSet")))
			{
				var propertyType = (INamedTypeSymbol)property.Type;
				var propertySetType = (INamedTypeSymbol)propertyType.TypeArguments.First()!;

				var idProperty = propertySetType.GetMembers().OfType<IPropertySymbol>()
					.FirstOrDefault(_ => string.Equals(_.Name, "id", StringComparison.OrdinalIgnoreCase) &&
						!_.IsStatic && _.DeclaredAccessibility == Accessibility.Public);

				if (idProperty is null)
				{
					idProperty = propertySetType.GetMembers().OfType<IPropertySymbol>()
						.FirstOrDefault(_ => _.GetAttributes().Any(_ => _.AttributeClass!.Name == "Key" || _.AttributeClass.Name == "KeyAttribute"));
				}

				tables.Add(new TableData(property.Name, propertySetType, idProperty?.Type as INamedTypeSymbol, idProperty?.Name));
			}

			if(tables.Count > 0)
			{
				using var writer = new StringWriter();
				using var indentWriter = new IndentedTextWriter(writer, "\t");

				var namespaces = new NamespaceGatherer();
				namespaces.Add("System");
				namespaces.Add("System.Collections.Generic");
				namespaces.Add("Fritz.InstantAPIs.Generators.Helpers");
				namespaces.Add("Microsoft.EntityFrameworkCore");
				namespaces.Add("Microsoft.AspNetCore.Builder");
				namespaces.Add("Microsoft.AspNetCore.Mvc");
				namespaces.Add("Microsoft.AspNetCore.Routing");
				namespaces.Add("Microsoft.AspNetCore.Http");

				if (!type.ContainingNamespace.IsGlobalNamespace)
				{
					indentWriter.WriteLine($"namespace {type.ContainingNamespace.ToDisplayString()}");
					indentWriter.WriteLine("{");
					indentWriter.Indent++;
				}

				TablesEnumBuilder.Build(indentWriter, type.Name, tables);
				indentWriter.WriteLine();
				IEndpointRouteBuilderExtensionsBuilder.Build(indentWriter, type, tables, namespaces);

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
	}
}
