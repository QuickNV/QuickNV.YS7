using Newtonsoft.Json;
using QuickNV.YS7;

var ys7ClientOptions = JsonConvert.DeserializeObject<Ys7ClientOptions>(File.ReadAllText("config.user"));
var ys7Client = new Ys7Client(ys7ClientOptions);
ys7Client.NewAccessToken += (sender, e) =>
{
    Console.WriteLine($"[新AccessToken事件]值：{e.AccessToken},过期时间：{e.AccessTokenExpireTime}");
};
//获取设备列表
{
    Console.WriteLine("正在获取设备列表...");
    var ret = await ys7Client.GetDeviceListAsync();
    Console.WriteLine("获取设备列表成功，结果：" + JsonConvert.SerializeObject(ret, Formatting.Indented));
}
//获取通道列表
{
    Console.WriteLine("正在获取通道列表...");
    var ret = await ys7Client.GetChannels();
    Console.WriteLine("获取通道列表成功，结果：" + JsonConvert.SerializeObject(ret, Formatting.Indented));

    foreach (var channel in ret.data)
    {
        Console.WriteLine($"正在抓拍通道[{channel.channelName}]的图片...");
        var ret2 = await ys7Client.Capture(channel.deviceSerial, channel.channelNo);
        Console.WriteLine($"通道[{channel.channelName}]的图片地址：{ret2.data.picUrl}");
    }
}