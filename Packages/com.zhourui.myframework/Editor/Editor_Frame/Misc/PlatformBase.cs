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
using static FileUtility;
using static StringUtility;
using static PlatformUtility;
using static FrameDefine;
using static EditorFileUtility;
using static UnityUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;
using static FrameMacro;

public abstract class PlatformBase
{
	public static string BUILD_TEMP_PATH = F_ASSETS_PATH + "../BuildTemp/";
	public static string INSTALL_TIME_TEMP_PATH = F_ASSETS_PATH + "../InstallTimeTemp/";
	public BuildTarget mTarget;
	public BuildTargetGroup mGroup;
	public NamedBuildTarget mNamedTarget;
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
			copyFile(file, dest + file.removeStartString(mAssetBundleFullPath));
		}

		// 只有全部文件都拷贝到指定文件夹以后才能更新文件列表信息
		writeFileList(dest);

		// 更新完文件列表信息以后,如果有仅显示指定文件的需求,再删除无关的文件
		if (containOnlyFileList != null)
		{
			List<string> newList = new(fileList);
			foreach (string file in fileList)
			{
				string relativePath = file.removeStartString(mAssetBundleFullPath);
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
        if (getAESKey().count() == 16)
		{
			log("开始加密生成的dll");
			encryptFileAES(mAssetBundleFullPath + HOTFIX_BYTES_FILE, getAESKey(), getAESIV());
			encryptFileAES(mAssetBundleFullPath + HOTFIX_FRAME_BYTES_FILE, getAESKey(), getAESIV());
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
		string content = generateFileList(path, mIgnoreFile);
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
			afterBuild(outputFullPath);
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
	public void setRemoteVersion(string version)
	{
		mRemoteVersion = version;
		updateEditVersionNumber();
	}
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
	// 由应用层提供自己的密钥,不提供则不会进行加密,Key和IV长度必须为16个字节
	protected virtual byte[] getAESKey() { return null; }
	protected virtual byte[] getAESIV() { return null; }
	// 获取需要动态下载的目录列表,GameResources下的相对路径,只要在这个目录中的资源,都不会在启动时的热更进行下载,而是在需要加载时才会从远端动态下载
	protected abstract List<string> getDynamicDownloadList();
	// 根据自己项目的情况在这个函数中去配置打包时需要的宏定义,比如是否启用热更,是否为测试客户端等,因为这些宏定义会影响代码编译,所以需要在打包前就配置好
	protected abstract void configureScriptingDefine();
	protected virtual bool preBuild()
	{
		// 即使不需要配置是否导出安卓工程,也要确认是打包apk还是导出工程
		// HybridCLR在mac上打包android时可能会将此变量设置为true,虽然源码中有还原操作,但是可能没有还原成功
		EditorUserBuildSettings.exportAsGoogleAndroidProject = mExportAndroidProject;
		EditorUserBuildSettings.buildAppBundle = mGooglePlay;
		PlayerSettings.bundleVersion = mBuildVersion;
		// 需要定位查看一次工程中所有的timeline文件,否则打包后无法播放timeline
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
		backupAssets();
		return true;
	}
	protected virtual void afterBuild(string fullPath)
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

		// GooglePlay平台的包需要在InstallTime备份目录中去计算文件列表
		if (mGooglePlay)
		{
			writeFileList(INSTALL_TIME_TEMP_PATH + mName + "/");
		}
		// 其他情况下需要在AssetBundle目录中生成
		else
		{
			writeFileList(mAssetBundleFullPath);
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
		return getDynamicDownloadList().contains(notPackFile => fullPath.startWith(mAssetBundleFullPath + notPackFile.ToLower()));
	}
}