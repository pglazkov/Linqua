using System;
using Windows.Networking.Connectivity;

namespace Linqua
{
	public static class ConnectionHelper
	{
		#region InternetConnectionChanged Event

		public static event EventHandler<InternetConnectionChangedEventArgs> InternetConnectionChanged;

		private static void OnInternetConnectionChanged(InternetConnectionChangedEventArgs e)
		{
			var handler = InternetConnectionChanged;
			if (handler != null) handler(null, e);
		}

		#endregion

		static ConnectionHelper()
		{
			NetworkInformation.NetworkStatusChanged += s =>
			{
				OnInternetConnectionChanged(new InternetConnectionChangedEventArgs(IsConnectedToInternet));
			};
		}

		public static bool IsConnectedToInternet
		{
			get
			{
				var profile = NetworkInformation.GetInternetConnectionProfile();
				var isConnected = (profile != null
					&& profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
				return isConnected;
			}
		}
	}

	public class InternetConnectionChangedEventArgs : EventArgs
	{
		private bool isConnected;

		public InternetConnectionChangedEventArgs(bool isConnected)
		{
			this.isConnected = isConnected;
		}

		public bool IsConnected
		{
			get { return isConnected; }
		}
	}
}