using System;
using Framework;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;
using JetBrains.Annotations;

namespace Linqua
{
	public static class Telemetry
	{
		private static Lazy<ITelemetryService> telemetryServiceLazy = new Lazy<ITelemetryService>(GetTelemetryService);

		[NotNull]
		public static ITelemetryService Client
		{
			get { return telemetryServiceLazy.Value; }
			set { telemetryServiceLazy = new Lazy<ITelemetryService>(() => value); }
		}

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