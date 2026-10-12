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
	// 项目可在解密后解码压缩传输格式；未设置时使用原始 DLL。
	public static Func<byte[], byte[]> mDecodeHotFixAssembly;
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
#if !USE_OBFUZ
		callback?.Invoke();
		return;
#else
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
		// 更新流程可能已提交新版本号,不能用版本相同判断缓存密钥仍与程序集匹配。
		// 密钥与DLL使用相同的资源选择策略,并校验实际文件内容。
		string sourcePath = availableReadPath(DYNAMIC_SECRET_FILE);
		string persistentPath = F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE;
		GameFileInfo sourceInfo = sourcePath == persistentPath ?
			mAssetVersionSystem.getPersistentAssetsFile().get(DYNAMIC_SECRET_FILE) :
			mAssetVersionSystem.getStreamingAssetsFile().get(DYNAMIC_SECRET_FILE);
		if (sourcePath.isEmpty() || sourceInfo == null)
		{
			logErrorBase("找不到当前资源版本的混淆密钥:" + DYNAMIC_SECRET_FILE);
			notifyLaunchFailed(errorCallback);
			return;
		}
		byte[] persistentBytes = openFileSync(persistentPath, false);
		if (persistentBytes != null && 
			persistentBytes.LongLength == sourceInfo.mFileSize &&
			generateFileMD5(persistentBytes) == sourceInfo.mMD5)
		{
			logBase("热更启动:混淆密钥校验通过");
			callback?.Invoke();
			return;
		}
		if (sourcePath == persistentPath)
		{
			logErrorBase("当前资源版本的缓存混淆密钥校验失败:" + DYNAMIC_SECRET_FILE);
			notifyLaunchFailed(errorCallback);
			return;
		}
		copyFileAsync(sourcePath, persistentPath, () =>
		{
			try
			{
				// copyFileAsync没有返回拷贝结果,所以回调后必须重新读取目标文件确认拷贝确实成功.
				byte[] copiedBytes = openFileSync(F_PERSISTENT_ASSETS_PATH + DYNAMIC_SECRET_FILE, false);
				if (copiedBytes == null || 
					copiedBytes.LongLength != sourceInfo.mFileSize ||
					!string.Equals(generateFileMD5(copiedBytes), sourceInfo.mMD5, StringComparison.OrdinalIgnoreCase))
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
				persistInfo.mFileName = sourceInfo.mFileName;
				persistInfo.mFileSize = sourceInfo.mFileSize;
				persistInfo.mMD5 = sourceInfo.mMD5;
				// 拷贝完并校验通过以后才更新FileList
				if (!writeFileList(F_PERSISTENT_ASSETS_PATH, mAssetVersionSystem.generatePersistentAssetFileList()))
				{
					logErrorBase("混淆密钥文件列表写入失败");
					notifyLaunchFailed(errorCallback);
					return;
				}
				logBase("热更启动:混淆密钥同步并校验完成");
				callback?.Invoke();
			}
			catch (Exception e)
			{
				logExceptionBase(e);
				notifyLaunchFailed(errorCallback);
			}
		});
#endif
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
					byte[] assemblyBytes = decryptAES(item.Value, FrameSettings.getAESKey(), FrameSettings.getAESIV());
					Assembly assembly = Assembly.Load(mDecodeHotFixAssembly != null ? mDecodeHotFixAssembly(assemblyBytes) : assemblyBytes);
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
