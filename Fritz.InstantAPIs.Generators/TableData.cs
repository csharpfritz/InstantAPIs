using Microsoft.CodeAnalysis;

namespace Fritz.InstantAPIs.Generators;

internal sealed class TableData
{
	internal TableData(string name, INamedTypeSymbol propertyType, INamedTypeSymbol? idType, string? idName) => 
		(Name, PropertyType, IdType, IdName) = (name, propertyType, idType, idName);

	public INamedTypeSymbol PropertyType { get; }
	public string? IdName { get; }
	public INamedTypeSymbol? IdType { get; }
	internal string Name { get; }
}
