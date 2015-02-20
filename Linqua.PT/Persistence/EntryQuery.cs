using System;
using System.Linq.Expressions;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	public class EntryQuery
	{
		public EntryQuery([NotNull] string id, [NotNull] Expression<Func<ClientEntry, bool>> expression)
		{
			Guard.NotNullOrEmpty(id, () => id);
			Guard.NotNull(expression, () => expression);

			Id = id;
			Expression = expression;
		}

		[NotNull]
		public string Id { get; private set; }

		[NotNull]
		public Expression<Func<ClientEntry, bool>> Expression { get; private set; } 
	}
}