using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
    public static class ExceptionUtils
    {
        public static IEnumerable<Exception> UnwrapExceptions(Exception exception)
        {
            Guard.NotNull(exception, nameof(exception));

            if (exception is ReflectionTypeLoadException)
            {
                return ((ReflectionTypeLoadException)exception).LoaderExceptions.SelectMany(UnwrapExceptions);
            }

            if (exception is TargetInvocationException)
            {
                return UnwrapExceptions(exception.InnerException);
            }

            if (exception is AggregateException)
            {
                return (exception as AggregateException).InnerExceptions.SelectMany(UnwrapExceptions);
            }

            if (exception.GetType() == typeof(Exception) && exception.InnerException != null)
            {
                return UnwrapExceptions(exception.InnerException);
            }

            return new[] {exception};
        }

        public static Exception UnwrapException(Exception exception)
        {
            Guard.NotNull(exception, nameof(exception));

            var unwrappedExceptions = UnwrapExceptions(exception).ToArray();

            if (unwrappedExceptions.Length > 1)
            {
                return new AggregateException(unwrappedExceptions);
            }

            return unwrappedExceptions[0];
        }
    }
}