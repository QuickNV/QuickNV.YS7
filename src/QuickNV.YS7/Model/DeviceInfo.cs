using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.YS7.Model
{
    public class DeviceInfo
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string deviceSerial { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string deviceName { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string deviceType { get; set; }
        /// <summary>
        /// 在线状态：0-不在线，1-在线
        /// </summary>
        public DeviceStatus status { get; set; }
        /// <summary>
        /// 具有防护能力的设备布撤防状态：0-睡眠，8-在家，16-外出，普通IPC布撤防状态：0-撤防，1-布防
        /// </summary>
        public int defence { get; set; }
        /// <summary>
        /// 设备版本号
        /// </summary>
        public string deviceVersion { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public long addTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public long updateTime { get; set; }
        /// <summary>
        /// 设备二级类目
        /// </summary>
        public string parentCategory { get; set; }
        /// <summary>
        /// 设备风险安全等级，0-安全，大于零，有风险，风险越高，值越大
        /// </summary>
        public int riskLevel { get; set; }
        /// <summary>
        /// 设备IP地址
        /// </summary>
        public string netAddress { get; set; }
    }
}
