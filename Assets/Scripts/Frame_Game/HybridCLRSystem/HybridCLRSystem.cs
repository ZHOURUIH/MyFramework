using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
#if USE_HYBRID_CLR
using HybridCLR;
#endif
using static FileUtility;
using static FrameBaseDefine;
using static StringUtility;
using static UnityUtility;
using static FrameBaseUtility;
using static FrameBase;

// HybridCLR系统,用于启动HybridCLR热更
public class HybridCLRSystem
{
	protected static bool mHotFixLaunched;
	public static void launchHotFix(byte[] aesKey, byte[] aesIV, Action<string, BytesIntCallback> openOrDownloadDll, Action errorCallback = null)
	{
		if (mHotFixLaunched)
		{
			logErrorBase("已经启动了热更逻辑,无法再次启动");
			return;
		}
		mHotFixLaunched = true;

		// 启动之前需要确认拷贝一下混淆密钥
		preLaunch(() =>
		{
			try
			{
				// 存储所有需要跨域的参数
				backupFrameParam();
				// 启动热更系统
#if UNITY_EDITOR || !USE_HYBRID_CLR
				launchEditor(errorCallback);
#else
				launchRuntime(aesKey, aesIV, openOrDownloadDll, errorCallback);
#endif
			}
			catch (Exception e)
			{
				logExceptionBase(e);
			}
		});
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void preLaunch(Action callback)
	{
		if (isEditor() || isWebGL())
		{
			callback?.Invoke();
			return;
		}
		// 如果StreamingAssets中的版本号大于PersistentData的版本号(所以这里的前提是版本号都是正确的,否则错误拷贝就会无法执行后面混淆后的代码),则需要将混淆密钥文件拷贝到PersistentData中
		// 确保PersistentData中的密钥文件肯定是最新的
		string streamVersion = mAssetVersionSystem.getStreamingAssetsVersion();
		string persistVersion = mAssetVersionSystem.getPersistentDataVersion();
		VERSION_COMPARE fullCompare = compareVersion3(streamVersion, persistVersion, out _, out _);
		logBase("streamVersion:" + streamVersion + ", persistVersion:" + persistVersion + ", fullCompare:" + fullCompare);
		logBase("isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE):" + isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE));
		if (fullCompare != VERSION_COMPARE.LOCAL_LOWER && isFileExist(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE))
		{
			callback?.Invoke();
			return;
		}
		copyFileAsync(F_ASSET_BUNDLE_PATH + DYNAMIC_SECRET_FILE, F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, () =>
		{
			GameFileInfo streamingInfo = mAssetVersionSystem.getStreamingAssetsFile().get(DYNAMIC_SECRET_FILE);
			if (streamingInfo != null)
			{
				var persistAssetsFiles = mAssetVersionSystem.getPersistentAssetsFile();
				GameFileInfo persistInfo = persistAssetsFiles.get(DYNAMIC_SECRET_FILE);
				if (persistInfo == null)
				{
					persistInfo = new();
					persistAssetsFiles.add(DYNAMIC_SECRET_FILE, persistInfo);
				}
				persistInfo.mFileName = streamingInfo.mFileName;
				persistInfo.mFileSize = streamingInfo.mFileSize;
				persistInfo.mMD5 = streamingInfo.mMD5;
				// 拷贝完以后更新FileList
				writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
			}
			callback?.Invoke();
		});
	}
	protected static void backupFrameParam()
	{
		FrameCrossParam.mLocalizationName = ResLocalizationText.mCurLanguage;
		FrameCrossParam.mDownloadURL = mResourceManager.getDownloadURL();
		FrameCrossParam.mStreamingAssetsVersion = mAssetVersionSystem.getStreamingAssetsVersion();
		FrameCrossParam.mPersistentDataVersion = mAssetVersionSystem.getPersistentDataVersion();
		FrameCrossParam.mRemoteVersion = mAssetVersionSystem.getRemoteVersion();
		FrameCrossParam.mStreamingAssetsFileList.setRange(mAssetVersionSystem.getStreamingAssetsFile());
		FrameCrossParam.mPersistentAssetsFileList.setRange(mAssetVersionSystem.getPersistentAssetsFile());
		FrameCrossParam.mRemoteAssetsFileList.setRange(mAssetVersionSystem.getRemoteAssetsFile());
		FrameCrossParam.mTotalDownloadedFiles.setRange(mAssetVersionSystem.getTotalDownloadedFiles());
		FrameCrossParam.mTotalDownloadByteCount = mAssetVersionSystem.getTotalDownloadedByteCount();
		FrameCrossParam.mAssetReadPath = mAssetVersionSystem.getAssetReadPath();
	}
	// 执行AOT补充元数据
	protected static void loadMetaDataForAOT(Action<string, BytesIntCallback> openOrDownloadDll, Action callback, Action errorCallback)
	{
#if USE_HYBRID_CLR
		Dictionary<string, byte[]> downloadFilesResource = new();
		foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
		{
			downloadFilesResource.Add(aotFile + ".bytes", null);
		}
		int finishCount = 0;
		foreach (string item in new List<string>(downloadFilesResource.Keys))
		{
			string fileDllName = item;
			openOrDownloadDll(fileDllName, (byte[] bytes, int length) =>
			{
				if (onAOTDownloaded(downloadFilesResource, ref finishCount, fileDllName, bytes, errorCallback))
				{
					callback?.Invoke();
				}
			});
		}
#else
		callback?.Invoke();
#endif
	}
	// 返回值表示是否已经全部下载完成
	protected static bool onAOTDownloaded(Dictionary<string, byte[]> downloadFilesResource, ref int finishCount, string fileDllName, byte[] bytes, Action errorCallback)
	{
		if (bytes == null)
		{
			downloadFilesResource = null;
			errorCallback?.Invoke();
			return false;
		}
		if (downloadFilesResource == null)
		{
			return false;
		}
		downloadFilesResource.set(fileDllName, bytes);
		if (++finishCount < downloadFilesResource.Count)
		{
			return false;
		}
#if USE_HYBRID_CLR
		foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
		{
			// 为aot assembly加载原始metadata
			// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
			// 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
			// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
			// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
			LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(downloadFilesResource.get(aotFile + ".bytes"), HomologousImageMode.SuperSet);
			if (err != LoadImageErrorCode.OK)
			{
				Debug.Log("LoadMetadataForAOTAssembly失败:" + aotFile + ", " + err);
				errorCallback?.Invoke();
				return false;
			}
		}
#endif
		return true;
	}
	protected static void launchRuntime(byte[] aesKey, byte[] aesIV, Action<string, BytesIntCallback> openOrDownloadDll, Action errorCallback)
	{
		loadMetaDataForAOT(openOrDownloadDll, ()=>
		{
			Dictionary<string, byte[]> downloadFiles = new()
			{
				{ HOTFIX_FRAME_BYTES_FILE, null },
				{ HOTFIX_BYTES_FILE, null }
			};
			int finishCount = 0;
			foreach (string item in new List<string>(downloadFiles.Keys))
			{
				string fileDllName = item;
				openOrDownloadDll(fileDllName, (byte[] bytes, int length) =>
				{
					onHotFixDllLoaded(downloadFiles, ref finishCount, fileDllName, bytes, aesKey, aesIV, errorCallback);
				});
			}
		}, errorCallback);
	}
	protected static void onHotFixDllLoaded(Dictionary<string, byte[]> downloadFiles, ref int finishCount, string fileDllName, byte[] bytes, byte[] aesKey, byte[] aesIV, Action errorCallback)
	{
		if (bytes == null)
		{
			downloadFiles = null;
			errorCallback?.Invoke();
			return;
		}
		if (downloadFiles == null)
		{
			return;
		}
		downloadFiles.set(fileDllName, bytes);
		if (++finishCount < downloadFiles.Count)
		{
			return;
		}
		// 加载以后不再卸载
		Assembly.Load(decryptAES(downloadFiles.get(HOTFIX_FRAME_BYTES_FILE), aesKey, aesIV));
		launchInternal(Assembly.Load(decryptAES(downloadFiles.get(HOTFIX_BYTES_FILE), aesKey, aesIV)));
	}
	protected static void launchEditor(Action errorCallback)
	{
		Assembly hotFixAssembly = null;
		string dllName = getFileNameNoSuffixNoDir(HOTFIX_FILE);
		foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (item.GetName().Name == dllName)
			{
				hotFixAssembly = item;
				break;
			}
		}
		if (hotFixAssembly == null)
		{
			errorCallback?.Invoke();
			return;
		}
		launchInternal(hotFixAssembly);
	}
	protected static void launchInternal(Assembly hotFixAssembly)
	{
		if (hotFixAssembly == null)
		{
			logErrorBase("加载热更程序集失败:" + HOTFIX_FILE);
			return;
		}
		Type type = hotFixAssembly.GetType("GameHotFix");
		if (type == null)
		{
			logErrorBase("在热更程序集中找不到GameHotFix类");
			return;
		}
		if (type.BaseType.Name != "GameHotFixBase")
		{
			logErrorBase("GameHotFix类需要继承自GameHotFixBase");
			return;
		}
		Action preStartCallback = () =>
		{
			// 创建热更对象示例的函数签名为public void createHotFixInstance()
			MethodInfo methodCreate = type.GetMethod("createHotFixInstance");
			if (methodCreate == null)
			{
				logErrorBase("在GameHotFix类中找不到静态函数createHotFixInstance");
				return;
			}
			// 查找start函数
			MethodInfo methodStart = type.GetMethod("start");
			if (methodStart == null)
			{
				logErrorBase("在GameHotFix类中找不到函数start");
				return;
			}
			// 执行热更的启动函数
			Action callback = () =>
			{
				Debug.Log("热更初始化完毕");
				// 热更初始化完毕后将非热更层加载的所有资源都清除,这样避免中间的黑屏
				GameEntry.getInstance().getFrameworkAOT().destroy();
				GameEntry.getInstance().setFrameworkAOT(null);
			};
			// 使用createHotFixInstance创建一个HotFix的实例,然后调用此实例的start函数
			methodStart.Invoke(methodCreate.Invoke(null, null), new object[1] { callback });
		};
		MethodInfo methodPreStart = getMethodRecursive(type, "preStart", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		if (methodPreStart == null)
		{
			logErrorBase("在GameHotFix类或者父类中找不到静态函数preStart");
			return;
		}
		methodPreStart.Invoke(null, new object[1] { preStartCallback });
	}
}