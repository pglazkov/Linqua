namespace Linqua.UI
{
	public interface IPivotContentView
	{
		void OnPivotItemLoaded(IPivotHostView host);
		void OnPivotItemUnloaded(IPivotHostView host);
	}
}