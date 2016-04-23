using System;

namespace Linqua.Translation.Microsoft
{
	public class MicrosoftApiException : Exception
	{
		public MicrosoftApiException()
		{
		}

		public MicrosoftApiException(string message) : base(message)
		{
		}

		public MicrosoftApiException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}