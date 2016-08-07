using System.Threading.Tasks;

namespace Linqua.Translation.Microsoft
{
    public interface IMicrosoftAccessTokenProvider
    {
        Task<string> GetAccessTokenAsync();
    }
}