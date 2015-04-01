using System;
using System.Text;
using MetroLog;
using MetroLog.Layouts;

namespace Framework.Logging
{
	public class LoggingLayout : Layout
	{
		public override string GetFormattedString(LogWriteContext context, LogEventInfo info)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("{0:0000000}", info.SequenceID);
			builder.Append("|");
			builder.Append(info.TimeStamp.ToString("s"));
			builder.Append("|");
			builder.AppendFormat("{0,-5}", info.Level.ToString().ToUpper());
			builder.Append("|");
			builder.AppendFormat("{0:000}", Environment.CurrentManagedThreadId);
			builder.Append("|");
			builder.AppendFormat(" [{0}]", info.Logger);
			builder.Append(" ");
			builder.Append(info.Message);
			if (info.Exception != null)
			{
				builder.AppendLine();
				builder.AppendLine("EXCEPTION:");
				builder.Append(string.Join(Environment.NewLine + Environment.NewLine, ExceptionUtils.UnwrapExceptions(info.Exception)));
			}

			return builder.ToString();
		} 
	}
}