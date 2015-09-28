using System;
using Framework;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;

namespace Linqua
{
	internal static class Telemetry
	{
		public static ITelemetryService Client
		{
			get
			{
				try
				{
					return CompositionManager.Current.GetInstance<ITelemetryService>();
				}
				catch (Exception)
				{
					return new DefaultTelemetryService();
				}
			}
		}
	}
}