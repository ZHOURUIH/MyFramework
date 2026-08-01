using System;
using System.Collections.Generic;
using System.Net;
using static HttpUtility;
using static TestAssert;

// HttpUtility 中不依赖真实网络的可测分支
// 注: httpPost/httpGet/httpDelete 等需要真实服务器，preparePost/prepareGet 在连接失败时会 logException
// 仅测试 null 校验等提前 return 的纯逻辑分支
public static class HttpUtilityTest
{
	public static void Run()
	{
		testPostFileNullFormList();
		testPostFileNullFormListWithTimeout();
		testPostFileAsyncNullFormList();
		testDownloadFileNullCallback();
	}

	// ─── httpPostFile null formList ───────────────────────────────────
	private static void testPostFileNullFormList()
	{
		string result = httpPostFile("http://localhost/test", out WebExceptionStatus status, out HttpStatusCode code, null);
		assertNull(result, "formList is null -> return null");
		assertEqual(WebExceptionStatus.UnknownError, status, "null formList status UnknownError");
		assertEqual(HttpStatusCode.OK, code, "null formList code OK");
	}

	private static void testPostFileNullFormListWithTimeout()
	{
		string result = httpPostFile("http://localhost/test", out WebExceptionStatus status, out HttpStatusCode code, null, 10000);
		assertNull(result, "null formList with timeout -> return null");
		assertEqual(WebExceptionStatus.UnknownError, status, "null formList timeout status UnknownError");
		assertEqual(HttpStatusCode.OK, code, "null formList timeout code OK");
	}

	private static void testPostFileAsyncNullFormList()
	{
		bool called = false;
		WebExceptionStatus callbackStatus = WebExceptionStatus.Success;
		HttpStatusCode callbackCode = HttpStatusCode.Accepted;
		httpPostFileAsync("http://localhost/test", null, (str, status, code) =>
		{
			called = true;
			callbackStatus = status;
			callbackCode = code;
		});
		assertTrue(called, "null formList triggers callback immediately");
		assertEqual(WebExceptionStatus.UnknownError, callbackStatus, "null formList callback status UnknownError");
		assertEqual(HttpStatusCode.OK, callbackCode, "null formList callback code OK");
	}

	// ─── downloadFile: null 回调不崩溃 ────────────────────────────────
	private static void testDownloadFileNullCallback()
	{
		// downloadFile 会尝试真实 HTTP 请求，必然失败
		byte[] result = downloadFile("http://127.0.0.1:1/nonexistent", 0, null, "", null, null);
		assertNull(result, "downloadFile failure returns null");
	}
}
