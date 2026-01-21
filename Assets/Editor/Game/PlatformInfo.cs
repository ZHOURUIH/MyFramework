using System;
using System.Net;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FileUtility;
using static UnityUtility;
using static EditorFileUtility;
using static FrameUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static GameUtility;
using static GameDefine;

// 游戏上架渠道,用于给添加后缀名,以及注入宏,来执行不同的sdk逻辑
public enum GAME_CHANNEL : byte
{
	NONE,
	TAP_TAP,			// TapTap平台,仅做示例
}

public abstract class PlatformInfo : PlatformBase
{

	public static Dictionary<GAME_CHANNEL, string> GAME_CHANNEL_NAME_LIST = new()
	{
		{ GAME_CHANNEL.TAP_TAP, "_TapTap"},
		{ GAME_CHANNEL.NONE, ""},
	};
	public GAME_CHANNEL mGameChannel;                   // 上架渠道
	public PlatformInfo()
	{
		// 如果用的华为云的Obs作为对象存储,就在这里进行初始化,设置所需的参数
		//ObsSystem.setURLAndKeys(OBS_URL, OBS_BUCKET_NAME, OBS_ACCESS_KEY, OBS_SECURE_KEY);
	}
	public static PlatformInfo create()
	{
		BuildTarget target = getBuildTarget();
		PlatformInfo info = null;
		if (target == BuildTarget.Android)
		{
			info = new PlatformAndroid();
		}
		else if (target == BuildTarget.iOS)
		{
			info = new PlatformIOS();
		}
		else if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
		{
			info = new PlatformWindows();
		}
		else if (target == BuildTarget.StandaloneOSX)
		{
			info = new PlatformMacOS();
		}
		else if (target == BuildTarget.WebGL)
		{
			info = new PlatformWebGL();
		}
		else
		{
			Debug.LogError("不支持的平台");
		}
		if (info != null)
		{
			info.mTarget = target;
			info.mAssetBundleFullPath = getAssetBundlePath(true);
		}
		return info;
	}
	public bool upload(bool autoUploadVersion, string uploadLocalPath = null)
	{
		// webgl不需要上传任何资源,如果需要,也是单独的上传接口
		if (isWebGL())
		{
			// 实际上webgl连版本号都不需要上传,不过为了打包方便,还是上传一下,否则远端版本号会一直跟本地对不上
			if (autoUploadVersion)
			{
				uploadVersion();
			}
			return true;
		}
		if (uploadLocalPath.isEmpty())
		{
			uploadLocalPath = mAssetBundleFullPath;
		}
		if (!isDirExist(uploadLocalPath))
		{
			dialog("错误", "上传的资源路径不存在:" + uploadLocalPath, "确定");
			return false;
		}
		string remotePath = getRemoteFolderInEditor(mLocalVersion);
		log("上传远端路径:" + remotePath);
		string displayTitle = "上传游戏资源";
		// 因为中间可能会上传失败,所以需要多次重试,最多尝试3次
		log("开始上传文件, path:" + uploadLocalPath);
		progressBar(displayTitle, "正在获取远端文件列表", 0.0f);
		var remoteFileList = ObsSystem.getFileList(remotePath);
		remoteFileList.remove(VERSION, FILE_LIST, FILE_LIST_MD5);
		log("远端共" + remoteFileList.Count + "个文件");
		progressBar(displayTitle, "正在计算本地文件列表", 0.0f);
		// 对比远端和本地的文件,删除远端无用的文件
		// 排除的文件和排除的目录
		// 优先读取文件列表的信息,同时也校验一下数量与本地实际数量是否一致
		string content = openTxtFile(uploadLocalPath + FILE_LIST, false);
		if (content.isEmpty())
		{
			logError("找不到本地的资源信息列表文件,path:" + uploadLocalPath + FILE_LIST);
			clearProgress();
			return false;
		}
		List<string> ignoreSuffix = new() { ".unity3d.manifest", ".meta" };
		Dictionary<string, GameFileInfo> localFileInfoList = new();
		string generatedContent = generateFileInfoList(uploadLocalPath, false, mIgnoreFile, null, ignoreSuffix);
		parseFileList(generatedContent, localFileInfoList);
		// 如果扫描出来不一样就更新本地文件列表
		if (generatedContent != content)
		{
			writeFileList(uploadLocalPath, generatedContent);
		}

		log("本地共" + localFileInfoList.Count + "个文件");
		clearProgress();

		// 对比需要删除的文件
		progressBar(displayTitle, "正在删除远端文件", 0.0f);
		bool hasError = doDelete(checkDeleteFile(remoteFileList, localFileInfoList), remotePath, displayTitle);

		// 对比需要上传的文件
		progressBar(displayTitle, "正在上传文件", 0.0f);
		List<string> modifyList = checkNeedUploadFile(remoteFileList, localFileInfoList);
		// 要将资源列表文件上传上去
		// 版本号文件不上传
		modifyList.add(FILE_LIST, FILE_LIST_MD5);
		modifyList.Remove(VERSION);
		int remainRetry = 5;
		doUpload(modifyList, uploadLocalPath, remotePath, displayTitle, (int failedCount) =>
		{
			log("上传完毕:" + uploadLocalPath + ", 失败数量:" + failedCount);
			if (failedCount > 0)
			{
				// 还有重试次数就自动重试,没有次数了就手动点击重试
				if (remainRetry-- > 0)
				{
					log("上传完成后有失败,正在自动重试");
					upload(autoUploadVersion, uploadLocalPath);
				}
				else if (messageYesNo("上传失败数量:" + failedCount + ", 是否重试?"))
				{
					upload(autoUploadVersion, uploadLocalPath);
				}
			}
			else
			{
				// 最后上传版本号
				if (autoUploadVersion)
				{
					uploadVersion();
				}
			}
		});
		return hasError;
	}
	public void updateRemoteVersion()
	{
		setRemoteVersion(ObsSystem.downloadTxt(getRemoteFolderInEditor("") + VERSION));
	}
	public bool uploadVersion()
	{
		uploadSingleFile(mAssetBundleFullPath + VERSION, getRemoteFolderInEditor("") + VERSION);
		updateRemoteVersion();
		return true;
	}
	public override string getDefaultPlatformDefine() 
	{
		string platformDefine = "USE_HYBRID_CLR;USE_OBFUSCATOR;PROJECT_2D;USE_URP";
		if (!isWebGL())
		{
			platformDefine += ";USE_SQLITE";
		}
		if (mEnableHotFix)
		{
			platformDefine += ";ENABLE_HOTFIX";
		}
		if (mTestClient)
		{
			platformDefine += ";TEST";
		}
		return platformDefine;
	}
	public override void generateFolderPreName()
	{
		string folderPreName = isWindows() ? "我的传奇" : "MicroLegend";
		if (mTestClient)
		{
			folderPreName += "_Test";
		}
		if (mEnableHotFix)
		{
			// 启用热更时,只有测试版才会添加HotFix,这是为了保证正式版的文件名是简洁的
			if (mTestClient)
			{
				folderPreName += "_HotFix";
			}
		}
		else
		{
			folderPreName += "_NoHotFix";
		}
		// 仅安卓平台才会在安装包的名字上面添加游戏渠道的后缀
		if (isAndroid())
		{
			folderPreName += GAME_CHANNEL_NAME_LIST.get(mGameChannel);
		}
		mFolderPreName = folderPreName;
	}
	public string getRemoteFolderInEditor(string version)
	{
		string folder = "Assets_";
		if (mTestClient)
		{
			folder += "Test_";
		}
		if (isAndroid())
		{
			folder += "Android";
			if (mGameChannel != GAME_CHANNEL.NONE)
			{
				folder += GAME_CHANNEL_NAME_LIST[mGameChannel];
			}
			folder += "/";
		}
		else if (isWindows())
		{
			folder += "Windows/";
		}
		else if (isIOS())
		{
			folder += "iOS/";
		}
		else if (isMacOS())
		{
			folder += "MacOS/";
		}
		else if (isWebGL())
		{
			folder += "WebGL/";
		}
		else
		{
			Debug.LogError("未知平台");
		}
		if (version.isEmpty())
		{
			return folder;
		}
		return folder + version + "/";
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override byte[] getAESKey() { return getAESKeyBytes(); }
	protected override byte[] getAESIV() { return getAESIVBytes(); }
	protected override List<string> getDynamicDownloadList() { return DYNAMIC_DOWNLOAD_LIST; }
	protected override void configureScriptingDefine()
	{
		string platformDefine = getDefaultPlatformDefine();
		// 添加宏定义
		// 安卓平台下根据要上架的不同平台添加对应的宏
		if (isAndroid())
		{
			if (mGameChannel == GAME_CHANNEL.TAP_TAP)
			{
				platformDefine += ";TAP_TAP";
			}
		}
		log("设置宏:" + platformDefine);
		PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), platformDefine);
	}
	protected static bool doDelete(List<string> deleteList, string remotePath, string displayTitle)
	{
		bool hasError = false;
		log("需要删除" + deleteList.Count + "个文件");
		for (int i = 0; i < deleteList.Count; ++i)
		{
			string deleteFullFile = remotePath + deleteList[i];
			log("删除文件:" + deleteFullFile);
			if (!ObsSystem.delete(deleteFullFile))
			{
				logWarning("删除文件失败,等待上传结束后重新尝试上传操作,文件名:" + deleteFullFile);
				hasError = true;
			}
			progressBar(displayTitle, "正在删除远端文件:" + deleteFullFile, i + 1, deleteList.Count);
		}
		clearProgress();
		return hasError;
	}
	protected static void doUpload(List<string> uploadList, string localFilePath, string remotePath, string displayTitle, Action<int> finishCallback)
	{
		log("需要上传" + uploadList.Count + "个文件");
		int failedCount = 0;
		for (int i = 0; i < uploadList.Count; ++i)
		{
			string localFullPath = localFilePath + uploadList[i];
			if (!ObsSystem.upload(localFullPath, openFile(localFullPath, true), remotePath + uploadList[i], out _, out _, 30000))
			{
				++failedCount;
			}
			progressBar(displayTitle, "进度:", i, uploadList.Count);
			log("完成上传文件:" + localFullPath);
		}
		log("上传完毕:" + localFilePath);
		clearProgress();
		finishCallback?.Invoke(failedCount);
	}
	protected static bool uploadSingleFile(string file, string remoteFullPath)
	{
		bool uploadResult;
		log("上传文件:" + file + ", 远端路径:" + remoteFullPath);
		// 如果上传失败,则最多重试5次
		WebExceptionStatus status = WebExceptionStatus.Success;
		HttpStatusCode code = HttpStatusCode.OK;
		try
		{
			uploadResult = ObsSystem.upload(file, openFile(file, true), remoteFullPath, out status, out code, 10000);
		}
		catch
		{
			uploadResult = false;
		}
		if (!uploadResult)
		{
			logError("上传失败:" + file + ", 远端路径:" + remoteFullPath + ", status:" + status + ", code:" + code);
		}
		return uploadResult;
	}
}