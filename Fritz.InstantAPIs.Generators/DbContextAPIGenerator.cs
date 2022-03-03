using Fritz.InstantAPIs.Generators.Builders;
using Fritz.InstantAPIs.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Fritz.InstantAPIs.Generators;

[Generator]
public sealed class DbContextAPIGenerator
	: IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) =>
			node is AttributeSyntax attributeNode &&
				(attributeNode.Name.ToString() == "InstantAPIsForDbContext" || attributeNode.Name.ToString() == "InstantAPIsForDbContextAttribute");

		static (AttributeSyntax, INamedTypeSymbol)? TransformTargets(GeneratorSyntaxContext context, CancellationToken token)
		{
			// We only want to return types with our attribute
			var node = (AttributeSyntax)context.Node;
			var model = context.SemanticModel;

			// AttributeSyntax maps to a IMethodSymbol (you're basically calling a constructor
			// when you declare an attribute on a member).
			var symbol = model.GetSymbolInfo(node, token).Symbol as IMethodSymbol;

			if (symbol is not null)
			{
				// Let's do a best guess that it's the attribute we're looking for.
				if(symbol.ContainingType.Name == "InstantAPIsForDbContextAttribute" && 
					symbol.ContainingNamespace.ToDisplayString() == "Fritz.InstantAPIs.Generators.Helpers")
				{
					// Find the attribute data for the node.
					var attributeData = model.Compilation.Assembly.GetAttributes().SingleOrDefault(
						_ => _.ApplicationSyntaxReference!.GetSyntax() == node);

					if (attributeData is not null &&
						attributeData.ConstructorArguments[0].Value is not null &&
						attributeData.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
					{
						return (node, typeSymbol);
					}
				}
			}

			return null;
		}

		var provider = context.SyntaxProvider
			 .CreateSyntaxProvider(IsSyntaxTargetForGeneration, TransformTargets)
			 .Where(static _ => _ is not null);
		var output = context.CompilationProvider.Combine(provider.Collect());

		context.RegisterSourceOutput(output,
			(context, source) => CreateOutput(source.Right, context));
	}

	private static void CreateOutput(ImmutableArray<(AttributeSyntax, INamedTypeSymbol)?> symbols, SourceProductionContext context)
	{
		static bool IsDbContext(INamedTypeSymbol type)
		{
			var baseType = type.BaseType;

			while (baseType is not null)
			{
				if (baseType.Name == "DbContext" && baseType.ContainingNamespace.ToDisplayString() == "Microsoft.EntityFrameworkCore")
				{
					return true;
				}

				baseType = baseType.BaseType;
			}

			return false;
		}

		var dbTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

		foreach(var symbol in symbols)
		{
			var node = symbol!.Value.Item1;
			var typeSymbol = symbol!.Value.Item2;

			if (!IsDbContext(typeSymbol))
			{
				context.ReportDiagnostic(NotADbContextDiagnostic.Create(typeSymbol, node));
			}
			else
			{
				if(!dbTypes.Add(typeSymbol))
				{
					context.ReportDiagnostic(DuplicateDefinitionDiagnostic.Create(node));						
				}
				else
				{
					var text = DbContextAPIBuilder.Build(typeSymbol);

					if (text is not null)
					{
						context.AddSource($"{typeSymbol.Name}_DbContextAPIGenerator.g.cs", text);
					}
				}
			}
		}
	}
}
