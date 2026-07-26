using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
#if USE_HYBRID_CLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
#endif
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using static FileUtility;
using static StringUtility;
using static PlatformUtility;
using static FrameDefine;
using static EditorFileUtility;
using static UnityUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;
using static FrameMacro;
using static FrameUtility;
using static FrameBaseUtility;

public abstract class PlatformBase
{
	public static string BUILD_TEMP_PATH = F_ASSETS_PATH + "../BuildTemp/";
	public static string INSTALL_TIME_TEMP_PATH = F_ASSETS_PATH + "../InstallTimeTemp/";
	public IObjectStorageSystem mObjectStorageSystem;			// 用于上传文件下载文件的对象,访问对象存储的
	public BuildTarget mTarget;									// 当前平台
	public string[] mVersionNumber;                             // 用于修改本次打包的版本号
	public List<string> mIgnoreFile;                            // 计算文件列表时需要忽略的文件名
	public string mAssetBundleFullPath;                         // AssetBundle的绝对路径
	public string mName;                                        // 平台名字,就是StreamingAssets中平台文件夹的名字
	public string mBuildVersion;                                // 打包时的版本号,仅在打包时使用
	public string mLocalVersion;                                // 当前本地文件中存储的版本号,打包时在build完成后会同步为PackVersion,所以在build过程中是不能使用LocalVersion
	public string mRemoteVersion;                               // 远端的版本号
	public string mOutputPath = F_PROJECT_PATH + "GameOutput/"; // 输出路径
	public string mFolderPreName;                               // 输出文件夹的名字或者安装包的名字前缀
	public bool mEnableHotFix;                                  // 生成的客户端是否启用热更,webgl暂时不启用热更
	public bool mTestClient;									// 是否为测试客户端
	public bool mBuildHybridCLR;                                // 打包时是否执行HybridCLR打包,一般都是要执行,检验打包过程时可以不执行以加速打包
	public bool mGooglePlay;                                    // 是否打包aab
	public bool mExportAndroidProject;                          // 是否导出为Android工程
	public bool mOpenExplorer = true;                           // 打包完成后是否显示所在文件夹
	public PlatformBase()
	{
		BuildTarget target = getBuildTarget();
		if (target == BuildTarget.Android)
		{
			mName = ANDROID;
		}
		else if (target == BuildTarget.iOS)
		{
			mName = IOS;
		}
		else if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneWindows)
		{
			mName = WINDOWS;
		}
		else if (target == BuildTarget.StandaloneOSX)
		{
			mName = MACOS;
		}
		else if (target == BuildTarget.WebGL)
		{
			mName = WEBGL;
		}
        mTarget = target;
        mAssetBundleFullPath = getAssetBundlePath(true);
    }
	// containOnlyFileList如果不为空,则表示只拷贝列表中指定的文件
	// 可用于单独更新某个文件,比如单独更新表格文件,使之既能够更新FileList,又能单独将要上传的文件放到独立的文件夹中
	public bool showNeedUploadFile(string destFolderName, string[] containOnlyFileList = null)
	{
		List<string> ignoreFile = new() { mName, mName + ".manifest" };
		List<string> ignoreSuffix = new() { ASSET_BUNDLE_SUFFIX + ".manifest", ".meta" };
		var fileList = findFileList(mAssetBundleFullPath, ignoreFile, null, ignoreSuffix);
		string dest = F_PROJECT_PATH + "../" + destFolderName + "/";
		deleteFolder(dest);
		foreach (string file in fileList)
		{
			copyFile(file, dest + file.removeStart(mAssetBundleFullPath));
		}

		// 只有全部文件都拷贝到指定文件夹以后才能更新文件列表信息
		writeFileList(dest);

		// 更新完文件列表信息以后,如果有仅显示指定文件的需求,再删除无关的文件
		if (containOnlyFileList != null)
		{
			List<string> newList = new(fileList);
			foreach (string file in fileList)
			{
				string relativePath = file.removeStart(mAssetBundleFullPath);
				// 删除指定
				if (relativePath != FILE_LIST && !containOnlyFileList.contains(relativePath))
				{
					deleteFile(dest + relativePath);
					newList.Remove(file);
				}
			}
			fileList = newList;
		}
		deleteEmptyFolder(dest);
		log("资源文件收集完成,共" + fileList.Count + "个文件");
		return true;
	}
	public bool buildHotFix(bool generateAll)
	{
#if USE_HYBRID_CLR
		// HybridCLR生成的所有文件,然后将热更dll文件拷贝到StreamingAssets下
		if (generateAll)
		{
			PrebuildCommand.GenerateAll();
		}
		else
		{
			CompileDllCommand.CompileDll(EditorUserBuildSettings.activeBuildTarget);
		}
		string hotFixSrcPath = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget) + "/";
		copyFile(hotFixSrcPath + HOTFIX_FILE, mAssetBundleFullPath + HOTFIX_BYTES_FILE);
		copyFile(hotFixSrcPath + HOTFIX_FRAME_FILE, mAssetBundleFullPath + HOTFIX_FRAME_BYTES_FILE);
		// 拷贝补充数据dll
		string aotDllSrcPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget) + "/";
		foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
		{
			copyFile(aotDllSrcPath + aotFile, mAssetBundleFullPath + aotFile + DATA_SUFFIX);
		}
		checkAccessMissingMetadata();

