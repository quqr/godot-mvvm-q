using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ViewTreeAttribute : Attribute
{
	public ViewTreeAttribute(string tscnRelativeToClassPath = null, bool traverseInstancedScenes = false,
	                         string root                    = "_",  [CallerFilePath] string classPath = null)
	{
		SceneFile = tscnRelativeToClassPath is null
			? Path.ChangeExtension(classPath, "tscn")
			: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(classPath), tscnRelativeToClassPath));

		TraverseInstancedScenes = traverseInstancedScenes;
		Root                    = root;
	}

	public string Root                    { get; }
	public string SceneFile               { get; }
	public bool   TraverseInstancedScenes { get; }
}