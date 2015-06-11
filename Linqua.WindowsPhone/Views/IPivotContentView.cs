namespace Linqua
{
	public interface IPivotContentView
	{
		void OnPivotItemLoaded(IPivotHostView host);
		void OnPivotItemUnloaded(IPivotHostView host);
	}
}