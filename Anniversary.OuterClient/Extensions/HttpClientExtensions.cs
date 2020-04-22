using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Anniversary.OuterClient.Extensions
{
    /// <summary>
    /// HttpClient扩展方法
    /// </summary>

    public class HttpClientExtensions
    {

        /// <summary>
        /// httpclient-post方法的简单处理，封装成一个方法，便于调用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_client"></param>
        /// <param name="_logger"></param>
        /// <param name="actionUrl">http://xxx.com/后面之后的地址</param>
        /// <param name="param">一个对象</param>
        /// <param name="ContentType"></param>
        /// <param name="BearerToken"></param>
        /// <returns></returns>
        public async static Task<T> PostData<T>(HttpClient _client, ILogger _logger, string actionUrl, dynamic param, string ContentType = "application/json", string BearerToken = "")
        {
            string funName = "PostData";
            string paramStr = JsonConvert.SerializeObject(param);
            string jrclientguid = Guid.NewGuid().ToString("n");
            try
            {
                _logger.LogInformation($"{funName}开始，url={_client.BaseAddress},action={actionUrl},postData={paramStr} ,jrclientguid={jrclientguid}---------");

                HttpResponseMessage response;
                using (HttpContent httpContent = new StringContent(paramStr, Encoding.UTF8))
                {
                    if (!string.IsNullOrWhiteSpace(BearerToken))
                    {
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BearerToken);
                    }

                    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ContentType);

                    response = await _client.GetAsync(actionUrl, httpContent);

                }
                if (response != null && response.IsSuccessStatusCode)
                {
                    Type t = typeof(T);
                    if (typeof(T) == typeof(string))
                    {
                        string respStr = await response.Content.ReadAsStringAsync();
                        return (T)Convert.ChangeType(respStr, typeof(T));
                    }
                    else
                    {
                        string respStr = response.Content.ReadAsStringAsync().Result;
                        T resp = JsonConvert.DeserializeObject<T>(respStr);

                        return resp;
                    }
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{funName}报错，url={_client.BaseAddress},action={actionUrl},postData={paramStr} ,jrclientguid={jrclientguid}---,ex={ex.Message}");
                throw;
            }
            finally
            {
                _logger.LogInformation($"{funName}结束，url={_client.BaseAddress},action={actionUrl},postData={paramStr} ,jrclientguid={jrclientguid}---------");
            }

        }
    }
}
