using System.Reflection;
using Godot;
using MVVM.ViewSourceGenerators.ViewSourceGenerators;
using MVVM.ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

namespace MVVM.Scripts.MVVM;

public partial class ViewBase : Node
{
	public          ViewModelBase ViewModel { get; set; }
	[Export] public ModelBase     Model     { get; set; }

	protected void DebugSourceGenerator()
	{
		var path        = GetType().GetCustomAttribute<ViewTreeAttribute>().SceneFile;
		var codeBuilder = new CodeBuilder();
		var nodeInfos   = SceneNodeParser.Parse(path);
		if (nodeInfos is null) return;
		var typeName = GetType().Name;
		codeBuilder.AppendHeader(typeName, GetType().Namespace);
		codeBuilder.AppendProperties(nodeInfos);
		codeBuilder.AppendInitialization(nodeInfos);
		codeBuilder.AppendBindingMethods(nodeInfos);
		codeBuilder.AppendEnd();
		GD.Print(codeBuilder.ToString());
	}
}