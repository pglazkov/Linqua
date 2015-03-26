using JetBrains.Annotations;

namespace Framework.PlatformServices
{
	public interface IStringResourceManager
	{
		string GetString([NotNull] string key);
	}
}