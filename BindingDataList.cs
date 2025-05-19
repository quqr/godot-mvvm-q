using Godot;

namespace MVVM;

[GlobalClass]
public partial class BindingDataList : Resource
{
	[Export] public BindingData[] BindingData { get; set; }
}