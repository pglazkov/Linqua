using System;
using Framework;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;
using JetBrains.Annotations;
using MetroLog;

namespace Linqua
{
    public static class Telemetry
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(Telemetry).Name);

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
                if (!CompositionManager.IsCurrentAvailable)
                {
                    Log.Warn("Cannot initialize the telemetry service because the composition container is not initialized yet.");

                    // Reset the lasy instance so when the next request comes we try to initialize it again. Otherwise the value is cached.
                    telemetryServiceLazy = new Lazy<ITelemetryService>(GetTelemetryService);

                    return new DefaultTelemetryService();
                }

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