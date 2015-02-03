using System;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua.DataObjects
{
	public class ClientEntry
	{
		public ClientEntry()
		{
		}

		public ClientEntry(string text)
		{
			Id = Guid.NewGuid().ToString();
			CreatedAt = DateTimeOffset.UtcNow;

			Text = text;
		}

		public string Id { get; set; }

		public string Text { get; set; }

		public string Definition { get; set; }

		[CreatedAt]
		public DateTimeOffset? CreatedAt { get; set; }
	}
}