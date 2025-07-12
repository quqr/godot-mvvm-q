using ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

namespace MVVM.Test;

[ViewTree("../control.tscn")]
public partial class TestView : ViewBase
{
	public override void _Ready()
	{
		InitializeComponent(new TestViewModel(Model, this));
		//DebugSourceGenerator();
	}
}