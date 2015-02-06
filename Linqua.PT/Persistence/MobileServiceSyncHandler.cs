using System.Composition;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Linqua.Persistence
{
	[Export(typeof(IMobileServiceSyncHandler))]
	public class MobileServiceSyncHandler : IMobileServiceSyncHandler
	{
		public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
		{
			return Task.FromResult(0);
		}

		public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
		{
			MobileServicePreconditionFailedException ex = null;
			JObject result = null;

			do
			{
				ex = null;
				try
				{
					result = await operation.ExecuteAsync();
				}
				catch (MobileServicePreconditionFailedException e)
				{
					ex = e;
				}

				if (ex != null)
				{
					// There was a conflict in the server

					var serverItem = ex.Value;
					// The server item will be returned in the `Value` property of the exception
					// object, but there are some scenarios in the .NET runtime where this is
					// not the case, so if the object wasn't returned, we retrieve it.
					if (serverItem == null)
					{
						serverItem = (JObject)(await operation.Table.LookupAsync((string)operation.Item["id"]));
					}

					// Update the version of the pending item so that it won't have another
					// conflict when the operation is retried.
					operation.Item[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];
				}
			} while (ex != null);

			return result;
		}
	}
}