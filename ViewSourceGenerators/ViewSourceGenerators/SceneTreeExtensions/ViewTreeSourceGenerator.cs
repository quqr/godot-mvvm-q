using System.IO;
using KW.ViewSourceGenerators.ViewSourceGenerators.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

[Generator]
internal class ViewTreeSourceGenerator : SourceGeneratorForDeclaredTypeWithAttribute<ViewTreeAttribute>
{
	protected override (string GeneratedCode, DiagnosticDetail Error) GenerateCode(Compilation compilation,
		SyntaxNode node, INamedTypeSymbol symbol, AttributeData attribute, AnalyzerConfigOptions options)
	{
		var sceneTree = ReconstructAttribute();

		if (!File.Exists(sceneTree.SceneFile))
			return (null, Diagnostics.SceneFileNotFound(sceneTree.SceneFile));

		var (nodeInfos, error) = SceneNodeParser.Parse(sceneTree.SceneFile);
		if (nodeInfos is null)
			return (null, error);

		var output = new CodeBuilder().AppendHeader(symbol.Name, symbol.NamespaceOrNull())
		                              .AppendProperties(nodeInfos)
		                              .AppendInitialization(nodeInfos)
		                              .AppendBindingMethods(nodeInfos)
		                              .AppendEnd()
		                              .ToString();

		return (output, null);

		ViewTreeAttribute ReconstructAttribute()
		{
			return new ViewTreeAttribute((string)attribute.ConstructorArguments[0].Value,
				(bool)attribute.ConstructorArguments[1].Value, (string)attribute.ConstructorArguments[2].Value,
				(string)attribute.ConstructorArguments[3].Value);
		}
	}
}