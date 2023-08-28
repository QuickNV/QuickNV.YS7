using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Epoch.net;
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
            var ret = await rep.Content.ReadFromJsonAsync<ApiResult<T>>();
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
    }
}
