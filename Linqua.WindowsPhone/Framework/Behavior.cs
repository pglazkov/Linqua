using Windows.UI.Xaml;
using Framework;
using Microsoft.Xaml.Interactivity;

namespace Linqua.Framework
{
	public class Behavior<T> : DependencyObject, IBehavior 
		where T : DependencyObject
	{
		private DependencyObject associatedObject;

		void IBehavior.Attach(DependencyObject associatedObject)
		{
			Guard.Assert(associatedObject is T, "associatedObject is T");

			this.associatedObject = associatedObject;

			OnAttached();
		}

		void IBehavior.Detach()
		{
			OnDetaching();
			associatedObject = null;
		}

		DependencyObject IBehavior.AssociatedObject
		{
			get { return associatedObject; }
		}

		protected T AssociatedObject
		{
			get { return (T)associatedObject; }
		}

		protected virtual void OnAttached()
		{
			
		}

		protected virtual void OnDetaching()
		{
			
		}
	}
}