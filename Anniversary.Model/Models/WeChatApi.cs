using System;
using System.Collections.Generic;
using System.Text;

namespace Anniversary.Model.Models
{
    public class WeChatApi
    {
        public string expires_in { get; set; }
        public string openid { get; set; }
        public string session_key { get; set; }
    }
}
