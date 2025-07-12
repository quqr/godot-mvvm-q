using System.Globalization;
using System.Text.RegularExpressions;

namespace KW.ViewSourceGenerators.ViewSourceGenerators.Utilities.Extensions;

internal static class StringExtensions
{
	private const string GAS_SPLIT_REGEX_STR = "[ _-]+|(?<=[a-z])(?=[A-Z])";

	private const string GAS_UNSAFE_CHARS_REGEX_STR = @"[^\w]+";

	private const string GAS_UNSAFE_FIRST_CHAR_REGEX_STR = "^[^a-zA-Z_]+";

	private static readonly Regex SplitRegex =
		new(GAS_SPLIT_REGEX_STR, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

	private static readonly Regex UnsafeCharsRegex =
		new(GAS_UNSAFE_CHARS_REGEX_STR, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

	private static readonly Regex UnsafeFirstCharRegex =
		new(GAS_UNSAFE_FIRST_CHAR_REGEX_STR, RegexOptions.Compiled | RegexOptions.ExplicitCapture);

	public static string ToTitleCase(this string source)
	{
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(SplitRegex.Replace(source, " ").ToLower());
	}

	public static string ToSafeName(this string source)
	{
		source = source.ToTitleCase().Replace(" ", "");
		source = UnsafeCharsRegex.Replace(source, "_");
		return UnsafeFirstCharRegex.IsMatch(source) ? $"_{source}" : source;
	}

	public static string Truncate(this string source, int maxChars)
	{
		return source.Length <= maxChars ? source : source.Substring(0, maxChars);
	}
}