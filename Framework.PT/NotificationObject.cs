using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Framework
{
    /// <summary>
    ///     Base class for items that support property notification.
    /// </summary>
    /// <remarks>
    ///     This class provides basic support for implementing the <see cref="INotifyPropertyChanged" /> interface and for
    ///     marshalling execution to the UI thread.
    /// </remarks>
    public abstract class NotificationObject : INotifyPropertyChanged
    {
        /// <summary>
        ///     Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
            Justification = "Method used to raise an event")]
		[NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event for each of the properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new value.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null) throw new ArgumentNullException("propertyNames");

            foreach (string name in propertyNames)
            {
// ReSharper disable ExplicitCallerInfoArgument
                RaisePropertyChanged(name);
// ReSharper restore ExplicitCallerInfoArgument
            }
        }

        /// <summary>
        ///     Raises this object's PropertyChanged event.
        /// </summary>
        /// <typeparam name="T">The type of the property that has a new value</typeparam>
        /// <param name="propertyExpression">A Lambda expression representing the property that has a new value.</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Method used to raise an event")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Cannot change the signature")]
		[NotifyPropertyChangedInvocator]
        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            string propertyName = PropertySupport.ExtractPropertyName(propertyExpression);
// ReSharper disable ExplicitCallerInfoArgument
            RaisePropertyChanged(propertyName);
// ReSharper restore ExplicitCallerInfoArgument
        }
    }
}