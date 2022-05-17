using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System;
using System.Collections.Generic;

namespace InstantAPIs.Generators.Tests;

// All of this code was grabbed from Refit
// (https://github.com/reactiveui/refit/pull/1216/files)
// based on a suggestion from
// sharwell - https://discord.com/channels/732297728826277939/732297994699014164/910258213532876861
// If the .NET Roslyn testing packages get updated to have something like this in the future
// I'll remove these helpers.
public static partial class CSharpIncrementalSourceGeneratorVerifier<TIncrementalGenerator>
	where TIncrementalGenerator : IIncrementalGenerator, new()
{
#pragma warning disable CA1034 // Nested types should not be visible
	public class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, XUnitVerifier>
#pragma warning restore CA1034 // Nested types should not be visible
	{
		public Test() =>
			this.SolutionTransforms.Add((solution, projectId) =>
			{
				if (solution is null)
				{
					throw new ArgumentNullException(nameof(solution));
				}

				if (projectId is null)
				{
					throw new ArgumentNullException(nameof(projectId));
				}

				var compilationOptions = solution.GetProject(projectId)!.CompilationOptions!;

				// NOTE: I commented this out, because I kept getting this error:
				// error CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
				// Which makes NO sense because I have "#nullable enable" emitted in my
				// generated code. So, best to just remove this for now.

				//compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
				//	 compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));

				solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

				return solution;
			});

		protected override IEnumerable<ISourceGenerator> GetSourceGenerators()
		{
			yield return new TIncrementalGenerator().AsSourceGenerator();
		}

		protected override ParseOptions CreateParseOptions()
		{
			var parseOptions = (CSharpParseOptions)base.CreateParseOptions();
			return parseOptions.WithLanguageVersion(LanguageVersion.Preview);
		}
	}

	static class CSharpVerifierHelper
	{
		/// <summary>
		/// By default, the compiler reports diagnostics for nullable reference types at
		/// <see cref="DiagnosticSeverity.Warning"/>, and the analyzer test framework defaults to only validating
		/// diagnostics at <see cref="DiagnosticSeverity.Error"/>. This map contains all compiler diagnostic IDs
		/// related to nullability mapped to <see cref="ReportDiagnostic.Error"/>, which is then used to enable all
		/// of these warnings for default validation during analyzer and code fix tests.
		/// </summary>
		internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

		static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
		{
			string[] args = { "/warnaserror:nullable" };
			var commandLineArguments = CSharpCommandLineParser.Default.Parse(
				args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
			return commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;
		}
	}
}
