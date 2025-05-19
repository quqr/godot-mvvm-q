using Godot;
using ViewSourceGenerators;

namespace MVVM;

[View("E:/CreatGames/Godot/mvvm/control.tscn")]
public partial class TestView : ViewBase
{
	public override void _Ready()
	{
		ViewModel = new TestViewModel();
		Model.Initialize();
		ViewModel.Model = Model;
		InitializeComponent();
		DebugSourceGenerator();
	}
}

public class TestViewModel : ViewModelBase
{
}