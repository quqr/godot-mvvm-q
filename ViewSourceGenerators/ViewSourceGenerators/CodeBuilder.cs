using System.Collections.Generic;
using System.Text;

namespace ViewSourceGenerators;

internal class CodeBuilder
{
	private readonly StringBuilder _sb        = new();
	private const    int           IndentSize = 4;

	public void AppendHeader(string className)
	{
		_sb.AppendLine("using Godot;");
		_sb.AppendLine("namespace MVVM;");
		_sb.AppendLine($"public partial class {className}");
		_sb.AppendLine("{");
	}

	public void AppendStart(string indent)
	{
		_sb.AppendLine($"{indent}{{");
	}

	public void AppendEnd(string indent = "")
	{
		_sb.AppendLine($"{indent}}}");
	}

	public void AppendError(string err)
	{
		_sb.AppendLine(err);
	}

	public void AppendProperties(List<NodeInfo> nodeInfos)
	{
		foreach (var nodeInfo in nodeInfos)
			_sb.AppendLine(
				$"{new string(' ', IndentSize)}public {nodeInfo.TypeName} {nodeInfo.NodeName} {{ get; set; }}");
	}

	public void AppendInitialization(List<NodeInfo> nodeInfos)
	{
		_sb.AppendLine($"{new string(' ', IndentSize)}public void InitializeComponent()");
		_sb.AppendLine($"{new string(' ', IndentSize)}{{");

		foreach (var nodeInfo in nodeInfos)
			_sb.AppendLine(
				$"{new string(' ', IndentSize * 2)}{nodeInfo.NodeName} = GetNode<{nodeInfo.TypeName}>(\"{nodeInfo.NodePath}\");");

		foreach (var nodeInfo in nodeInfos)
			if (nodeInfo.Bindings is not null)
				_sb.AppendLine($"{new string(' ', IndentSize * 2)}Bind{nodeInfo.NodeName}();");


		_sb.AppendLine($"{new string(' ', IndentSize)}}}");
	}

	public void AppendBindingMethods(List<NodeInfo> nodeInfos)
	{
		foreach (var nodeInfo in nodeInfos)
		{
			if (nodeInfo.Bindings is null) continue;
			_sb.AppendLine($"{new string(' ', IndentSize)}private void Bind{nodeInfo.NodeName}()");
			_sb.AppendLine($"{new string(' ', IndentSize)}{{");
			foreach (var bindingData in nodeInfo.Bindings.BindingData)
			{
				AppendBindingLogic(nodeInfo, bindingData);
				AppendCommandLogic(nodeInfo, bindingData);
			}

			_sb.AppendLine($"{new string(' ', IndentSize)}}}");
		}
	}

	private void AppendCommandLogic(NodeInfo nodeInfo, BindingData bindingData)
	{
		if (string.IsNullOrEmpty(bindingData.Command)) return;
		var indent = new string(' ', IndentSize * 2);
		AppendEventBinding(indent, nodeInfo, bindingData, true);
	}

	private void AppendBindingLogic(NodeInfo nodeInfo, BindingData bindingData)
	{
		if (bindingData.BindingMode == BindingMode.None) return;
		var indent = new string(' ', IndentSize * 2);
		AppendStart(indent);
		indent = new string(' ', IndentSize * 3);
		_sb.AppendLine(
			$"{indent}var value = ViewModel.GetValue<{bindingData.SourceType}>(\"{bindingData.ModelSource}\");");
		_sb.AppendLine(
			$"{indent}if (value == null){{GD.PrintErr(\" {nodeInfo.NodeName} : {bindingData.ViewSource} binding {bindingData.ModelSource} failed, value is null\"); return;}} ");
		_sb.AppendLine(
			$"{indent}{nodeInfo.NodeName}.Set(\"{bindingData.ViewSource}\", Variant.From(value));");
		switch (bindingData.BindingMode)
		{
			case BindingMode.OneWay:
				AppendOneWayBinding(indent, nodeInfo, bindingData);
				break;
			case BindingMode.TwoWay:
				AppendTwoWayBinding(indent, nodeInfo, bindingData);
				break;
			case BindingMode.OneTime:
				AppendOneTimeBinding(indent, nodeInfo, bindingData);
				break;
			case BindingMode.OneWayToSource:
				AppendOneWayToSourceBinding(indent, nodeInfo, bindingData);
				break;
			case BindingMode.None:
				break;
		}

		indent = new string(' ', IndentSize * 2);
		AppendEnd(indent);
	}

