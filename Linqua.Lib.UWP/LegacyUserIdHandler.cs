using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Linqua
{
    public class LegacyUserIdHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (LegacyUserId.Value != null)
            {
                request.Headers.Add(LegacyUserId.HeaderKey, LegacyUserId.Value);
            }

            // Do the request
            return base.SendAsync(request, cancellationToken);
        }
    }
}
