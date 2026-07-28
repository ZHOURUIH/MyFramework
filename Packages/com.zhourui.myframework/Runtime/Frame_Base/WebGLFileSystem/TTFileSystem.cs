#if BYTE_DANCE
using TTSDK;
#endif
using System.Collections;

public class TTFileSystem
{
	public static bool isDirectoryExist(string dirPath)
	{
#if BYTE_DANCE
		return TT.GetFileSystemManager().AccessSync(dirPath);
#else
		return false;
#endif
	}
	public static void createDirectory(string dirPath)
	{
#if BYTE_DANCE
		TT.GetFileSystemManager().MkdirSync(dirPath, true);
#endif
	}
	public static void deleteDirectory(string dirPath)
	{
		if (isDirectoryExist(dirPath))
		{
#if BYTE_DANCE
			TT.GetFileSystemManager().RmdirSync(dirPath, true);
#endif
		}
	}
	public static bool isFileExist(string filePath)
	{
#if BYTE_DANCE
		return TT.GetFileSystemManager().AccessSync(filePath);
#else
		return false;
#endif
	}
	public static void deleteFile(string filePath)
	{
		if (isFileExist(filePath))
		{
#if BYTE_DANCE
			TT.GetFileSystemManager().UnlinkSync(filePath);
#endif
		}
	}
	public static void copyFile(string sourceFilePath, string destFilePath, bool overwrite)
	{
#if BYTE_DANCE
		TT.GetFileSystemManager().CopyFileSync(sourceFilePath, destFilePath);
#endif
	}
	public static void writeText(string filePath, string fileContent)
	{
#if BYTE_DANCE
		TT.GetFileSystemManager().WriteFileSync(filePath, fileContent);
#endif
	}
	public static void writeBytes(string filePath, byte[] byteArray)
	{
#if BYTE_DANCE
		TT.GetFileSystemManager().WriteFileSync(filePath, byteArray);
#endif
	}
	// 只能读取PersistentData中的文件
	public static byte[] readBytes(string filePath)
	{
#if BYTE_DANCE
		return TT.GetFileSystemManager().ReadFileSync(filePath);
#else
		return null;
#endif
	}
	public static IEnumerator readBytesAsync(string filePath, BytesCallback callback)
	{
		bool isReading = true;
		byte[] content = null;
#if BYTE_DANCE
		ReadFileParam readFileParam = new ReadFileParam()
		{
			filePath = filePath,
			encoding = "binary",
			success = (rsp) =>
			{
				content = rsp.binData;
				isReading = false;
			},
			fail = (rsp) =>
			{
				content = null;
				isReading = false;
			}
		};
		TT.GetFileSystemManager().ReadFile(readFileParam);
#else
		isReading = false;
#endif
		while (isReading)
		{
			yield return null;
		}
		callback?.Invoke(content);
	}
}