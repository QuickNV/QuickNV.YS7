using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Epoch.net;
using Newtonsoft.Json;
using QuickNV.YS7.Events;
using QuickNV.YS7.Model;

namespace QuickNV.YS7
{
    public class Ys7Client
    {
        private Ys7ClientOptions options;
        private string accessToken { get; set; }
        private DateTime accessTokenExpireTime { get; set; }
        private HttpClient httpClient { get; set; }

        public event EventHandler<NewAccessTokenEventArgs> NewAccessToken;

        public Ys7Client(Ys7ClientOptions options)
        {
            this.options = options;
            accessToken = options.AccessToken;
            accessTokenExpireTime = options.AccessTokenExpireTime;

            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(options.ServerUrl);
        }

        //获取可用的AccessToken
        private async Task<Dictionary<string, string>> fillAccessTokenAsync(Dictionary<string, string> dict)
        {
            //如果10分钟内过期，则更新AccessToken
            if ((accessTokenExpireTime - DateTime.Now).TotalMinutes < 10)
            {
                var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["appKey"] = options.AppKey,
                    ["appSecret"] = options.Secret
                });
                var rep = await httpClient.PostAsync("/api/lapp/token/get", httpContent);
                if (!rep.IsSuccessStatusCode)
                    throw new IOException($"{rep.StatusCode} {rep.ReasonPhrase}");
                var ret = await rep.Content.ReadFromJsonAsync<ApiResult<AccessTokenInfo>>();
                if (!ret.IsSuccess)
                    throw new IOException($"{ret.code} {ret.msg}");
                accessToken = ret.data.accessToken;
                accessTokenExpireTime = ret.data.expireTime.ToLongEpochTime().DateTime.ToLocalTime();
                NewAccessToken?.Invoke(this, new NewAccessTokenEventArgs()
                {
                    AccessToken = accessToken,
                    AccessTokenExpireTime = accessTokenExpireTime
                });
            }
            dict[nameof(accessToken)] = accessToken;
            return dict;
        }

        private async Task<ApiResult<T>> InvokeApiAsync<T>(string apiUrl, Dictionary<string, string> dict)
        {
            var httpContent = new FormUrlEncodedContent(await fillAccessTokenAsync(dict));
            var rep = await httpClient.PostAsync(apiUrl, httpContent);
            if (!rep.IsSuccessStatusCode)
                throw new IOException($"{rep.StatusCode} {rep.ReasonPhrase}");
            var json = await rep.Content.ReadAsStringAsync();
            var ret = JsonConvert.DeserializeObject<ApiResult<T>>(json);
            if (!ret.IsSuccess)
                throw new IOException($"{ret.code} {ret.msg}");
            return ret;
        }

        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="pageStart">分页起始页，从0开始</param>
        /// <param name="pageSize">分页大小，默认为10，最大为50</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public async Task<ApiResult<DeviceInfo[]>> GetDeviceListAsync(int pageStart = 0, int pageSize = 10)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(pageStart)] = pageStart.ToString(),
                [nameof(pageSize)] = pageSize.ToString()
            };
            return await InvokeApiAsync<DeviceInfo[]>("/api/lapp/device/list", dict);
        }

        /// <summary>
        /// 获取指定设备通道列表
        /// </summary>
        /// <param name="deviceSerial">设备序列号</param>
        /// <returns></returns>
        public async Task<ApiResult<ChannelInfo[]>> GetDeviceChannels(string deviceSerial)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial
            };
            return await InvokeApiAsync<ChannelInfo[]>("/api/lapp/device/camera/list", dict);
        }


        /// <summary>
        /// 获取通道列表
        /// </summary>
        /// <param name="pageStart">分页起始页，从0开始</param>
        /// <param name="pageSize">分页大小，默认为10，最大为50</param>
        /// <returns></returns>
        public async Task<ApiResult<ChannelInfo[]>> GetChannels(int pageStart = 0, int pageSize = 10)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(pageStart)] = pageStart.ToString(),
                [nameof(pageSize)] = pageSize.ToString()
            };
            return await InvokeApiAsync<ChannelInfo[]>("/api/lapp/camera/list", dict);
        }

        /// <summary>
        /// 抓拍
        /// </summary>
        /// <param name="deviceSerial">设备序列号</param>
        /// <param name="channelNo">通道编号</param>
        /// <param name="quality">视频清晰度,0-流畅,1-高清(720P),2-4CIF,3-1080P,4-400w</param>
        /// <returns></returns>
        public async Task<ApiResult<CaptureInfo>> Capture(string deviceSerial, int channelNo, int? quality = null)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial,
                [nameof(channelNo)] = channelNo.ToString()
            };
            if (quality.HasValue)
                dict[nameof(quality)] = quality.ToString();
            return await InvokeApiAsync<CaptureInfo>("/api/lapp/device/capture", dict);
        }

        /// <summary>
        /// 获取播放地址
        /// </summary>
        /// <param name="deviceSerial">设备序列号例如427734222，均采用英文符号，限制最多50个字符</param>
        /// <param name="channelNo">通道号，非必选，默认为1</param>
        /// <param name="protocol">流播放协议，1-ezopen、2-hls、3-rtmp、4-flv，默认为1</param>
        /// <param name="code">ezopen协议地址的设备的视频加密密码</param>
        /// <param name="expireTime">过期时长，单位秒；针对hls/rtmp/flv设置有效期，相对时间；30秒-720天</param>
        /// <param name="type">地址的类型，1-预览，2-本地录像回放，3-云存储录像回放，非必选，默认为1；回放仅支持rtmp、ezopen、flv协议</param>
        /// <param name="quality">视频清晰度，1-高清（主码流）、2-流畅（子码流）</param>
        /// <param name="startTime">本地录像/云存储录像回放开始时间,云存储开始结束时间必须在同一天，示例：2019-12-01 00:00:00</param>
        /// <param name="stopTime">本地录像/云存储录像回放结束时间,云存储开始结束时间必须在同一天，示例：2019-12-01 23:59:59</param>
        /// <param name="supportH265">请判断播放端是否要求播放视频为H265编码格式,1表示需要，0表示不要求</param>
        /// <param name="playbackSpeed">回放倍速。倍速为 -1（ 支持的最大倍速）、0.5、1、2、4、8、16；仅支持protocol为4-flv且type为2-本地录像回放（ 部分设备可能不支持16倍速） 或者 3-云存储录像回放</param>
        /// <param name="gbchannel">国标设备的通道编号，视频通道编号ID</param>
        /// <returns></returns>
        public async Task<ApiResult<LiveAddressInfo>> GetLiveAddress(
            string deviceSerial,
            int? channelNo = null,
            VideoProtocol? protocol = null,
            string code = null,
            int? expireTime = null,
            string type = null,
            VideoQuality? quality = null,
            DateTime? startTime = null,
            DateTime? stopTime = null,
            int? supportH265 = null,
            string playbackSpeed = null,
            string gbchannel = null)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial,
            };
            if (channelNo.HasValue)
                dict[nameof(channelNo)] = channelNo.ToString();
            if (protocol.HasValue)
                dict[nameof(protocol)] = ((int)protocol.Value).ToString();
            if (!string.IsNullOrEmpty(code))
                dict[nameof(code)] = code;
            if (expireTime.HasValue)
                dict[nameof(expireTime)] = expireTime.ToString();
            if (!string.IsNullOrEmpty(type))
                dict[nameof(type)] = type;
            if (quality.HasValue)
                dict[nameof(quality)] = ((int)quality.Value).ToString();
            if (startTime.HasValue)
                dict[nameof(startTime)] = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (stopTime.HasValue)
                dict[nameof(stopTime)] = stopTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (supportH265.HasValue)
                dict[nameof(supportH265)] = supportH265.ToString();
            if (!string.IsNullOrEmpty(playbackSpeed))
                dict[nameof(playbackSpeed)] = playbackSpeed;
            if (!string.IsNullOrEmpty(gbchannel))
                dict[nameof(gbchannel)] = gbchannel;
            return await InvokeApiAsync<LiveAddressInfo>("/api/lapp/v2/live/address/get", dict);
        }

        /// <summary>
        /// 失效播放地址
        /// </summary>
        /// <param name="deviceSerial">设备序列号例如427734222，均采用英文符号，限制最多50个字符</param>
        /// <param name="channelNo">通道号</param>
        /// <param name="urlId">直播地址Id</param>
        /// <returns></returns>
        public async Task<ApiResult<object>> DisableLiveAddress(string deviceSerial, int channelNo, string urlId)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial,
                [nameof(channelNo)] = channelNo.ToString(),
                [nameof(urlId)] = urlId,
            };
            return await InvokeApiAsync<object>("/api/lapp/v2/live/address/disable", dict);
        }

        /// <summary>
        /// 开始云台控制
        /// </summary>
        /// <param name="deviceSerial">设备序列号例如427734222，均采用英文符号，限制最多50个字符</param>
        /// <param name="channelNo">通道号</param>
        /// <param name="direction">操作命令：0-上，1-下，2-左，3-右，4-左上，5-左下，6-右上，7-右下，8-放大，9-缩小，10-近焦距，11-远焦距，16-自动控制</param>
        /// <param name="speed">云台速度：0-慢，1-适中，2-快，海康设备参数不可为0</param>
        /// <returns></returns>
        public async Task<ApiResult<object>> StartPtz(string deviceSerial, int channelNo, PTZCommand direction, int speed)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial,
                [nameof(channelNo)] = channelNo.ToString(),
                [nameof(direction)] = ((int)direction).ToString(),
                [nameof(speed)] = speed.ToString()
            };
            return await InvokeApiAsync<object>("/api/lapp/device/ptz/start", dict);
        }

        /// <summary>
        /// 停止云台控制
        /// </summary>
        /// <param name="deviceSerial">设备序列号例如427734222，均采用英文符号，限制最多50个字符</param>
        /// <param name="channelNo">通道号</param>
        /// <param name="direction">操作命令：0-上，1-下，2-左，3-右，4-左上，5-左下，6-右上，7-右下，8-放大，9-缩小，10-近焦距，11-远焦距，16-自动控制</param>
        /// <returns></returns>
        public async Task<ApiResult<object>> StopPtz(string deviceSerial, int channelNo, PTZCommand? direction)
        {
            var dict = new Dictionary<string, string>
            {
                [nameof(deviceSerial)] = deviceSerial,
                [nameof(channelNo)] = channelNo.ToString()
            };
            if (direction.HasValue)
                dict[nameof(direction)] = ((int)direction.Value).ToString();
            return await InvokeApiAsync<object>("/api/lapp/device/ptz/stop", dict);
        }
    }
}
