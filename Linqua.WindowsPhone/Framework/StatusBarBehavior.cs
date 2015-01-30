using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Linqua.Framework
{
	// Take from here: http://www.visuallylocated.com/post/2014/04/08/Creating-a-behavior-to-control-the-new-StatusBar-(SystemTray)-in-Windows-Phone-81-XAML-apps.aspx
	public class StatusBarBehavior : DependencyObject, IBehavior
	{
		#region IsVisible DP

		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public static readonly DependencyProperty IsVisibleProperty =
			DependencyProperty.Register("IsVisible",
										typeof(bool),
										typeof(StatusBarBehavior),
										new PropertyMetadata(true, OnIsVisibleChanged));

		#endregion

		#region BackgroundOpacity DP

		public double BackgroundOpacity
		{
			get { return (double)GetValue(BackgroundOpacityProperty); }
			set { SetValue(BackgroundOpacityProperty, value); }
		}

		public static readonly DependencyProperty BackgroundOpacityProperty =
			DependencyProperty.Register("BackgroundOpacity",
										typeof(double),
										typeof(StatusBarBehavior),
										new PropertyMetadata(0d, OnOpacityChanged));

		#endregion

		#region ForegroundColor DP

		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		public static readonly DependencyProperty ForegroundColorProperty =
			DependencyProperty.Register("ForegroundColor",
										typeof(Color),
										typeof(StatusBarBehavior),
										new PropertyMetadata(null, OnForegroundColorChanged));

		#endregion

		#region BackgroundColor DP

		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		public static readonly DependencyProperty BackgroundColorProperty =
			DependencyProperty.Register("BackgroundColor",
										typeof(Color),
										typeof(StatusBarBehavior),
										new PropertyMetadata(null, OnBackgroundChanged));

		#endregion

		public void Attach(DependencyObject associatedObject)
		{

		}

		public void Detach()
		{
		}

		public DependencyObject AssociatedObject { get; private set; }

		private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			bool isvisible = (bool)e.NewValue;
			if (isvisible)
			{
				StatusBar.GetForCurrentView().ShowAsync();
			}
			else
			{
				StatusBar.GetForCurrentView().HideAsync();
			}
		}

		private static void OnOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			StatusBar.GetForCurrentView().BackgroundOpacity = (double)e.NewValue;
		}

		private static void OnForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			StatusBar.GetForCurrentView().ForegroundColor = (Color)e.NewValue;
		}

		private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var behavior = (StatusBarBehavior)d;
			StatusBar.GetForCurrentView().BackgroundColor = behavior.BackgroundColor;

			// if they have not set the opacity, we need to so the new color is shown
			if (Math.Abs(behavior.BackgroundOpacity) < double.Epsilon)
			{
				behavior.BackgroundOpacity = 1;
			}
		}
	}
}