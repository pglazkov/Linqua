using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
    public class OfflineSyncArguments
    {
        public static readonly OfflineSyncArguments Default = new OfflineSyncArguments();

        [CanBeNull]
        public EntryQuery Query { get; set; }

        public bool PurgeCache { get; set; }

        #region Equality Members

        protected bool Equals(OfflineSyncArguments other)
        {
            return Equals(Query, other.Query) && PurgeCache == other.PurgeCache;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OfflineSyncArguments)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Query != null ? Query.GetHashCode() : 0) * 397) ^ PurgeCache.GetHashCode();
            }
        }

        #endregion
    }
}