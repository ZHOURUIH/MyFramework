using UnityEditor;
using UnityEditor.Build;
#if USE_HYBRID_CLR
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
#endif
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Runtime.InteropServices;
#if USE_OBFUSCATOR
using Flower.UnityObfuscator;
#endif
using static FileUtility;
using static StringUtility;
using static FrameDefine;
using static EditorFileUtility;
using static CSharpUtility;
using static BinaryUtility;
using static UnityUtility;
using static EditorCommonUtility;
using static FrameDefineBase;
using static FrameEditorUtility;

public abstract class PlatformBase
{
	public static string BUILD_TEMP_PATH = F_ASSETS_PATH + "../BuildTemp/";
	public static string INSTALL_TIME_TEMP_PATH = F_ASSETS_PATH + "../InstallTimeTemp/";
	public string mPlatformDefine;
	public BuildTarget mTarget;
	public BuildTargetGroup mGroup;
	public NamedBuildTarget mNamedTarget;
	public List<string> mIgnoreFile;					// 计算文件列表时需要忽略的文件名
	public string mAssetBundleFullPath;
	public string mName;
	public string mVersion;
	public string mOutputPath;                           // 输出路径
	public string mFolderPreName;                        // 输出文件夹的名字或者安装包的名字前缀
	public bool mEnableHotFix;                           // 生成的客户端是否启用热更
	public bool mBuildHybridCLR;
	public bool mGooglePlay;
	public bool mExportAndroidProject;
	public bool mOpenExplorer;
	public bool showNeedUploadFile(string[] fileNameList = null)
	{
		List<string> ignoreFile = new() { FILE_LIST_REMOTE, mName, mName + ".manifest" };
		List<string> ignoreSuffix = new() { ".unity3d.manifest", ".meta" };
		var fileList = findFileList(mAssetBundleFullPath, ignoreFile, null, ignoreSuffix);
		string destPath = F_PROJECT_PATH + "../" + generateRemotePath() + "/";
		deleteFolder(destPath);
		foreach (string file in fileList)
		{
			copyFile(file, destPath + file.removeStartString(mAssetBundleFullPath));
		}

		// 更新文件列表信息
		writeFileList(destPath);
		foreach (string file in fileList)
		{
			string relativePath = file.removeStartString(mAssetBundleFullPath);
			if (relativePath != FILE_LIST && 
				relativePath != FILE_LIST_MD5 &&
				fileNameList != null &&
				!arrayContains(fileNameList, relativePath))
			{
				deleteFile(destPath + relativePath);
			}
		}
		deleteEmptyFolder(destPath);
		log("资源文件收集完成,共" + fileList.Count + "个文件");
		return true;
	}
	public abstract string generateRemotePath();
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
		MissingMetadataChecker checker = new (aotDir, new List<string> { HOTFIX, HOTFIX_FRAME });
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
			copyFile(aotDllSrcPath + aotFile, mAssetBundleFullPath + aotFile + ".bytes");
		}
		checkAccessMissingMetadata();

		// 对dll进行混淆,dll顺序很重要,被依赖的需要在前面
#if USE_OBFUSCATOR
		log("开始混淆dll");
		var obfuscatorConfig = loadAssetAtPath<ObfuscatorConfig>(Const.ConfigAssetPath);
		obfuscatorConfig.obfuscateDllPaths = new string[]
		{
			P_ASSET_BUNDLE_PATH + HOTFIX_FRAME_BYTES_FILE,
			P_ASSET_BUNDLE_PATH + HOTFIX_BYTES_FILE,
		};
		string nameMapFolder = "UnityObfuscatorNameMap";
		createDir(F_PROJECT_PATH + nameMapFolder);
		CodeObfuscator.DoObfuscate(obfuscatorConfig, nameMapFolder + "/UnityObfuscator-Name_Obfuscate_Map_" + mVersion + ".txt");
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
	public bool writeVersion()
	{
		writeTxtFile(mAssetBundleFullPath + VERSION, mVersion);
		return true;
	}
	public bool writeFileList(string path)
	{
		List<string> ignoreSuffix = new() { ".unity3d.manifest", ".meta" };
		writeFileList(path, generateFileInfoList(path, false, mIgnoreFile, null, ignoreSuffix));
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
	//------------------------------------------------------------------------------------------------------------------------------
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
		PlayerSettings.bundleVersion = mVersion;
		// 需要定位查看一次工程中所有的timeline文件,否则打包后无法播放timeline
		foreach (string file in findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".playable"))
		{
			Selection.activeObject = loadAsset(file);
			EditorGUIUtility.PingObject(Selection.activeObject);
		}

		// 添加宏定义
		mPlatformDefine = PlayerSettings.GetScriptingDefineSymbols(getBuildBuildTarget());
		// 对当前的宏进行检查,避免由于上一次打包失败没有正确还原宏而导致打包出现问题
		if (mPlatformDefine != getDefaultPlatformDefine())
		{
			logWarning("当前的宏定义错误:" + mPlatformDefine + ", 已还原为:" + getDefaultPlatformDefine());
			mPlatformDefine = getDefaultPlatformDefine();
			PlayerSettings.SetScriptingDefineSymbols(getBuildBuildTarget(), mPlatformDefine);
		}
		log("备份宏:" + mPlatformDefine);
		configureScriptingDefine();

		if (mBuildHybridCLR)
		{
			buildHotFix(true);
		}

		createDir(mOutputPath);

		AssetDatabase.Refresh();

		// 需要先更新版本号文件
		writeVersion();
		backupAssets();
		return true;
	}
	protected virtual void afterBuild(string fullPath)
	{
		recoverAssets();
		// 还原宏定义
		PlayerSettings.SetScriptingDefineSymbols(getBuildBuildTarget(), mPlatformDefine);
		log("还原宏:" + mPlatformDefine);
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
			// 0表示不备份,1表示备份到BuildTemp,2表示备份到InstallTimeTemp
			int backupDest = 1;
			if (file.StartsWith(mAssetBundleFullPath))
			{
				// 如果是GooglePlay的安装包,则需要将当前平台下非动态下载的所有资源文件备份到指定临时目录
				if (mGooglePlay)
				{
					// meta和manifest文件不打进包里,所以备份到临时目录
					// 动态下载的文件备份到BuildTemp,其他的备份到InstallTimeTemp
					backupDest = file.endWith(".meta", false) || file.endWith(".manifest", false) || isDynamicDownloadAsset(file) ? 1 : 2;
				}
				// 版本号文件不备份
				else if (file.EndsWith(VERSION))
				{
					backupDest = 0;
				}
				// 启用热更时,动态下载的文件备份到临时目录,其他不进行备份
				else if (mEnableHotFix)
				{
					if (isDynamicDownloadAsset(file))
					{
						backupDest = 1;
					}
					else
					{
						backupDest = 0;
					}
				}
				// 未启用热更时,所有文件都不进行备份
				else
				{
					backupDest = 0;
				}
			}
			if (backupDest == 1)
			{
				backupFileToBuildTemp(file);
			}
			else if (backupDest == 2)
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
}