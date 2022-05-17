using Microsoft.CodeAnalysis;
using System.Globalization;

namespace InstantAPIs.Generators.Diagnostics;

public static class NotADbContextDiagnostic
{
	public static Diagnostic Create(INamedTypeSymbol type, SyntaxNode attribute) =>
		Diagnostic.Create(new DiagnosticDescriptor(
			NotADbContextDiagnostic.Id, NotADbContextDiagnostic.Title,
			string.Format(CultureInfo.CurrentCulture, NotADbContextDiagnostic.Message, type.Name),
			DescriptorConstants.Usage, DiagnosticSeverity.Error, true,
			helpLinkUri: HelpUrlBuilder.Build(
				NotADbContextDiagnostic.Id, NotADbContextDiagnostic.Title)),
			attribute.GetLocation());

	public const string Id = "IA1";
	public const string Message = "The given type, {0}, does not derive from DbContext.";
	public const string Title = "Not a DbContext";
}