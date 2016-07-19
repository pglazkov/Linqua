using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Framework
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, [NotNull] IEnumerable<T> itemsToAdd)
        {
            Guard.NotNull(itemsToAdd, nameof(itemsToAdd));

            if (collection == null)
            {
                return;
            }

            foreach (var item in itemsToAdd)
            {
                collection.Add(item);
            }
        }
    }
}