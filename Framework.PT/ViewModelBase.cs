using System.Collections.Generic;
using System.Composition;
using System.Windows.Input;
using Framework.PlatformServices;
using Framework.PlatformServices.DefaultImpl;
using JetBrains.Annotations;

namespace Framework
{
    public abstract class ViewModelBase : NotificationObject, IViewModelWithBehaviors
    {
	    private ICompositionFactory compositionFactory;
	    private IEventAggregator eventAggregator;
	    private IDispatcherService dispatcher;

	    public ViewModelBase()
	    {
			Behaviors = new Dictionary<string, IViewModelBahavior>();
		    AttachedCommands = new Dictionary<string, ICommand>();
	    }

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

		public IDictionary<string, IViewModelBahavior> Behaviors { get; private set; }
		public IDictionary<string, ICommand> AttachedCommands { get; private set; }

	    public void RegisterAttachedCommand([NotNull] string commandKey, [NotNull] ICommand command)
	    {
		    Guard.NotNullOrEmpty(commandKey, () => commandKey);
		    Guard.NotNull(command, () => command);

		    if (!AttachedCommands.ContainsKey(commandKey))
		    {
			    AttachedCommands.Add(commandKey, command);
		    }
	    }

	    public void AddBehavior(string key, IViewModelBahavior behavior)
	    {
		    Guard.NotNullOrEmpty(key, () => key);
		    Guard.NotNull(behavior, () => behavior);

		    if (Behaviors.ContainsKey(key))
		    {
			    return;
		    }

		    Behaviors.Add(key, behavior);
		    behavior.Attach(this);
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