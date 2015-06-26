using System;
using System.Reactive;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Framework
{
	/// <summary>
	/// An implementation of <see cref="IObservable{T}"/> that represents an abstract synchronization event.
	/// An instance of this class can be exposed to the clients as <see cref="IObservable{T}"/> that they can observe to wait for a certain result to be ready.
	/// The process of producing the result is marked by the owner class by calling the <see cref="Reset"/> and the <see cref="Publish"/> methods.
	/// If the event was already generated and no new process is started then the subscribers will immidiately receive the latest produced event upon subscription. 
	/// If the event hasn't been published or the process is in progress then the subscribers will wait for the event.
	/// </summary>
	/// <typeparam name="T">Type of the event data.</typeparam>
	public class ObservableSyncEvent<T> : IObservable<T>, IDisposable
	{
		private readonly string name;
		private readonly object syncRoot = new object();
		private bool isInitialBlockCompleted;
		private bool isInProgress = true;
		private ReplaySubject<T> currentSubject = new ReplaySubject<T>();

		public ObservableSyncEvent([NotNull] string name)
		{
			Guard.NotNullOrEmpty(name, () => name);

			this.name = name;
		}

		public bool CanPublish
		{
			get { return isInProgress; }
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			return currentSubject.Subscribe(observer);
		}

		public void Reset()
		{
			lock (syncRoot)
			{
				Cancel();

				if (isInitialBlockCompleted)
				{
					currentSubject = new ReplaySubject<T>();
				}

				isInProgress = true;
			}
		}

		public void Publish(T result)
		{
			lock (syncRoot)
			{
				if (!CanPublish)
				{
					throw new InvalidOperationException("You must call Reset first before calling this method.");
				}

				currentSubject.OnNext(result);

				OnCompleted();
			}
		}

		public void Cancel()
		{
			lock (syncRoot)
			{
				if (isInProgress)
				{
					currentSubject.OnError(new OperationCanceledException(string.Format("Current \"{0}\" event has been cancelled. Subscribe again to receive further notifications.", name)));

					OnCompleted();
				}
			}
		}

		public void Exception(Exception exception)
		{
			Guard.NotNull(exception, () => exception);

			lock (syncRoot)
			{
				if (isInProgress)
				{
					currentSubject.OnError(exception);

					OnCompleted();
				}
			}
		}

		private void OnCompleted()
		{
			currentSubject.OnCompleted();

			isInProgress = false;
			isInitialBlockCompleted = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Cancel();
				currentSubject.Dispose();
			}
		}

		~ObservableSyncEvent()
		{
			Dispose(false);
		}
	}

	public class ObservableSyncEvent : ObservableSyncEvent<Unit>
	{
		public ObservableSyncEvent([NotNull] string name)
			: base(name)
		{
		}

		public void Publish()
		{
			Publish(Unit.Default);
		}
	}
}