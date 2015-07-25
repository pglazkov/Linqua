using Linqua.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Tables;

namespace Linqua.Service.DataObjects
{
	public class ClientEntry : EntityData
	{
		public string Text { get; set; }

		public string Definition { get; set; }

        public TranslationState TranslationState { get; set; }

        public string UserId { get; set; }

		public bool IsLearnt { get; set; }
	}
}