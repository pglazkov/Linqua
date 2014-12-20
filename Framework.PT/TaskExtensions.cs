using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework
{
	public static class TaskExtensions
	{
		 public static void FireAndForget(this Task task)
		 {
			 Guard.NotNull(task, () => task);

			 SynchronizationContext ctx = SynchronizationContext.Current;

			 task.ContinueWith(t =>
				 {
					 if (t.IsFaulted)
					 {
						 if (ctx != null)
						 {
							 ctx.Post(_ =>
								 {
									 throw new AggregateException(t.Exception);
								 }, null);
						 }
					 }
				 });
		 }
	}
}