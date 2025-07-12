namespace ViewSourceGenerators.ViewSourceGenerators;

public record DiagnosticDetail
{
	public string Id       { get; set; }
	public string Category { get; set; }
	public string Title    { get; set; }
	public string Message  { get; set; }

	public override string ToString()
	{
		return $"{Id}: {Title} - {Message}";
	}
}