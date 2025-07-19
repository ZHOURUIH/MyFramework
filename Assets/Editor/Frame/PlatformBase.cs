using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
#if USE_HYBRID_CLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
#endif
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if USE_OBFUSCATOR
using Obfuz;
using Obfuz.Settings;
using Obfuz.Utils;
using Obfuz.EncryptionVM;
#endif
using static FileUtility;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static EditorFileUtility;
using static CSharpUtility;
using static BinaryUtility;
using static UnityUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;

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
	public bool mEnableHotFix;                                  // 生成的客户端是否启用热更
	public bool mBuildHybridCLR;                                // 打包时是否执行HybridCLR打包,一般都是要执行,检验打包过程时可以不执行以加速打包
	public bool mGooglePlay;                                    // 是否打包aab
	public bool mExportAndroidProject;                          // 是否导出为Android工程
	public bool mOpenExplorer = true;                           // 打包完成后是否显示所在文件夹
	// containOnlyFileList如果不为空,则表示只拷贝列表中指定的文件
	// 可用于单独更新某个文件,比如单独更新表格文件,使之既能够更新FileList,又能单独将要上传的文件放到独立的文件夹中
	public bool showNeedUploadFile(string destFolderName, string[] containOnlyFileList = null)
	{
		List<string> ignoreFile = new() { FILE_LIST_REMOTE, mName, mName + ".manifest" };
		List<string> ignoreSuffix = new() { ".unity3d.manifest", ".meta" };
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
				if (relativePath != FILE_LIST &&
					relativePath != FILE_LIST_MD5 &&
					!arrayContains(containOnlyFileList, relativePath))
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
	public static void checkAccessMissingMetadata()
	{
#if USE_HYBRID_CLR
		BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
		// aotDir指向 构建主包时生成的裁剪aot dll目录，而不是最新的SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录。
		// 一般来说，发布热更新包时，由于中间可能调用过generate/all，SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录中包含了最新的aot dll，
		// 肯定无法检查出类型或者函数裁剪的问题。
		// 需要在构建完主包后，将当时的aot dll保存下来，供后面补充元数据或者裁剪检查。
		string aotDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);

		// 第2个参数excludeDllNames为要排除的aot dll。一般取空列表即可。对于旗舰版本用户，
		// excludeDllNames需要为dhe程序集列表，因为dhe 程序集会进行热更新，热更新代码中
		// 引用的dhe程序集中的类型或函数肯定存在。
		MissingMetadataChecker checker = new(aotDir, new List<string> { HOTFIX, HOTFIX_FRAME });
		string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
		foreach (string dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
		{
			if (!checker.Check(hotUpdateDir + "/" + dll))
			{
				logError("发现访问了被裁剪的代码,dll:" + dll);
			}
		}
#endif
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

#if USE_OBFUSCATOR
		// 对dll进行混淆,dll顺序很重要,被依赖的需要在前面
		log("开始混淆dll");
		// 重命名dll,因为混淆时需要dll文件
		renameFile(F_ASSET_BUNDLE_PATH + HOTFIX_BYTES_FILE, F_ASSET_BUNDLE_PATH + HOTFIX_FILE);
		renameFile(F_ASSET_BUNDLE_PATH + HOTFIX_FRAME_BYTES_FILE, F_ASSET_BUNDLE_PATH + HOTFIX_FRAME_FILE);
		obfuscate(mBuildVersion);
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
		writeFileList(path, generateFileInfoList(path, false, mIgnoreFile, null, new() { ".unity3d.manifest", ".meta" }));
		return true;
	}
	public static void writeFileList(string path, string content)
	{
		writeTxtFile(path + FILE_LIST, content);
		// 再生成此文件的MD5文件,用于客户端校验文件内容是否改变
		writeTxtFile(path + FILE_LIST_MD5, generateFileMD5(stringToBytes(content), -1));
	}
	public abstract string getDefaultPlatformDefine();
	public virtual void generateFolderPreName() { mFolderPreName = ""; }
	public static string generateFileInfoList(string assetBundlePath, bool upperOrLower, List<string> ignoreFiles = null, List<string> ignorePath = null, List<string> ignoreSuffix = null)
	{
		string fileContent = EMPTY;
		List<string> fileInfoList = findFileList(assetBundlePath, ignoreFiles, ignorePath, ignoreSuffix);
		// 计算文件信息
		// 将所有文件信息写入文件
		fileContent += IToS(fileInfoList.Count) + "\n";
		foreach (string item in fileInfoList)
		{
			fileContent += item.removeStartString(assetBundlePath) + "\t" + IToS(getFileSize(item)) + "\t" + generateFileMD5(item, upperOrLower) + "\n";
		}
		return fileContent;
	}
	// 备份一个文件或文件夹到一个临时目录
	public static void backupFileToBuildTemp(string fileName)
	{
		moveFile(fileName, fileName.replace(F_STREAMING_ASSETS_PATH, BUILD_TEMP_PATH));
	}
	public static void backupFileToInstallTimeTemp(string fileName)
	{
		moveFile(fileName, fileName.replace(F_STREAMING_ASSETS_PATH, INSTALL_TIME_TEMP_PATH));
	}
	// 从临时目录恢复一个文件或文件夹
	public static void recoverFileFromBuildTemp(string fileName)
	{
		moveFile(fileName, fileName.replace(BUILD_TEMP_PATH, F_STREAMING_ASSETS_PATH));
	}
	public static void recoverFileFromInstallTimeTemp(string fileName)
	{
		moveFile(fileName, fileName.replace(INSTALL_TIME_TEMP_PATH, F_STREAMING_ASSETS_PATH));
	}
	public static void dialog(string title, string info, string button)
	{
		EditorUtility.DisplayDialog(title, info, button);
	}
	public static void progressBar(string title, string info, float progress)
	{
		EditorUtility.DisplayProgressBar(title, info, progress);
	}
	public static void progressBar(string title, string preInfo, int curCount, int totalCount)
	{
		displayProgressBar(title, preInfo, curCount, totalCount);
	}
	public static void clearProgress()
	{
		EditorUtility.ClearProgressBar();
	}
	public static List<string> findFileList(string assetBundlePath, List<string> ignoreFiles = null, List<string> ignorePath = null, List<string> ignoreSuffix = null)
	{
		List<string> fileInfoList = new();
		foreach (string newPath in findFilesNonAlloc(assetBundlePath))
		{
			if (matchSuffix(ignoreSuffix, newPath) || isIgnorePath(newPath, ignorePath))
			{
				continue;
			}
			if (ignoreFiles != null && ignoreFiles.Contains(newPath.removeStartString(assetBundlePath)))
			{
				continue;
			}
			fileInfoList.Add(newPath);
		}
		return fileInfoList;
	}
	public static bool matchSuffix(List<string> ignoreSuffix, string fileName)
	{
		if (ignoreSuffix == null)
		{
			return false;
		}
		for (int i = 0; i < ignoreSuffix.Count; ++i)
		{
			if (fileName.endWith(ignoreSuffix[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool build(bool buildHybridCLR, bool exportAndroidProject)
	{
		try
		{
			mExportAndroidProject = exportAndroidProject;
			mBuildHybridCLR = buildHybridCLR;
			DateTime buildStartTime = DateTime.Now;
			bool result = buildInternal();
			log("打包完成:" + result + ", 耗时:" + (DateTime.Now - buildStartTime));
			return result;
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
		string number1 = IToS(SToI(mVersionNumber[1]) + 1);
		string number2 = "1";
		return stringsToString(new List<string>() { number0, number1, number2 }, '.');
	}
	public string generateSubVersion()
	{
		string number0 = mVersionNumber[0];
		string number1 = mVersionNumber[1];
		string number2 = IToS(SToI(mVersionNumber[2]) + 1);
		return stringsToString(new List<string>() { number0, number1, number2 }, '.');
	}
	public bool isMinVersionGreater()
	{
		if (mVersionNumber == null)
		{
			return false;
		}
		return getVersionPart(mRemoteVersion, 0) == SToL(mVersionNumber[0]) &&
			   getVersionPart(mRemoteVersion, 1) == SToL(mVersionNumber[1]) &&
			   getVersionPart(mRemoteVersion, 2) < SToL(mVersionNumber[2]);
	}
	public static long getVersionPart(string version, int index)
	{
		if (version.isEmpty())
		{
			return 0;
		}
		List<long> numbers = SToLsNonAlloc(version, '.');
		if (index < 0 || index >= numbers.Count)
		{
			logError("获取版本号数字错误");
			return 0;
		}
		return numbers[index];
	}
	public void setRemoteVersion(string version)
	{
		mRemoteVersion = version;
		updateEditVersionNumber();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void updateEditVersionNumber()
	{
		mVersionNumber = split(mRemoteVersion, ".");
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
	protected abstract bool buildInternal();
	// 由应用层提供自己的密钥,不提供则不会进行加密,Key和IV长度必须为16个字节
	protected virtual byte[] getAESKey() { return null; }
	protected virtual byte[] getAESIV() { return null; }
	protected BuildOptions generateBuildOption(bool isTest)
	{
		BuildOptions options = BuildOptions.None;
		options |= BuildOptions.CompressWithLz4HC;
		if (isTest)
		{
			options |= BuildOptions.Development;
			// 不再开启自动连接Profiler,因为这会使打出来的程序无法在其他电脑调试
			//options |= BuildOptions.ConnectWithProfiler;
			options |= BuildOptions.EnableDeepProfilingSupport;
		}
		return options;
	}
	protected abstract List<string> getDynamicDownloadList();
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
		foreach (string notPackFile in getDynamicDownloadList())
		{
			if (fullPath.StartsWith(mAssetBundleFullPath + notPackFile.ToLower()))
			{
				return true;
			}
		}
		return false;
	}
	// 将本地文件夹的所有文件上传到linux服务器的指定目录中,返回值表示是否上传成功并且检测通过,remoteDeletePath是相对路径,removeCopyFullPath是绝对路径
	protected bool uploadFileToLinuxServer(string localPath, string remoteDeletePath, string removeCopyFullPath, string userNameAndIP, string password)
	{
		string deleteShell = "rm -r " + remoteDeletePath;
		string fetchListShell = "ls -lR " + remoteDeletePath;
		// 执行命令行将其上传到服务器
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			// 删除远端指定目录
			string cmd0 = "echo y | plink -ssh " + userNameAndIP + " -pw " + password + " " + deleteShell;
			// 切换本地磁盘路径
			string cmd1 = F_PROJECT_PATH.startString(1) + ":";
			// 将本地的文件夹上传到远端
			string cmd2 = "echo y | pscp -v -r -pw " + password + " " + localPath + " " + userNameAndIP + ":" + removeCopyFullPath;
			executeCmd(new string[] { cmd0, cmd1, cmd2 }, false, false);

			List<string> allInfo = new();
			string cmd3 = "echo y | plink -ssh " + userNameAndIP + " -pw " + password + " " + fetchListShell;
			executeCmd(new string[] { cmd3 }, false, true, (string info) => { allInfo.Add(info); });
			return checkUploadedFile(allInfo, remoteDeletePath, localPath);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			// 删除远端指定目录
			string cmd0 = "/opt/homebrew/bin/sshpass -p \'" + password + "\' ssh " + userNameAndIP + " -tt " + deleteShell;
			// 将本地的文件夹上传到远端
			string cmd1 = "/opt/homebrew/bin/sshpass -p \'" + password + "\' scp -r " + localPath + " " + userNameAndIP + ":" + removeCopyFullPath;
			executeCmd(new string[] { cmd0, cmd1 }, false, false);

			List<string> allInfo = new();
			string cmd3 = "/opt/homebrew/bin/sshpass -p \'" + password + "\' ssh " + userNameAndIP + " -tt " + fetchListShell;
			executeCmd(new string[] { cmd3 }, false, true, (string info) => { allInfo.Add(info); });
			return checkUploadedFile(allInfo, remoteDeletePath, localPath);
		}
		return false;
	}
	protected bool checkUploadedFile(List<string> output, string remoteDeletePath, string localPath)
	{
		Dictionary<string, GameFileInfo> fileInfoList = new();
		string curDirectory = "";
		foreach (string line in output)
		{
			string[] keys = split(line, ' ');
			if (keys.Length >= 9)
			{
				if (keys[0][0] == '-')
				{
					var fileInfo = RemoteFileInfo.parse(keys);
					if (fileInfo.mFileName == FILE_LIST ||
						fileInfo.mFileName == FILE_LIST_MD5 ||
						fileInfo.mFileName == VERSION)
					{
						continue;
					}
					fileInfo.mFileName = curDirectory + fileInfo.mFileName;
					fileInfoList.Add(fileInfo.mFileName, fileInfo.toGameFileInfo());
				}
			}
			else
			{
				if (line.startWith("total "))
				{
					// 目录中文件数量
				}
				else
				{
					// 当前的目录,去除前缀,去除最后的:,如果路径不为空,就加上/,如果通过
					curDirectory = line.removeAll('\'').removeStartString(remoteDeletePath).removeEndString(":");
					if (!curDirectory.isEmpty())
					{
						curDirectory += "/";
					}
				}
			}
		}

		// 对比文件列表
		Dictionary<string, GameFileInfo> localFileInfoList = new();
		parseFileList(openTxtFile(localPath + "/" + FILE_LIST, true), localFileInfoList);

		if (checkDiff(localFileInfoList, fileInfoList, false))
		{
			log("上传成功");
			return true;
		}
		else
		{
			logError("上传后本地与远端文件列表不一致");
			return false;
		}
	}
#if USE_OBFUSCATOR
	protected static List<string> getSearchPath()
	{
		string editorExePath = EditorApplication.applicationPath;
		editorExePath = getFilePath(editorExePath);
		List<string> searchPaths = new();
		searchPaths.AddRange(ObfuscatorBuilder.BuildUnityAssemblySearchPaths());
		searchPaths.Add(editorExePath + "/Data/Managed");
		searchPaths.Add(F_PROJECT_PATH + SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
		searchPaths.Add(F_PROJECT_PATH + "Library/PackageCache/com.unity.burst@59eb6f11d242");
		searchPaths.Add(F_PROJECT_PATH + "Library/PackageCache/com.unity.collections@d49facba0036/Unity.Collections.LowLevel.ILSupport");
		// mac中的路径
		searchPaths.Add(EditorApplication.applicationPath + "/Contents/Managed/UnityEngine");
		searchPaths.Add(EditorApplication.applicationPath + "/Contents/Managed");
		searchPaths.Add(F_ASSET_BUNDLE_PATH);
		return searchPaths;
	}
	protected static void obfuscate(string version)
	{
		SecretSettings secretSettings = ObfuzSettings.Instance.secretSettings;
		// 因为只混淆热更程序集,所以也只生成动态的密钥
		secretSettings.defaultDynamicSecretKey = "Code Philosophy-Dynamic" + randomInt(0, 100000000);
		secretSettings.assembliesUsingDynamicSecretKeys = ObfuzSettings.Instance.assemblySettings.assembliesToObfuscate;
		secretSettings.randomSeed = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
		ObfuzSettings.Save();
		byte[] dynamicSecretBytes = KeyGenerator.GenerateKey(secretSettings.defaultDynamicSecretKey, VirtualMachine.SecretKeyLength);
		writeFile(F_ASSET_BUNDLE_PATH + DYNAMIC_SECRET_FILE, dynamicSecretBytes);
		AssetDatabase.Refresh();

		var obfuscatorBuilder = ObfuscatorBuilder.FromObfuzSettings(ObfuzSettings.Instance, EditorUserBuildSettings.activeBuildTarget, false);
		var searchPaths = getSearchPath();
		foreach (string item in searchPaths)
		{
			log("search path:" + item);
		}
		obfuscatorBuilder.InsertTopPriorityAssemblySearchPaths(searchPaths);
		try
		{
			obfuscatorBuilder.Build().Run();
			string srcPath = obfuscatorBuilder.CoreSettingsFacade.obfuscatedAssemblyOutputPath;
			foreach (string dllName in ObfuzSettings.Instance.assemblySettings.assembliesToObfuscate)
			{
				copyFile(srcPath + "/" + dllName + ".dll", F_ASSET_BUNDLE_PATH + "/" + dllName + ".dll.bytes", true);
				// 因为obfuscatorBuilder.Build().Run();中会将原始dll拷贝到SteamingAssets中,所以需要删除一下
				deleteFile(F_ASSET_BUNDLE_PATH + "/" + dllName + ".dll");
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
		// 混淆完以后,将Symbol-mapping.xml备份一下,文件名加上版本号,方便后面还原堆栈
		// 因为Symbol-mapping.xml会在混淆的时候读取,所以尽量不去动这个文件
		string originMappingFile = F_PROJECT_PATH + ObfuzSettings.Instance.symbolObfusSettings.symbolMappingFile;
		copyFile(originMappingFile, replaceSuffix(originMappingFile, version + ".xml"));
	}
#endif
}