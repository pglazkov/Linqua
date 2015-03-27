using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
	[DefaultExport(typeof(IStringResourceManager))]
	public class DefaultStringResourceManager : IStringResourceManager
	{
		public string GetString(string key, string defaultValue)
		{
			return key;
		}
	}
}