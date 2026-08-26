using System;
using System.Text;
// Spine拆分动画资源命名工具,运行时和编辑器共同使用。
public static class SpineAnimationFileNameUtility
{
    public const string FILE_SUFFIX = ".bytes";
    public const string ANIMATION_DIRECTORY_SUFFIX = "_Animations";
    public const string SKELETON_ONLY_SUFFIX = "_SkeletonOnly";
    public const string SKELETON_DATA_SUFFIX = "_SkeletonData";
    public const string COMMON_SUFFIX = "_Common";
    private const int MAX_FILE_NAME_LENGTH = 240;
    private static readonly string[] WINDOWS_RESERVED_NAMES = new string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
    public static string getSkeletonResourceName(string skeletonDataAssetName)
    {
        if (string.IsNullOrEmpty(skeletonDataAssetName))
        {
            throw new ArgumentException("SkeletonDataAsset名称为空", nameof(skeletonDataAssetName));
        }
        string generatedSuffix = SKELETON_ONLY_SUFFIX + SKELETON_DATA_SUFFIX;
        if (skeletonDataAssetName.EndsWith(generatedSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return skeletonDataAssetName.Substring(0, skeletonDataAssetName.Length - generatedSuffix.Length);
        }
        if (skeletonDataAssetName.EndsWith(SKELETON_DATA_SUFFIX, StringComparison.OrdinalIgnoreCase))
        {
            return skeletonDataAssetName.Substring(0, skeletonDataAssetName.Length - SKELETON_DATA_SUFFIX.Length);
        }
        return skeletonDataAssetName;
    }
    public static string getAnimationlessSkeletonFileName(string sourceSkeletonName)
    {
        return sanitizeFileNamePart(sourceSkeletonName) + SKELETON_ONLY_SUFFIX + ".skel.bytes";
    }
    public static string getAnimationlessSkeletonDataAssetName(string sourceSkeletonName)
    {
        return sanitizeFileNamePart(sourceSkeletonName) + SKELETON_ONLY_SUFFIX + SKELETON_DATA_SUFFIX;
    }
    public static string getCommonFileName(string skeletonDataAssetName)
    {
        return getCommonAssetName(skeletonDataAssetName) + FILE_SUFFIX;
    }
    public static string getCommonAssetName(string skeletonDataAssetName)
    {
        return validateGeneratedName(sanitizeFileNamePart(skeletonDataAssetName) + COMMON_SUFFIX);
    }
    public static string getAnimationFileName(string skeletonDataAssetName, string animationName)
    {
        return getAnimationAssetName(skeletonDataAssetName, animationName) + FILE_SUFFIX;
    }
    public static string getAnimationAssetName(string skeletonDataAssetName, string animationName)
    {
        return validateGeneratedName(sanitizeFileNamePart(skeletonDataAssetName) + "_" + sanitizeAnimationName(animationName));
    }
    public static string getAnimationDirectoryName(string sourceSkeletonName)
    {
        if (string.IsNullOrEmpty(sourceSkeletonName))
        {
            throw new ArgumentException("源Skeleton名称为空", nameof(sourceSkeletonName));
        }
        return sanitizeFileNamePart(sourceSkeletonName) + ANIMATION_DIRECTORY_SUFFIX;
    }
    public static string getCommonRelativePath(string sourceSkeletonName, string skeletonDataAssetName)
    {
        return combineAssetPath(getAnimationDirectoryName(sourceSkeletonName), getCommonFileName(skeletonDataAssetName));
    }
    public static string getAnimationRelativePath(string sourceSkeletonName, string skeletonDataAssetName, string animationName)
    {
        return combineAssetPath(getAnimationDirectoryName(sourceSkeletonName), getAnimationFileName(skeletonDataAssetName, animationName));
    }
    public static string sanitizeAnimationName(string animationName)
    {
        if (string.IsNullOrEmpty(animationName))
        {
            throw new ArgumentException("动画名称为空", nameof(animationName));
        }
        return sanitizeFileNamePart(animationName);
    }
    public static string combineAssetPath(string directory, string fileName)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return fileName;
        }
        return directory.TrimEnd('/', '\\') + "/" + fileName;
    }
    private static string sanitizeFileNamePart(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("文件名称为空", nameof(value));
        }
        StringBuilder builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; ++i)
        {
            char character = value[i];
            builder.Append(isInvalidFileNameCharacter(character) ? '_' : character);
        }
        for (int i = builder.Length - 1; i >= 0 && (builder[i] == ' ' || builder[i] == '.'); --i)
        {
            builder[i] = '_';
        }
        string result = builder.ToString();
        if (result.Length == 0)
        {
            result = "_";
        }
        if (isWindowsReservedName(result))
        {
            result = "_" + result;
        }
        return result;
    }
    private static string validateGeneratedName(string value)
    {
        if (value.Length + FILE_SUFFIX.Length > MAX_FILE_NAME_LENGTH)
        {
            throw new ArgumentException("生成的Spine动画资源文件名过长:" + value);
        }
        return value;
    }
    private static bool isInvalidFileNameCharacter(char character)
    {
        if (character < 32)
        {
            return true;
        }
        switch (character)
        {
            case '/':
            case '\\':
            case ':':
            case '*':
            case '?':
            case '"':
            case '<':
            case '>':
            case '|':
                return true;
        }
        return false;
    }
    private static bool isWindowsReservedName(string value)
    {
        for (int i = 0; i < WINDOWS_RESERVED_NAMES.Length; ++i)
        {
            if (string.Equals(value, WINDOWS_RESERVED_NAMES[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
