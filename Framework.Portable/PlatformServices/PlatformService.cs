using System;
using System.Reflection;
using System.Threading;
using Framework.PlatformServices.DefaultImpl;

namespace Framework.PlatformServices
{
	public static class PlatformService
	{
		private const string ProviderClassName = "PlatformServiceProvider";
		private const string PortableNameSuffix = ".Portable";

		private static IPlatformServiceProvider CreateProvider()
		{
			// Starting from our core assembly, determine the matching enlightenment assembly (with the same version/strong name if applicable)

			var assemblyQualifiedName = typeof (IPlatformServiceProvider).AssemblyQualifiedName;

			var coreAssemblyFullName = assemblyQualifiedName.Substring(assemblyQualifiedName.IndexOf(",", StringComparison.Ordinal) + 1).Trim();

			var enlightenmentAssemblyName = new AssemblyName(coreAssemblyFullName);
			enlightenmentAssemblyName.Name = enlightenmentAssemblyName.Name.Replace(PortableNameSuffix, string.Empty);

			// Attempt to load the enlightenment provider from that assembly.
			
			var providerFullTypeName = typeof (IPlatformServiceProvider).Namespace + "." + ProviderClassName + ", " + enlightenmentAssemblyName.FullName;

			var enlightenmentProviderType = Type.GetType(providerFullTypeName, false);

			if (enlightenmentProviderType == null)
				return new DefaultPlatformServiceProvider();

			return (IPlatformServiceProvider)Activator.CreateInstance(enlightenmentProviderType);
		}

		private static IPlatformServiceProvider platform;

		public static IPlatformServiceProvider Platform
		{
			get
			{
				if (platform == null)
					Interlocked.CompareExchange(ref platform, CreateProvider(), null);
				return platform;
			}
			set { platform = value; }
		}
	}
}