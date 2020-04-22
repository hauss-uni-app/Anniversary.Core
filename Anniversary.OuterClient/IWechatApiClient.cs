using Anniversary.Model;
using Anniversary.Model.Models;
using System.Threading.Tasks;

namespace Anniversary.OuterClient
{
    public interface IWechatApiClient
    {
        Task<MessageModel<WeChatApi>> GetOpenIDAsync(string appid, string secret, string code);
    }
}