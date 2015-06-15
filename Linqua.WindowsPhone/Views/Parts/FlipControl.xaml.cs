using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Framework;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Linqua
{
	public sealed partial class FlipControl : UserControl
	{
		private bool isFlippedInternalChange = false;

		public FlipControl()
		{
			this.InitializeComponent();

			DataContextChanged += OnDataContextChanged;
		}

		#region FrontSideContent DP

		public object FrontSideContent
		{
			get { return (object)GetValue(FrontSideContentProperty); }
			set { SetValue(FrontSideContentProperty, value); }
		}

		public static readonly DependencyProperty FrontSideContentProperty =
			DependencyProperty.Register("FrontSideContent", typeof(object), typeof(FlipControl), new PropertyMetadata(null));

		#endregion

		#region BackSideContent DP

		public object BackSideContent
		{
			get { return (object)GetValue(BackSideContentProperty); }
			set { SetValue(BackSideContentProperty, value); }
		}

		public static readonly DependencyProperty BackSideContentProperty =
			DependencyProperty.Register("BackSideContent", typeof(object), typeof(FlipControl), new PropertyMetadata(null));

		#endregion

		#region FrontSideContentTemplate DP

		public DataTemplate FrontSideContentTemplate
		{
			get { return (DataTemplate)GetValue(FrontSideContentTemplateProperty); }
			set { SetValue(FrontSideContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty FrontSideContentTemplateProperty =
			DependencyProperty.Register("FrontSideContentTemplate", typeof(DataTemplate), typeof(FlipControl), new PropertyMetadata(null));

		#endregion

		#region BackSideContentTemplate DP

		public DataTemplate BackSideContentTemplate
		{
			get { return (DataTemplate)GetValue(BackSideContentTemplateProperty); }
			set { SetValue(BackSideContentTemplateProperty, value); }
		}

		public static readonly DependencyProperty BackSideContentTemplateProperty =
			DependencyProperty.Register("BackSideContentTemplate", typeof(DataTemplate), typeof(FlipControl), new PropertyMetadata(null));

		#endregion

		#region IsFlipped DP

		public bool IsFlipped
		{
			get { return (bool)GetValue(IsFlippedProperty); }
			set { SetValue(IsFlippedProperty, value); }
		}

		public static readonly DependencyProperty IsFlippedProperty =
			DependencyProperty.Register("IsFlipped", typeof(bool), typeof(FlipControl), new PropertyMetadata(false, OnIsFlippedChanged));

		private static void OnIsFlippedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var this_ = (FlipControl)d;
			this_.OnIsFlippedChanged(e);
		}

		private void OnIsFlippedChanged(DependencyPropertyChangedEventArgs e)
		{
			bool isFlipped = (bool)e.NewValue;

			if (isFlipped)
			{
				VisualStateManager.GoToState(this, "FlipCardFront", true);
			}
			else
			{
				VisualStateManager.GoToState(this, "FlipCardBack", true);
			}
		}

		#endregion

		private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			VisualStateManager.GoToState(this, "Initial", false);
		}

		private void OnTapped(object sender, TappedRoutedEventArgs e)
		{
			var isButton = e.OriginalSource is ButtonBase || 
				(e.OriginalSource is FrameworkElement && (e.OriginalSource as FrameworkElement).GetFirstAncestorOfType<ButtonBase>() != null);

			if (isButton)
			{
				return;
			}

			SetValue(IsFlippedProperty, !IsFlipped);
		}

		private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerPressed", true);
		}

		private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			VisualStateManager.GoToState(this, "PointerReleased", true);
		}
	}
}
