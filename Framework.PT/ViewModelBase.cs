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
	    private IDialogService dialogService;
	    private IStringResourceManager resources;

	    public ViewModelBase()
	    {
			Behaviors = new Dictionary<string, IViewModelBahavior>();
		    AttachedCommands = new Dictionary<string, ICommand>();
	    }

		[Import]
	    public ICompositionFactory CompositionFactory
		{
			get { return compositionFactory ?? (compositionFactory = CompositionManager.Current.GetInstance<ICompositionFactory>()); }
			set { compositionFactory = value; }
		}

	    [Import]
	    public IEventAggregator EventAggregator
	    {
			get { return eventAggregator ?? (eventAggregator = DesignTimeDetection.IsInDesignTool ? DesignTimeHelper.EventAggregator : CompositionManager.Current.GetInstance<IEventAggregator>()); }
		    set { eventAggregator = value; }
	    }

		[Import]
		public IDispatcherService Dispatcher
		{
			get { return dispatcher ?? (dispatcher = DesignTimeDetection.IsInDesignTool ? new DefaultDispatcherService() : CompositionManager.Current.GetInstance<IDispatcherService>()); }
			set { dispatcher = value; }
		}

	    public IDialogService DialogService
	    {
		    get { return dialogService ?? (dialogService = DesignTimeDetection.IsInDesignTool ? new DefaultDialogService() : CompositionManager.Current.GetInstance<IDialogService>()); }
		    set { dialogService = value; }
	    }

	    public IStringResourceManager Resources
	    {
		    get { return resources ?? (resources = DesignTimeDetection.IsInDesignTool ? new DefaultStringResourceManager() : CompositionManager.Current.GetInstance<IStringResourceManager>()); }
		    set { resources = value; }
	    }

	    public IDictionary<string, IViewModelBahavior> Behaviors { get; }
		public IDictionary<string, ICommand> AttachedCommands { get; }

	    public void RegisterAttachedCommand([NotNull] string commandKey, [NotNull] ICommand command)
	    {
		    Guard.NotNullOrEmpty(commandKey, nameof(commandKey));
		    Guard.NotNull(command, nameof(command));

		    if (!AttachedCommands.ContainsKey(commandKey))
		    {
			    AttachedCommands.Add(commandKey, command);
		    }
	    }

	    public void AddBehavior(string key, IViewModelBahavior behavior)
	    {
		    Guard.NotNullOrEmpty(key, nameof(key));
		    Guard.NotNull(behavior, nameof(behavior));

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