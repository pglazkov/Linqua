using System;
using System.Composition;
using System.Composition.Hosting;

namespace Framework
{
    [Export(typeof(ICompositionFactory))]
    [Shared]
    public class CompositionFactory : ICompositionFactory
    {
        private ICompositionManager compositionManager;

        public ICompositionManager CompositionManager
        {
            get { return compositionManager ?? Framework.CompositionManager.Current; }
            set { compositionManager = value; }
        }

        public T Create<T>(params object[] args) where T : class
        {
            T result;

            try
            {
                bool populateDependencies = false;

                if (args == null || args.Length == 0)
                {
                    // There are no parameters for contructor, so
                    // try to create and instance by asking the container.
                    result = CompositionManager.GetInstance<T>();

                    if (result == null)
                    {
                        // The object type is not exported. Just create an instance using 
                        // reflection and then populate all the dependencied using the 
                        // container.
                        result = Activator.CreateInstance<T>();
                        populateDependencies = true;
                    }
                }
                else
                {
                    // There are constructor parameters. Create an instance using those 
                    // parameters
                    // and then populate all the dependencied using the container.
                    result = (T)Activator.CreateInstance(typeof(T), args);
                    populateDependencies = true;
                }

                // Populate dependencies if needed
                if (populateDependencies)
                {
                    CompositionManager.Compose(result);
                }
            }
            catch (Exception ex)
            {
                throw new CompositionFailedException(
                    $"Unable to create and configure an instance of type {typeof(T)}." + " An error occured. See inner exception for details.", ex);
            }

            return result;
        }
    }
}