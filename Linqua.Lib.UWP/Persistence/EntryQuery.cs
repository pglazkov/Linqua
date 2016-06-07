using System;
using System.Linq.Expressions;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
    public class EntryQuery
    {
        public EntryQuery([NotNull] string id, [NotNull] Expression<Func<Entry, bool>> expression)
        {
            Guard.NotNullOrEmpty(id, nameof(id));
            Guard.NotNull(expression, nameof(expression));

            Id = id;
            Expression = expression;
        }

        [NotNull]
        public string Id { get; private set; }

        [NotNull]
        public Expression<Func<Entry, bool>> Expression { get; private set; }

        #region Equality Members

        protected bool Equals(EntryQuery other)
        {
            return string.Equals(Id, other.Id) && Expression.Equals(other.Expression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EntryQuery)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ Expression.GetHashCode();
            }
        }

        #endregion
    }
}