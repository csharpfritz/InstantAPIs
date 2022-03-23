using Microsoft.CodeAnalysis;

namespace InstantAPIs.Generators.Diagnostics;

public class DuplicateDefinitionDiagnostic
{
	public static Diagnostic Create(SyntaxNode currentNode) =>
		Diagnostic.Create(new DiagnosticDescriptor(
			DuplicateDefinitionDiagnostic.Id, DuplicateDefinitionDiagnostic.Title,
			DuplicateDefinitionDiagnostic.Message, DescriptorConstants.Usage, DiagnosticSeverity.Warning, true,
			helpLinkUri: HelpUrlBuilder.Build(
				DuplicateDefinitionDiagnostic.Id, DuplicateDefinitionDiagnostic.Title)),
			currentNode.GetLocation());

	public const string Id = "IA2";
	public const string Message = "The given DbContext has already been defined.";
	public const string Title = "Duplicate DbContext Definition";
}
