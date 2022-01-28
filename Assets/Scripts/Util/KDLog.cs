using UnityLog = UnityEngine.Debug;

namespace KDGame.Util
{
	public class KDLog
	{
		private string _tag;
		private bool _useTrace = false;
		private string _colorStr;

		public KDLog(string tag)
		{
			_tag = EmbTag(tag);
		}

		public KDLog(string tag, bool trace)
		{
			_tag = EmbTag(tag);
			_useTrace = trace;
		}

		public KDLog(string tag, string color)
		{
			_tag = EmbTag(tag);
			_colorStr = color;
		}

		public KDLog(string tag, string color, bool trace)
		{
			_tag = EmbTag(tag);
			_useTrace = trace;
			_colorStr = color;
		}

		private string EmbTag(string tag)
		{
			return "[" + tag + "] ";
		}

		private string FormatLog(string color, string reg, params object[] param)
		{
			// 指定了颜色后无论什么日志都使用指定的颜色
			if (_colorStr != null)
				color = _colorStr;
			var log = $"<color={color}>[{_tag}]</color>{string.Format(reg, param)}";
			// 连接调用栈
			if (_useTrace)
				log += new System.Diagnostics.StackTrace();

			return log;
		}

		public void Info(string reg, params object[] param)
		{
			UnityLog.Log(_tag + reg);
		}

		public void Debug(string reg, params object[] param)
		{
			UnityLog.Log(FormatLog(LogColor.Debug, reg, param));
		}

		public void Warning(string reg, params object[] param)
		{
			UnityLog.LogWarning(FormatLog(LogColor.Warning, reg, param));
		}

		public void Error(string reg, params object[] param)
		{
			UnityLog.LogError(FormatLog(LogColor.Error, reg, param));
		}
	}

	public static class LogColor
	{
		public const string Debug = "#00FFFF";
		public const string Warning = "#FF00FF";
		public const string Error = "#FFFF00";
	}
}