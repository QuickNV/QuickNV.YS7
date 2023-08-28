using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.YS7
{
    public class Ys7ClientOptions
    {
        public string ServerUrl { get; set; } = "https://open.ys7.com";
        
        public string AppKey { get; set; }
        public string Secret { get; set; }

        public string AccessToken { get; set; }
        public DateTime AccessTokenExpireTime { get; set; }
    }
}
