using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.YS7.Events
{
    public class NewAccessTokenEventArgs:EventArgs
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpireTime { get; set; }
    }
}
