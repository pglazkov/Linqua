using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework
{
    public static class Retry
    {
        public static void Do(Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                        Task.Delay(retryInterval).RunSynchronously();

                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }

        public static async Task DoAsync(Func<Task> action, TimeSpan retryInterval, int retryCount = 3, Action<Exception> onExceptionAction = null)
        {
            await DoAsync(async () =>
            {
                await action();
                return true;
            }, retryInterval, retryCount, onExceptionAction);
        }

        public static async Task<T> DoAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int retryCount = 3, Action<Exception> onExceptionAction = null)
        {
            var uniqueExceptions = new Dictionary<string, Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (!uniqueExceptions.ContainsKey(ex.Message))
                    {
                        uniqueExceptions.Add(ex.Message, ex);
                    }

                    if (retry < (retryCount - 1))
                    {
                        if (onExceptionAction != null)
                        {
                            try
                            {
                                onExceptionAction(ex);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        await Task.Delay(retryInterval);
                    }
                }
            }

            throw new AggregateException(uniqueExceptions.Values);
        }
    }
}