using InstantAPIs.Generators.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace InstantAPIs.Generators.Tests.Diagnostics;

public static class DuplicateDefinitionDiagnosticTests
{
	[Fact]
	public static void Create()
	{
		var diagnostic = DuplicateDefinitionDiagnostic.Create(SyntaxFactory.Attribute(SyntaxFactory.ParseName("A")));

		Assert.Equal(DuplicateDefinitionDiagnostic.Message, diagnostic.GetMessage());
		Assert.Equal(DuplicateDefinitionDiagnostic.Title, diagnostic.Descriptor.Title);
		Assert.Equal(DuplicateDefinitionDiagnostic.Id, diagnostic.Id);
		Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
		Assert.Equal(DescriptorConstants.Usage, diagnostic.Descriptor.Category);
	}
}
