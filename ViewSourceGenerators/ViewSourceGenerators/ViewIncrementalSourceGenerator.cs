using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ViewSourceGenerators;

[Generator(LanguageNames.CSharp)]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:不要使用禁用于分析器的 API")]
public sealed class ViewIncrementalSourceGenerator : IIncrementalGenerator
{
	private static bool IsClassWithViewAttribute(SyntaxNode node)
	{
		return node is ClassDeclarationSyntax classDecl
		       && classDecl.AttributeLists
		                   .SelectMany(al => al.Attributes)
		                   .Any(a => a.Name.ToString() == "View");
	}

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
			predicate: static (s, _) => IsClassWithViewAttribute(s), transform: static (ctx, _) =>
			{
				var classDecl = (ClassDeclarationSyntax)ctx.Node;
				var symbol    = ctx.SemanticModel.GetDeclaredSymbol(classDecl);
				var attribute = symbol?.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "ViewAttribute");
				return (classDecl, attribute);
			}).Where(static c => c.attribute != null);
		var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());
		context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
		{
			var (_, classes) = source;
			foreach (var (classDecl, attribute) in classes)
			{
				// 提取路径参数
				var scenePath = attribute?.ConstructorArguments[0].Value?.ToString();
				GenerateCode(spc, classDecl, scenePath);
			}
		});
	}

	private static void GenerateCode(SourceProductionContext context, ClassDeclarationSyntax classDecl,
		string?                                              scenePath)
	{
		if (!File.Exists(scenePath))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				new DiagnosticDescriptor("MVVM001", "Scene path is invalid",
					$"scene path: {scenePath} is invalid.", "Usage",
					DiagnosticSeverity.Error, true),
				classDecl.GetLocation()));
			return;
		}

		SceneNodeParser.Context   = context;
		SceneNodeParser.ClassDecl = classDecl;
		GenerateBindingCode(classDecl.Identifier.Text, scenePath, context);
	}

	private static void GenerateBindingCode(string className, string tresPath, SourceProductionContext context)
	{
		var codeBuilder = new CodeBuilder();
		var nodeInfos   = SceneNodeParser.Parse(tresPath);
		if (nodeInfos is null) return;
		codeBuilder.AppendHeader(className);
		codeBuilder.AppendProperties(nodeInfos);
		codeBuilder.AppendInitialization(nodeInfos);
		codeBuilder.AppendBindingMethods(nodeInfos);
		codeBuilder.AppendEnd();
		context.AddSource($"{className}.g.cs", codeBuilder.ToString());
	}
}