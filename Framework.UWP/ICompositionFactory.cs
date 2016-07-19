namespace Framework
{
    public interface ICompositionFactory
    {
        T Create<T>(params object[] args) where T : class;
    }
}