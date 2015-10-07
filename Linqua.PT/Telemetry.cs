using System;
using Framework;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;

namespace Linqua
{
	public static class Telemetry
	{
		private static readonly Lazy<ITelemetryService> TelemetryServiceLazy = new Lazy<ITelemetryService>(GetTelemetryService);

		public static ITelemetryService Client => TelemetryServiceLazy.Value;

		private static ITelemetryService GetTelemetryService()
		{
			try
			{
				return CompositionManager.Current.GetInstance<ITelemetryService>();
			}
			catch (Exception ex)
			{
				ExceptionHandlingHelper.HandleNonFatalError(ex, "Error getting an instance of the telemetry service.", sendTelemetry: false);

				return new DefaultTelemetryService();
			}
		}
	}
}