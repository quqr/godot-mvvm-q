namespace ViewSourceGenerators.ViewSourceGenerators;

public class NodeInfo
{
	private string _nodeName          = string.Empty;
	private string _nodeNameWithSharp = string.Empty;
	private string _parent            = ".";

	public string NodeName
	{
		get => _nodeName;
		set
		{
			_nodeName          = value.Replace("#", "");
			_nodeNameWithSharp = value;
		}
	}

	public string TypeName { get; set; }

	public string Parent
	{
		get => _parent;
		set
		{
			_parent  = value;
			NodePath = $"{_parent}/{_nodeNameWithSharp}";
		}
	}

	public string           NodePath { get; set; }
	public BindingDataList? Bindings { get; set; }
}