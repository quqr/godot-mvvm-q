using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

namespace ViewSourceGenerators.ViewSourceGenerators;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:不要使用禁用于分析器的 API")]
public static class SceneNodeParser
{
	private const string GasNodeHeaderPattern = """
	                                            \[node\s+name\s*=\s*"([^"]+)"\s+type\s*=\s*"([^"]*)"(?:\s+parent\s*=\s*"([^"]*)")?\]?
	                                            """;

	private const string GasMetaDataRegexStr = """
	                                           ^metadata/([\w/]+)\s*=\s*SubResource\((\"[^"]+\")\)
	                                           """;

	private const string GasResourceHeaderRegexStr = """
	                                                 \[sub_resource type="([^"]*)" id="([^"]*)"\]
	                                                 """;

	private const string GasResourceDataRegexStr = """
	                                               ^\s*(\w+)\s*=\s*(.*?)\s*$(?=\n^\s*\w+|\n\[|\Z)
	                                               """;

	private const string GasResourceArrayDataRegexStr = """
	                                                    SubResource\((\"[^"]+\")\)
	                                                    """;

	private static readonly Regex NodeHeaderRegex     = new(GasNodeHeaderPattern, RegexOptions.Compiled);
	private static readonly Regex MetaDataRegex       = new(GasMetaDataRegexStr, RegexOptions.Compiled);
	private static readonly Regex ResourceHeaderRegex = new(GasResourceHeaderRegexStr, RegexOptions.Compiled);
	private static readonly Regex ResourceDataRegex   = new(GasResourceDataRegexStr, RegexOptions.Compiled);

	private static readonly Regex ResourceArrayDataRegex = new(GasResourceArrayDataRegexStr, RegexOptions.Compiled);

	private static readonly Dictionary<string, Dictionary<string, string>> Resources = [];

	private static readonly Dictionary<string, Action<BindingData, string>> Handlers =
		new(StringComparer.OrdinalIgnoreCase)
		{
			["BindingMode"]   = (bd, v) => bd.BindingMode = (BindingMode)Enum.Parse(typeof(BindingMode), v),
			["ControlSource"] = (bd, v) => bd.ViewSource = v,
			["ModelSource"]   = (bd, v) => bd.ModelSource = v,
			["SourceType"]    = (bd, v) => bd.SourceType = v,
			["Event"]         = (bd, v) => bd.Event = v,
			["EventArgs"]     = (bd, v) => bd.EventArgs = v,
			["EventArgsType"] = (bd, v) => bd.EventArgsType = v,
			["Command"]       = (bd, v) => bd.Command = v,
			["Converter"]     = (bd, v) => bd.Converter = v
		};

	public static (List<NodeInfo>? nodeInfos, DiagnosticDetail Error) Parse(string scenePath)
	{
		Dictionary<string, NodeInfo> nodeInfoDict = [];
		Resources.Clear();
		var nodes        = new List<NodeInfo>();
		var sceneContent = File.ReadAllLines(scenePath);
		foreach (var content in sceneContent)
		{
			var resourceMatch = ResourceHeaderRegex.Match(content);
			if (resourceMatch.Success)
			{
				Resources.Add(resourceMatch.Groups[2].Value, null);
				continue;
			}

			var resourceDataMatch = ResourceDataRegex.Match(content);
			if (resourceDataMatch.Success)
			{
				if (Resources.Count == 0) continue;
				var resource = Resources.Last().Key;
				Resources[resource] ??= new Dictionary<string, string>();

				Resources[resource][resourceDataMatch.Groups[1].Value] = resourceDataMatch.Groups[2].Value.Trim('"');
				continue;
			}

			var nodeMatch = NodeHeaderRegex.Match(content);
			if (!nodeMatch.Success)
			{
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

			if (!nodeMatch.Groups[1].Value.Contains('#')) continue;

			var nodeInfo = new NodeInfo
			{
				NodeName = nodeMatch.Groups[1].Value.Trim('"'),
				TypeName = nodeMatch.Groups[2].Value.Trim('"'),
				Parent   = nodeMatch.Groups[3].Value.Trim('"')
			};
			if (nodeInfoDict.ContainsKey(nodeInfo.NodeName))
				return (null, Diagnostics.SceneExistsSameNameNode(nodeInfo));

			nodeInfoDict.Add(nodeInfo.NodeName, nodeInfo);
			nodes.Add(nodeInfo);
		}

		return (nodes, null);
	}

	private static BindingDataList ParseResources(Dictionary<string, string> resource)
	{
		var bindingDataList = new BindingDataList();
		foreach (var subResource in resource)
		{
			if (!subResource.Key.Equals("BindingData")) continue;
			var dataMatch = ResourceArrayDataRegex.Matches(subResource.Value);
			foreach (Match data in dataMatch)
				bindingDataList.BindingData.Add(ParseBindingData(Resources[data.Groups[1].Value.Trim('"')]));
		}


		return bindingDataList;
	}

	private static BindingData ParseBindingData(Dictionary<string, string> subResources)
	{
		var bindingData = new BindingData();

		foreach (var res in subResources)
			if (Handlers.TryGetValue(res.Key, out var handler))
				handler(bindingData, res.Value);

		return bindingData;
	}
}