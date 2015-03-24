using System;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace Framework.Controls
{
	// Copied from here: http://developer.nokia.com/community/wiki/Tabbed_interface_with_Pivot_animation_for_Windows_Phone
	public class SplitPanel : Panel
	{
		protected override Size MeasureOverride(Size availableSize)
		{
			// the final measure size is the available size for the width, and the maximum
			// desired size of our children for the height
			Size finalSize = new Size();

			double maxWidth = 0;

			var widthPerItem = availableSize.Width;

			if (Children.Count != 0)
				widthPerItem = availableSize.Width / Children.Count;

			var itemAvailableSize = new Size(widthPerItem, availableSize.Height);

			foreach (var current in Children)
			{
				current.Measure(itemAvailableSize);

				Size desiredSize = current.DesiredSize;
				finalSize.Height = Math.Max(finalSize.Height, desiredSize.Height);
				maxWidth = Math.Max(maxWidth, desiredSize.Width);
			}

			finalSize.Width = Math.Min(maxWidth * Children.Count, availableSize.Width);

			// make sure it will works in design time mode
			if (double.IsPositiveInfinity(finalSize.Height) || double.IsPositiveInfinity(finalSize.Width))
				return Size.Empty;

			return finalSize;
		}

		protected override Size ArrangeOverride(Size arrangeSize)
		{
			Rect finalRect = new Rect(new Point(), arrangeSize);
			double width = arrangeSize.Width / Children.Count;

			foreach (var child in Children)
			{
				finalRect.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);
				finalRect.Width = width;

				child.Arrange(finalRect);

				// move each child by the width increment 
				finalRect.X += width;
			}

			return arrangeSize;
		}
	}
}