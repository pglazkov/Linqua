using System;

namespace Framework
{
    /// <summary>
    /// Provides an ability to get an event of specied type.
    /// </summary>
    public interface IEventLocator
    {
        /// <summary>
        /// Gets an instance of an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <remarks>
        /// <example>
        /// Here is an example of how to subscribe to an event:
        /// <code>
        /// <![CDATA[
        /// GetEvent<TagAppliedEvent>().Subscribe(e => OnTagApplied(e));
        /// ]]> 
        /// </code>
        /// </example>
        /// </remarks>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <returns>An instance of <see cref="IObservable{TEvent}"/> that can be used to subscribe to the event.</returns>
        IObservable<TEvent> GetEvent<TEvent>() where TEvent : EventBase;
    }
}