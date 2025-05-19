using System.Collections.Generic;
using Godot;

namespace MVVM;

public static class Extension
{
	public static List<Node> GetAllChildren(this Node node)
	{
		var children = new List<Node> { node };
		foreach (var child in node.GetChildren()) children.AddRange(child.GetAllChildren());
		return children;
	}
}