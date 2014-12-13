using System;
using System.Diagnostics.Contracts;

namespace Framework
{
    [ContractClass(typeof (CompositionManagerContract))]
    public interface ICompositionManager
    {
        T GetInstance<T>();
        T GetInstance<T>(string name);
        object GetInstance(Type serviceType);
        object GetInstance(Type serviceType, string name);

        void Compose(object attributedObject);
    }

    [ContractClassFor(typeof (ICompositionManager))]
// ReSharper disable InconsistentNaming
    internal abstract class CompositionManagerContract : ICompositionManager
    {
// ReSharper restore InconsistentNaming
        public T GetInstance<T>()
        {
            throw new NotImplementedException();
        }

        public T GetInstance<T>(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));

            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            Contract.Requires(serviceType != null);

            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType, string name)
        {
            Contract.Requires(serviceType != null);
            Contract.Requires(!string.IsNullOrEmpty(name));

            throw new NotImplementedException();
        }

        public void Compose(object attributedObject)
        {
            Contract.Requires(attributedObject != null);

            throw new NotImplementedException();
        }
    }
}