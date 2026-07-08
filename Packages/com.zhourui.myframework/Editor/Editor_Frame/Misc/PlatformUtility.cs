using UnityEditor;
#if USE_HYBRID_CLR
using HybridCLR.Editor;
using HybridCLR.Editor.HotUpdate;
#endif
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if USE_OBFUZ
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
using static FrameUtility;
using static UnityUtility;
using static EditorCommonUtility;
using static FrameBaseDefine;
using static PlatformBase;

public class PlatformUtility
{
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
    public static string generateFileList(string assetBundlePath, List<string> ignoreFiles = null)
    {
        string fileContent = EMPTY;
        List<string> fileInfoList = findFileList(assetBundlePath, ignoreFiles, null, new() { ASSET_BUNDLE_SUFFIX + ".manifest", ".meta" });
        // 将所有文件信息写入文件
        fileContent += fileInfoList.Count.IToS() + "\n";
        foreach (string item in fileInfoList)
        {
            fileContent += item.removeStartString(assetBundlePath) + "\t" + getFileSize(item).IToS() + "\t" + generateFileMD5(item, false) + "\n";
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
            if (matchSuffix(ignoreSuffix, newPath) ||
                isIgnorePath(newPath, ignorePath) ||
                ignoreFiles.contains(newPath.removeStartString(assetBundlePath)))
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
    public static long getVersionPart(string version, int index)
    {
        if (version.isEmpty())
        {
            return 0;
        }
        List<long> numbers = version.SToLsNonAlloc('.');
        if (index < 0 || index >= numbers.Count)
        {
            logError("获取版本号数字错误");
            return 0;
        }
        return numbers[index];
    }
    public static BuildOptions generateBuildOption(bool isTest)
    {
        BuildOptions options = BuildOptions.None;
        options |= BuildOptions.CompressWithLz4HC;
        if (isTest)
        {
            options |= BuildOptions.Development;
            // 不再开启自动连接Profiler,因为这会使打出来的程序无法在其他电脑调试
            options |= BuildOptions.ConnectWithProfiler;
            // 深度分析会导致卡顿严重,谨慎开启
            options |= BuildOptions.EnableDeepProfilingSupport;
        }
        return options;
    }
    // 将本地文件夹的所有文件上传到linux服务器的指定目录中,返回值表示是否上传成功并且检测通过,remoteDeletePath是相对路径,removeCopyFullPath是绝对路径
    public static bool uploadFileToLinuxServer(string localPath, string remoteDeletePath, string removeCopyFullPath, string userNameAndIP, string password)
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
    public static bool checkUploadedFile(List<string> output, string remoteDeletePath, string localPath)
    {
        Dictionary<string, GameFileInfo> fileInfoList = new();
        string curDirectory = "";
        foreach (string line in output)
        {
            string[] keys = line.split(' ');
            if (keys.Length >= 9)
            {
                if (keys[0][0] == '-')
                {
                    var fileInfo = RemoteFileInfo.parse(keys);
                    if (fileInfo.mFileName == FILE_LIST || fileInfo.mFileName == VERSION)
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
#if USE_OBFUZ
    public static List<string> getSearchPath()
    {
        string editorExePath = EditorApplication.applicationPath;
        editorExePath = getFilePath(editorExePath);
        List<string> searchPaths = new();
        searchPaths.AddRange(ObfuscatorBuilder.BuildUnityAssemblySearchPaths());
        searchPaths.Add(editorExePath + "/Data/Managed");
#if USE_HYBRID_CLR
        searchPaths.Add(F_PROJECT_PATH + SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget));
#endif
        searchPaths.Add(F_PROJECT_PATH + "Library/PackageCache/com.unity.burst@59eb6f11d242");
        searchPaths.Add(F_PROJECT_PATH + "Library/PackageCache/com.unity.collections@d49facba0036/Unity.Collections.LowLevel.ILSupport");
        // mac中的路径
        searchPaths.Add(EditorApplication.applicationPath + "/Contents/Managed/UnityEngine");
        searchPaths.Add(EditorApplication.applicationPath + "/Contents/Managed");
        searchPaths.Add(F_ASSET_BUNDLE_PATH);
        return searchPaths;
    }
    public static void obfuscate(string version, bool isTest)
    {
        SecretSettings secretSettings = ObfuzSettings.Instance.secretSettings;
        // 因为只混淆热更程序集,所以也只生成动态的密钥
        secretSettings.defaultDynamicSecretKey = "Code Philosophy-Dynamic" + randomInt(0, 100000000);
        secretSettings.assembliesUsingDynamicSecretKeys = ObfuzSettings.Instance.assemblySettings.assembliesToObfuscate;
        secretSettings.randomSeed = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
        ObfuzSettings.Instance.symbolObfusSettings.debug = isTest;
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