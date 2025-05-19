using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ViewSourceGenerators;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:不要使用禁用于分析器的 API")]
public static class SceneNodeParser
{
	public static SourceProductionContext Context;
	public static ClassDeclarationSyntax  ClassDecl;

	private static readonly Regex NodeHeaderRegex        = new(NodeHeaderPattern, RegexOptions.Compiled);
	private static readonly Regex MetaDataRegex          = new(MetaDataRegexStr, RegexOptions.Compiled);
	private static readonly Regex ResourceHeaderRegex    = new(ResourceHeaderRegexStr, RegexOptions.Compiled);
	private static readonly Regex ResourceDataRegex      = new(ResourceDataRegexStr, RegexOptions.Compiled);
	private static readonly Regex ResourceArrayDataRegex = new(ResourceArrayDataRegexStr, RegexOptions.Compiled);

	private const string NodeHeaderPattern =
		"""
		\[node\s+name\s*=\s*"([^"]+)"\s+type\s*=\s*"([^"]*)"(?:\s+parent\s*=\s*"([^"]*)")?\]?
		""";

	private const string MetaDataRegexStr =
		"""
		^metadata/([\w/]+)\s*=\s*SubResource\((\"[^"]+\")\)
		""";

	private const string ResourceHeaderRegexStr =
		"""
		\[sub_resource type="([^"]*)" id="([^"]*)"\]
		""";

	private const string ResourceDataRegexStr =
		"""
		^\s*(\w+)\s*=\s*(.*?)\s*$(?=\n^\s*\w+|\n\[|\Z)
		""";

	private const string ResourceArrayDataRegexStr =
		"""
		SubResource\((\"[^"]+\")\)
		""";

	private static readonly Dictionary<string, Dictionary<string, string>> Resources = [];

	public static List<NodeInfo>? Parse(string scenePath)
	{
		Dictionary<string, NodeInfo> nodeInfoDict = [];
		Resources.Clear();
		var nodes        = new List<NodeInfo>();
		var sceneContent = File.ReadAllLines(scenePath);
		foreach (var content in sceneContent)
		{
			//var resourceMatch = Regex.Match(content, ResourceHeaderRegexStr);
			var resourceMatch = ResourceHeaderRegex.Match(content);
			if (resourceMatch.Success)
			{
				Resources.Add(resourceMatch.Groups[2].Value, null);
				continue;
			}

			//var resourceDataMatch = Regex.Match(content, ResourceDataRegexStr);
			var resourceDataMatch = ResourceDataRegex.Match(content);
			if (resourceDataMatch.Success)
			{
				if (Resources.Count == 0) continue;
				var resource = Resources.Last().Key;
				Resources[resource] ??= new Dictionary<string, string>();

				Resources[resource][resourceDataMatch.Groups[1].Value] = resourceDataMatch.Groups[2].Value.Trim('"');
				continue;
			}

			//var nodeMatch = Regex.Match(content, NodeHeaderPattern);
			var nodeMatch = NodeHeaderRegex.Match(content);
			if (!nodeMatch.Success)
			{
				//var bindingMatch = Regex.Match(content, MetaDataRegexStr);
				var bindingMatch = MetaDataRegex.Match(content);
				if (bindingMatch.Success)
				{
					var resource = bindingMatch.Groups[2].Value.Trim('"');
					if (Resources.TryGetValue(resource, out var res))
					{
						var node = nodes.Last();
						node.Bindings = ParseResources(res);
					}
				}

				continue;
			}

			if (!nodeMatch.Groups[1].Value.Contains('#'))
			{
				continue;
			}

			var nodeInfo = new NodeInfo
			{
				NodeName = nodeMatch.Groups[1].Value.Trim('"'),
				TypeName = nodeMatch.Groups[2].Value.Trim('"'),
				Parent   = nodeMatch.Groups[3].Value.Trim('"'),
			};
			if (nodeInfoDict.ContainsKey(nodeInfo.NodeName))
			{
				Context.ReportDiagnostic(Diagnostic.Create(
					new DiagnosticDescriptor("MVVM002", "Same name node",
						"the scene have the same name node", "Usage",
						DiagnosticSeverity.Error, true), ClassDecl.GetLocation()));
				return null;
			}

			nodeInfoDict.Add(nodeInfo.NodeName, nodeInfo);
			nodes.Add(nodeInfo);
		}

		return nodes;
	}

	private static BindingDataList ParseResources(Dictionary<string, string> resource)
	{
		var bindingDataList = new BindingDataList();
		foreach (var subResource in resource)
		{
			if (!subResource.Key.Equals("BindingData")) continue;
			//var dataMatch = Regex.Matches(subResource.Value, ResourceArrayDataRegexStr);
			var dataMatch = ResourceArrayDataRegex.Matches(subResource.Value);
			foreach (Match data in dataMatch)
			{
				bindingDataList.BindingData.Add(ParseBindingData(Resources[data.Groups[1].Value.Trim('"')]));
			}
		}


		return bindingDataList;
	}

	private static BindingData ParseBindingData(Dictionary<string, string> subResources)
	{
		var bindingData = new BindingData();
		foreach (var subResource in subResources)
		{
			switch (subResource.Key)
			{
				case "BindingMode":
					bindingData.BindingMode = (BindingMode)Enum.Parse(typeof(BindingMode), subResource.Value);
					break;
				case "ControlSource":
					bindingData.ViewSource = subResource.Value;
					break;
				case "ModelSource":
					bindingData.ModelSource = subResource.Value;
					break;
				case "SourceType":
					bindingData.SourceType = subResource.Value;
					break;
				case "Event":
					bindingData.Event = subResource.Value;
					break;
				case "EventArgs":
					bindingData.EventArgs = subResource.Value;
					break;
				case "EventArgsType":
					bindingData.EventArgsType = subResource.Value;
					break;
				case "Command":
					bindingData.Command = subResource.Value;
					break;
				case "Converter":
					bindingData.Converter = subResource.Value;
					break;
				default:
					continue;
			}
		}

		return bindingData;
	}
}