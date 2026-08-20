using System;
using static TestAssert;

// Frame_Game 精简层 FrameCallback 委托测试
// 用 typeof 直接检查 delegate 签名参数数(同程序集内, 编译期即验证存在)
public static class FrameCallbackTest
{
	public static void Run()
	{
		testGameLayoutCallback();
		testDownloadFileListCallback();
		testGameDownloadCallback();
		testGameDownloadTipCallback();
	}

	static void checkDelegate(Type type, int paramCount)
	{
		var inv = type.GetMethod("Invoke");
		assertNotNull(inv, type.Name + " 应有 Invoke");
		assertEqual(paramCount, inv.GetParameters().Length, type.Name + " 参数数");
	}

	// (GameLayout layout)
	static void testGameLayoutCallback()
	{
		checkDelegate(typeof(GameLayoutCallback), 1);
	}

	// (StringCallback callback)
	static void testDownloadFileListCallback()
	{
		checkDelegate(typeof(DownloadFileListCallback), 1);
	}

	// (float progress, PROGRESS_TYPE type, string info, int bytesPerSecond, int downloadRemainSeconds)
	static void testGameDownloadCallback()
	{
		checkDelegate(typeof(GameDownloadCallback), 5);
	}

	// (DOWNLOAD_ERROR type)
	static void testGameDownloadTipCallback()
	{
		checkDelegate(typeof(GameDownloadTipCallback), 1);
	}
}
