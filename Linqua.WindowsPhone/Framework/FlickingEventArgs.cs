using System;
using Windows.UI.Input;

namespace Linqua.Framework
{
	public class FlickingEventArgs : EventArgs
	{
		public FlickingEventArgs(FlickDirection direction, ManipulationDelta delta)
		{
			Direction = direction;
			Delta = delta;
			CanContinue = true;
		}

		public FlickDirection Direction { get; private set; }
		public ManipulationDelta Delta { get; private set; }

		public bool CanContinue { get; set; }
	}
}