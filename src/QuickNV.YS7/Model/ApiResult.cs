using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.YS7.Model
{
    public class ApiResult<T>
    {
        public string code { get; set; }
        public string msg { get; set; }
        public PageInfo page { get; set; }
        public T data { get; set; }
        public bool IsSuccess => "200" == code;
    }
}
