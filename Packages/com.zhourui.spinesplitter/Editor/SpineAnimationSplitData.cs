using System;
using System.Collections.Generic;

// Spine动画拆分结果,与具体Spine版本无关。
public class SpineAnimationSplitResult
{
    public bool mSuccess;
    public string mError = string.Empty;
    public string mSourceSkeletonAssetPath = string.Empty;
    public string mSourceSkeletonDataAssetPath = string.Empty;
    public string mGeneratedSkeletonAssetPath = string.Empty;
    public string mGeneratedSkeletonDataAssetPath = string.Empty;
    public string mAnimationDirectoryAssetPath = string.Empty;
    public int mAnimationCount;
    public int mClearedGeneratedFileCount;
    public long mTotalOutputBytes;
}

// Spine动画拆分输出计划,与具体Spine版本无关。
public class SpineAnimationSplitOutputPlan
{
    public string mCommonFileName = string.Empty;
    public readonly HashSet<string> mExpectedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public readonly List<string> mAnimationFileNameByIndex = new List<string>();
    public readonly Dictionary<string, string> mAnimationNameByFileName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
