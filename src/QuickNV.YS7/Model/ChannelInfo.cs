namespace QuickNV.YS7.Model
{
    public class ChannelInfo
    {
        /// <summary>
        /// 设备序列号
        /// </summary>
        public string deviceSerial { get; set; }
        /// <summary>
        /// IPC序列号
        /// </summary>
        public string ipcSerial { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public int channelNo { get; set; }
        /// <summary>
        /// 设备名
        /// </summary>
        public string deviceName { get; set; }
        /// <summary>
        /// 设备上报名称
        /// </summary>
        public string localName { get; set; }
        /// <summary>
        /// 通道名
        /// </summary>
        public string channelName { get; set; }
        /// <summary>
        /// 图片地址（大图），若在萤石客户端设置封面则返回封面图片，未设置则返回默认图片
        /// </summary>
        public string picUrl { get; set; }
        /// <summary>
        /// 是否加密，0：不加密，1：加密
        /// </summary>
        public int? isEncrypt { get; set; }
        /// <summary>
        /// 视频质量：0-流畅，1-均衡，2-高清，3-超清
        /// </summary>
        public int? videoLevel { get; set; }
        /// <summary>
        /// 当前通道是否关联IPC：true-是，false-否。设备未上报或者未关联都是false
        /// </summary>
        public bool? relatedIpc { get; set; }
        /// <summary>
        /// 是否显示，0：隐藏，1：显示
        /// </summary>
        public int? isAdd { get; set; }
        /// <summary>
        /// camera设备类型
        /// </summary>
        public string devType { get; set; }
    }
}
