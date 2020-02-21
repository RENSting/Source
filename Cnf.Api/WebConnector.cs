using Cnf.CodeBase.Serialize;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cnf.Api
{
    public class WebConnector
    {
        public const string DEFAULT = "default";

        string InvokeApi(string fullUrl, bool isPost, string contentType, string postData)
        {
            WebRequest request = WebRequest.Create(fullUrl);
            if (isPost)
            {
                request.Method = "POST";
                request.ContentType = contentType;
                byte[] buffer = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = buffer.Length;
                using (Stream post = request.GetRequestStream())
                {
                    post.Write(buffer, 0, buffer.Length);
                    post.Close();
                }
            }
            else
            {
                request.Method = "GET";
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd().ToString();
                }
            }
        }

        async Task<string> InvokeApiAsync(string fullUrl, bool isPost, string contentType, string postData)
        {
            WebRequest request = WebRequest.Create(fullUrl);
            if (isPost)
            {
                request.Method = "POST";
                request.ContentType = contentType;
                byte[] buffer = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = buffer.Length;
                using (Stream post = request.GetRequestStream())
                {
                    post.Write(buffer, 0, buffer.Length);
                    post.Close();
                }
            }
            else
            {
                request.Method = "GET";
            }

            using (WebResponse response = await request.GetResponseAsync())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd().ToString();
                }
            }
        }

        /// <summary>
        /// key: AppName, value: BaseUrl
        /// </summary>
        public Dictionary<string, string> Connectors { get; set; }

        public WebConnector()
        {
            Connectors = new Dictionary<string, string>();
        }

        public WebConnector(string appName, string baseUrl) : this()
        {
            AddConnector(appName, baseUrl);
        }

        public WebConnector(string defaultUrl) : this(DEFAULT, defaultUrl)
        {
            ;
        }

        public void AddConnector(string appName, string baseUrl) =>
            Connectors.Add(appName, baseUrl);

        string GetBaseUrl(string appName) => Connectors.ContainsKey(appName) ? Connectors[appName] : string.Empty;

        string BuildUrl(string appName, string route)
        {
            if (route.StartsWith("/"))
                route = route.Substring(1);

            return GetBaseUrl(appName).EndsWith("/") ? GetBaseUrl(appName) + route : GetBaseUrl(appName) + "/" + route;
        }

        /// <summary>
        /// 标准GET调用API，
        /// 输入：urls[api]/route
        /// 输出：Json(ApiResult)
        /// </summary>
        /// <typeparam name="TReturn">封装在调用返回结果中的数据类型</typeparam>
        /// <param name="appName">应用程序路径集合的key</param>
        /// <param name="route">Web API 的方法：controller/action?parameters</param>
        /// <returns></returns>
        public ApiResult<TReturn> Get<TReturn>(string appName, string route) where TReturn : new() =>
            SerializationHelper.JsonDeserialize<ApiResult<TReturn>>(InvokeApi(BuildUrl(appName, route), false, "", ""));

        /// <summary>
        /// 标准GET调用API，
        /// 输入：urls[api]/route
        /// 输出：Json(ApiResult)
        /// </summary>
        /// <typeparam name="TReturn">封装在调用返回结果中的数据类型</typeparam>
        /// <param name="appName">应用程序路径集合的key</param>
        /// <param name="route">Web API 的方法：controller/action?parameters</param>
        /// <returns></returns>
        public async Task<ApiResult<TReturn>> GetAsync<TReturn>(string appName, string route) where TReturn : new() =>
            SerializationHelper.JsonDeserialize<ApiResult<TReturn>>(await InvokeApiAsync(BuildUrl(appName, route), false, "", ""));

        /// <summary>
        /// GET调用API，采用默认（default）application url。
        /// </summary>
        /// <typeparam name="TReturn">封装在调用返回结果中的数据类型</typeparam>
        /// <param name="route"></param>
        /// <returns></returns>
        public ApiResult<TReturn> Get<TReturn>(string route) where TReturn : new() => Get<TReturn>(DEFAULT, route);

        /// <summary>
        /// GET调用API，采用默认（default）application url。
        /// </summary>
        /// <typeparam name="TReturn">封装在调用返回结果中的数据类型</typeparam>
        /// <param name="route"></param>
        /// <returns></returns>
        public async Task<ApiResult<TReturn>> GetAsync<TReturn>(string route) where TReturn : new() => await GetAsync<TReturn>(DEFAULT, route);

        /// <summary>
        /// 标准POST调用API，地址：urls[api]/route
        /// 输入：Json(T post)，
        /// 输出：Json(ApiResult<TReturn>)。
        /// </summary>
        /// <typeparamref name="T">POST数据的类型</typeparamref>
        /// <typeparamref name="TReturn">封装在调用返回结果中的数据类型</typeparamref>
        /// <param name="appName">应用程序路径集合的key</param>
        /// <param name="route">Web API方法：controller/action</param>
        /// <param name="post">POST的对象</param>
        /// <returns></returns>
        public ApiResult<TReturn> Post<T, TReturn>(string appName, string route, T post) 
            where T : new() where TReturn : new() =>
            SerializationHelper.JsonDeserialize<ApiResult<TReturn>>(
                InvokeApi(BuildUrl(appName, route), true, "application/json;charset=utf-8",
                    SerializationHelper.JsonSerialize(post)));

        /// <summary>
        /// 标准POST调用API，地址：urls[api]/route
        /// 输入：Json(T post)，
        /// 输出：Json(ApiResult<TReturn>)。
        /// </summary>
        /// <typeparamref name="T">POST数据的类型</typeparamref>
        /// <typeparamref name="TReturn">封装在调用返回结果中的数据类型</typeparamref>
        /// <param name="appName">应用程序路径集合的key</param>
        /// <param name="route">Web API方法：controller/action</param>
        /// <param name="post">POST的对象</param>
        /// <returns></returns>
        public async Task<ApiResult<TReturn>> PostAsync<T, TReturn>(string appName, string route, T post)
            where T : new() where TReturn : new() =>
            SerializationHelper.JsonDeserialize<ApiResult<TReturn>>(
                await InvokeApiAsync(BuildUrl(appName, route), true, "application/json;charset=utf-8",
                    SerializationHelper.JsonSerialize(post)));

        /// <summary>
        /// 向默认（default）应用发起POST调用
        /// </summary>
        /// <typeparamref name="T">POST数据的类型</typeparamref>
        /// <typeparamref name="TReturn">封装在调用返回结果中的数据类型</typeparamref>
        /// <returns></returns>
        public ApiResult<TReturn> Post<T, TReturn>(string route, T post)
            where T: new() where TReturn:new() =>
            Post<T, TReturn>(DEFAULT, route, post);

        /// <summary>
        /// 向默认（default）应用发起POST调用
        /// </summary>
        /// <typeparamref name="T">POST数据的类型</typeparamref>
        /// <typeparamref name="TReturn">封装在调用返回结果中的数据类型</typeparamref>
        /// <returns></returns>
        public async Task<ApiResult<TReturn>> PostAsync<T, TReturn>(string route, T post)
            where T : new() where TReturn : new() =>
            await PostAsync<T, TReturn>(DEFAULT, route, post);
    }
}
