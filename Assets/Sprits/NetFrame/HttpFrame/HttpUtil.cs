using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CI.HttpClient;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
public class HttpUtil:MonoBehaviour
{
	private static HttpClient httpClient = createHttpClient ();
	private static Action<HttpStatusCode> networkError;
    private static Action<string> invalidSession;
    private static Action<string> concurrentLogin;
    private static Action<string> serverError;
    static bool isPrompt;
    static GameObject prompt;
    static GameObject load;
    public static bool isShw;
    private static HttpClient createHttpClient ()
	{
		HttpClient httpClient = new HttpClient ();

		// 连接超时 10 秒; 读写超时 20 秒
		httpClient.Timeout = 10 * 1000;
		httpClient.ReadWriteTimeout = 20 * 1000;

		// 支持 cookie
		httpClient.Cookies = new CookieContainer ();

		// https 证书校验, 开发和测试不校验
		ServicePointManager.ServerCertificateValidationCallback += (o, certificate, chain, errors) => {
			return true;
		};

		// 处理网络错误
		networkError = (status) => {
            ShowPromptBox("服务器连接失败");
        };

		// 处理未登录或会话过期
		invalidSession = (message) => {
            ShowPromptBox(message);
        };

        // 处理重复登录
        concurrentLogin = (message) =>
        {
            ShowPromptBox(message);
        };

        // 处理服务器错误
        serverError = (message) => {
            ShowPromptBox(message);
        };

		return httpClient;
	}

    public static void DownLoadImage(string url,Transform tran)
    {
        httpClient.GetByteArray(new Uri(url), HttpCompletionOption.AllResponseContent, (r) => {
            Texture2D tex = new Texture2D(200, 200);
            tex.LoadImage(r.Data);
            tran.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        });
    }

    public static void ShowPromptBox(string show, UnityEngine.Events.UnityAction preAction = null)
    {

        //增加预先设置的代理方法
        if(preAction!=null)
        {
            prompt.transform.Find("").GetComponent<Button>().onClick.AddListener(preAction);
        }
    }
	public static void GetApi (string url, Action<string> callback)
	{
		httpClient.GetString (new Uri (url), (response) => {
			HandleResponse (response, callback);
		});
	}

	public static void PostApi (String url, Dictionary<string, string> parameters, Action<string> callback)
	{
        if (parameters == null) {
            parameters = new Dictionary<string, string> ();
		}

		httpClient.Post (new Uri (url), new FormUrlEncodedContent (parameters), (response) => {
            HandleResponse (response, callback);
		});
	}

	private static void HandleResponse (HttpResponseMessage<string> response, Action<string> callback)
	{
        
        if (response.StatusCode != HttpStatusCode.OK) {
            networkError (response.StatusCode);
			return;
		}
        JObject jObject = JObject.Parse (response.Data);
		String code = jObject ["code"].Value<string> ();
		if (string.Equals (code, "401")) {
            invalidSession (jObject["message"].Value<string>());
			return;
		}

        if (string.Equals (code, "409"))
        {
            concurrentLogin(jObject["message"].Value<string>());
            return;
        }

		if (!string.Equals (code, "200")) {
            serverError (jObject ["message"].Value<string> ());
			return;
		}

		callback (jObject ["data"].ToString ());
	}
   
  
}
