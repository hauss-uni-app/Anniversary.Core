using Anniversary.Common.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anniversary.OuterClient.Extensions
{
    public static class WechatApiClientServiceCollectionSetup
    {
        public static IServiceCollection AddWechatApiClient(this IServiceCollection services)
        {
            services.AddHttpClient("wecaht")
                    .ConfigureHttpClient(config => {
                        config.BaseAddress = new Uri(Appsettings.app(new string[] {"Wechat", "Url" }));
                        config.Timeout = TimeSpan.FromSeconds(30);
                    });

            return services;
        }
    }
}
