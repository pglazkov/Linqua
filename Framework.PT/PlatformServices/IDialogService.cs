using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Framework.PlatformServices
{
	public interface IDialogService
	{
		[NotNull]
		Task<bool> ShowConfirmation([NotNull] string title, [NotNull] string message, string okCommandText = null, string cancelCommandText = null);
	}
}