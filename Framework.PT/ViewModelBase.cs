using System.Composition;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;

namespace Framework
{
    public abstract class ViewModelBase : NotificationObject, IViewModel
    {
	    private ICompositionFactory compositionFactory;
	    private IEventAggregator eventAggregator;
	    private IDispatcherService dispatcher;

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

		[Import]
		public IDispatcherService Dispatcher
		{
			get { return dispatcher ?? (DesignTimeDetection.IsInDesignTool ? new DefaultDispatcherService() : CompositionManager.Current.GetInstance<IDispatcherService>()); }
			set { dispatcher = value; }
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