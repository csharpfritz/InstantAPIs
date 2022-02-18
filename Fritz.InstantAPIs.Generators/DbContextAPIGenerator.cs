using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;

namespace Fritz.InstantAPIs.Generators
{
	[Generator]
	public sealed class DbContextAPIGenerator
		: IIncrementalGenerator
	{
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token) =>
				node is ClassDeclarationSyntax;

			static ISymbol? TransformTargets(GeneratorSyntaxContext context, CancellationToken token)
			{
				static bool IsDbContext(INamedTypeSymbol type)
				{
					var baseType = type.BaseType;

					while(baseType is not null)
					{
						if(baseType.Name == "DbContext" && baseType.ContainingNamespace.ToDisplayString() == "Microsoft.EntityFrameworkCore")
						{
							return true;
						}

						baseType = baseType.BaseType;
					}

					return false;
				}

				var model = context.SemanticModel;
				var nodeType = model.GetDeclaredSymbol(context.Node, token) as INamedTypeSymbol;

				if(nodeType is not null && IsDbContext(nodeType))
				{
					return nodeType;
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

		private static void CreateOutput(ImmutableArray<ISymbol?> symbols, SourceProductionContext context)
		{
			foreach(var type in symbols.OfType<INamedTypeSymbol>())
			{
				var builder = new DbContextAPIBuilder(type);

				if(builder.Text is not null)
				{
					context.AddSource($"{type.Name}_DbContextAPIGenerator.g.cs", builder.Text);
				}
			}
		}
	}
}
