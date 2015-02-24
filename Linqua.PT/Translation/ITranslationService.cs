using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Linqua.Translation
{
	public interface ITranslationService
	{
		[NotNull]
		Task<string> DetectLanguageAsync([NotNull] string text);

		[NotNull]
		Task<string> TranslateAsync([NotNull] string text, [NotNull] string fromLanguageCode, string toLanguageCode);
	}
}