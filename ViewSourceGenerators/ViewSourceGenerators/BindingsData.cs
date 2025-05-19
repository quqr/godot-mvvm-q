using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ViewSourceGenerators;

public class BindingData
{
	public  BindingMode BindingMode   { get; set; }
	public  string      ViewSource    { get; set; }
	public  string      ModelSource   { get; set; }
	public  string      SourceType    { get; set; }
	public  string      Event         { get; set; }
	public  string      EventArgs     { get; set; }
	public  string      EventArgsType { get; set; }
	public  string      Command       { get; set; }
	private string      _converter;

	public string Converter
	{
		get => _converter;
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				_converter = $".{value.Replace("\\", "")}";
			}
		}
	}
}

public static class BindingsDataExtensions
{
	public static string ToPascalCase(this string input)
	{
		if (string.IsNullOrEmpty(input)) return input;

		// 分割单词（支持空格、下划线、连字符）
		var words = input.Split([' ', '_', '-'], StringSplitOptions.RemoveEmptyEntries);

#pragma warning disable RS1035
		var textInfo = CultureInfo.CurrentCulture.TextInfo;
#pragma warning restore RS1035

		// 首字母大写后拼接
		for (int i = 0; i < words.Length; i++)
		{
			words[i] = textInfo.ToTitleCase(words[i].ToLower());
		}

		return string.Concat(words);
	}

	public static string ToSnakeCase(this string input)
	{
		if (string.IsNullOrEmpty(input)) return input;

		// 在大写字母前插入下划线，处理连续大写（如 "XML" → "_x_m_l"）
		string intermediate = Regex.Replace(input, "(?<!^)([A-Z])", "_$1");
		// 转换为全小写并合并
		return intermediate.ToLower().Replace("__", "_").Trim('_');
	}
}