#if USE_OBFUZ
		// 对dll进行混淆,dll顺序很重要,被依赖的需要在前面
		log("开始混淆dll");
		// 重命名dll,因为混淆时需要dll文件,在obfuscate会进行还原
		renameFile(mAssetBundleFullPath + HOTFIX_BYTES_FILE, mAssetBundleFullPath + HOTFIX_FILE);
		renameFile(mAssetBundleFullPath + HOTFIX_FRAME_BYTES_FILE, mAssetBundleFullPath + HOTFIX_FRAME_FILE);
		obfuscate(mBuildVersion, mTestClient);
		log("完成混淆dll");
#endif

        // 对自己编译的热更dll进行加密,检查完以后再加密
        if (FrameSettings.getAESKey().count() == 16)
		{
			log("开始加密生成的dll");
			encryptFileAES(mAssetBundleFullPath + HOTFIX_BYTES_FILE, FrameSettings.getAESKey(), FrameSettings.getAESIV());
			encryptFileAES(mAssetBundleFullPath + HOTFIX_FRAME_BYTES_FILE, FrameSettings.getAESKey(), FrameSettings.getAESIV());
			log("完成加密生成的dll");
		}
#endif

		// 检查本地必需的dll.bytes文件是否正确
		if (!checkAllDllExist())
		{
			logError("有必需的dll.bytes文件不存在,请检查并重试");
			return false;
		}
		return true;
	}
	public virtual bool writeVersion()
	{
		writeTxtFile(mAssetBundleFullPath + VERSION, mBuildVersion);
		mLocalVersion = mBuildVersion;
		return true;
	}
	public bool writeFileList(string path)
	{
		string content = generateFileList(path, mIgnoreFile, FrameSettings.getDynamicDownloadList());
		writeTxtFile(path + FILE_LIST, content);
		return true;
	}
	// 检查所有的热更dll,以及AOT的dll是否都存在
	public bool checkAllDllExist()
	{
		List<string> dllList = new()
		{
			mAssetBundleFullPath + HOTFIX_BYTES_FILE,
			mAssetBundleFullPath + HOTFIX_FRAME_BYTES_FILE
		};
#if USE_HYBRID_CLR
        foreach (string aotFile in AOTGenericReferences.PatchedAOTAssemblyList)
		{
			dllList.Add(mAssetBundleFullPath + aotFile + DATA_SUFFIX);
		}
#endif
		bool allExist = true;
		foreach (string file in dllList)
		{
			if (!isFileExist(file))
			{
				logError("文件不存在:" + file);
				allExist = false;
			}
		}
		return allExist;
	}
	// 框架中只根据是否启用热更和是否为测试客户端来增加对应的宏
	public string getDefaultPlatformDefine()
	{
        string platformDefine = getDefaultPlatformDefineInternal();
        if (mEnableHotFix)
        {
            platformDefine += ";" + ENABLE_HOTFIX;
        }
        if (mTestClient)
        {
            platformDefine += ";" + TEST;
        }
        return platformDefine;
	}
	// 除了动态配置以外的宏,比如USE_HYBRID_CLR,USE_OBFUZ等基本固定的宏,一般都是使用FrameMacro中定义的值,由应用层自己决定
	public abstract string getDefaultPlatformDefineInternal();
	public virtual void generateFolderPreName() { mFolderPreName = ""; }
	public bool build(bool buildHybridCLR, bool exportAndroidProject)
	{
		try
		{
			mExportAndroidProject = exportAndroidProject;
			mBuildHybridCLR = buildHybridCLR;
			DateTime buildStartTime = DateTime.Now;
			if (!preBuild())
			{
				return false;
			}
			BuildResult result = buildInternal(out string outputFullPath);
			// 通用打包后处理
			postBuild(outputFullPath);
			log("打包完成:" + result + ", 耗时:" + (DateTime.Now - buildStartTime));
			return result == BuildResult.Succeeded;
		}
		catch (Exception e)
		{
			logError("打包错误:" + e.Message + ", stack:" + e.StackTrace);
			return false;
		}
	}
	public bool updateLocalVersion()
	{
		mLocalVersion = openTxtFile(mAssetBundleFullPath + VERSION, false);
		if (mLocalVersion.isEmpty())
		{
			mLocalVersion = "0.0.0";
		}
		return true;
	}
	public string generateMainVersion()
	{
		string number0 = mVersionNumber[0];
		string number1 = (mVersionNumber[1].SToI() + 1).IToS();
		string number2 = "1";
		return new List<string>() { number0, number1, number2 }.stringsToString('.');
	}
	public string generateSubVersion()
	{
		string number0 = mVersionNumber[0];
		string number1 = mVersionNumber[1];
		string number2 = (mVersionNumber[2].SToI() + 1).IToS();
		return new List<string>() { number0, number1, number2 }.stringsToString('.');
	}
	// 是否仅本地版本号的低位版本号大于远端的低位版本号
	public bool isMinVersionGreater()
	{
		if (mVersionNumber == null)
		{
			return false;
		}
		return getVersionPart(mRemoteVersion, 0) == mVersionNumber[0].SToL() &&
			   getVersionPart(mRemoteVersion, 1) == mVersionNumber[1].SToL() &&
			   getVersionPart(mRemoteVersion, 2) < mVersionNumber[2].SToL();
	}
	// 下载远端的版本号
	public void updateRemoteVersion()
	{
		mRemoteVersion = mObjectStorageSystem.downloadTxt(getRemotePathInEditor("") + VERSION);
		log("更新远端版本号:" + mRemoteVersion);
		updateEditVersionNumber();
	}
	// 将本地的版本号上传到远端
	public bool uploadVersion()
	{
		string remotePath = getRemotePathInEditor("") + VERSION;
		uploadSingleFile(mAssetBundleFullPath + VERSION, remotePath, true);
		// 上传版本号以后立即刷新cdn
		mObjectStorageSystem.refreshCDN(remotePath);
		updateRemoteVersion();
		return true;
	}
	// 获取在远端资源的路径,一般都会根据版本号来隔离每个版本的资源,而且在应用层最好自己再实现一个利用宏来判断的路径
	// 在编辑器非运行模式下就不要用宏来判断了,因为此时本身就要去添加编译宏,所以编辑器非运行模式下的宏可能更新没那么及时,会导致获取到错误的值
	// 比如本地StreamingAssets/1.txt对应的远端位置是domain/ProjectName/Verison/1.txt,那么这里返回的就应该是ProjectName/Verison/
	public abstract string getRemotePathInEditor(string version);
	//------------------------------------------------------------------------------------------------------------------------------
	protected void updateEditVersionNumber()
	{
		mVersionNumber = mRemoteVersion.split('.');
		// 需要确保版本号只有3个部分
		if (mVersionNumber.count() != 3)
		{
			string[] newVersionNumber = new string[3];
			for (int i = 0; i < newVersionNumber.Length; ++i)
			{
				newVersionNumber[i] = i < mVersionNumber.count() ? mVersionNumber[i] : "0";
			}
			mVersionNumber = newVersionNumber;
		}
	}
	protected abstract BuildResult buildInternal(out string outputFullPath);
	// 根据自己项目的情况在这个函数中去配置打包时需要的宏定义,比如是否启用热更,是否为测试客户端等,因为这些宏定义会影响代码编译,所以需要在打包前就配置好
	protected void configureScriptingDefine()
	{
		string platformDefine = getDefaultPlatformDefine();
		log("设置宏:" + platformDefine);
		PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), platformDefine);
	}
	protected virtual bool preBuild()
	{
		// 即使不需要配置是否导出安卓工程,也要确认是打包apk还是导出工程
		// HybridCLR在mac上打包android时可能会将此变量设置为true,虽然源码中有还原操作,但是可能没有还原成功
		EditorUserBuildSettings.exportAsGoogleAndroidProject = mExportAndroidProject;
		EditorUserBuildSettings.buildAppBundle = mGooglePlay;
		PlayerSettings.bundleVersion = mBuildVersion;
		// 需要定位查看一次工程中所有的timeline文件,否则打包后无法播放timeline,暂时还不清楚这个bug的原因
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".playable"))
		{
			Selection.activeObject = loadAsset(file);
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		// 添加宏定义
		string platformDefine = PlayerSettings.GetScriptingDefineSymbols(getNameBuildTarget());
		// 对当前的宏进行检查,避免由于上一次打包失败没有正确还原宏而导致打包出现问题
		if (platformDefine != getDefaultPlatformDefine())
		{
			logWarning("当前的宏定义错误:" + platformDefine + ", 已还原为:" + getDefaultPlatformDefine());
			PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), getDefaultPlatformDefine());
		}
		log("备份宏:" + getDefaultPlatformDefine());
		configureScriptingDefine();

		if (mBuildHybridCLR)
		{
			buildHotFix(true);
		}

		createDir(mOutputPath);

		// 打包时只启用第一个场景,因为微信平台的打包是直接读的编辑器设置,而不能自己传参
		for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
		{
			EditorBuildSettings.scenes[i].enabled = i == 0;
		}

		AssetDatabase.Refresh();

		// 需要先更新版本号文件
		writeVersion();
		// 在备份文件之前计算文件列表
		writeFileList(mAssetBundleFullPath);
		backupAssets();
		return true;
	}
	protected virtual void postBuild(string fullPath)
	{
		// 打包时只启用第一个场景,因为微信平台的打包是直接读的编辑器设置,而不能自己传参
		for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
		{
			EditorBuildSettings.scenes[i].enabled = true;
		}
		recoverAssets();
		// 还原宏定义
		PlayerSettings.SetScriptingDefineSymbols(getNameBuildTarget(), getDefaultPlatformDefine());
		log("还原宏:" + getDefaultPlatformDefine());
		EditorSceneManager.SaveOpenScenes();
		// 打开生成文件所在的目录
		if (!fullPath.isEmpty() && mOpenExplorer)
		{
			EditorUtility.RevealInFinder(fullPath);
		}
	}
	protected virtual void backupAssets()
	{
		// 其他平台的所有文件全部备份到其他目录,先删除之前可能存在的临时目录
		deleteFolder(BUILD_TEMP_PATH);
		deleteFile(removeEndSlash(BUILD_TEMP_PATH) + ".meta");
		deleteFolder(INSTALL_TIME_TEMP_PATH);
		deleteFile(removeEndSlash(INSTALL_TIME_TEMP_PATH) + ".meta");
		createDir(BUILD_TEMP_PATH);
		createDir(INSTALL_TIME_TEMP_PATH);
		foreach (string file in findFilesNonAlloc(F_STREAMING_ASSETS_PATH))
		{
			BACKUP_TARGET backupDest = BACKUP_TARGET.BUILD_TEMP;
			if (file.StartsWith(mAssetBundleFullPath))
			{
				// 如果是GooglePlay的安装包,则需要将当前平台下非动态下载的所有资源文件备份到指定临时目录
				if (mGooglePlay)
				{
					// meta和manifest文件不打进包里,所以备份到临时目录
					// 动态下载的文件备份到BuildTemp,其他的备份到InstallTimeTemp
					if (file.endWith(".meta", false) || file.endWith(".manifest", false) || isDynamicDownloadAsset(file))
					{
						backupDest = BACKUP_TARGET.BUILD_TEMP;
					}
					else
					{
						backupDest = BACKUP_TARGET.INSTALL_TIME_TEMP;
					}
				}
				// 版本号文件不备份
				else if (file.EndsWith(VERSION))
				{
					backupDest = BACKUP_TARGET.NONE;
				}
				// webgl中需要将所有文件都备份到临时目录,这些文件不打包到包体中,这是需要上传到cdn
				else if (isWebGL())
				{
					backupDest = BACKUP_TARGET.BUILD_TEMP;
				}
				// 启用热更时,动态下载的文件备份到临时目录,其他不进行备份
				else if (mEnableHotFix)
				{
					if (isDynamicDownloadAsset(file))
					{
						backupDest = BACKUP_TARGET.BUILD_TEMP;
					}
					else
					{
						backupDest = BACKUP_TARGET.NONE;
					}
				}
				// 未启用热更时,所有文件都不进行备份
				else
				{
					backupDest = BACKUP_TARGET.NONE;
				}
			}
			if (backupDest == BACKUP_TARGET.BUILD_TEMP)
			{
				backupFileToBuildTemp(file);
			}
			else if (backupDest == BACKUP_TARGET.INSTALL_TIME_TEMP)
			{
				backupFileToInstallTimeTemp(file);
			}
		}
		deleteEmptyFolder(F_STREAMING_ASSETS_PATH);

		// GooglePlay平台的包需要在InstallTime备份目录中去重新计算文件列表
		if (mGooglePlay)
		{
			writeFileList(INSTALL_TIME_TEMP_PATH + mName + "/");
		}
	}
	protected virtual void recoverAssets()
	{
		// 还原文件
		foreach (string file in findFilesNonAlloc(BUILD_TEMP_PATH))
		{
			recoverFileFromBuildTemp(file);
		}
		deleteFolder(BUILD_TEMP_PATH);
		deleteFile(removeEndSlash(BUILD_TEMP_PATH) + ".meta");
		foreach (string file in findFilesNonAlloc(INSTALL_TIME_TEMP_PATH))
		{
			recoverFileFromInstallTimeTemp(file);
		}
		deleteFolder(INSTALL_TIME_TEMP_PATH);
		deleteFile(removeEndSlash(INSTALL_TIME_TEMP_PATH) + ".meta");
	}
	protected bool isDynamicDownloadAsset(string fullPath)
	{
		return FrameSettings.getDynamicDownloadList().contains(notPackFile => fullPath.startWith(mAssetBundleFullPath + notPackFile.ToLower()));
	}
	public bool uploadResources(bool autoUploadVersion, string uploadLocalPath = null, int rertyCount = 5)
	{
		if (uploadLocalPath.isEmpty())
		{
			uploadLocalPath = mAssetBundleFullPath;
		}
		if (!isDirExist(uploadLocalPath))
		{
			dialog("错误", "上传的资源路径不存在:" + uploadLocalPath, "确定");
			return false;
		}
		string remotePath = getRemotePathInEditor(mLocalVersion);
		log("上传远端路径:" + remotePath);
		string displayTitle = "上传游戏资源";
		// 因为中间可能会上传失败,所以需要多次重试,最多尝试3次
		log("开始上传文件, path:" + uploadLocalPath);
		progressBar(displayTitle, "正在获取远端文件列表");
		var remoteFileList = mObjectStorageSystem.getFileList(remotePath);
		remoteFileList.remove(mIgnoreFile);
		log("远端共" + remoteFileList.Count + "个文件");
		progressBar(displayTitle, "正在计算本地文件列表");
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
		string generatedContent = generateFileList(uploadLocalPath, mIgnoreFile, FrameSettings.getDynamicDownloadList());
		// 如果扫描出来不一样就更新本地文件列表
		if (generatedContent != content)
		{
			logError("扫描的本地文件信息与FileList中记录的信息不一致,请检查并重试");
			clearProgress();
			return false;
		}
		Dictionary<string, GameFileInfo> localFileInfoList = new();
		parseFileList(generatedContent, localFileInfoList);
		// 检查本地必需的dll.bytes文件是否正确
		if (!checkAllDllExist())
		{
			logError("有必需的dll.bytes文件不存在,请检查并重试");
			clearProgress();
			return false;
		}

		log("本地共" + localFileInfoList.Count + "个文件");
		clearProgress();

		// 对比远端需要删除的文件
		progressBar(displayTitle, "正在删除远端文件");
		bool hasError = doDelete(checkDeleteFile(localFileInfoList, remoteFileList), remotePath, displayTitle);

		// 对比需要上传的文件,计算出上传的文件列表
		progressBar(displayTitle, "正在上传文件");
		List<string> modifyList = checkNeedUploadFile(remoteFileList, localFileInfoList);
		// 要将资源列表文件上传上去
		// 版本号文件不上传
		modifyList.add(FILE_LIST);
		modifyList.Remove(VERSION);
		Dictionary<string, string> uploadList = new();
		foreach (string item in modifyList)
		{
			uploadList.add(uploadLocalPath + item, remotePath + item);
		}
		// 如果是微信小游戏,还需要上传webgl.data.unityweb.bin.txt
		if (isWebGL() && isWeiXin())
		{
			foreach (string file in findFilesNonAlloc(mOutputPath + "WeiXinMiniGame/webgl", ".webgl.data.unityweb.bin.txt"))
			{
				uploadList.add(file, getFileNameWithSuffix(file));
			}
		}

		// 将文件全部上传,如果上传失败,则最多重试5次
		doUpload(uploadList, displayTitle, (int failedCount) =>
		{
			log("上传完毕:" + uploadLocalPath + ", 失败数量:" + failedCount);
			if (failedCount > 0)
			{
				// 还有重试次数就自动重试,没有次数了就手动点击重试
				if (rertyCount > 0)
				{
					log("上传完成后有失败,正在自动重试");
					uploadResources(autoUploadVersion, uploadLocalPath, rertyCount - 1);
				}
				else if (messageYesNo("上传失败数量:" + failedCount + ", 是否重试?"))
				{
					uploadResources(autoUploadVersion, uploadLocalPath, rertyCount - 1);
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
	protected bool doDelete(List<string> deleteList, string remotePath, string displayTitle)
	{
		bool hasError = false;
		log("需要删除" + deleteList.Count + "个文件");
		for (int i = 0; i < deleteList.Count; ++i)
		{
			string deleteFullFile = remotePath + deleteList[i];
			log("删除文件:" + deleteFullFile);
			if (!mObjectStorageSystem.delete(deleteFullFile))
			{
				logWarning("删除文件失败,等待上传结束后重新尝试上传操作,文件名:" + deleteFullFile);
				hasError = true;
			}
			progressBar(displayTitle, "正在删除远端文件:" + deleteFullFile, i + 1, deleteList.Count);
		}
		clearProgress();
		return hasError;
	}
	// uploadList的key是本地要上传文件的绝对路径,value是此文件存储到远端的路径,一般是域名后面的相对路径
	protected void doUpload(Dictionary<string, string> uploadList, string displayTitle, Action<int> finishCallback)
	{
		log("需要上传" + uploadList.Count + "个文件");
		int failedCount = 0;
		int index = 0;
		foreach (var item in uploadList)
		{
			if (!uploadSingleFile(item.Key, item.Value, false))
			{
				++failedCount;
			}
			progressBar(displayTitle, "进度:", index++, uploadList.Count);
			// 不知道为什么有时候会不显示进度条,就加个暂停50毫秒试试
			Thread.Sleep(50);
			log("完成上传文件:" + item.Key);
		}
		log("上传完毕");
		clearProgress();
		finishCallback?.Invoke(failedCount);
	}
	protected bool uploadSingleFile(string file, string remotePath, bool noCache)
	{
		log("上传文件:" + file + ", 远端路径:" + remotePath);
		// 如果上传失败,则最多重试5次
		HttpStatusCode code = 0;
		try
		{
			code = mObjectStorageSystem.upload(file, remotePath, noCache);
		}
		catch { }
		if (code != HttpStatusCode.OK)
		{
			logError("上传失败:" + file + ", 远端路径:" + remotePath + ", code:" + code);
		}
		return code == HttpStatusCode.OK;
	}
}