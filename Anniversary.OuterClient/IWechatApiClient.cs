using Anniversary.Model;
using System.Threading.Tasks;

namespace Anniversary.OuterClient
{
    public interface IWechatApiClient
    {
        Task<MessageModel<string>> GetOpenIDAsync(string appid, string secret, string code);
    }
}