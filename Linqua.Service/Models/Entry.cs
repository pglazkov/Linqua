using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service.Models
{
	public class Entry : EntityData
	{
		public string Text { get; set; }

		public string Definition { get; set; }

		public string UserId { get; set; }
	}
}