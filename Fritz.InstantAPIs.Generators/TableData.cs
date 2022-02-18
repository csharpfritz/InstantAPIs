using Microsoft.CodeAnalysis;

namespace Fritz.InstantAPIs.Generators
{
	internal sealed class TableData
	{
		internal TableData(string name, INamedTypeSymbol propertyType, INamedTypeSymbol? idType) => 
			(Name, PropertyType, IdType) = (name, propertyType, idType);

		public INamedTypeSymbol PropertyType { get; }
		public INamedTypeSymbol? IdType { get; }
		internal string Name { get; }
	}
}
