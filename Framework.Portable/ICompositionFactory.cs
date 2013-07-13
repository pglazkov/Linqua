using System.Diagnostics.Contracts;

namespace Framework
{
    [ContractClass(typeof(CompositionFactoryContract))]
    public interface ICompositionFactory
    {
        T Create<T>(params object[] args) where T : class;
    }

    [ContractClassFor(typeof(ICompositionFactory))]
    // ReSharper disable InconsistentNaming
    internal abstract class CompositionFactoryContract : ICompositionFactory
    {
        // ReSharper restore InconsistentNaming
        public T Create<T>(params object[] args) where T : class
        {
            Contract.Ensures(Contract.Result<T>() != null);

            throw new System.NotImplementedException();
        }
    }
}