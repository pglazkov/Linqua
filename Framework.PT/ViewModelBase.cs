using System.Composition;

namespace Framework
{
    public abstract class ViewModelBase : NotificationObject, IViewModel
    {
	    private ICompositionFactory compositionFactory;
	    private IEventAggregator eventAggregator;

		[Import]
	    public ICompositionFactory CompositionFactory
		{
			get { return compositionFactory ?? CompositionManager.Current.GetInstance<ICompositionFactory>(); }
			set { compositionFactory = value; }
		}

	    [Import]
	    public IEventAggregator EventAggregator
	    {
			get { return eventAggregator ?? (DesignTimeDetection.IsInDesignTool ? DesignTimeHelper.EventAggregator : CompositionManager.Current.GetInstance<IEventAggregator>()); }
		    set { eventAggregator = value; }
	    }

	    public void Cleanup()
	    {
		    CleanupOverride();
	    }

	    protected virtual void CleanupOverride()
	    {
		    
	    }
    }
}