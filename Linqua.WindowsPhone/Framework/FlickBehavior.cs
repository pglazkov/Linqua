using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Framework;

namespace Linqua.Framework
{
	public class FlickBehavior : Behavior<FrameworkElement>
	{
		private double startPositionX;
		private readonly TranslateTransform translateTransform = new TranslateTransform();
		private GeneralTransform containerTransform;
		private bool isFlickedAway;

		#region Container DP

		public FrameworkElement Container
		{
			get { return (FrameworkElement)GetValue(ContainerProperty); }
			set { SetValue(ContainerProperty, value); }
		}

		public static readonly DependencyProperty ContainerProperty =
			DependencyProperty.Register("Container", typeof(FrameworkElement), typeof(FlickBehavior), new PropertyMetadata(null));

		#endregion

		#region IsEnabled DP

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsEnabledProperty =
			DependencyProperty.Register("IsEnabled", typeof(bool), typeof(FlickBehavior), new PropertyMetadata(true, OnIsEnabledChanged));

		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var this_ = (FlickBehavior)d;

			this_.OnIsEnabledChanged(e);
		}

		private void OnIsEnabledChanged(DependencyPropertyChangedEventArgs e)
		{
			
		}

		#endregion

		#region FlickedAway

		public event EventHandler FlickedAway;

		protected virtual void OnFlickedAway()
		{
			var handler = FlickedAway;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		protected override void OnAttached()
		{
			AssociatedObject.Loaded += OnLoaded;
			AssociatedObject.Unloaded += OnUnloaded;
			AssociatedObject.DataContextChanged += OnDataContextChanged;

			AssociatedObject.RenderTransform = translateTransform;
		}

		protected override void OnDetaching()
		{
			AssociatedObject.Loaded -= OnLoaded;
			AssociatedObject.Unloaded -= OnUnloaded;
			AssociatedObject.DataContextChanged -= OnDataContextChanged;
		}

		private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			translateTransform.X = -translateTransform.X;

			var sb = new Storyboard();

			var doubleAnimation = new DoubleAnimation
			{
				To = 0,
				Duration = new Duration(TimeSpan.FromMilliseconds(400)),
				EasingFunction = new ExponentialEase()
			};

			Storyboard.SetTarget(doubleAnimation, translateTransform);
			Storyboard.SetTargetProperty(doubleAnimation, "X");

			sb.Children.Add(doubleAnimation);

			sb.Begin();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			Container = Container ?? AssociatedObject.GetFirstAncestorOfType<Panel>();

			containerTransform = AssociatedObject.TransformToVisual(Container);

			AssociatedObject.ManipulationMode =
				ManipulationModes.TranslateX |
				ManipulationModes.TranslateInertia;

			AssociatedObject.ManipulationStarting += OnAssociatedObjectManipulationStarting;
			AssociatedObject.ManipulationDelta += OnAssociatedObjectManipulationDelta;
			AssociatedObject.ManipulationCompleted += OnManipulationCompleted;
		}

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			AssociatedObject.ManipulationStarting -= OnAssociatedObjectManipulationStarting;
			AssociatedObject.ManipulationDelta -= OnAssociatedObjectManipulationDelta;
			AssociatedObject.ManipulationCompleted -= OnManipulationCompleted;
		}

		private void OnAssociatedObjectManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
		{
			startPositionX = translateTransform.X;
		}

		private void OnAssociatedObjectManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			if (!IsEnabled)
			{
				return;
			}

			var dx = e.Cumulative.Translation.X;

			var x = startPositionX + dx;

			translateTransform.X = x;

			var positionRelativeToContainer = containerTransform.TransformPoint(new Point(0, 0));

			var realPositionX = positionRelativeToContainer.X + translateTransform.X;

			if (realPositionX < 0)
			{
				SetIsFlickedOutside(realPositionX + AssociatedObject.ActualWidth < 0);
			}
			else
			{
				SetIsFlickedOutside(realPositionX > Container.ActualWidth);
			}

			if (isFlickedAway)
			{
				e.Complete();
			}
		}

		private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			if (isFlickedAway)
			{
				OnFlickedAway();
			}
			else
			{
				var sb = new Storyboard();

				var doubleAnimation = new DoubleAnimation
				{
					To = 0,
					Duration = new Duration(TimeSpan.FromMilliseconds(400)),
					EasingFunction = new ExponentialEase()
				};

				Storyboard.SetTarget(doubleAnimation, translateTransform);
				Storyboard.SetTargetProperty(doubleAnimation, "X");

				sb.Children.Add(doubleAnimation);

				sb.Begin();
			}
		}

		private void SetIsFlickedOutside(bool value)
		{
			isFlickedAway = value;
		}
	}
}