using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 用于加载Android平台下的资源
public class AndroidAssetLoader : FrameSystem
{
	protected static AndroidJavaObject mAssetLoader;
	public AndroidAssetLoader()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		mAssetLoader = AndroidPluginManager.getMainActivity().Get<AndroidJavaObject>("mAssetLoader");
#endif
	}
	public override void destroy()
	{
		mAssetLoader?.Dispose();
		mAssetLoader = null;
		base.destroy();
	}
	// 相对于StreamingAssets的路径
	public static byte[] loadAsset(string path, bool errorIfNull)
	{
		if(mAssetLoader == null)
		{
			return null;
		}
		return mAssetLoader.Call<byte[]>("loadAsset", path, errorIfNull);
	}
	public static string loadTxtAsset(string path, bool errorIfNull)
	{
		if (mAssetLoader == null)
		{
			return null;
		}
		return mAssetLoader.Call<string>("loadTxtAsset", path, errorIfNull);
	}
	public static bool isAssetExist(string path)
	{
		if (mAssetLoader == null)
		{
			return false;
		}
		return mAssetLoader.Call<bool>("isAssetExist", path);
	}
	public static void findAssets(string path, List<string> fileList, List<string> patterns, bool recursive)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		string pattern = stringArrayToString(patterns, " ");
		var fileListObject = mAssetLoader.Call<AndroidJavaObject>("startFindAssets", path, pattern, recursive);
		javaListToList(fileListObject, fileList, 1024);
	}
	public static void findAssetsFolder(string path, List<string> fileList, bool recursive)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		var fileListObject = mAssetLoader.Call<AndroidJavaObject>("startFindAssetsFolder", path, recursive);
		javaListToList(fileListObject, fileList, 1024);
	}
	//-------------------------------------------------------------------------------------------------------------------------------------------
	// 以下函数只能用于Android平台的persistentDataPath目录操作,path为绝对路径
	public static byte[] loadFile(string path, bool errorIfNull)
	{
		if (mAssetLoader == null)
		{
			return null;
		}
		checkPersistenDataPath(path);
		return mAssetLoader.CallStatic<byte[]>("loadFile", path, errorIfNull);
	}
	public static string loadTxtFile(string path, bool errorIfNull)
	{
		if (mAssetLoader == null)
		{
			return null;
		}
		checkPersistenDataPath(path);
		return mAssetLoader.CallStatic<string>("loadTxtFile", path, errorIfNull);
	}
	public static new void writeFile(string path, byte[] buffer, int writeCount, bool appendData)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		mAssetLoader.CallStatic("writeFile", path, buffer, writeCount, appendData);
	}
	public static new void writeTxtFile(string path, string str, bool appendData)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		mAssetLoader.CallStatic("writeTxtFile", path, str, appendData);
	}
	public static new void deleteFile(string path)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		mAssetLoader.CallStatic("deleteFile", path);
	}
	public static new bool isDirExist(string path)
	{
		if (mAssetLoader == null)
		{
			return false;
		}
		checkPersistenDataPath(path);
		return mAssetLoader.CallStatic<bool>("isDirExist", path);
	}
	public static new bool isFileExist(string path)
	{
		if (mAssetLoader == null)
		{
			return false;
		}
		checkPersistenDataPath(path);
		return mAssetLoader.CallStatic<bool>("isFileExist", path);
	}
	public static new int getFileSize(string path)
	{
		if (mAssetLoader == null)
		{
			return 0;
		}
		checkPersistenDataPath(path);
		return mAssetLoader.CallStatic<int>("getFileSize", path);
	}
	public static new void findFiles(string path, List<string> fileList, List<string> patterns, bool recursive)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		string pattern = stringArrayToString(patterns, " ");
		var fileListObject = mAssetLoader.CallStatic<AndroidJavaObject>("startFindFiles", path, pattern, recursive);
		javaListToList(fileListObject, fileList, 1024);
	}
	public static new void findFolders(string path, List<string> fileList, bool recursive)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		var fileListObject = mAssetLoader.CallStatic<AndroidJavaObject>("startFindFolders", path, recursive);
		javaListToList(fileListObject, fileList, 1024);
	}
	public static void createDirectory(string path)
	{
		if (mAssetLoader == null)
		{
			return;
		}
		checkPersistenDataPath(path);
		mAssetLoader.CallStatic("createDirectory", path);
	}
	//------------------------------------------------------------------------------------------------------------------------------------------------
	protected static void checkPersistenDataPath(string path)
	{
		addEndSlash(ref path);
		if (!startWith(path, FrameDefine.F_PERSISTENT_DATA_PATH))
		{
			logError("path must start with " + FrameDefine.F_PERSISTENT_DATA_PATH + ", path : " + path);
		}
	}
	protected static void javaListToList(AndroidJavaObject javaListObject, List<string> list, int maxCount)
	{
		for (int i = 0; i < maxCount; ++i)
		{
			string fileName = mAssetLoader.CallStatic<string>("getListElement", javaListObject, i);
			if (isEmpty(fileName))
			{
				break;
			}
			list.Add(fileName);
		}
	}
}