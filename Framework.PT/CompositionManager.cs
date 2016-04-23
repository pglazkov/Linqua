using System;
using System.Composition;
using System.Composition.Hosting;

namespace Framework
{
    public class CompositionManager : ICompositionManager
    {
        private static ICompositionManager instance;
        private readonly CompositionHost container;

        private CompositionManager(CompositionHost container)
        {
            Guard.NotNull(container, nameof(container));

            this.container = container;
        }

        public static bool IsCurrentAvailable
        {
            get { return instance != null; }
        }

        public static ICompositionManager Current
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException(
						"Please call the Initialize method before accessing the CompositionManager.Current property.");
                }

                return instance;
            }
        }

        public T GetInstance<T>()
        {
            T result;

            container.TryGetExport(out result);

            return result;
        }

        public object GetInstance(Type serviceType)
        {
            object result;

            container.TryGetExport(serviceType, out result);

            return result;
        }

        public T GetInstance<T>(string name)
        {
            T result;

            container.TryGetExport(name, out result);

            return result;
        }

        public object GetInstance(Type serviceType, string name)
        {
            object result;

            container.TryGetExport(serviceType, name, out result);

            return result;
        }

        public void Compose(object attributedObject)
        {
            container.SatisfyImports(attributedObject);
        }

        public static ICompositionManager Initialize(CompositionHost container)
        {
            Guard.NotNull(container, nameof(container));

	        instance = new CompositionManager(container);

            return instance;
        }

        public static ICompositionManager Initialize(ICompositionManager impl)
        {
            Guard.NotNull(impl, nameof(impl));

            instance = impl;

            return instance;
        }
    }
}