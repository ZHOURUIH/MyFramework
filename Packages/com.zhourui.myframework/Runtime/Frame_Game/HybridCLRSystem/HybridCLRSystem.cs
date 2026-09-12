using System;
using System.Reflection;
using System.Collections.Generic;
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
	protected static bool mHotFixLaunching;
	protected static bool mHotFixLaunched;
	public static void launchHotFix(Action errorCallback = null)
	{
		if (mHotFixLaunched)
		{
			logErrorBase("已经启动了热更逻辑,无法再次启动");
			return;
		}
		if (mHotFixLaunching)
		{
			logErrorBase("热更逻辑正在启动中,无法重复启动");
			return;
		}
		mHotFixLaunching = true;

		// 启动之前需要确认拷贝一下混淆密钥
		preLaunch(() =>
		{
			try
			{
				// 存储所有需要跨域的参数
				backupFrameParam();
				// 启动热更系统
				if (isEditor() || !isUseHybridCLR())
				{
					launchEditor(errorCallback);
				}
				else
				{
					launchRuntime(errorCallback);
				}
			}
			catch (Exception e)
			{
				logExceptionBase(e);
				notifyLaunchFailed(errorCallback);
			}
		}, errorCallback);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void preLaunch(Action callback, Action errorCallback)
	{
		if (isEditor())
		{
			callback?.Invoke();
			return;
		}
		if (!isEnableHotFix())
		{
			copyFileAsync(F_ASSET_BUNDLE_PATH + DYNAMIC_SECRET_FILE, F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, callback);
			return;
		}
		// 在热更全部下载完成后,执行此函数,再启动热更.
		// 这个函数的目的是确保最新的混淆密钥文件一定存在于PersistenPath中
		// 因为在启动热更时GameHotFixBase会固定从PersistenPath中加载密钥文件
		// 如果加载的密钥文件不是最新的,则无法启动游戏
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
		GameFileInfo streamingInfo = mAssetVersionSystem.getStreamingAssetsFile().get(DYNAMIC_SECRET_FILE);
		if (streamingInfo == null)
		{
			logErrorBase("StreamingAssets中找不到混淆密钥文件信息:" + DYNAMIC_SECRET_FILE);
			notifyLaunchFailed(errorCallback);
			return;
		}
		copyFileAsync(F_ASSET_BUNDLE_PATH + DYNAMIC_SECRET_FILE, F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, () =>
		{
			try
			{
				// copyFileAsync没有返回拷贝结果,所以回调后必须重新读取目标文件确认拷贝确实成功.
				byte[] copiedBytes = openFileSync(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, false);
				if (copiedBytes == null || 
					copiedBytes.LongLength != streamingInfo.mFileSize ||
					!string.Equals(generateFileMD5(copiedBytes), streamingInfo.mMD5, StringComparison.OrdinalIgnoreCase))
				{
					logErrorBase("混淆密钥文件拷贝后校验失败:" + DYNAMIC_SECRET_FILE);
					notifyLaunchFailed(errorCallback);
					return;
				}

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
				// 拷贝完并校验通过以后才更新FileList
				writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList());
				callback?.Invoke();
			}
			catch (Exception e)
			{
				logExceptionBase(e);
				notifyLaunchFailed(errorCallback);
			}
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
	protected static void loadMetaDataForAOT(Action callback, Action errorCallback)
	{
#if USE_HYBRID_CLR
		try
		{
			Dictionary<string, byte[]> downloadFilesResource = new();
			foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
			{
				string fileName = aotFile + ".bytes";
				byte[] bytes = openFileSync(availableReadPath(fileName), true);
				if (bytes == null)
				{
					logErrorBase("读取AOT补充元数据文件失败:" + fileName);
					notifyLaunchFailed(errorCallback);
					return;
				}
				downloadFilesResource.add(fileName, bytes);
			}

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
					logErrorBase("LoadMetadataForAOTAssembly失败:" + aotFile + ", " + err);
					notifyLaunchFailed(errorCallback);
					return;
				}
			}
			callback?.Invoke();
		}
		catch (Exception e)
		{
			logExceptionBase(e);
			notifyLaunchFailed(errorCallback);
		}
#else
		callback?.Invoke();
#endif
	}
	protected static void launchRuntime(Action errorCallback)
	{
		loadMetaDataForAOT(() =>
		{
			try
			{
				Dictionary<string, byte[]> downloadFiles = new();
				foreach (string name in FrameSettings.getHotFixList())
				{
					string fileDllName = name + ".dll.bytes";
					byte[] bytes = openFileSync(availableReadPath(fileDllName), true);
					if (bytes == null)
					{
						logErrorBase("读取热更程序集失败:" + fileDllName);
						notifyLaunchFailed(errorCallback);
						return;
					}
					downloadFiles.add(fileDllName, bytes);
				}

				// 加载以后不再卸载
				Assembly hotfix = null;
				foreach (var item in downloadFiles)
				{
					Assembly assembly = Assembly.Load(decryptAES(item.Value, FrameSettings.getAESKey(), FrameSettings.getAESIV()));
					if (item.Key == "HotFix.dll.bytes")
					{
						hotfix = assembly;
					}
				}
				launchInternal(hotfix, errorCallback);
			}
			catch (Exception e)
			{
				logExceptionBase(e);
				notifyLaunchFailed(errorCallback);
			}
		}, errorCallback);
	}
	protected static void launchEditor(Action errorCallback)
	{
		try
		{
			Assembly hotFixAssembly = null;
			foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (item.GetName().Name == "HotFix")
				{
					hotFixAssembly = item;
					break;
				}
			}
			if (hotFixAssembly == null)
			{
				logErrorBase("编辑器中找不到HotFix程序集");
				notifyLaunchFailed(errorCallback);
				return;
			}
			launchInternal(hotFixAssembly, errorCallback);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
			notifyLaunchFailed(errorCallback);
		}
	}
	protected static void launchInternal(Assembly hotFixAssembly, Action errorCallback)
	{
		if (hotFixAssembly == null)
		{
			logErrorBase("加载热更程序集失败:" + "HotFix");
			notifyLaunchFailed(errorCallback);
			return;
		}
		Type type = hotFixAssembly.GetType("GameHotFix");
		if (type == null)
		{
			logErrorBase("在热更程序集中找不到GameHotFix类");
			notifyLaunchFailed(errorCallback);
			return;
		}
		if (type.BaseType?.Name != "GameHotFixBase`1")
		{
			logErrorBase("GameHotFix类需要继承自GameHotFixBase");
			notifyLaunchFailed(errorCallback);
			return;
		}
		Action preStartCallback = () =>
		{
			try
			{
				// 由于createHotFixInstance是在基类中的,而查找静态函数是不会自动去基类中查找的,所以这里需要手动去基类中查找
				MethodInfo methodCreate = type.BaseType.GetMethod("createHotFixInstance");
				if (methodCreate == null)
				{
					logErrorBase("在GameHotFix类中找不到静态函数createHotFixInstance");
					notifyLaunchFailed(errorCallback);
					return;
				}
				// 查找start函数,会自动从基类中查找
				MethodInfo methodStart = type.GetMethod("start");
				if (methodStart == null)
				{
					logErrorBase("在GameHotFix类中找不到函数start");
					notifyLaunchFailed(errorCallback);
					return;
				}
				// 执行热更的启动函数
				Action callback = () =>
				{
					logBase("热更初始化完毕");
					// 到这里才表示热更真正启动成功
					mHotFixLaunching = false;
					mHotFixLaunched = true;
					// 热更初始化完毕后将非热更层加载的所有资源都清除,这样避免中间的黑屏
					GameEntryBase.getInstance().getFrameworkAOT().destroy();
					GameEntryBase.getInstance().setFrameworkAOT(null);
				};
				// 使用createHotFixInstance创建一个HotFix的实例,然后调用此实例的start函数
				object hotFixInstance = methodCreate.Invoke(null, null);
				if (hotFixInstance == null)
				{
					logErrorBase("createHotFixInstance创建热更实例失败");
					notifyLaunchFailed(errorCallback);
					return;
				}
				methodStart.Invoke(hotFixInstance, new object[1] { callback });
			}
			catch (Exception e)
			{
				logExceptionBase(e);
				notifyLaunchFailed(errorCallback);
			}
		};
		try
		{
			MethodInfo methodPreStart = getMethodRecursive(type, "preStart", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (methodPreStart == null)
			{
				logErrorBase("在GameHotFix类或者父类中找不到静态函数preStart");
				notifyLaunchFailed(errorCallback);
				return;
			}
			methodPreStart.Invoke(null, new object[1] { preStartCallback });
		}
		catch (Exception e)
		{
			logExceptionBase(e);
			notifyLaunchFailed(errorCallback);
		}
	}
	protected static void notifyLaunchFailed(Action errorCallback)
	{
		// 同一轮启动可能存在多个后续回调,失败以后只允许通知一次
		if (!mHotFixLaunching)
		{
			return;
		}
		mHotFixLaunching = false;
		mHotFixLaunched = false;
		errorCallback?.Invoke();
	}
}
