using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Framework
{
	public static class ExceptionUtils
	{
		public static IEnumerable<Exception> UnwrapExceptions(Exception exception)
		{
			Contract.Requires(exception != null);
			Contract.Ensures(Contract.Result<IEnumerable<Exception>>() != null);
			Contract.Ensures(Contract.Result<IEnumerable<Exception>>().Any());

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

			return new[] { exception };
		}

		public static Exception UnwrapException(Exception exception)
		{
			Contract.Requires(exception != null);
			Contract.Ensures(Contract.Result<Exception>() != null);

			var unwrappedExceptions = UnwrapExceptions(exception).ToArray();

			if (unwrappedExceptions.Length > 1)
			{
				throw new InvalidOperationException(
				    $"Exception {exception.GetType()} was unwrapped into more than 1 instance of other exceptions, but this method can return only one exception. Please call UnwrapExceptions that returns an enumerable to get all unwrapped exceptions");
			}

			return unwrappedExceptions[0];
		}
	}
}