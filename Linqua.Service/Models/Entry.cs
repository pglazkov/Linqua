using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service.Models
{
	public class Entry : EntityData
	{
		public string Text { get; set; }

		public string Definition { get; set; }

        public TranslationState TranslationState { get; set; }

		public string UserId { get; set; }

		public bool IsLearnt { get; set; }
	}
}