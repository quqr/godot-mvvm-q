using Godot;

namespace KW.Scripts.MVVM;

[GlobalClass]
public partial class BindingData : Resource
{
	[Export] public BindingMode BindingMode   { get; set; }
	[Export] public string      ControlSource { get; set; }
	[Export] public string      ModelSource   { get; set; }
	[Export] public string      SourceType    { get; set; }
	[Export] public string      Event         { get; set; }
	[Export] public string      EventArgs     { get; set; }
	[Export] public string      EventArgsType { get; set; }
	[Export] public string      Command       { get; set; }
	[Export] public string      Converter     { get; set; }
}

public enum BindingMode
{
    /// <summary>
    ///     No binding
    /// </summary>
    None,

    /// <summary>
    ///     源 => 目标
    /// </summary>
    OneWay,

    /// <summary>
    ///     源 <=> 目标
    /// </summary>
    TwoWay,

    /// <summary>
    ///     源 => 目标 仅初始化一次
    /// </summary>
    OneTime,

    /// <summary>
    ///     源 <= 目标
    /// </summary>
    OneWayToSource
}