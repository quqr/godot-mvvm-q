using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using Godot;

namespace MVVM.Scripts.MVVM;

public class ViewModelBase
{
	public ViewModelBase(ModelBase modelBase)
	{
		modelBase.Initialize();
		WeakModel           = new WeakReference<ModelBase>(modelBase);
		modelBase.ViewModel = new WeakReference<ViewModelBase>(this);
	}

	private WeakReference<ModelBase> WeakModel { get; }

	protected ModelBase? Model => WeakModel.TryGetTarget(out var model) ? model : null;

	public void Bind<T>(string bindingPropertyName, Action<T> action)
	{
		if (!Model.Properties.ContainsKey(bindingPropertyName))
			throw new ArgumentException($"Property {bindingPropertyName} not found in Model {Model.GetType().Name}");

		Model.PropertyChanged += (_, args) =>
		{
			if (!string.IsNullOrEmpty(args.PropertyName) && args.PropertyName.Equals(bindingPropertyName))
				action.Invoke(GetValue<T>(bindingPropertyName));
		};
	}

	public void ExecuteCommand(string commandName)
	{
		if (Model.Properties.TryGetValue(commandName, out var method))
		{
			var resetButtonCommand = method.GetValue(Model) as IRelayCommand;
			try
			{
				resetButtonCommand?.Execute(null);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error executing command {commandName}: {e.Message}");
			}
		}
		else
		{
			GD.PrintErr($"Command {commandName} not found in Model");
		}
	}

	public void ExecuteCommand<T>(string commandName, T? commandParameter)
	{
		if (Model.Properties.TryGetValue(commandName, out var method))
		{
			var resetButtonCommand = (IRelayCommand<T>)method.GetValue(Model);
			try
			{
				resetButtonCommand?.Execute(commandParameter);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error executing command {commandName}: {e.Message}");
			}
		}
		else
		{
			GD.PrintErr($"Command {commandName} not found in Model");
		}
	}

	public T? GetValue<T>(string propertyName)
	{
		if (!Model.Properties.TryGetValue(propertyName, out var propertyInfo))
			throw new KeyNotFoundException($"Property {propertyName} not found in {GetType().Name}");
		if (propertyInfo.GetMethod?.Invoke(Model, null) is not T value)
			throw new InvalidCastException($"Property {propertyName} is not of type {typeof(T).Name}");
		return value;
	}

	public void SetValue<T>(string propertyName, T value)
	{
		if (!Model.Properties.TryGetValue(propertyName, out var propertyInfo))
			throw new KeyNotFoundException($"Property {propertyName} not found in {GetType().Name}");
		propertyInfo.SetMethod?.Invoke(Model, [value]);
	}
}