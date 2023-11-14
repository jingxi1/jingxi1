using RestSharp;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Authentication;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace restlib;



public static class RestUtilityLib
{
    /// <summary>
    /// 构建client
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public static RestClient BuildClient(this string baseUrl, Action<RestClientOptions>? option = null)
    {
        var opt = new RestClientOptions(baseUrl);
        option?.Invoke(opt);
        return new RestClient(opt);
    }
    public static RestClientOptions NSSLOption() => new RestClientOptions()
    {
        RemoteCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true
    };


    /// <summary>
    /// 构建client 依赖注入
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public static RestClient BuildClient(this IServiceProvider sp, Action<IServiceProvider, RestClientOptions>? option = null)
    {
        var opt = new RestClientOptions();
        option?.Invoke(sp, opt);
        return new RestClient(sp.GetRequiredService<HttpClient>());
    }

    public static RestResponse AttachClientToExecute(this RestRequest request, RestClient client)
    {
        return client.Execute(request);
    }
    public static string ResponseString(this RestResponse response)
    {
        return response.Content!;
    }
    /// <summary>
    /// 处理body,当body对象不为空则加入request
    /// </summary>
    /// <param name="req"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static RestRequest BodyProc(this RestRequest req, object body = null)
    {
        return body == null ? req : req.AddBody(body);
    }
    /// <summary>
    /// 对reqest处理身份认证,bearer
    /// </summary>
    /// <param name="request"></param>
    /// <param name="BearerToken"></param>
    /// <returns></returns>
    public static RestRequest RequestAddAuth(this RestRequest request, string BearerToken)
    {
        request.Authenticator = new RestSharp.Authenticators.OAuth2.
           OAuth2AuthorizationRequestHeaderAuthenticator(BearerToken, "Bearer");
        return request;
    }
    /// <summary>
    /// 新建request,默认为get
    /// </summary>
    /// <param name="source"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public static RestRequest NewRequest(this string source, Method method = Method.Get)
        => new RestRequest(source, method);
    /// <summary>
    /// 处理异步对泛型类型的支持
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task<T> GetTaskResultTAsync<T>(this Task<RestResponse> response) => 
        JsonSerializer.Deserialize<T>((await response).Content!)!;
    /// <summary>
    /// 处理结构,返回string
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task<string> Response2StringAsyncWithLog(this Task<RestResponse> response, ILogger logger = null/*,RestClient client, Func<RestClient, string>? GetTokenFunc = null*/)
    {
        var rp = await response;
        if (rp.StatusCode == System.Net.HttpStatusCode.OK || rp.StatusCode == System.Net.HttpStatusCode.Accepted
            || rp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger?.LogInformation($"{rp.StatusCode.ToString()}  {DateTimeOffset.Now.ToUnixTimeSeconds()} {rp.Request.Method.ToString()}=>{rp.ResponseUri.ToString()}");
            return ((rp).Content)!;
        }
        //else if (rp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //{
        //    throw new UnauthorizedAccessException();
        //}
        else
        {
            logger?.LogCritical($"rest lib unknown issue of {rp.StatusCode.ToString()} {rp.Request.Resource} {rp.Request.Method.ToString()}");
            return ((rp).Content)!;
        }
    }

    /// <summary>
    /// 处理请求,当认证失败时,调用GetTokenFunc,重新获取token,并重新执行请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="client"></param>
    /// <param name="GetTokenFunc"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public static async Task<RestResponse> ExeTaskResponseAsync(this RestRequest request, RestClient client, Func<RestClient, string>? GetTokenFunc = null)
    {
        var rp = await client.ExecuteAsync(request);
        //if (rp.StatusCode == System.Net.HttpStatusCode.OK || rp.StatusCode == System.Net.HttpStatusCode.Accepted
        //               || rp.StatusCode == System.Net.HttpStatusCode.NotFound)
        //{
        //    return rp;//((rp).Content)!;
        //}
        if (rp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return GetTokenFunc == null
                ? throw new UnauthorizedAccessException("NXApi Authorization Access Faild.")
                : await client.ExecuteAsync(request.RequestAddAuth(GetTokenFunc(client)));
        }
        else
        {
            return rp;
        }
    }

    /// <summary>
    /// 处理结构,返回jsonnode
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static async Task<JsonNode> Response2JsonNodeAsync(this Task<RestResponse> response, ILogger logger = null) =>
        (await response.Response2StringAsyncWithLog(logger)).string2Node();
    /// <summary>
    /// 同步处理jsonnode
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static JsonNode Response2JsonNode(this RestResponse response) => 
        response.ResponseString().string2Node();
    /// <summary>
    /// 异步处理jsonnode
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static async Task<JsonNode> String2JsonNodeAsync(this Task<string> str) =>
        (await str).string2Node();
    static JsonNode string2Node(this string s) =>
        s != "" ? JsonNode.Parse(s)! : JsonNode.Parse("{}")!;
    public static JsonNode ToJsonNode(this string str) => (str) != "" ? JsonNode.Parse(str)! : JsonNode.Parse("{}")!;
    /// <summary>
    /// 将当前node加入到一个空的节点中,标记头
    /// </summary>
    /// <param name="node"></param>
    /// <param name="head"></param>
    /// <returns></returns>
    public static JsonNode AddNodeToRoot(this JsonNode node, string head)
    {
        var root = JsonNode.Parse("{}");
        root.AsObject().Add(head, node);
        return root;
    }
    /// <summary>
    /// 向jsonnode添加节点
    /// </summary>
    /// <param name="TargetNode"></param>
    /// <param name="head"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static JsonNode NodeAddMore(
        this JsonNode TargetNode, string head, JsonNode source)
    {
        TargetNode.AsObject().Add(head, source);
        return TargetNode;
    }

}
