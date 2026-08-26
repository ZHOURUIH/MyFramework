using System;
using System.IO;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using static SpineAnimationFileNameUtility;
#if SPINE_RUNTIME_43
using SpineAnimationFileVersion = Spine43AnimationFile;
using SpineAnimationCommonDataVersion = Spine43AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine43SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine43AnimationBinaryReader;
#elif SPINE_RUNTIME_42
using SpineAnimationFileVersion = Spine42AnimationFile;
using SpineAnimationCommonDataVersion = Spine42AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine42SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine42AnimationBinaryReader;
#elif SPINE_RUNTIME_41
using SpineAnimationFileVersion = Spine41AnimationFile;
using SpineAnimationCommonDataVersion = Spine41AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine41SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine41AnimationBinaryReader;
#elif SPINE_RUNTIME_40
using SpineAnimationFileVersion = Spine40AnimationFile;
using SpineAnimationCommonDataVersion = Spine40AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine40SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine40AnimationBinaryReader;
#else
#error SpineSplitter仅支持通过UPM安装的Spine 4.0、4.1、4.2或4.3 Runtime。
#endif

// 验证当前选中的拆分SkeletonDataAsset是否可以正确读取全部单动画文件,不会把验证动画加入SkeletonData。
public static class SpineDynamicAnimationVerifier
{
    [MenuItem("Tools/Spine/验证动态单动画文件")]
    public static void verifySelectedSkeleton()
    {
        SkeletonDataAsset skeletonDataAsset = Selection.activeObject as SkeletonDataAsset;
        if (skeletonDataAsset == null)
        {
            Debug.LogError("请先在Project窗口选择拆分后的SkeletonDataAsset");
            return;
        }
        SkeletonData skeletonData = skeletonDataAsset.GetSkeletonData(false);
        if (skeletonData == null)
        {
            Debug.LogError("SkeletonDataAsset读取失败", skeletonDataAsset);
            return;
        }
        string generatedSuffix = SKELETON_ONLY_SUFFIX + SKELETON_DATA_SUFFIX;
        if (!skeletonDataAsset.name.EndsWith(generatedSuffix, StringComparison.Ordinal))
        {
            Debug.LogError("当前SkeletonDataAsset不是拆分后生成的资源:" + skeletonDataAsset.name, skeletonDataAsset);
            return;
        }
        string skeletonResourceName = getSkeletonResourceName(skeletonDataAsset.name);
        string skeletonDataAssetPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
        string sourceDirectory = Path.GetDirectoryName(skeletonDataAssetPath).Replace('\\', '/');
        string animationDirectory = combineAssetPath(sourceDirectory, getAnimationDirectoryName(skeletonResourceName));
        string commonPath = combineAssetPath(animationDirectory, getCommonFileName(skeletonDataAsset.name));
        TextAsset commonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(commonPath);
        if (commonAsset == null)
        {
            Debug.LogError("没有找到与当前SkeletonDataAsset对应的动画公共文件:" + commonPath, skeletonDataAsset);
            return;
        }
        SpineAnimationCommonDataVersion commonData;
        try
        {
            commonData = SpineAnimationFileVersion.readCommon(commonAsset.bytes);
        }
        catch (Exception exception)
        {
            Debug.LogError("读取动画公共文件失败:" + commonPath + "\n" + exception.Message, commonAsset);
            Debug.LogException(exception);
            return;
        }
        if (!isMatchedSkeleton(skeletonData, commonData))
        {
            Debug.LogError("动画公共文件与当前SkeletonDataAsset不匹配:" + commonPath, commonAsset);
            return;
        }
        string absoluteDirectory = assetPathToAbsolutePath(animationDirectory);
        if (!Directory.Exists(absoluteDirectory))
        {
            Debug.LogError("动画目录不存在:" + animationDirectory, skeletonDataAsset);
            return;
        }
        string[] files = Directory.GetFiles(absoluteDirectory, "*.bytes", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.Ordinal);
        int animationFileCount = 0;
        int successCount = 0;
        try
        {
            for (int i = 0; i < files.Length; ++i)
            {
                string filePath = files[i];
                byte[] bytes = File.ReadAllBytes(filePath);
                if (!SpineAnimationFileVersion.isAnimationFile(bytes))
                {
                    continue;
                }
                ++animationFileCount;
                EditorUtility.DisplayProgressBar("验证Spine动态动画", Path.GetFileName(filePath), (float)animationFileCount / Mathf.Max(1, files.Length));
                SpineSingleAnimationDataVersion animationData = SpineAnimationFileVersion.readAnimationNoCopy(bytes);
                if (animationData.mSkeletonHash != commonData.mSkeletonHash)
                {
                    Debug.LogError("Skeleton Hash不一致:" + filePath);
                    return;
                }
                if (!string.Equals(animationData.mSpineVersion, commonData.mSpineVersion, StringComparison.Ordinal))
                {
                    Debug.LogError("Spine版本不一致:" + filePath);
                    return;
                }
                SpineAnimationBinaryReaderVersion reader = new SpineAnimationBinaryReaderVersion();
                Spine.Animation animation = reader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData,
                                                                skeletonDataAsset.scale, animationData.mAnimationName);
                if (animation == null || !string.Equals(animation.Name, animationData.mAnimationName, StringComparison.Ordinal))
                {
                    Debug.LogError("动画解析结果不正确:" + filePath);
                    return;
                }
                ++successCount;
            }
            Debug.Log("Spine动态单动画验证完成" +
                "\nSkeletonDataAsset:" + skeletonDataAssetPath +
                "\n公共文件:" + commonPath +
                "\n动画目录:" + animationDirectory +
                "\n动画文件:" + animationFileCount +
                "\n验证成功:" + successCount,
                skeletonDataAsset);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    //---------------------------------------------------------------------------------------------------------------------------
    private static bool isMatchedSkeleton(SkeletonData skeletonData, SpineAnimationCommonDataVersion commonData)
    {
        if (!string.Equals(skeletonData.Version, commonData.mSpineVersion, StringComparison.Ordinal))
        {
            return false;
        }
        return SpineSkeletonHashUtility.getStableHash(skeletonData.Hash) == commonData.mSkeletonHash;
    }
    private static string assetPathToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }
}
