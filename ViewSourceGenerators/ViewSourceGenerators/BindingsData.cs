namespace ViewSourceGenerators.ViewSourceGenerators;

public class BindingData
{
	private string      _converter;
	public  BindingMode BindingMode   { get; set; }
	public  string      ViewSource    { get; set; }
	public  string      ModelSource   { get; set; }
	public  string      SourceType    { get; set; }
	public  string      Event         { get; set; }
	public  string      EventArgs     { get; set; }
	public  string      EventArgsType { get; set; }
	public  string      Command       { get; set; }

	public string Converter
	{
		get => _converter;
		set
		{
			if (!string.IsNullOrEmpty(value)) _converter = $".{value.Replace("\\", "")}";
		}
	}
}