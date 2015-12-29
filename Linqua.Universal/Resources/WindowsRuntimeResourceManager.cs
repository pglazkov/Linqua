using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Windows.ApplicationModel.Resources;

namespace Linqua.Resources
{
	public class WindowsRuntimeResourceManager : ResourceManager
	{
		private readonly ResourceLoader resourceLoader;

		private WindowsRuntimeResourceManager(string baseName, Assembly assembly) : base(baseName, assembly)
		{
			resourceLoader = ResourceLoader.GetForViewIndependentUse(baseName);
		}

		public static void InjectIntoResxGeneratedApplicationResourcesClass(Type resxGeneratedApplicationResourcesClass)
		{
			resxGeneratedApplicationResourcesClass.GetRuntimeFields()
			  .First(m => m.Name == "resourceMan")
			  .SetValue(null, new WindowsRuntimeResourceManager(resxGeneratedApplicationResourcesClass.FullName, resxGeneratedApplicationResourcesClass.GetTypeInfo().Assembly));
		}

		public override string GetString(string name, CultureInfo culture)
		{
			return resourceLoader.GetString(name);
		}
	}
}