#if SPINE_RUNTIME_43 || SPINE_RUNTIME_42 || SPINE_RUNTIME_41 || SPINE_RUNTIME_40
using System;
using System.Collections.Generic;
using System.IO;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using static SpineAnimationFileNameUtility;
#if SPINE_RUNTIME_43
using static Spine43AnimationSplitter;
#elif SPINE_RUNTIME_42
using static Spine42AnimationSplitter;
#elif SPINE_RUNTIME_41
using static Spine41AnimationSplitter;
#elif SPINE_RUNTIME_40
using static Spine40AnimationSplitter;
#endif
using static UnityEditor.AssetDatabase;

// 监听当前Spine版本支持的源Skeleton文件和SkeletonDataAsset导入并自动执行动画拆分。
public class SpineAnimationAssetPostprocessor : AssetPostprocessor
{
    private const bool VERIFY_AFTER_GENERATE = true;
    private static readonly HashSet<string> mPendingSkeletonAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private static bool mDelayCallRegistered;
    private static bool mProcessing;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        // 拆分过程中会主动Import SkeletonOnly并最终Refresh全部生成文件。
        // 这些回调全部属于当前拆分产生的结果,不能再次进入待拆分队列。
        if (mProcessing)
        {
            return;
        }
        for (int i = 0; i < importedAssets.Length; ++i)
        {
            collectImportedAsset(importedAssets[i]);
        }
        for (int i = 0; i < movedAssets.Length; ++i)
        {
            collectImportedAsset(movedAssets[i]);
        }
        scheduleProcess();
    }

    private static void collectImportedAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return;
        }
        assetPath = normalizeAssetPath(assetPath);
        // 源Skeleton资源直接触发拆分。具体支持.json还是.skel.bytes由当前Spine版本Splitter决定。
        if (isSourceSkeletonAssetPath(assetPath))
        {
            mPendingSkeletonAssetPaths.Add(assetPath);
            return;
        }
        if (!assetPath.EndsWith(".asset"))
        {
            return;
        }
        // 我们自己生成的*_SkeletonOnly_SkeletonData.asset不是输入资源。
        if (isGeneratedSkeletonDataAssetPath(assetPath))
        {
            return;
        }
        SkeletonDataAsset skeletonDataAsset = LoadAssetAtPath<SkeletonDataAsset>(assetPath);
        if (skeletonDataAsset == null || skeletonDataAsset.skeletonJSON == null)
        {
            return;
        }
        string sourceSkeletonAssetPath = normalizeAssetPath(GetAssetPath(skeletonDataAsset.skeletonJSON));
        if (!isSourceSkeletonAssetPath(sourceSkeletonAssetPath))
        {
            return;
        }
        mPendingSkeletonAssetPaths.Add(sourceSkeletonAssetPath);
    }

    private static bool isGeneratedSkeletonDataAssetPath(string assetPath)
    {
        return Path.GetFileNameWithoutExtension(assetPath).EndsWith(SKELETON_ONLY_SUFFIX + SKELETON_DATA_SUFFIX);
    }

    [MenuItem("Tools/Spine/重新拆分全部Spine")]
    private static void splitAllSpines()
    {
        if (mProcessing)
        {
            Debug.LogWarning("当前正在执行Spine动画拆分");
            return;
        }
        List<string> sourceSkeletonAssetPaths = findAllSourceSkeletonAssetPaths();
        if (sourceSkeletonAssetPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("重新拆分全部Spine", "没有找到可以拆分的原始Spine Skeleton源文件。", "确定");
            return;
        }
        if (!EditorUtility.DisplayDialog("重新拆分全部Spine", "即将重新拆分" + sourceSkeletonAssetPaths.Count + "个Spine资源。" +
                                         "\n每个Spine都会重新生成SkeletonOnly、Common和全部单动画文件。", "开始拆分", "取消"))
        {
            return;
        }
        mPendingSkeletonAssetPaths.Clear();
        mProcessing = true;
        int successCount = 0;
        int failCount = 0;
        try
        {
            for (int i = 0; i < sourceSkeletonAssetPaths.Count; ++i)
            {
                string assetPath = sourceSkeletonAssetPaths[i];
                EditorUtility.DisplayProgressBar("重新拆分全部Spine", "[" + (i + 1) + "/" + sourceSkeletonAssetPaths.Count + "] " + 
                                                assetPath, (float)i / sourceSkeletonAssetPaths.Count);
                SpineAnimationSplitResult result = split(assetPath, VERIFY_AFTER_GENERATE, false);
                if (result.mSuccess)
                {
                    ++successCount;
                }
                else
                {
                    ++failCount;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            mProcessing = false;
        }
        Debug.Log("全部Spine重新拆分完成,总数:" + sourceSkeletonAssetPaths.Count + ",成功:" + successCount + ",失败:" + failCount);
        EditorUtility.DisplayDialog("重新拆分全部Spine", "处理完成。\n总数:" + sourceSkeletonAssetPaths.Count + "\n成功:" + successCount + "\n失败:" + failCount, "确定");
    }

    private static void scheduleProcess()
    {
        if (mProcessing || mDelayCallRegistered || mPendingSkeletonAssetPaths.Count == 0)
        {
            return;
        }
        mDelayCallRegistered = true;
        EditorApplication.delayCall += processPendingSkeletons;
    }

    private static void processPendingSkeletons()
    {
        mDelayCallRegistered = false;
        if (mProcessing || mPendingSkeletonAssetPaths.Count == 0)
        {
            return;
        }
        List<string> sourceSkeletonAssetPaths = new List<string>(mPendingSkeletonAssetPaths);
        mPendingSkeletonAssetPaths.Clear();
        sourceSkeletonAssetPaths.Sort(StringComparer.OrdinalIgnoreCase);
        mProcessing = true;
        try
        {
            for (int i = 0; i < sourceSkeletonAssetPaths.Count; ++i)
            {
                string assetPath = sourceSkeletonAssetPaths[i];
                if (!isSourceSkeletonAssetPath(assetPath))
                {
                    continue;
                }
                Debug.Log("检测到Spine资源导入,开始完整重新拆分:" + assetPath);
                SpineAnimationSplitResult result = split(assetPath, VERIFY_AFTER_GENERATE, false);
                if (!result.mSuccess)
                {
                    Debug.LogError("自动拆分Spine失败:" + assetPath + "\n" + result.mError);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            mProcessing = false;
        }
    }

    private static string normalizeAssetPath(string assetPath)
    {
        return string.IsNullOrEmpty(assetPath) ? string.Empty : assetPath.Replace('\\', '/');
    }
}
#endif