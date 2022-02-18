namespace Microsoft.AspNetCore.Builder;

[Flags]
public enum ApiMethodsToGenerate
{
	Get = 1,
	GetById = 2,
	Insert = 4,
	Update = 8,
	Delete = 16,
	All = 31
}

public record TableApiMapping(string TableName, ApiMethodsToGenerate MethodsToGenerate = ApiMethodsToGenerate.All);