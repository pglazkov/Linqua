using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Framework
{
    public static class ReactiveExtensions
    {
        /// <summary>
        /// Creates an observable property by given expression. The expression represents a property
        /// to observe using the <see cref="INotifyPropertyChanged"/> interface.
        /// </summary>
        /// <remarks>
        /// Based on the following link: http://geekswithblogs.net/Silverlight2/archive/2010/11/30/observe-a-property-in-mvvm-pattern-in-silverlightwpf-using-rx.aspx
        /// </remarks>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="targetObject">
        /// Object on which to observe the property (the one that implements <see cref="INotifyPropertyChanged"/>). 
        /// If <c>Null</c> the object will be determined from the <paramref name="propertyExpression"/>.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public static IObservable<T> ObservableProperty<T>(Expression<Func<T>> propertyExpression, object targetObject = null)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }

            var memberExpression = propertyExpression.Body as MemberExpression;

            if (memberExpression == null)
            {
                var unaryExpression = propertyExpression.Body as UnaryExpression;

                if (unaryExpression != null)
                {
                    if (unaryExpression.NodeType != ExpressionType.Convert)
                    {
                        throw new InvalidOperationException($"Cannot interpret member from {unaryExpression}");
                    }

                    memberExpression = unaryExpression.Operand as MemberExpression;
                }

                if (memberExpression == null)
                {
                    throw new ArgumentException("The expression is not a member access expression.", "propertyExpression");
                }
            }

            var member = memberExpression.Expression;

            if (member == null)
            {
                throw new ArgumentException("The member is not valid");
            }

            var property = memberExpression.Member as PropertyInfo;

            if (property == null)
            {
                throw new ArgumentException("The member access expression does not access a property.");
            }

            var constantExpression = member as ConstantExpression;

            if (constantExpression != null && targetObject == null)
            {
                targetObject = constantExpression.Value;
            }

            if (targetObject == null)
            {
                throw new InvalidOperationException(
                    "Target object that implements INotifyPropertyChanged cannot be determined automatically. Please specify the 'targetObject' parameter.");
            }

            if (!(targetObject is INotifyPropertyChanged))
            {
                throw new ArgumentException("The member doesn't implement INotifyPropertyChanged interface");
            }

            string propertyName = property.Name;

            return Observable
                .FromEventPattern<PropertyChangedEventArgs>(targetObject, "PropertyChanged")
                .Where(prop => prop.EventArgs.PropertyName == propertyName)
                .Select(_ => property.GetValue(targetObject, null))
                //.DistinctUntilChanged()
                .Cast<T>();
        }

        /// <summary>
        /// Subscribes to the <paramref name="observable"/> using a weak reference, so the subscription is active
        /// only while <paramref name="target"/> is alive.
        /// </summary>
        /// <remarks>
        /// The <paramref name="onNext"/> handler should not be an instance method on the target itself, otherwise an exception will be thrown. 
        /// You can still call subscribe using instance methods, but you need to to it by calling it from the instance passed to the
        /// <paramref name="onNext"/> delegate. Here is an example:
        /// <example>
        /// <code>
        /// observable.SubscribeWeakly(this, (target, eventArgs) => target.HandleEvent(eventArgs));
        /// 
        /// private void HandleEvent(NotifyCollectionChangedEventArgs item)
        /// {
        ///      Console.WriteLine("Event received by Weak subscription");
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TTarget">The type of the target object for the weak reference.</typeparam>
        /// <param name="observable">The observable.</param>
        /// <param name="target">The target for the weak reference.</param>
        /// <param name="onNext">The handler delegate.</param>
        /// <param name="onError"></param>
        /// <param name="onFinally"></param>
        /// <exception cref="ArgumentException">When <paramref name="onNext"/> is an instance method on the target.</exception>
        public static IDisposable SubscribeWeakly<T, TTarget>(this IObservable<T> observable, TTarget target, Action<TTarget, T> onNext, Action<TTarget, Exception> onError = null, Action<TTarget> onFinally = null) where TTarget : class
        {
            var subscription = new WeakSubscription<TTarget, T>(target, onNext, onError, onFinally);

            subscription.UnsubscribeHandler = observable.Subscribe(subscription);

            return subscription;
        }
    }
}