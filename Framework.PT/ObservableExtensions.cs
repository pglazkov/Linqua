using System;
using System.Threading.Tasks;

namespace Framework
{
    public static class ObservableExtensions
    {
        public static IDisposable SubscribeWithAsync<T>(this IObservable<T> observable, Func<T, Task> onNext)
        {
            return observable.Subscribe(x => onNext(x).FireAndForget());
        }
    }
}