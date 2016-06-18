using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Framework
{
    public static class RetryHelper
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

        public static async Task DoAsync(Func<Task> action,
                                         AsyncRetryPolicy policy = null,
                                         CancellationToken? cancellationToken = null)
        {
            await DoAsync(async () =>
            {
                await action();
                return true;
            },
            policy, cancellationToken);
        }

        public static async Task<T> DoAsync<T>(Func<Task<T>> action,
                                               AsyncRetryPolicy policy = null,
                                               CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;
            policy = policy ?? new AsyncRetryPolicy();

            var uniqueExceptions = new Dictionary<string, Exception>();

            for (var retry = 0; retry < policy.RetryCount; retry++)
            {
                try
                {
                    cancellationToken.Value.ThrowIfCancellationRequested();

                    return await action();
                }
                catch (Exception ex) when (!(ex is OperationCanceledException) && policy.ExceptionFilter(ex))
                {
                    if (!uniqueExceptions.ContainsKey(ex.Message))
                    {
                        uniqueExceptions.Add(ex.Message, ex);
                    }

                    if (retry < policy.RetryCount - 1)
                    {
                        try
                        {
                            policy.OnExceptionAction(ex);
                        }
                        catch (Exception) { }

                        await Task.Delay(policy.RetryInterval, cancellationToken.Value);
                    }
                }
            }

            throw new AggregateException(uniqueExceptions.Values);
        }
    }

    public class AsyncRetryPolicy
    {
        public AsyncRetryPolicy(TimeSpan? retryInterval = null,
                                int retryCount = 3,
                                Func<Exception, bool> exceptionFilter = null,
                                Action<Exception> onExceptionAction = null)
        {
            RetryInterval = retryInterval ?? TimeSpan.FromSeconds(3);
            RetryCount = retryCount;
            ExceptionFilter = exceptionFilter ?? (ex => true);
            OnExceptionAction = onExceptionAction ?? (ex => { });
        }

        public TimeSpan RetryInterval { get; private set; }
        public int RetryCount { get; private set; }

        public Func<Exception, bool> ExceptionFilter { get; private set; }

        public Action<Exception> OnExceptionAction { get; private set; }
    }
}