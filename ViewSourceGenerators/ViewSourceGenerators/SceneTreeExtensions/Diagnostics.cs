namespace MVVM.ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

internal static class Diagnostics
{
	public static DiagnosticDetail SceneFileNotFound(string tscn)
	{
		return new DiagnosticDetail { Title = "Scene file not found", Message = $"Could not find scene file: {tscn}" };
	}
}