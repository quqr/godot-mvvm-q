using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Godot;

namespace MVVM;

[INotifyPropertyChanged]
[GlobalClass]
public partial class ModelBase : Resource
{
	public Dictionary<string, PropertyInfo> Properties { get; private set; } = [];

	public virtual void Initialize()
	{
		Properties = GetType().GetProperties().ToDictionary(x => x.Name, x => x);
	}
}