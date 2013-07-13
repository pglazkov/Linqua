namespace Framework.PlatformServices
{
	public interface IPlatformServiceProvider
	{
		T CreateService<T>(); 
	}
}