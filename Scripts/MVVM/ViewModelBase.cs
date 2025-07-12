using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Godot;

namespace MVVM;

public class ViewModelBase
{
	public ViewModelBase(ModelBase modelBase, ViewBase viewBase)
	{
		modelBase.Initialize();
		Model = modelBase;
		View  = viewBase;
	}

	public ModelBase Model { get; set; }
	public ViewBase  View  { get; set; }

	public virtual void Bind<T>(string bindingPropertyName, Action<T> action)
	{
		if (!Model.Properties.ContainsKey(bindingPropertyName))
			throw new ArgumentException($"Property {bindingPropertyName} not found in Model {Model.GetType().Name}");

		Model.PropertyChanged += (_, args) =>
		{
			if (!string.IsNullOrEmpty(args.PropertyName) && args.PropertyName.Equals(bindingPropertyName))
				action.Invoke(GetValue<T>(bindingPropertyName));
		};
	}

	public virtual void ExecuteCommand(string commandName)
	{
		if (Model.Properties.TryGetValue(commandName, out var method))
		{
			var command = method.GetValue(Model) as IRelayCommand;
			command?.Execute(null);
		}
		else
		{
			GD.PrintErr($"Command {commandName} not found in Model");
		}
	}

	public virtual void ExecuteCommand<T>(string commandName, T? commandParameter)
	{
		if (Model.Properties.TryGetValue(commandName, out var method))
		{
			var command = (IRelayCommand<T>)method.GetValue(Model);
			command?.Execute(commandParameter);
		}
		else
		{
			GD.PrintErr($"Command {commandName} not found in Model");
		}
	}

	public virtual T? GetValue<T>(string propertyName)
	{
		if (!Model.Properties.TryGetValue(propertyName, out var propertyInfo))
			throw new KeyNotFoundException($"Property {propertyName} not found in {GetType().Name}");
		if (propertyInfo.GetMethod?.Invoke(Model, null) is not T value)
			throw new InvalidCastException($"Property {propertyName} is not of type {typeof(T).Name}");
		return value;
	}

	public virtual void SetValue<T>(string propertyName, T value)
	{
		if (!Model.Properties.TryGetValue(propertyName, out var propertyInfo))
			throw new KeyNotFoundException($"Property {propertyName} not found in {GetType().Name}");
		propertyInfo.SetMethod?.Invoke(Model, [value]);
	}
}