	/// <summary>
	/// not support now
	/// </summary>
	private void AppendOneWayToSourceBinding(string indent, NodeInfo nodeInfo, BindingData bindingData)
	{
		AppendEventBinding(indent, nodeInfo, bindingData);
	}

	private void AppendOneWayBinding(string indent, NodeInfo nodeInfo, BindingData bindingData)
	{
		_sb.AppendLine(
			$"{indent}ViewModel.Bind<{bindingData.SourceType}>(\"{bindingData.ModelSource}\", val =>");
		_sb.AppendLine($"{indent}{{");
		_sb.AppendLine(
			$"{indent}    {nodeInfo.NodeName}.Set(\"{bindingData.ViewSource}\", Variant.From(val{bindingData.Converter}));");
		_sb.AppendLine($"{indent}}});");
	}

	private void AppendTwoWayBinding(string indent, NodeInfo nodeInfo, BindingData bindingData)
	{
		AppendOneWayBinding(indent, nodeInfo, bindingData);
		AppendEventBinding(indent, nodeInfo, bindingData);
	}

	private void AppendOneTimeBinding(string indent, NodeInfo nodeInfo, BindingData bindingData)
	{
		AppendEventBinding(indent, nodeInfo, bindingData);
	}

	private void AppendEventBinding(string indent, NodeInfo nodeInfo, BindingData bindingData, bool isCommand = false)
	{
		if (string.IsNullOrEmpty(bindingData.Event))
		{
			if (isCommand)
			{
				AppendError(
					$"throw new ArgumentException(\" {nodeInfo.NodeName} : The Event can not be empty when binding Command.\");");
			}

			return;
		}

		var args       = string.Empty;
		var ignoreArgs = true;
		if (!string.IsNullOrEmpty(bindingData.EventArgs))
		{
			ignoreArgs = false;
			if (isCommand)
			{
				if (string.IsNullOrEmpty(bindingData.EventArgsType))
					AppendError(
						$"throw new ArgumentException(\"{nodeInfo.NodeName} : EventArgsType can not be empty.\");");
				args = $"{bindingData.EventArgsType} val";
			}
			else
			{
				if (string.IsNullOrEmpty(bindingData.SourceType))
					AppendError(
						$"throw new ArgumentException(\"{nodeInfo.NodeName} : SourceType can not be empty.\");");
				args = $"{bindingData.SourceType} val";
			}
		}

		_sb.AppendLine($"{indent}{nodeInfo.NodeName}.Connect(\"{bindingData.Event}\", Callable.From(({args}) =>");
		_sb.AppendLine($"{indent}{{");
		var arg = ignoreArgs ? $"{nodeInfo.NodeName}.{bindingData.ViewSource}" : "val";
		if (isCommand)
		{
			if (!arg.Equals("val"))
			{
				_sb.AppendLine(
					$"{indent}    ViewModel.ExecuteCommand(\"{bindingData.Command}Command\");");
			}
			else if (!string.IsNullOrEmpty(bindingData.EventArgsType))
			{
				_sb.AppendLine(
					$"{indent}    ViewModel.ExecuteCommand<{bindingData.EventArgsType}>(\"{bindingData.Command}Command\", {arg});");
			}
			else
			{
				AppendError($"throw new ArgumentException(\"{nodeInfo.NodeName} : EventArgsType can not be empty.\");");
			}
		}
		else
		{
			_sb.AppendLine(
				$"{indent}    ViewModel.SetValue(\"{bindingData.ModelSource}\", {arg});");
		}

		_sb.AppendLine($"{indent}}}));");
	}

	public override string ToString()
	{
		return _sb.ToString();
	}
}