using System;
using CommunityToolkit.Mvvm.Input;
using Godot;

namespace MVVM;

public class ViewModelBase
{
	public ModelBase Model { get; set; } = new();

	public void Bind<T>(string bindingPropertyName, Action<T> action)
	{
		if (!Model.Properties.ContainsKey(bindingPropertyName))
		{
#if DEBUG
			throw new ArgumentException($"Property {bindingPropertyName} not found in Model {Model.GetType().Name}");
#endif
			GD.PrintErr($"Property {bindingPropertyName} not found in Model");
			return;
		}

		Model.PropertyChanged += (_, args) =>
		{
			if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName.Equals(bindingPropertyName))
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
		Model.Properties.TryGetValue(propertyName, out var value);
		if (value?.GetValue(Model) is T val) return val;
#if DEBUG
		throw new ArgumentException($"Property {propertyName} not found in Model {Model.GetType().Name}");
#endif
		GD.PrintErr($"Property {propertyName} not found in Model");
		return default;
	}

	public void SetValue<T>(string propertyName, T value)
	{
		Model.Properties.TryGetValue(propertyName, out var property);
		if (property is not null)
		{
			property.SetMethod?.Invoke(Model, [value]);
		}
		else
		{
#if DEBUG
			throw new ArgumentException($"Property {propertyName} not found in Model {Model.GetType().Name}");
#endif
			GD.PrintErr($"Property {propertyName} not found in Model");
		}
	}
}