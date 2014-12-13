namespace Framework
{
    public abstract class ViewModelBase : NotificationObject, IViewModel
    {
	    private ICompositionFactory compositionFactory;

	    public ICompositionFactory CompositionFactory
		{
			get { return compositionFactory ?? CompositionManager.Current.GetInstance<ICompositionFactory>(); }
			set { compositionFactory = value; }
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