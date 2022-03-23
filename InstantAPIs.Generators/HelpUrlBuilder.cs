namespace InstantAPIs.Generators;

internal static class HelpUrlBuilder
{
   internal static string Build(string identifier, string title) =>
	 $"https://github.com/csharpfritz/InstantAPIs/tree/main/docs/{identifier}-{title}.md";
}