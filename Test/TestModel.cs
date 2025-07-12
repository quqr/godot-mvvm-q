using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Godot;

namespace MVVM.Test;

[GlobalClass]
public partial class TestModel : ModelBase
{
	[ObservableProperty] [Export] private float  _alphaValue = 1;
	[ObservableProperty] [Export] private float  _blueValue;
	[ObservableProperty] [Export] private Color  _color;
	[ObservableProperty] [Export] private float  _greenValue;
	[ObservableProperty] [Export] private float  _redValue;
	[ObservableProperty] [Export] private float  _sliderMaxValue;
	[ObservableProperty] [Export] private float  _sliderMinValue;
	[ObservableProperty] [Export] private float  _sliderValue;
	[ObservableProperty] [Export] private string _text;

	[RelayCommand]
	private void ChangeRandomColor()
	{
		RedValue   = (float)GD.RandRange(.0, 1.0);
		GreenValue = (float)GD.RandRange(.0, 1.0);
		BlueValue  = (float)GD.RandRange(.0, 1.0);
	}

	[RelayCommand]
	private void MouseEnteredColorRect()
	{
		Color = Colors.Aquamarine;
	}

	[RelayCommand]
	private void MouseExitedColorRect()
	{
		Color = new Color(RedValue, GreenValue, BlueValue, AlphaValue);
	}

	partial void OnRedValueChanged(float value)
	{
		Color = new Color(value, _greenValue, _blueValue, _alphaValue);
	}

	partial void OnGreenValueChanged(float value)
	{
		Color = new Color(_redValue, value, _blueValue, _alphaValue);
	}

	partial void OnBlueValueChanged(float value)
	{
		Color = new Color(_redValue, _greenValue, value, _alphaValue);
		Text  = value.ToString("0.00");
	}

	partial void OnAlphaValueChanged(float value)
	{
		Color = new Color(_redValue, _greenValue, _blueValue, value);
	}

	[RelayCommand]
	private void ResetColor()
	{
		RedValue   = 0;
		GreenValue = 0;
		BlueValue  = 0;
		AlphaValue = 1;
	}

	[RelayCommand]
	private void ChangeRedColor()
	{
		RedValue   = 1;
		GreenValue = 0;
		BlueValue  = 0;
		AlphaValue = 1;
	}

	[RelayCommand]
	private void ChangeGreenColor()
	{
		RedValue   = 0;
		GreenValue = 1;
		BlueValue  = 0;
		AlphaValue = 1;
	}

	[RelayCommand]
	private void ChangeBlueColor(bool value)
	{
		if (value)
		{
			RedValue   = 0;
			GreenValue = 0;
			BlueValue  = 1;
			AlphaValue = 1;
		}
		else
		{
			ResetColor();
		}
	}

	[RelayCommand]
	private void MouseEntered()
	{
		GD.Print("Hello!");
	}
}