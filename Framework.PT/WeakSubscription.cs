using System;
using JetBrains.Annotations;

namespace Framework
{
    public class WeakSubscription<TTarget, TValue> : IObserver<TValue>, IDisposable
        where TTarget : class
    {
        private Action<TTarget, TValue> onNext;
        private Action<TTarget, Exception> onError;
        private Action<TTarget> onFinally;
        private readonly WeakReference targetReference;

        public WeakSubscription(TTarget target, [NotNull] Action<TTarget, TValue> onNext, Action<TTarget, Exception> onError = null, Action<TTarget> onFinally = null)
        {
            Guard.NotNull(target, nameof(target));
            Guard.NotNull(onNext, nameof(onNext));

            if (ReferenceEquals(onNext.Target, target))
            {
                throw new ArgumentException("onNext should not be an instance method on the target itself, or else the subscription will still hold a strong reference to the target.");
            }

            if (onError != null && ReferenceEquals(onError.Target, target))
            {
                throw new ArgumentException("onError should not be an instance method on the target itself, or else the subscription will still hold a strong reference to the target.");
            }

            if (onFinally != null && ReferenceEquals(onFinally.Target, target))
            {
                throw new ArgumentException("onFinally should not be an instance method on the target itself, or else the subscription will still hold a strong reference to the target.");
            }

            targetReference = new WeakReference(target);

            this.onNext = onNext;
            this.onError = onError ?? ((_, __) => { });
            this.onFinally = onFinally ?? (_ => { });
        }

        public IDisposable UnsubscribeHandler { get; set; }

        public void OnNext(TValue value)
        {
            ActOrDispose(t => onNext(t, value));
        }

        public void OnError(Exception error)
        {
            ActOrDispose(t => onError(t, error));
        }

        public void OnCompleted()
        {
            ActOrDispose(t => onFinally(t));
        }

        private void ActOrDispose(Action<TTarget> a)
        {
            var aliveTarget = targetReference.Target as TTarget;

            if (aliveTarget != null)
            {
                a(aliveTarget);
            }
            else
            {
                Unsubscribe();
            }
        }

        private void Unsubscribe()
        {
            var s = UnsubscribeHandler;

            if (s != null)
            {
                s.Dispose();
                UnsubscribeHandler = null;
            }
        }

        public void Dispose()
        {
            Unsubscribe();
            onNext = (_, __) => { };
            onError = (_, __) => { };
            onFinally = _ => { };
        }
    }
}