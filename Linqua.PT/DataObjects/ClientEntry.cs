using System;

namespace Linqua.DataObjects
{
	public class ClientEntry
	{
		public ClientEntry()
		{
		}

		public ClientEntry(string text)
		{
			Text = text;
		}

		public ClientEntry(string text, DateTimeOffset? createdAt)
		{
			Text = text;
			CreatedAt = createdAt;
		}

		public string Text { get; set; }

		public string Definition { get; set; }

		public DateTimeOffset? CreatedAt { get; set; }
	}
}