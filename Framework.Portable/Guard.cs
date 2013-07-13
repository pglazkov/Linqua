using System;
using System.Linq.Expressions;

namespace Framework
{
	public static class Guard
	{
		public static void NotNull<T>(Expression<Func<T>> parameterExpression)
		{
			if (Equals(parameterExpression.Compile()(), default(T)))
			{
				var param = PropertySupport.ExtractPropertyName(parameterExpression, false);

				throw new ArgumentNullException(param);
			}
		}

		public static void Condition(bool condition, string paramName, string message)
		{
			if (!condition)
			{
				throw new ArgumentException(message, paramName);
			}
		}
	}
}