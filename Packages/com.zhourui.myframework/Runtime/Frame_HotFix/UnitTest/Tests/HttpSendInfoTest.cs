using System;
using static TestAssert;

// HttpSendInfo 单元测试 — 覆盖 HTTP 发送信息字段的默认值 / cloneFrom 复制
public static class HttpSendInfoTest
{
	public static void Run()
	{
		testResetPropertyDefaults();
		testCloneFrom();
		testCloneFromNull();
	}

	// ─── resetProperty 默认值 ─────────────────────────────────────────
	private static void testResetPropertyDefaults()
	{
		var info = new HttpSendInfo();
		info.resetProperty();
		assertNull(info.mParamsForGet, "reset 后 paramsForGet 为 null");
		assertNull(info.mCallback, "reset 后 callback 为 null");
		assertNull(info.mMessage, "reset 后 message 为 null");
		assertNull(info.mType, "reset 后 type 为 null");
		assertNull(info.mUrl, "reset 后 url 为 null");
		assertEqual(HTTP_METHOD.POST, info.mMethod, "reset 后 method 默认 POST");
		assertEqual(0, info.mTimeout, "reset 后 timeout 为 0");
		assertEqual(0, info.mRemainRetryCount, "reset 后 remainRetryCount 为 0");
	}

	// ─── cloneFrom 复制所有字段 ───────────────────────────────────────
	private static void testCloneFrom()
	{
		var src = new HttpSendInfo();
		src.mParamsForGet = new System.Collections.Generic.Dictionary<string, string> { { "k", "v" } };
		src.mCallback = (pkt) => { };
		src.mMessage = "msg";
		src.mUrl = "http://test.com";
		src.mType = typeof(string);
		src.mMethod = HTTP_METHOD.GET;
		src.mTimeout = 3000;
		src.mRemainRetryCount = 2;

		var dst = new HttpSendInfo();
		dst.cloneFrom(src);
		assertEqual(src.mParamsForGet, dst.mParamsForGet, "clone 后 paramsForGet 引用一致");
		assertEqual(src.mCallback, dst.mCallback, "clone 后 callback 引用一致");
		assertEqual("msg", dst.mMessage, "clone 后 message 一致");
		assertEqual("http://test.com", dst.mUrl, "clone 后 url 一致");
		assertEqual(typeof(string), dst.mType, "clone 后 type 一致");
		assertEqual(HTTP_METHOD.GET, dst.mMethod, "clone 后 method 一致");
		assertEqual(3000, dst.mTimeout, "clone 后 timeout 一致");
		assertEqual(2, dst.mRemainRetryCount, "clone 后 remainRetryCount 一致");
	}

	// ─── cloneFrom 拷贝 null 源字段(复制源的默认值,而非目标原有值) ─────
	private static void testCloneFromNull()
	{
		var src = new HttpSendInfo();
		var dst = new HttpSendInfo();
		dst.cloneFrom(src);
		assertNull(dst.mUrl, "clone null 源后 url 为 null");
		// 新实例构造时 mMethod 默认是 NONE(POST 只在 resetProperty 中设置), cloneFrom 直接复制源字段
		assertEqual(HTTP_METHOD.NONE, dst.mMethod, "clone 后 method 复制源的默认值 NONE");
		assertEqual(0, dst.mTimeout, "clone 后 timeout 为 0");
	}
}
