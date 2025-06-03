using MVVM.Scripts.MVVM;
using MVVM.ViewSourceGenerators.ViewSourceGenerators.SceneTreeExtensions;

namespace MVVM.Test;

[ViewTree("../control.tscn")]
public partial class TestView : ViewBase
{
	public override void _Ready()
	{
		ViewModel = new TestViewModel(Model);
		InitializeComponent();
		//DebugSourceGenerator();
	}
}