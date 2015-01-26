using System.Diagnostics.Contracts;
using JetBrains.Annotations;

namespace Framework
{
    public interface IEventPublisher
    {
        void Publish<TEvent>([NotNull] TEvent sampleEvent) where TEvent : EventBase;
    }
}