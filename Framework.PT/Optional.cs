using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Framework
{
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        private readonly T value;

        public Optional(T value) : this()
        {
            HasValue = true;
            this.value = value;
        }

        public bool HasValue { get; private set; }

        public T Value
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException($"There is no value. Please check the {nameof(HasValue)} property before accessing {nameof(Value)}.");
                }

                return value;
            }
        }

        public T GetValueOrDefault(T defaultValue = default(T))
        {
            return HasValue ? Value : defaultValue;
        }

        public override string ToString()
        {
            return HasValue ? Convert.ToString(Value) : "<no value>";
        }

        #region Implicit Cast Operators

        public static implicit operator T(Optional<T> optional)
        {
            return optional.Value;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        #endregion

        #region Equality Members

        public bool Equals(Optional<T> other)
        {
            return HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return (obj is Optional<T> && Equals((Optional<T>)obj)) || (obj is T && HasValue && EqualityComparer<T>.Default.Equals(Value, (T)obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
            }
        }

        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}