using System;
using System.Composition;
using System.Net;
using System.Threading.Tasks;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Linqua.Persistence
{
    [Export(typeof(IMobileServiceSyncHandler))]
    public class SyncHandler : MobileServiceSyncHandler
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<SyncHandler>();

        public override async Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.Status == HttpStatusCode.Conflict)
                {
                    await error.CancelAndUpdateItemAsync(error.Result);
                    error.Handled = true;
                }
            }

            await base.OnPushCompleteAsync(result);
        }

        public override async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
        {
            MobileServicePreconditionFailedException ex = null;
            JObject result = null;

            do
            {
                ex = null;
                try
                {
                    result = await base.ExecuteTableOperationAsync(operation);
                }
                catch (MobileServicePreconditionFailedException e)
                {
                    ExceptionHandlingHelper.HandleNonFatalError(e, "Synchronization failed. Looks like there is a conflict.");

                    ex = e;
                }
                catch (MobileServiceConflictException e)
                {
                    ExceptionHandlingHelper.HandleNonFatalError(e, "Synchronization failed. Looks like there is a conflict.");

                    throw;
                }
                catch (MobileServiceInvalidOperationException e)
                {
                    ExceptionHandlingHelper.HandleNonFatalError(e, "Synchronization failed. An unexpected exception occured.");

                    throw;
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