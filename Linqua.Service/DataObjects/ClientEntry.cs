using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;

namespace Linqua.Service.DataObjects
{
	public class ClientEntry : EntityData
	{
		public string Text { get; set; }

		public string Definition { get; set; }

		public string UserId { get; set; }
	}
}