using System;
using System.Collections.Generic;
using System.IO;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using static SpineAnimationFileNameUtility;
using static UnityEditor.AssetDatabase;

// Spine 4.0动画拆分器,不包含任何窗口逻辑。
// 输入原始.skel.bytes,自动找到引用它的SkeletonDataAsset,生成无动画Skeleton、Common和全部单动画文件。
public static class Spine40AnimationSplitter
{
    public static bool isSourceSkeletonAssetPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return false;
        }
        assetPath = normalizeAssetPath(assetPath);
        if (!assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (!assetPath.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        string fileName = Path.GetFileName(assetPath);
        if (fileName.EndsWith(SKELETON_ONLY_SUFFIX + ".skel.bytes", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return true;
    }

    public static SpineAnimationSplitResult split(string sourceSkeletonAssetPath, bool verifyAfterGenerate = true, bool showProgress = false)
    {
        SpineAnimationSplitResult result = new SpineAnimationSplitResult();
        result.mSourceSkeletonAssetPath = normalizeAssetPath(sourceSkeletonAssetPath);
        SkeletonDataAsset sourceSkeletonDataAsset = null;
        try
        {
            if (!isSourceSkeletonAssetPath(result.mSourceSkeletonAssetPath))
            {
                throw new Exception("不是可拆分的原始Spine .skel.bytes文件:" + result.mSourceSkeletonAssetPath);
            }
            TextAsset sourceTextAsset = LoadAssetAtPath<TextAsset>(result.mSourceSkeletonAssetPath);
            if (sourceTextAsset == null)
            {
                throw new Exception("无法加载Spine源文件:" + result.mSourceSkeletonAssetPath);
            }
            sourceSkeletonDataAsset = findSourceSkeletonDataAsset(result.mSourceSkeletonAssetPath);
            if (sourceSkeletonDataAsset == null)
            {
                throw new Exception("没有找到引用该.skel.bytes的SkeletonDataAsset:" + result.mSourceSkeletonAssetPath);
            }
            result.mSourceSkeletonDataAssetPath = normalizeAssetPath(GetAssetPath(sourceSkeletonDataAsset));
            if (string.IsNullOrEmpty(result.mSourceSkeletonDataAssetPath))
            {
                throw new Exception("无法获取源SkeletonDataAsset路径:" + sourceSkeletonDataAsset.name);
            }
            byte[] sourceBytes = sourceTextAsset.bytes;
            if (sourceBytes == null || sourceBytes.Length == 0)
            {
                throw new Exception("Spine源文件为空:" + result.mSourceSkeletonAssetPath);
            }
            displayProgress(showProgress, "自动拆分Spine动画", "正在扫描:" + result.mSourceSkeletonAssetPath, 0.02f);
            Spine40BinaryScanner scanner = new Spine40BinaryScanner();
            SpineBinaryScanResult scanResult = scanner.scan(sourceBytes);
            if (scanResult == null)
            {
                throw new Exception("Spine二进制扫描失败:" + result.mSourceSkeletonAssetPath);
            }
            if (string.IsNullOrEmpty(scanResult.mVersion) || !scanResult.mVersion.StartsWith("4.0", StringComparison.Ordinal))
            {
                throw new Exception("当前自动拆分只支持Spine 4.0,实际版本:" + scanResult.mVersion + ",文件:" + result.mSourceSkeletonAssetPath);
            }
            string sourceDirectory = normalizeAssetPath(Path.GetDirectoryName(result.mSourceSkeletonAssetPath));
            string sourceSkeletonName = removeSkeletonBinarySuffix(Path.GetFileName(result.mSourceSkeletonAssetPath));
            string sourceSkeletonDataAssetName = Path.GetFileNameWithoutExtension(result.mSourceSkeletonDataAssetPath);
            string skeletonResourceName = getSkeletonResourceName(sourceSkeletonDataAssetName);
            string generatedSkeletonDataAssetName = getAnimationlessSkeletonDataAssetName(skeletonResourceName);
            result.mGeneratedSkeletonAssetPath = combineAssetPath(sourceDirectory, getAnimationlessSkeletonFileName(skeletonResourceName));
            result.mGeneratedSkeletonDataAssetPath = combineAssetPath(sourceDirectory, generatedSkeletonDataAssetName + ".asset");
            result.mAnimationDirectoryAssetPath = combineAssetPath(sourceDirectory, getAnimationDirectoryName(skeletonResourceName));
            result.mAnimationCount = scanResult.mAnimations.Count;
            SpineAnimationSplitOutputPlan outputPlan = createOutputPlan(scanResult, generatedSkeletonDataAssetName);
            string generatedSkeletonAbsolutePath = assetPathToAbsolutePath(result.mGeneratedSkeletonAssetPath);
            string animationDirectoryAbsolutePath = assetPathToAbsolutePath(result.mAnimationDirectoryAssetPath);
            validateOutputPaths(result);
            displayProgress(showProgress, "自动拆分Spine动画", "正在清理旧生成资源...", 0.06f);
            result.mClearedGeneratedFileCount = clearGeneratedOutput(result);
            displayProgress(showProgress, "自动拆分Spine动画", "正在生成无动画Skeleton:" + result.mGeneratedSkeletonAssetPath, 0.08f);
            writeAnimationlessSkeleton(generatedSkeletonAbsolutePath, sourceBytes, scanResult);
            ImportAsset(result.mGeneratedSkeletonAssetPath, ImportAssetOptions.ForceSynchronousImport);
            TextAsset generatedSkeletonTextAsset = LoadAssetAtPath<TextAsset>(result.mGeneratedSkeletonAssetPath);
            if (generatedSkeletonTextAsset == null)
            {
                throw new Exception("无法加载生成的无动画Skeleton:" + result.mGeneratedSkeletonAssetPath);
            }
            if (verifyAfterGenerate)
            {
                displayProgress(showProgress, "自动拆分Spine动画", "正在验证无动画Skeleton...", 0.14f);
                verifyAnimationlessSkeleton(sourceSkeletonDataAsset, generatedSkeletonTextAsset);
            }
            createOrUpdateGeneratedSkeletonDataAsset(sourceSkeletonDataAsset, generatedSkeletonTextAsset, result.mGeneratedSkeletonDataAssetPath);
            result.mTotalOutputBytes += new FileInfo(generatedSkeletonAbsolutePath).Length;
            if (File.Exists(assetPathToAbsolutePath(result.mGeneratedSkeletonDataAssetPath)))
            {
                result.mTotalOutputBytes += new FileInfo(assetPathToAbsolutePath(result.mGeneratedSkeletonDataAssetPath)).Length;
            }
            Directory.CreateDirectory(animationDirectoryAbsolutePath);
            Spine40AnimationCommonData commonData = createAnimationCommonData(scanResult, sourceSkeletonName);
            string commonAbsolutePath = Path.Combine(animationDirectoryAbsolutePath, outputPlan.mCommonFileName);
            displayProgress(showProgress, "自动拆分Spine动画", "正在生成动画公共数据...", 0.18f);
            Spine40AnimationFile.writeCommon(commonAbsolutePath, commonData);
            if (verifyAfterGenerate)
            {
                verifyGeneratedCommonFile(commonAbsolutePath, commonData);
            }
            result.mTotalOutputBytes += new FileInfo(commonAbsolutePath).Length;
            for (int i = 0; i < scanResult.mAnimations.Count; ++i)
            {
                SpineAnimationBinaryRange range = scanResult.mAnimations[i];
                string fileName = outputPlan.mAnimationFileNameByIndex[i];
                string absoluteFilePath = Path.Combine(animationDirectoryAbsolutePath, fileName);
                float progress = 0.20f + 0.72f * (i + 1) / Math.Max(1, scanResult.mAnimations.Count);
                displayProgress(showProgress, "自动拆分Spine动画", "正在生成:" + range.mName, progress);
                Spine40SingleAnimationData animationData = createSingleAnimationData(range, sourceBytes, scanResult);
                Spine40AnimationFile.writeAnimation(absoluteFilePath, animationData);
                if (verifyAfterGenerate)
                {
                    verifyGeneratedSingleAnimationFile(absoluteFilePath, animationData);
                }
                result.mTotalOutputBytes += new FileInfo(absoluteFilePath).Length;
            }
            result.mClearedGeneratedFileCount += deleteObsoleteAnimationMetaFiles(animationDirectoryAbsolutePath);
            displayProgress(showProgress, "自动拆分Spine动画", "正在刷新Unity资源...", 0.98f);
            Refresh(ImportAssetOptions.ForceSynchronousImport);
            SaveAssets();
            result.mSuccess = true;
            UnityEngine.Object logContext = LoadAssetAtPath<SkeletonDataAsset>(result.mGeneratedSkeletonDataAssetPath);
            if (logContext == null)
            {
                logContext = generatedSkeletonTextAsset;
            }
            Debug.Log("Spine自动拆分完成" +
                "\n源SkeletonDataAsset:" + result.mSourceSkeletonDataAssetPath +
                "\n源文件:" + result.mSourceSkeletonAssetPath +
                "\n无动画Skeleton:" + result.mGeneratedSkeletonAssetPath +
                "\n无动画SkeletonDataAsset:" + result.mGeneratedSkeletonDataAssetPath +
                "\n动画目录:" + result.mAnimationDirectoryAssetPath +
                "\n动画数量:" + result.mAnimationCount +
                "\n清理旧生成文件:" + result.mClearedGeneratedFileCount +
                "\n输出总大小:" + getMemoryText(result.mTotalOutputBytes), logContext);
        }
        catch (Exception exception)
        {
            result.mError = exception.Message;
            Debug.LogError("Spine自动拆分失败:" + result.mSourceSkeletonAssetPath + "\n" + exception, sourceSkeletonDataAsset);
        }
        finally
        {
            if (showProgress)
            {
                EditorUtility.ClearProgressBar();
            }
        }
        return result;
    }

    public static SkeletonDataAsset findSourceSkeletonDataAsset(string sourceSkeletonAssetPath)
    {
        sourceSkeletonAssetPath = normalizeAssetPath(sourceSkeletonAssetPath);
        if (!isSourceSkeletonAssetPath(sourceSkeletonAssetPath))
        {
            return null;
        }
        string sourceDirectory = normalizeAssetPath(Path.GetDirectoryName(sourceSkeletonAssetPath));
        List<SkeletonDataAsset> matches = findMatchingSkeletonDataAssets(sourceSkeletonAssetPath, new string[] { sourceDirectory });
        if (matches.Count == 1)
        {
            return matches[0];
        }
        if (matches.Count > 1)
        {
            throw createMultipleSkeletonDataAssetException(sourceSkeletonAssetPath, matches);
        }
        matches = findMatchingSkeletonDataAssets(sourceSkeletonAssetPath, null);
        if (matches.Count == 1)
        {
            return matches[0];
        }
        if (matches.Count > 1)
        {
            throw createMultipleSkeletonDataAssetException(sourceSkeletonAssetPath, matches);
        }
        return null;
    }

    public static List<string> findAllSourceSkeletonAssetPaths()
    {
        string[] guids = FindAssets("t:TextAsset", new string[] { "Assets" });
        List<string> paths = new List<string>();
        HashSet<string> pathSet = new HashSet<string>();
        for (int i = 0; i < guids.Length; ++i)
        {
            string assetPath = normalizeAssetPath(GUIDToAssetPath(guids[i]));
            if (!isSourceSkeletonAssetPath(assetPath) || !pathSet.Add(assetPath))
            {
                continue;
            }
            paths.Add(assetPath);
        }
        paths.Sort();
        return paths;
    }
    //---------------------------------------------------------------------------------------------------------------------------
    private static List<SkeletonDataAsset> findMatchingSkeletonDataAssets(string sourceSkeletonAssetPath, string[] searchFolders)
    {
        string[] guids = searchFolders == null ? FindAssets("t:SkeletonDataAsset") : FindAssets("t:SkeletonDataAsset", searchFolders);
        List<SkeletonDataAsset> matches = new List<SkeletonDataAsset>();
        HashSet<string> matchedPaths = new HashSet<string>();
        for (int i = 0; i < guids.Length; ++i)
        {
            string assetPath = normalizeAssetPath(GUIDToAssetPath(guids[i]));
            if (string.IsNullOrEmpty(assetPath) || !matchedPaths.Add(assetPath))
            {
                continue;
            }
            SkeletonDataAsset skeletonDataAsset = LoadAssetAtPath<SkeletonDataAsset>(assetPath);
            if (skeletonDataAsset == null || skeletonDataAsset.skeletonJSON == null)
            {
                continue;
            }
            string referencedSkeletonPath = normalizeAssetPath(GetAssetPath(skeletonDataAsset.skeletonJSON));
            if (string.Equals(referencedSkeletonPath, sourceSkeletonAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(skeletonDataAsset);
            }
        }
        return matches;
    }

    private static Exception createMultipleSkeletonDataAssetException(string sourceSkeletonAssetPath, List<SkeletonDataAsset> matches)
    {
        List<string> paths = new List<string>();
        for (int i = 0; i < matches.Count; ++i)
        {
            paths.Add(normalizeAssetPath(GetAssetPath(matches[i])));
        }
        paths.Sort(StringComparer.OrdinalIgnoreCase);
        return new Exception("同一个.skel.bytes被多个SkeletonDataAsset引用,无法确定自动拆分时应使用哪一个:" + sourceSkeletonAssetPath + "\n" + string.Join("\n", paths));
    }

    private static SpineAnimationSplitOutputPlan createOutputPlan(SpineBinaryScanResult scanResult, string generatedSkeletonDataAssetName)
    {
        SpineAnimationSplitOutputPlan plan = new SpineAnimationSplitOutputPlan();
        plan.mCommonFileName = getCommonFileName(generatedSkeletonDataAssetName);
        plan.mExpectedFileNames.Add(plan.mCommonFileName);
        for (int i = 0; i < scanResult.mAnimations.Count; ++i)
        {
            SpineAnimationBinaryRange range = scanResult.mAnimations[i];
            string fileName = getAnimationFileName(generatedSkeletonDataAssetName, range.mName);
            if (!plan.mExpectedFileNames.Add(fileName))
            {
                string existingName;
                if (!plan.mAnimationNameByFileName.TryGetValue(fileName, out existingName))
                {
                    existingName = "Common";
                }
                throw new Exception("Spine动画输出文件名冲突:" + existingName + " <-> " + range.mName + " => " + fileName);
            }
            plan.mAnimationFileNameByIndex.Add(fileName);
            plan.mAnimationNameByFileName.Add(fileName, range.mName);
        }
        return plan;
    }

    private static void validateOutputPaths(SpineAnimationSplitResult result)
    {
        string generatedSkeletonDataAssetAbsolutePath = assetPathToAbsolutePath(result.mGeneratedSkeletonDataAssetPath);
        if (Directory.Exists(generatedSkeletonDataAssetAbsolutePath))
        {
            throw new Exception("生成SkeletonDataAsset路径已经被目录占用:" + result.mGeneratedSkeletonDataAssetPath);
        }
        string animationDirectoryAbsolutePath = assetPathToAbsolutePath(result.mAnimationDirectoryAssetPath);
        if (File.Exists(animationDirectoryAbsolutePath))
        {
            throw new Exception("动画输出目录路径已经被文件占用:" + result.mAnimationDirectoryAssetPath);
        }
    }

    // 删除旧的生成内容,但保留已有.meta。
    // 这样每次都是重新生成文件内容,同时不会因为删除.meta导致Unity资源GUID变化。
    private static int clearGeneratedOutput(SpineAnimationSplitResult result)
    {
        int clearedFileCount = 0;
        string generatedSkeletonAbsolutePath = assetPathToAbsolutePath(result.mGeneratedSkeletonAssetPath);
        if (File.Exists(generatedSkeletonAbsolutePath))
        {
            File.Delete(generatedSkeletonAbsolutePath);
            ++clearedFileCount;
        }
        string animationDirectoryAbsolutePath = assetPathToAbsolutePath(result.mAnimationDirectoryAssetPath);
        if (!Directory.Exists(animationDirectoryAbsolutePath))
        {
            Directory.CreateDirectory(animationDirectoryAbsolutePath);
            return clearedFileCount;
        }
        string[] oldFiles = Directory.GetFiles(animationDirectoryAbsolutePath, "*.bytes", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < oldFiles.Length; ++i)
        {
            File.Delete(oldFiles[i]);
            ++clearedFileCount;
        }
        return clearedFileCount;
    }

    // 已经不存在的动画不会重新生成,此时删除它遗留下来的.meta。
    // 当前仍然存在的Common和动画文件已经重新写回,对应.meta会继续保留。
    private static int deleteObsoleteAnimationMetaFiles(string animationDirectoryAbsolutePath)
    {
        if (!Directory.Exists(animationDirectoryAbsolutePath))
        {
            return 0;
        }
        int deletedCount = 0;
        string[] metaFiles = Directory.GetFiles(animationDirectoryAbsolutePath, "*.bytes.meta", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < metaFiles.Length; ++i)
        {
            string assetFilePath = metaFiles[i].Substring(0, metaFiles[i].Length - ".meta".Length);
            if (File.Exists(assetFilePath))
            {
                continue;
            }
            File.Delete(metaFiles[i]);
            ++deletedCount;
        }
        return deletedCount;
    }

    private static void writeAnimationlessSkeleton(string absoluteOutputPath, byte[] sourceBytes, SpineBinaryScanResult scanResult)
    {
        string outputDirectory = Path.GetDirectoryName(absoluteOutputPath);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        using (FileStream outputStream = new FileStream(absoluteOutputPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            int prefixLength = checked((int)scanResult.mAnimationCountPosition);
            outputStream.Write(sourceBytes, 0, prefixLength);
            writePositiveVarInt(outputStream, 0);
        }
    }

    private static void verifyAnimationlessSkeleton(SkeletonDataAsset sourceSkeletonDataAsset, TextAsset generatedTextAsset)
    {
        SkeletonDataAsset verifyAsset = UnityEngine.Object.Instantiate(sourceSkeletonDataAsset);
        try
        {
            verifyAsset.skeletonJSON = generatedTextAsset;
            verifyAsset.Clear();
            SkeletonData skeletonData = verifyAsset.GetSkeletonData(false);
            if (skeletonData == null)
            {
                throw new Exception("生成的无动画Skeleton无法被原版Spine读取器解析");
            }
            if (skeletonData.Animations.Count != 0)
            {
                throw new Exception("无动画Skeleton中仍然存在动画,数量:" + skeletonData.Animations.Count);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(verifyAsset);
        }
    }

    private static void createOrUpdateGeneratedSkeletonDataAsset(SkeletonDataAsset sourceSkeletonDataAsset, TextAsset generatedTextAsset, string outputAssetPath)
    {
        UnityEngine.Object existingObject = LoadMainAssetAtPath(outputAssetPath);
        if (existingObject != null && !(existingObject is SkeletonDataAsset))
        {
            throw new Exception("生成SkeletonDataAsset路径已经存在其他类型资源:" + outputAssetPath);
        }
        SkeletonDataAsset generatedAsset = existingObject as SkeletonDataAsset;
        bool isNewAsset = generatedAsset == null;
        if (isNewAsset)
        {
            generatedAsset = UnityEngine.Object.Instantiate(sourceSkeletonDataAsset);
        }
        else
        {
            EditorUtility.CopySerialized(sourceSkeletonDataAsset, generatedAsset);
        }
        generatedAsset.name = Path.GetFileNameWithoutExtension(outputAssetPath);
        generatedAsset.skeletonJSON = generatedTextAsset;
        generatedAsset.fromAnimation = new string[0];
        generatedAsset.toAnimation = new string[0];
        generatedAsset.duration = new float[0];
        generatedAsset.Clear();
        if (isNewAsset)
        {
            CreateAsset(generatedAsset, outputAssetPath);
        }
        else
        {
            EditorUtility.SetDirty(generatedAsset);
        }
    }

    private static Spine40AnimationCommonData createAnimationCommonData(SpineBinaryScanResult scanResult, string sourceSkeletonName)
    {
        Spine40AnimationCommonData commonData = new Spine40AnimationCommonData();
        commonData.mFileVersion = Spine40AnimationFile.CURRENT_VERSION;
        commonData.mSourceSkeletonName = sourceSkeletonName;
        commonData.mSpineVersion = scanResult.mVersion;
        commonData.mSkeletonHash = scanResult.mSkeletonHash;
        commonData.mStrings = new string[scanResult.mStrings.Length];
        Array.Copy(scanResult.mStrings, commonData.mStrings, scanResult.mStrings.Length);
        return commonData;
    }

    private static Spine40SingleAnimationData createSingleAnimationData(SpineAnimationBinaryRange range, byte[] sourceBytes, SpineBinaryScanResult scanResult)
    {
        int startPosition = checked((int)range.mStartPosition);
        int length = checked((int)range.mLength);
        byte[] animationBytes = new byte[length];
        Buffer.BlockCopy(sourceBytes, startPosition, animationBytes, 0, length);
        Spine40SingleAnimationData animationData = new Spine40SingleAnimationData();
        animationData.mFileVersion = Spine40AnimationFile.CURRENT_VERSION;
        animationData.mSpineVersion = scanResult.mVersion;
        animationData.mSkeletonHash = scanResult.mSkeletonHash;
        animationData.mAnimationName = range.mName;
        animationData.mBinaryData = animationBytes;
        return animationData;
    }

    private static void verifyGeneratedCommonFile(string absolutePath, Spine40AnimationCommonData expectedData)
    {
        Spine40AnimationCommonData actualData = Spine40AnimationFile.readCommon(File.ReadAllBytes(absolutePath));
        if (actualData.mFileVersion != Spine40AnimationFile.CURRENT_VERSION ||
            actualData.mSkeletonHash != expectedData.mSkeletonHash ||
            !string.Equals(actualData.mSourceSkeletonName, expectedData.mSourceSkeletonName, StringComparison.Ordinal) ||
            !string.Equals(actualData.mSpineVersion, expectedData.mSpineVersion, StringComparison.Ordinal))
        {
            throw new Exception("动画公共文件基础信息验证失败:" + absolutePath);
        }
        if (actualData.mStrings.Length != expectedData.mStrings.Length)
        {
            throw new Exception("动画公共文件共享字符串数量不一致:" + actualData.mStrings.Length + " != " + expectedData.mStrings.Length);
        }
        for (int i = 0; i < expectedData.mStrings.Length; ++i)
        {
            if (!string.Equals(actualData.mStrings[i], expectedData.mStrings[i], StringComparison.Ordinal))
            {
                throw new Exception("动画公共文件第" + i + "个共享字符串不一致");
            }
        }
    }

    private static void verifyGeneratedSingleAnimationFile(string absolutePath, Spine40SingleAnimationData expectedData)
    {
        Spine40SingleAnimationData actualData = Spine40AnimationFile.readAnimation(File.ReadAllBytes(absolutePath));
        if (actualData.mFileVersion != Spine40AnimationFile.CURRENT_VERSION ||
            actualData.mSkeletonHash != expectedData.mSkeletonHash ||
            !string.Equals(actualData.mSpineVersion, expectedData.mSpineVersion, StringComparison.Ordinal) ||
            !string.Equals(actualData.mAnimationName, expectedData.mAnimationName, StringComparison.Ordinal))
        {
            throw new Exception("单动画文件基础信息验证失败:" + absolutePath);
        }
        if (actualData.mBinaryData.Length != expectedData.mBinaryData.Length)
        {
            throw new Exception("单动画文件长度验证失败:" + expectedData.mAnimationName);
        }
        for (int i = 0; i < expectedData.mBinaryData.Length; ++i)
        {
            if (actualData.mBinaryData[i] != expectedData.mBinaryData[i])
            {
                throw new Exception("单动画文件二进制验证失败:" + expectedData.mAnimationName + ",位置:" + i);
            }
        }
    }

    private static void writePositiveVarInt(Stream stream, int value)
    {
        uint unsignedValue = unchecked((uint)value);
        while (true)
        {
            if ((unsignedValue & ~0x7FU) == 0)
            {
                stream.WriteByte((byte)unsignedValue);
                return;
            }
            stream.WriteByte((byte)((unsignedValue & 0x7FU) | 0x80U));
            unsignedValue >>= 7;
        }
    }

    private static string removeSkeletonBinarySuffix(string fileName)
    {
        if (fileName.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase))
        {
            return fileName.Substring(0, fileName.Length - ".skel.bytes".Length);
        }
        if (fileName.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
        {
            return fileName.Substring(0, fileName.Length - ".bytes".Length);
        }
        return Path.GetFileNameWithoutExtension(fileName);
    }

    private static string normalizeAssetPath(string assetPath)
    {
        return string.IsNullOrEmpty(assetPath) ? string.Empty : assetPath.Replace('\\', '/');
    }

    private static string assetPathToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }

    public static string getMemoryText(long bytes)
    {
        if (bytes < 1024L)
        {
            return bytes + " B";
        }
        if (bytes < 1024L * 1024L)
        {
            return (bytes / 1024.0).ToString("F2") + " KB";
        }
        return (bytes / (1024.0 * 1024.0)).ToString("F2") + " MB";
    }

    private static void displayProgress(bool showProgress, string title, string info, float progress)
    {
        if (showProgress)
        {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }
    }
}
