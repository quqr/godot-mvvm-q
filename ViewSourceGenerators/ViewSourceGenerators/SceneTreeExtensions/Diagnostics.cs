namespace ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

internal static class Diagnostics
{
	public static DiagnosticDetail SceneFileNotFound(string tscn)
	{
		return new DiagnosticDetail { Title = "Scene file not found", Message = $"Could not find scene file: {tscn}" };
	}

	public static DiagnosticDetail SceneExistsSameNameNode(NodeInfo nodeInfo)
	{
		return new DiagnosticDetail
			{ Title = "Same name node", Message = $"the scene have the same name node {nodeInfo.NodeName}" };
	}
}