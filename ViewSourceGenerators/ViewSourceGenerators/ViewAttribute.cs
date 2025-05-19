using System;

namespace ViewSourceGenerators;

[AttributeUsage(AttributeTargets.Class)]
public class ViewAttribute(string packedScenePath) : Attribute
{
	public string PackedScenePath { get; set; } = packedScenePath;
}