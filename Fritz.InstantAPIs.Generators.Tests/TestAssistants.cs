using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fritz.InstantAPIs.Generators.Tests
{
	using GeneratorTest = CSharpIncrementalSourceGeneratorVerifier<DbContextAPIGenerator>;

	internal static class TestAssistants
	{
		internal static async Task RunAsync(string code,
			IEnumerable<(Type, string, string)> generatedSources,
			IEnumerable<DiagnosticResult> expectedDiagnostics)
		{
			var test = new GeneratorTest.Test
			{
				ReferenceAssemblies = ReferenceAssemblies.Net.Net60,
				TestState =
				{
					Sources = { code },
				},
			};

			foreach (var generatedSource in generatedSources)
			{
				test.TestState.GeneratedSources.Add(generatedSource);
			}

			test.TestState.AdditionalReferences.Add(typeof(DbContextAPIGenerator).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(DbContext).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(WebApplication).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(FromServicesAttribute).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(EndpointRouteBuilderExtensions).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(IApplicationBuilder).Assembly);
			test.TestState.AdditionalReferences.Add(typeof(IHost).Assembly);
			test.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
			await test.RunAsync().ConfigureAwait(false);
		}
	}
}