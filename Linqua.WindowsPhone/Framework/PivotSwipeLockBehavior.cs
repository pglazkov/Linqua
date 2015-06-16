using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Framework;
using Microsoft.Xaml.Interactivity;

namespace Linqua.Framework
{
	public class PivotSwipeLockBehavior : DependencyObject, IBehavior
	{
		private readonly PointerEventHandler pivotItemPointerPressedHandler;
		private readonly PointerEventHandler pivotItemPointerReleasedHandler;

		public PivotSwipeLockBehavior()
		{
			pivotItemPointerPressedHandler = PivotItem_OnPointerPressed;
			pivotItemPointerReleasedHandler = PivotItem_OnPointerReleased;
		}

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
			args.Item.AddHandler(UIElement.PointerPressedEvent, pivotItemPointerPressedHandler, true);
			args.Item.AddHandler(UIElement.PointerReleasedEvent, pivotItemPointerReleasedHandler, true);
		}

		private void OnPivotItemUnloaded(Pivot sender, PivotItemEventArgs args)
		{
			args.Item.RemoveHandler(UIElement.PointerPressedEvent, pivotItemPointerPressedHandler);
			args.Item.RemoveHandler(UIElement.PointerReleasedEvent, pivotItemPointerReleasedHandler);
		}

		private void PivotItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			Pivot.IsLocked = true;
		}

		private void PivotItem_OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			Pivot.IsLocked = false;
		}
	}
}