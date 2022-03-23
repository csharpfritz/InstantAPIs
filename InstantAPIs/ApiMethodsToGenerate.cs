namespace InstantAPIs;

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

public record TableApiMapping(
	string TableName, 
	ApiMethodsToGenerate MethodsToGenerate = ApiMethodsToGenerate.All, 
	string BaseUrl = ""
);

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ApiMethodAttribute : Attribute
{
	public ApiMethodsToGenerate MethodsToGenerate { get; set; }
	public ApiMethodAttribute(ApiMethodsToGenerate apiMethodsToGenerate)
	{
		this.MethodsToGenerate = apiMethodsToGenerate;
	}
}