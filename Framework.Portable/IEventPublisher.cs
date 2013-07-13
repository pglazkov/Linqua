using System.Diagnostics.Contracts;

namespace Framework
{
    [ContractClass(typeof(IEventPublisherContract))]
    public interface IEventPublisher
    {
        void Publish<TEvent>(TEvent sampleEvent);
    }

    [ContractClassFor(typeof(IEventPublisher))]
    // ReSharper disable InconsistentNaming
    internal abstract class IEventPublisherContract : IEventPublisher
    {
        // ReSharper restore InconsistentNaming
        public void Publish<TEvent>(TEvent sampleEvent)
        {
            Contract.Requires(sampleEvent != null);

            throw new System.NotImplementedException();
        }
    }
}