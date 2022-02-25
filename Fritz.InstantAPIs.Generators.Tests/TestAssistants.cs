using Fritz.InstantAPIs.Generators.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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

			var referencedAssemblies = new HashSet<Assembly>
			{
				typeof(DbContextAPIGenerator).Assembly,
				typeof(DbContext).Assembly,
				typeof(WebApplication).Assembly,
				typeof(FromServicesAttribute).Assembly,
				typeof(EndpointRouteBuilderExtensions).Assembly,
				typeof(IApplicationBuilder).Assembly,
				typeof(IHost).Assembly,
				typeof(KeyAttribute).Assembly,
				typeof(Included).Assembly,
				typeof(IEndpointRouteBuilder).Assembly,
				typeof(RouteData).Assembly
			};

			foreach(var referencedAssembly in referencedAssemblies)
			{
				test.TestState.AdditionalReferences.Add(referencedAssembly);
			}

			//test.TestState.AdditionalReferences.Add(typeof(DbContextAPIGenerator).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(DbContext).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(WebApplication).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(FromServicesAttribute).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(EndpointRouteBuilderExtensions).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(IApplicationBuilder).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(IHost).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(KeyAttribute).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(Included).Assembly);
			//test.TestState.AdditionalReferences.Add(typeof(IEndpointRouteBuilder).Assembly);
			test.TestState.ExpectedDiagnostics.AddRange(expectedDiagnostics);
			await test.RunAsync().ConfigureAwait(false);
		}
	}
}