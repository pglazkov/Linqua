using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Framework;
using Microsoft.Xaml.Interactivity;

namespace Linqua.Framework
{
	public class PivotSwipeLockBehavior : DependencyObject, IBehavior
	{
		public DependencyObject AssociatedObject { get; private set; }

		private Pivot Pivot
		{
			get { return (Pivot)AssociatedObject; }
		}

		public void Attach(DependencyObject associatedObject)
		{
			Guard.Assert(associatedObject is Pivot, "associatedObject is Pivot");

			AssociatedObject = associatedObject;

			Pivot.PivotItemLoaded += OnPivotItemLoaded;
			Pivot.PivotItemUnloaded += OnPivotItemUnloaded;
		}

		public void Detach()
		{
			Pivot.PivotItemLoaded -= OnPivotItemLoaded;
			Pivot.PivotItemUnloaded -= OnPivotItemUnloaded;

			AssociatedObject = null;
		}

		private void OnPivotItemLoaded(Pivot sender, PivotItemEventArgs args)
		{
			args.Item.ManipulationMode =
				ManipulationModes.TranslateX |
				ManipulationModes.TranslateInertia;
		}

		private void OnPivotItemUnloaded(Pivot sender, PivotItemEventArgs args)
		{
			args.Item.ManipulationMode = ManipulationModes.None;
		}
	}
}