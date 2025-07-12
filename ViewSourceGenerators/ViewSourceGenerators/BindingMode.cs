namespace ViewSourceGenerators.ViewSourceGenerators;

public enum BindingMode
{
    /// <summary>
    ///     No binding
    /// </summary>
    None,

    /// <summary>
    ///     source => target
    /// </summary>
    OneWay,

    /// <summary>
    ///     source <=> target
    /// </summary>
    TwoWay,

    /// <summary>
    ///     source => target, but only once
    /// </summary>
    OneTime,

    /// <summary>
    ///     Source <= Target
    /// </summary>
    OneWayToSource
}