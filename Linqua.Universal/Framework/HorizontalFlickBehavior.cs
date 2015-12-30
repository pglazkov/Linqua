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
	public sealed class HorizontalFlickBehavior : Behavior<FrameworkElement>
	{
		private double startPositionX;
		private TranslateTransform translateTransform;
		private GeneralTransform containerTransform;
		private bool isFlickedAway;
		private FlickDirection? lastDirection;

		#region Container DP

		public FrameworkElement Container
		{
			get { return (FrameworkElement)GetValue(ContainerProperty); }
			set { SetValue(ContainerProperty, value); }
		}

		public static readonly DependencyProperty ContainerProperty =
			DependencyProperty.Register("Container", typeof(FrameworkElement), typeof(HorizontalFlickBehavior), new PropertyMetadata(null));

		#endregion

		#region IsEnabled DP

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsEnabledProperty =
			DependencyProperty.Register("IsEnabled", typeof(bool), typeof(HorizontalFlickBehavior), new PropertyMetadata(true, OnIsEnabledChanged));

		private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var this_ = (HorizontalFlickBehavior)d;

			this_.OnIsEnabledChanged(e);
		}

		private void OnIsEnabledChanged(DependencyPropertyChangedEventArgs e)
		{
			
		}

		#endregion

		#region OverrideRenderTransform DP

		public bool OverrideRenderTransform
		{
			get { return (bool)GetValue(OverrideRenderTransformProperty); }
			set { SetValue(OverrideRenderTransformProperty, value); }
		}

		public static readonly DependencyProperty OverrideRenderTransformProperty =
			DependencyProperty.Register("OverrideRenderTransform", typeof(bool), typeof(HorizontalFlickBehavior), new PropertyMetadata(false));

		#endregion

		#region FlickedAway

		public event EventHandler<FlickedAwayEventArgs> FlickedAway;

		private void OnFlickedAway(FlickDirection direction)
		{
			var handler = FlickedAway;
			if (handler != null) handler(this, new FlickedAwayEventArgs(direction));
		}

		#endregion

		#region Flicking Event

		public event EventHandler<FlickingEventArgs> Flicking;

		private void OnFlicking(FlickingEventArgs e)
		{
			var handler = Flicking;
			if (handler != null) handler(this, e);
		}

		#endregion

		protected override void OnAttached()
		{
			AssociatedObject.Loaded += OnLoaded;
			AssociatedObject.Unloaded += OnUnloaded;
			AssociatedObject.DataContextChanged += OnDataContextChanged;

			var existingRenderTransform = AssociatedObject.RenderTransform as TranslateTransform;

			if (existingRenderTransform != null)
			{
				translateTransform = existingRenderTransform;
			}
			else if (!OverrideRenderTransform)
			{
				throw new InvalidOperationException("Associated object must have RenderTransform set to TranslateTransform or the " +
				                                    "OverrideRenderTransform property on this behavior must be set to True. This behavior needs to manipulate the " +
				                                    "position of the object via a TranslateTransofrm.");
			}
			else
			{
				translateTransform = new TranslateTransform();
			}

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
			if (args.NewValue == null) return;

			translateTransform.X = -translateTransform.X;

			AnimateBackToOriginalPosition();
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
				e.Complete();
				return;
			}

		    if (Math.Abs(e.Delta.Translation.X) < double.Epsilon)
		    {
		        return;
		    }

			lastDirection = e.Delta.Translation.X > 0 ? FlickDirection.Right : FlickDirection.Left;

			var flickingEventArgs = new FlickingEventArgs(lastDirection.Value, e.Delta);

			OnFlicking(flickingEventArgs);

			if (!flickingEventArgs.CanContinue)
			{
				e.Complete();
                //return;
			}

			var dx = e.Cumulative.Translation.X;

			var x = startPositionX + dx;

			translateTransform.X = x;

			var positionRelativeToContainer = containerTransform.TransformPoint(new Point(0, 0));

			var realPositionX = positionRelativeToContainer.X + translateTransform.X;

			if (realPositionX < 0)
			{
				isFlickedAway = realPositionX + AssociatedObject.ActualWidth < 0;
			}
			else
			{
				isFlickedAway = realPositionX > Container.ActualWidth;
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
				OnFlickedAway(lastDirection ?? FlickDirection.Right);
			}
			else
			{
				AnimateBackToOriginalPosition();
			}
		}

		private void AnimateBackToOriginalPosition()
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
}