using System.Collections.Generic;
using System.Linq;
using Godot;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace MVVM;

[INotifyPropertyChanged]
[GlobalClass]
public partial class ModelBase : Resource
{
	public Dictionary<string, PropertyInfo> Properties { get; private set; } = [];

	public void Initialize()
	{
		Properties = GetType()
		            .GetProperties()
		            .ToDictionary(x => x.Name, x => x);
	}
}