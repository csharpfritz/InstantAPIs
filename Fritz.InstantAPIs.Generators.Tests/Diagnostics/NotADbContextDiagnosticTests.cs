using Fritz.InstantAPIs.Generators.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using Xunit;
using System.Linq;

namespace Fritz.InstantAPIs.Generators.Tests.Diagnostics;

public static class NotADbContextDiagnosticTests
{
	[Fact]
	public static void Create()
	{
		var syntaxTree = CSharpSyntaxTree.ParseText("public class A { }");
		var typeSyntax = syntaxTree.GetRoot().DescendantNodes(_ => true).OfType<TypeDeclarationSyntax>().Single();
		var references = AppDomain.CurrentDomain.GetAssemblies()
			.Where(_ => !_.IsDynamic && !string.IsNullOrWhiteSpace(_.Location))
			.Select(_ => MetadataReference.CreateFromFile(_.Location));
		var compilation = CSharpCompilation.Create("generator", new SyntaxTree[] { syntaxTree },
			references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var model = compilation.GetSemanticModel(syntaxTree, true);

		var typeSymbol = model.GetDeclaredSymbol(typeSyntax)!;

		var diagnostic = NotADbContextDiagnostic.Create(typeSymbol, typeSyntax);

		Assert.Equal("The given type, A, does not derive from DbContext.", diagnostic.GetMessage());
		Assert.Equal(NotADbContextDiagnostic.Title, diagnostic.Descriptor.Title);
		Assert.Equal(NotADbContextDiagnostic.Id, diagnostic.Id);
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
		Assert.Equal(DescriptorConstants.Usage, diagnostic.Descriptor.Category);
	}
}