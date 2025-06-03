namespace MVVM.ViewSourceGenerators.ViewSourceGenerators;

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