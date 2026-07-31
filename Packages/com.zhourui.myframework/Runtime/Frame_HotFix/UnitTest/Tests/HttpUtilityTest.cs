using System.Collections.Generic;
using System.Net;
using static HttpUtility;
using static TestAssert;

// HttpUtility 中不依赖真实网络的可测分支
// 注: 同步/异步请求函数需要真实服务器, 此处仅测试 null 校验等纯逻辑分支
public static class HttpUtilityTest
{
	public static void Run()
	{
		testPostFileNullFormList();
		testPostFileNullFormListWithTimeout();
		testPostFileAsyncNullFormList();
	}

	private static void testPostFileNullFormList()
	{
		string result = httpPostFile("http://localhost/test", out WebExceptionStatus status, out HttpStatusCode code, null);
		assertNull(result, "formList 为空时应返回 null");
		assertEqual(WebExceptionStatus.UnknownError, status, "formList 为空时状态应为 UnknownError");
		assertEqual(HttpStatusCode.OK, code, "formList 为空时 code 应为 OK");
	}

	private static void testPostFileNullFormListWithTimeout()
	{
		string result = httpPostFile("http://localhost/test", out WebExceptionStatus status, out HttpStatusCode code, null, 10000);
		assertNull(result, "formList 为空时应返回 null");
		assertEqual(WebExceptionStatus.UnknownError, status, "带超时 formList 为空时状态应为 UnknownError");
		assertEqual(HttpStatusCode.OK, code, "带超时 formList 为空时 code 应为 OK");
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
		assertTrue(called, "formList 为空时应立即触发回调");
		assertEqual(WebExceptionStatus.UnknownError, callbackStatus, "formList 为空时回调状态应为 UnknownError");
		assertEqual(HttpStatusCode.OK, callbackCode, "formList 为空时回调 code 应为 OK");
	}
}
