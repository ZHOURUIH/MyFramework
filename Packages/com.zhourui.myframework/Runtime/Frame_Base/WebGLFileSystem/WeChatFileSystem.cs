#if UNITY_WEIXINMINIGAME
using WeChatWASM;
#endif

using System;
using System.Collections;

public class WeChatFileSystem
{
	private const string LegalCode = "access:ok";
	public static bool isDirectoryExist(string dirPath)
	{
#if UNITY_WEIXINMINIGAME
        return WXBase.GetFileSystemManager().AccessSync(dirPath) == LegalCode;
#else
		return false;
#endif
	}
	public static void createDirectory(string dirPath)
	{
#if UNITY_WEIXINMINIGAME
        WXBase.GetFileSystemManager().MkdirSync(dirPath, true);
#endif
	}
	public static void deleteDirectory(string dirPath)
	{
#if UNITY_WEIXINMINIGAME
        if (WXBase.GetFileSystemManager().AccessSync(dirPath) == LegalCode)
        {
            WXBase.GetFileSystemManager().RmdirSync(dirPath, true);   
        }
#endif
	}
	public static bool isFileExist(string filePath)
	{
#if UNITY_WEIXINMINIGAME
        return WXBase.GetFileSystemManager().AccessSync(filePath) == LegalCode;
#else
		return false;
#endif
	}
	public static void deleteFile(string filePath)
	{
		if (isFileExist(filePath))
		{
#if UNITY_WEIXINMINIGAME
			WXBase.GetFileSystemManager().UnlinkSync(filePath);
#endif
		}
	}
	public static void copyFile(string sourceFilePath, string destFilePath, bool overwrite)
	{
#if UNITY_WEIXINMINIGAME
        WXBase.GetFileSystemManager().CopyFileSync(sourceFilePath, destFilePath);
#endif
	}
	public static void writeText(string filePath, string fileContent)
	{
#if UNITY_WEIXINMINIGAME
        WXBase.GetFileSystemManager().WriteFileSync(filePath, fileContent);
#endif
	}
	public static void writeBytes(string filePath, byte[] byteArray)
	{
#if UNITY_WEIXINMINIGAME
        WXBase.GetFileSystemManager().WriteFileSync(filePath, byteArray);
#endif
	}
	// 只能读取PersistentData中的文件
	public static byte[] readBytes(string filePath)
	{
#if UNITY_WEIXINMINIGAME
		return WXBase.GetFileSystemManager().ReadFileSync(filePath);
#else
		return null;
#endif
	}
	public static IEnumerator readBytesAsync(string filePath, BytesCallback callback)
	{
		bool isLoading = true;
		byte[] byteData = null;
#if UNITY_WEIXINMINIGAME
		WXBase.GetFileSystemManager().ReadFile(new ReadFileParam()
		{
			filePath = filePath,
			success = res =>
			{
				isLoading = false;
				byteData = res.binData;
			},
			fail = res =>
			{
				Debug.LogError($"WeChatFileSystem read file bytes async failed - {res.errCode}");
			},
			position = 0
		});
#else
		isLoading = false;
#endif
		while (isLoading)
		{
			yield return null;
		}
		callback?.Invoke(byteData);
	}
}