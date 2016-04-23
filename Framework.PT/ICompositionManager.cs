using System;

namespace Framework
{
    public interface ICompositionManager
    {
        T GetInstance<T>();
        T GetInstance<T>(string name);
        object GetInstance(Type serviceType);
        object GetInstance(Type serviceType, string name);

        void Compose(object attributedObject);
    }
}