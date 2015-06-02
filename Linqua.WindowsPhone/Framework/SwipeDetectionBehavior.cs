using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Framework;
using Microsoft.Xaml.Interactivity;

namespace Linqua.Framework
{
	public class SwipeDetectionBehavior : DependencyObject, IBehavior
	{
		private GestureRecognizer gestureRecognizer;

		#region HorizontalSwipe Event

		public event TypedEventHandler<GestureRecognizer, CrossSlidingEventArgs> HorizontalSwipe;

		protected virtual void OnHorizontalSwipe(GestureRecognizer sender, CrossSlidingEventArgs args)
		{
			var handler = HorizontalSwipe;
			if (handler != null) handler(sender, args);
		}

		#endregion

		public DependencyObject AssociatedObject { get; private set; }

		private UIElement UIElement
		{
			get { return (UIElement)AssociatedObject; }
		}

		public void Attach(DependencyObject associatedObject)
		{
			Guard.Assert(associatedObject is UIElement, "associatedObject is UIElement");

			AssociatedObject = associatedObject;

			gestureRecognizer = new GestureRecognizer
			{
				GestureSettings = GestureSettings.CrossSlide
			};

			//CrossSliding distance thresholds are disabled by default. Use CrossSlideThresholds to set these values.   
			var cst = new CrossSlideThresholds
			{
				SelectionStart = 2,
				SpeedBumpStart = 3,
				SpeedBumpEnd = 4,
				RearrangeStart = 5
			};

			gestureRecognizer.CrossSlideHorizontally = true; //Enable horinzontal slide   
			gestureRecognizer.CrossSlideThresholds = cst;

			gestureRecognizer.CrossSliding += gestureRecognizer_CrossSliding; 


			UIElement.PointerCanceled += OnPointerCanceled;
			UIElement.PointerPressed += OnPointerPressed;
			UIElement.PointerReleased += OnPointerReleased;
			UIElement.PointerMoved += OnPointerMoved; 
		}

		public void Detach()
		{
			UIElement.PointerCanceled -= OnPointerCanceled;
			UIElement.PointerPressed -= OnPointerPressed;
			UIElement.PointerReleased -= OnPointerReleased;
			UIElement.PointerMoved -= OnPointerMoved;

			gestureRecognizer.CrossSliding -= gestureRecognizer_CrossSliding;
			gestureRecognizer = null;

			AssociatedObject = null;
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs args)
		{
			// Route teh events to the gesture recognizer   
			gestureRecognizer.ProcessDownEvent(args.GetCurrentPoint(UIElement));
			// Set the pointer capture to the element being interacted with   
			UIElement.CapturePointer(args.Pointer);
			// Mark the event handled to prevent execution of default handlers   
			args.Handled = true;
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs args)
		{
			gestureRecognizer.CompleteGesture();
			args.Handled = true;
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs args)
		{
			gestureRecognizer.ProcessUpEvent(args.GetCurrentPoint(UIElement));
			args.Handled = true;
		}

		void OnPointerMoved(object sender, PointerRoutedEventArgs args)
		{
			gestureRecognizer.ProcessMoveEvents(args.GetIntermediatePoints(UIElement));
		}

		void gestureRecognizer_CrossSliding(GestureRecognizer sender, CrossSlidingEventArgs args)
		{
			OnHorizontalSwipe(sender, args);
		}
	}
}