using System;
using System.Collections.Generic;
using System.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Framework
{
    [Export(typeof(IEventLocator))]
    [Export(typeof(IEventPublisher))]
	[Export(typeof(IEventAggregator))]
	[Shared]
    public class EventManager : IEventAggregator
    {
        private readonly Dictionary<Type, object> subjects = new Dictionary<Type, object>();

        public IObservable<TEvent> GetEvent<TEvent>() where TEvent : EventBase
        {
            var subject =
                (ISubject<TEvent>)GetOrAddSubject(typeof(TEvent),
                                                  t => new Subject<TEvent>());

            return subject.AsObservable();
        }

        public void Publish<TEvent>(TEvent sampleEvent) where TEvent : EventBase
        {
            object subject;

            if (subjects.TryGetValue(typeof(TEvent), out subject))
            {
                ((ISubject<TEvent>)subject).OnNext(sampleEvent);
            }
        }

        private object GetOrAddSubject(Type key, Func<Type, object> factory)
        {
            lock (subjects)
            {
                object result;

                if (!subjects.TryGetValue(key, out result))
                {
                    result = factory(key);
                    subjects.Add(key, result);
                }

                return result;
            }
        }
    }
}