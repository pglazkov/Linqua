using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Framework
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Applies the given function to each element of the sequence and returns the sequence comprised of the results for each element 
		/// where the function returns non-null <see cref="Nullable"/> with some value.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="sequence"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult?> func)
			where TResult : struct
		{
			return
				sequence
					.Select(func)
					.ChooseValues();
		}

		/// <summary>
		/// Applies the given function to each element of the sequence and returns the sequence comprised of the results for each element 
		/// where the function returns non-null <see cref="Nullable{T}"/> with some value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="sequence"></param>
		/// <returns></returns>
		public static IEnumerable<T> ChooseValues<T>(this IEnumerable<T?> sequence)
			where T : struct
		{
			return
				sequence
					.WhereNotNull()
					.Select(v => v.Value);
		}

		/// <summary>
		/// Applies the given function to each element of the sequence and returns the sequence comprised of the results for each element 
		/// where the function returns non-null value.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="sequence"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> sequence, Func<TSource, TResult> func)
			where TResult : class
		{
			return
				sequence
					.Select(func)
					.WhereNotNull();
		}

		public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
		{
			return
				sequence
					.Where(v => !predicate(v));
		}

		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> sequence)
			where T : class
		{
			return
				sequence
					.Where(v => v != null);
		}

		public static IEnumerable<T?> WhereNotNull<T>(this IEnumerable<T?> sequence)
			where T : struct
		{
			return
				sequence
					.Where(v => v.HasValue);
		}

		public static IEnumerable<object> WhereEx(this IEnumerable collection, Func<object, bool> filter)
		{
			foreach (var item in collection)
			{
				if (filter(item))
				{
					yield return item;
				}
			}
		}

		public static ILookup<TKey, TValue> EmptyLookup<TKey, TValue>()
		{
			return Enumerable.Empty<TValue>().ToLookup(e => default(TKey));
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
		{
			return keyValuePairs.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
		}

		public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs, IEqualityComparer<TKey> equalityComparer)
		{
			return keyValuePairs.ToLookup(kvp => kvp.Key, kvp => kvp.Value, equalityComparer);
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> sequence)
		{
			return new HashSet<T>(sequence);
		}

		public static HashSet<TKey> ToHashSet<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> itemSelector)
		{
			return new HashSet<TKey>(sequence.Select(itemSelector));
		}

		public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> sequence)
		{
			return ToStrings(sequence, item => item.ToString());
		}

		public static IEnumerable<T> UniqueBy<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
		{
			var group = comparer != null
							? sequence.GroupBy(keySelector, comparer)
							: sequence.GroupBy(keySelector);

			return group.Select(g => g.First());
		}

		public static IEnumerable<string> ToStrings<T>(this IEnumerable<T> sequence, Func<T, string> converter)
		{
			return sequence.Select(converter);
		}

		public static string FormatToString(this IEnumerable sequence)
		{
			if (sequence == null)
			{
				return string.Empty;
			}

			var result = string.Join(Environment.NewLine, sequence.Cast<object>().ToStrings());

			return result;
		}

		public static string FormatToString<T>(this IEnumerable<T> sequence, string emptyText = null)
		{
			if (sequence == null)
			{
				return string.Empty;
			}

			var result = string.Join(Environment.NewLine, sequence.ToStrings());

			if (string.IsNullOrWhiteSpace(result) && !string.IsNullOrEmpty(emptyText))
			{
				result = emptyText;
			}

			return result;
		}

		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> sequence)
		{
			if (sequence == null)
			{
				return null;
			}

			var result = sequence as ReadOnlyCollection<T> ?? new ReadOnlyCollection<T>(sequence.ToList());

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
		public static IEnumerable<T> Merge<T>(this IEnumerable<T> sequence1, IEnumerable<T> sequence2,
			IEqualityComparer<T> equalityComparer = null, Action<T> onIntersect = null)
		{
			equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
			onIntersect = onIntersect ?? new Action<T>(s => { });

			var addedItems = new Dictionary<T, T>(equalityComparer);

			foreach (var item in sequence1)
			{
				addedItems.Add(item, item);
				yield return item;
			}

			foreach (var item in sequence2)
			{
				if (!addedItems.ContainsKey(item))
				{
					yield return item;
				}
				else
				{
					onIntersect(addedItems[item]);
				}
			}
		}

		/// <summary>
		/// Compares two collections item by item. Two collections are equal when all its items are equal.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">Comparer to use in order to compare the elements in the collection</param>
		/// <returns>A value indicating whether the two collections are equal.</returns>
		public static bool ItemsEqual<T>(this IEnumerable<T> collection1, IEnumerable<T> collection2, IEqualityComparer<T> equalityComparer = null)
		{
			if (collection1 == null && collection2 == null)
			{
				return true;
			}

			if (collection1 == null || collection2 == null)
			{
				return false;
			}

			if (collection1.Count() != collection2.Count())
			{
				return false;
			}

			equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;

			foreach (var item in collection1)
			{
				if (!collection2.Contains(item, equalityComparer))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Compares two collections item by item. Two collections are equal when all its items are equal.
		/// </summary>
		/// <param name="collection1">The first collection.</param>
		/// <param name="collection2">The second collection.</param>
		/// <param name="equalityComparer">Comparer to use in order to compare the elements in the collection</param>
		/// <returns>A value indicating whether the two collections are equal.</returns>
		public static bool ItemsEqual(this IEnumerable collection1, IEnumerable collection2, IEqualityComparer equalityComparer = null)
		{
			if (collection1 == null && collection2 == null)
			{
				return true;
			}

			if (collection1 == null || collection2 == null)
			{
				return false;
			}

			if (collection1 is object[] && collection2 is object[])
			{
				return ItemsEqual(collection1 as object[], collection2 as object[], (IEqualityComparer<object>)equalityComparer);
			}

			return ItemsEqual(collection1.Cast<object>(), collection2.Cast<object>(), (IEqualityComparer<object>)equalityComparer);
		}

		/// <summary>
		/// Gets the hash code based on the collection items' hash codes, not the collection itself.
		/// Shall be used in combination with <see cref="ItemsEqual{T}"/>
		/// </summary>
		/// <typeparam name="T">Type of the collection items.</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="getItemHashCodeDelegate">An optional delegate that is used to compute the hash code of individual items.</param>
		/// <returns>Hash code calculated using the collection items.</returns>
		public static int GetItemsHashCode<T>(this IEnumerable<T> collection, Func<T, int> getItemHashCodeDelegate = null)
		{
			return GetItemsHashCode((IEnumerable)collection, getItemHashCodeDelegate != null ? i => getItemHashCodeDelegate((T)i) : (Func<object, int>)null);
		}

		/// <summary>
		/// Gets the hash code based on the collection items' hash codes, not the collection itself.
		/// Shall be used in combination with <see cref="ItemsEqual"/>
		/// </summary>
		/// <param name="collection">The collection.</param>
		/// <param name="getItemHashCodeDelegate">An optional delegate that is used to compute the hash code of individual items.</param>
		/// <returns>Hash code calculated using the collection items.</returns>
		public static int GetItemsHashCode(this IEnumerable collection, Func<object, int> getItemHashCodeDelegate = null)
		{
			if (collection == null)
			{
				return 0;
			}

			unchecked
			{
				getItemHashCodeDelegate = getItemHashCodeDelegate ?? (i => i.GetHashCode());

				int? result = null;

				foreach (var item in collection)
				{
					if (result == null)
					{
						result = getItemHashCodeDelegate(item);
					}
					else
					{
						result = (result * 397) ^ getItemHashCodeDelegate(item);
					}
				}

				return result ?? 0;
			}
		}

		/// <summary>
		/// Executes <paramref name="actionToExecute"/> for all items in the collection.
		/// </summary>
		/// <typeparam name="T">Type of items in the collection</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="actionToExecute">The action to execute.</param>
		public static void WithAll<T>(this IEnumerable<T> collection, Action<T> actionToExecute)
		{
			Contract.Requires(collection != null);
			Contract.Requires(actionToExecute != null);

			foreach (var item in collection)
			{
				actionToExecute(item);
			}
		}

		/// <summary>
		/// Appends the specified item to the enumerable sequence.
		/// </summary>
		/// <typeparam name="T">Type of items in the collection</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="itemToAdd">The item to add.</param>
		/// <returns></returns>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T itemToAdd)
		{
			Contract.Requires(collection != null);

			return collection.Concat(new T[] { itemToAdd });
		}

		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T item)
		{
			yield return item;
			foreach (T sequenceItem in sequence)
			{
				yield return sequenceItem;
			}
		}

		public static IEnumerable<TR> SelectSafe<T, TR>(this IEnumerable<T> sequence, Func<T, TR> mapper)
		{
			if (sequence == null)
				return Enumerable.Empty<TR>();
			return sequence.Select(mapper);
		}

		public static IEnumerable<T> SelectSafe<T>(this IEnumerable<T> sequence)
		{
			if (sequence == null)
				return Enumerable.Empty<T>();
			return sequence;
		}

		/// <summary>
		/// Returns first element in the sequence or throws an exception if the sequence is emply (exception 
		/// message is specified by <paramref name="noElementsExceptionMessage"/>).
		/// </summary>
		/// <typeparam name="T">Type of elements in the sequence.</typeparam>
		/// <param name="collection">The sequence.</param>
		/// <param name="noElementsExceptionMessage">The exception message if the sequence is empty.</param>
		/// <returns>First element in the sequence.</returns>
		public static T First<T>(this IEnumerable<T> collection, string noElementsExceptionMessage)
		{
			T result = collection.FirstOrDefault();

			if (Equals(result, default(T)))
			{
				throw new InvalidOperationException(noElementsExceptionMessage);
			}

			return result;
		}

		public static IEnumerable<T> ExceptOne<T>(this IEnumerable<T> collection, T itemToExclude)
		{
			Contract.Requires(collection != null);
			Contract.Requires(itemToExclude != null);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			return collection.Except(new[] { itemToExclude });
		}

		public static bool TryGetFirst<T>(this IEnumerable<T> sequence, out T firstItem)
		{
			Contract.Requires(sequence != null);

			firstItem = default(T);
			using (IEnumerator<T> current = sequence.GetEnumerator())
			{
				if (current.MoveNext())
				{
					firstItem = current.Current;
					return true;
				}
			}

			return false;
		}

		public static bool TryGetLast<T>(this IEnumerable<T> sequence, out T lastItem)
		{
			Contract.Requires(sequence != null);

			bool hasAtLeastOne = false;
			lastItem = default(T);

			using (IEnumerator<T> current = sequence.GetEnumerator())
			{
				while (current.MoveNext())
				{
					hasAtLeastOne = true;
					lastItem = current.Current;
				}
			}
			return hasAtLeastOne;
		}

		public static bool None<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
		{
			return sequence.Any(predicate) == false;
		}

		public static T Fold<T, U>(this IEnumerable<U> list, Func<T, U, T> func, T acc)
		{
			foreach (var item in list)
			{
				acc = func(acc, item);
			}

			return acc;
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
		{
			return enumerable == null || !enumerable.Any();
		}

		public static T CommonValue<T>(this IEnumerable<T> enumerable)
		{
			if (enumerable == null)
			{
				return default(T);
			}

			var result = enumerable.Aggregate((x1, x2) => Equals(x1, x2) ? x1 : default(T));

			return result;
		}

		public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
		{
			return k == 0
					   ? new[] { new T[0] }
					   : elements.SelectMany((e, i) =>
											 elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
		}

		public static bool In<T>(this T value, params T[] inList) where T : struct
		{
			return inList.Any(lv => value.Equals(lv));
		}

		public static IEnumerable<T> UnZip<T>(this IEnumerable<T> items, Predicate<T> predicate, out IEnumerable<T> whenIsNotTrue)
		{
			var lookup = items.ToLookup(i => predicate(i));
			whenIsNotTrue = lookup[false];
			return lookup[true];
		}

		public static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, int count, int skip)
		{
			Queue<IList<TSource>> buffers = new Queue<IList<TSource>>();
			int i = 0;
			foreach (TSource source1 in source)
			{
				if (i % skip == 0)
					buffers.Enqueue((IList<TSource>)new List<TSource>(count));
				foreach (ICollection<TSource> collection in buffers)
				{
					collection.Add(source1);
				}
				if (buffers.Count > 0 && buffers.Peek().Count == count)
					yield return buffers.Dequeue();
				++i;
			}
			while (buffers.Count > 0)
			{
				yield return buffers.Dequeue();
			}
		}

		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null)
			{
				return;
			}

			foreach (var item in items)
			{
				action(item);
			}
		}
	}
}