using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;

namespace Linqua
{
	public static class TelemetryExtensions
	{
		public static void TrackMemberCall([NotNull] this ITelemetryService telemetryService, [CallerMemberName] string memberName = null)
		{
			Guard.NotNull(telemetryService, nameof(telemetryService));
			Guard.NotNullOrEmpty(memberName, nameof(memberName));

			telemetryService.TrackTrace("Member Call", TelemetrySeverityLevel.Information, new Dictionary<string, string>
			{
				{"MemberName", memberName}
			});
		}

		public static void TrackUserAction([NotNull] this ITelemetryService telemetryService, [NotNull] string userActionName, string featureName = null)
		{
			Guard.NotNull(telemetryService, nameof(telemetryService));
			Guard.NotNullOrEmpty(userActionName, nameof(userActionName));

			telemetryService.TrackTrace("User Action", TelemetrySeverityLevel.Information, new Dictionary<string, string>
			{
				{"Feature", featureName ?? "General"},
				{"ActionName", userActionName}
			});
		}
	}
}