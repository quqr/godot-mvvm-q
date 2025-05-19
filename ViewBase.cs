using System.Reflection;
using Godot;
using ViewSourceGenerators;

namespace MVVM;

public partial class ViewBase : Node
{
	public          ViewModelBase ViewModel { get; set; } = new();
	[Export] public ModelBase     Model     { get; set; }

	protected void DebugSourceGenerator()
	{
		var path        = GetType().GetCustomAttribute<ViewAttribute>().PackedScenePath;
		var codeBuilder = new CodeBuilder();
		var nodeInfos   = SceneNodeParser.Parse(path);
		if (nodeInfos is null) return;
		var typeName = GetType().Name;
		codeBuilder.AppendHeader(typeName);
		codeBuilder.AppendProperties(nodeInfos);
		codeBuilder.AppendInitialization(nodeInfos);
		codeBuilder.AppendBindingMethods(nodeInfos);
		codeBuilder.AppendEnd();
		GD.Print(codeBuilder.ToString());
	}
}