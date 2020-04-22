using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Model.Models
{
    public class WeChatApiPara
    {
        public string appid { get; set; }
        public string secret { get; set; }
        public string js_code { get; set; }

        public string grant_type = "authorization_code";
    }
}
