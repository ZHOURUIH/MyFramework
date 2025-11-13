using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

// 封装的Http的相关操作,因为其中全是静态工具函数,所以名字为Utility,但是由于需要管理一些线程,所以与普通的工具函数类不同
public class HttpUtility
{
	// 异步get请求,webgl可用
	public static void httpGetAsyncWebGL(string url, Dictionary<string, string> paramList, UnityHttpCallback callback)
	{
		GameEntry.startCoroutine(unityPrepareGet(url, "application/x-www-form-urlencoded", null, paramList, callback, 10));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static IEnumerator unityPrepareGet(string url, string contentType, Dictionary<string, string> header, Dictionary<string, string> paramList, UnityHttpCallback callback, int timeoutSecond)
	{
		url += generateGetParams(paramList);
		// 创建一个UnityWebRequest对象，指定为GET请求
		UnityWebRequest request = new(url, UnityWebRequest.kHttpVerbGET);
		request.downloadHandler = new DownloadHandlerBuffer();
		// 设置Content-Type头为application/json
		request.SetRequestHeader("Content-Type", contentType);
		request.timeout = timeoutSecond;
		if (header != null)
		{
			foreach (var item in header)
			{
				request.SetRequestHeader(item.Key, item.Value);
			}
		}
		// 发送请求并等待完成
		yield return request.SendWebRequest();
		callback?.Invoke(request.downloadHandler.text, request.result, request.responseCode);
	}
	protected static string generateGetParams(Dictionary<string, string> paramList)
	{
		if (paramList.isEmpty())
		{
			return "";
		}
		int count = paramList.Count;
		StringBuilder parameters = new();
		parameters.Append("?");
		// 从集合中取出所有参数，设置表单参数（AddField())
		int index = 0;
		foreach (var item in paramList)
		{
			parameters.Append(item.Key);
			parameters.Append("=");
			parameters.Append(item.Value);
			if (index++ != count - 1)
			{
				parameters.Append('&');
			}
		}
		return parameters.ToString();
	}
}