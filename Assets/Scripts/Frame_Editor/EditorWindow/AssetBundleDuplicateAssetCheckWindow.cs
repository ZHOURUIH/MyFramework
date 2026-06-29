using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FrameDefine;
using static EditorCommonUtility;

// 可以检查是否存在同一个资源文件被打包到多个AssetBundle中的情况
public class AssetBundleDuplicateAssetCheckWindow : GameEditorWindow
{
    private class BundleFileInfo
    {
        public string mBundlePath;
        public string mBundleRelativePath;
        public List<string> mAssetNames = new();
        public List<string> mScenePaths = new();
    }
    private class DuplicateBuiltAssetInfo
    {
        public string mAssetName;
        public bool mIsScene;
        public List<string> mBundleRelativePaths = new();
    }
    private readonly List<BundleFileInfo> mBundleFileList = new();
    private readonly List<DuplicateBuiltAssetInfo> mDuplicateAssetList = new();
    private readonly List<string> mCandidateFileList = new();
    private Dictionary<string, HashSet<string>> mAssetBundleSetMap;
    private Dictionary<string, string> mDisplayNameMap;
    private Dictionary<string, bool> mSceneFlagMap;
    private Vector2 mScrollPos;
    private string mBundleRootFolder = "";
    private string mSearchText = "";
    private string mCurrentScanFile = "";
    private bool mHasAnalyzed;
    private bool mIgnoreCase = true;
    private bool mIncludeSceneBundles = true;
    private bool mIsScanning;
    private int mInvalidFileCount;
    private int mManifestBundleCount;
    private int mEmptyBundleCount;
    private int mScanIndex;
    private double mLastRepaintTime;
    private const int MAX_SHOW_BUNDLE_COUNT = 30;
    private const int UNLOAD_UNUSED_INTERVAL = 10;
    private const float TOOLBAR_HEIGHT = 98.0f;
    private const float SUMMARY_HEIGHT = 104.0f;
    private const float PADDING = 6.0f;
    private void OnDisable()
    {
        StopScan(false);
    }
    protected override void onGUI()
    {
        Rect resultRect = new(0.0f, TOOLBAR_HEIGHT + SUMMARY_HEIGHT, position.width, position.height - TOOLBAR_HEIGHT - SUMMARY_HEIGHT);
        DrawToolbar();
        DrawSummary();
        DrawResultList(resultRect);
    }
    public void setBundleRootFolder(string folder) { mBundleRootFolder = folder; }
    private void DrawToolbar()
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                labelWidth("AB目录", 46);
                if (textField(ref mBundleRootFolder, GUILayout.MinWidth(260.0f)))
                {
                    Repaint();
                }
                if (button("选择目录", 80))
                {
                    string selectedFolder = EditorUtility.OpenFolderPanel("选择已构建AssetBundle目录", string.IsNullOrEmpty(mBundleRootFolder) ? Application.dataPath : mBundleRootFolder, "");
                    if (!selectedFolder.isEmpty())
                    {
                        mBundleRootFolder = selectedFolder;
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
                if (!mIsScanning)
                {
                    if (button("开始检测", 80))
                    {
                        StartScan();
                    }
                }
                else
                {
                    if (button("停止", 80))
                    {
                        StopScan(true);
                    }
                }
                if (button("导出TXT", 80))
                {
                    ExportTxt();
                }
            }
            space(5);
            using (new GUILayout.HorizontalScope())
            {
                labelWidth("搜索", 46);
                if (textField(ref mSearchText, 300))
                {
                    Repaint();
                }
                if (button("清空", 55))
                {
                    mSearchText = "";
                    GUI.FocusControl(null);
                    Repaint();
                }
                space(12);
                if (toggle(ref mIgnoreCase, "路径忽略大小写"))
                {
                    Repaint();
                }
                if (toggle(ref mIncludeSceneBundles, "检测Scene路径"))
                {
                    Repaint();
                }
            }
        }
    }
    private void DrawSummary()
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.HorizontalScope())
            {
                labelWidth("已分析: " + (mHasAnalyzed ? "是" : "否"), 80);
                labelWidth("有效AB: " + mBundleFileList.Count, 90);
                labelWidth("Manifest包: " + mManifestBundleCount, 100);
                labelWidth("无资源AB: " + mEmptyBundleCount, 90);
                labelWidth("无效文件: " + mInvalidFileCount, 90);
                labelWidth("重复资源: " + mDuplicateAssetList.Count, 110);
                labelWidth("当前显示: " + GetFilteredDuplicateAssetList().Count, 110);
            }
            if (mIsScanning)
            {
                float progress = mCandidateFileList.Count > 0 ? (float)mScanIndex / mCandidateFileList.Count : 0.0f;
                Rect progressRect = GUILayoutUtility.GetRect(10.0f, 18.0f, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(progressRect, progress, "正在检测: " + mScanIndex + "/" + mCandidateFileList.Count);
                label("当前文件: " + mCurrentScanFile, EditorStyles.miniLabel);
                label("如果卡在某个AB，当前文件这一行就是正在读取的AB。", EditorStyles.miniLabel);
            }
            else if (!mHasAnalyzed)
            {
                EditorGUILayout.HelpBox("请选择AssetBundle的输出目录，然后点击“开始检测”。这个工具不会重新打包。", MessageType.Info);
            }
            else if (mDuplicateAssetList.Count == 0)
            {
                EditorGUILayout.HelpBox("未发现同名资源路径出现在多个已构建AB文件中。", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("发现重复资源: " + mDuplicateAssetList.Count + " 个。", MessageType.Error);
            }
        }
    }
    private void DrawResultList(Rect rect)
    {
        GUILayout.BeginArea(new Rect(rect.x + PADDING, rect.y + PADDING, rect.width - PADDING * 2.0f, rect.height - PADDING * 2.0f));
        mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);

        if (!mHasAnalyzed)
        {
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            return;
        }

        List<DuplicateBuiltAssetInfo> filteredList = GetFilteredDuplicateAssetList();
        if (filteredList.Count == 0)
        {
            label("没有匹配的重复资源。", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            return;
        }

        for (int i = 0; i < filteredList.Count; ++i)
        {
            DuplicateBuiltAssetInfo info = filteredList[i];
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    string typeText = info.mIsScene ? "Scene" : "Asset";
                    label((i + 1) + ". [" + typeText + "][" + info.mBundleRelativePaths.Count + "个AB] " + info.mAssetName, EditorStyles.boldLabel);

                    if (!info.mIsScene && info.mAssetName.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        if (GUILayout.Button("定位资源", GUILayout.Width(70.0f)))
                        {
                            pingAsset(info.mAssetName);
                        }
                    }
                }
                int showCount = Mathf.Min(info.mBundleRelativePaths.Count, MAX_SHOW_BUNDLE_COUNT);
                for (int j = 0; j < showCount; ++j)
                {
                    label("    " + info.mBundleRelativePaths[j], EditorStyles.miniLabel);
                }
                if (info.mBundleRelativePaths.Count > showCount)
                {
                    label("    ... 还有 " + (info.mBundleRelativePaths.Count - showCount) + " 个AB未显示，可导出TXT查看完整列表。", EditorStyles.miniLabel);
                }
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    private void StartScan()
    {
        if (mBundleRootFolder.isEmpty() || !Directory.Exists(mBundleRootFolder))
        {
            EditorUtility.DisplayDialog("提示", "请选择有效的已构建AssetBundle目录。", "OK");
            return;
        }

        StopScan(false);

        mBundleFileList.Clear();
        mDuplicateAssetList.Clear();
        mCandidateFileList.Clear();
        mInvalidFileCount = 0;
        mManifestBundleCount = 0;
        mEmptyBundleCount = 0;
        mScanIndex = 0;
        mCurrentScanFile = "";
        mHasAnalyzed = false;

        mAssetBundleSetMap = new(GetPathComparer());
        mDisplayNameMap = new(GetPathComparer());
        mSceneFlagMap = new(GetPathComparer());

        foreach (string file in Directory.GetFiles(mBundleRootFolder, "*" + ASSET_BUNDLE_SUFFIX, SearchOption.AllDirectories))
        {
            mCandidateFileList.addIf(file, !ShouldSkipFileByExtension(file));
        }
        mCandidateFileList.Sort(StringComparer.OrdinalIgnoreCase);
        if (mCandidateFileList.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "目录中没有可检测的AB文件。", "OK");
            return;
        }

        mIsScanning = true;
        EditorApplication.update += ScanUpdate;
        Repaint();
    }
    private void StopScan(bool showDialog)
    {
        if (mIsScanning)
        {
            EditorApplication.update -= ScanUpdate;
            mIsScanning = false;
            mCurrentScanFile = "";
            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();
            if (showDialog)
            {
                EditorUtility.DisplayDialog("提示", "已停止检测。", "OK");
            }
            Repaint();
        }
    }
    private void ScanUpdate()
    {
        if (!mIsScanning)
        {
            return;
        }
        if (mScanIndex >= mCandidateFileList.Count)
        {
            FinishScan();
            return;
        }

        string filePath = mCandidateFileList[mScanIndex];
        mCurrentScanFile = GetRelativePath(mBundleRootFolder, filePath);
        ReadSingleBundleFile(filePath, mCurrentScanFile);
        ++mScanIndex;
        if (mScanIndex % UNLOAD_UNUSED_INTERVAL == 0)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();
        }
        double time = EditorApplication.timeSinceStartup;
        if (time - mLastRepaintTime > 0.1f)
        {
            mLastRepaintTime = time;
            Repaint();
        }

        if (mScanIndex >= mCandidateFileList.Count)
        {
            FinishScan();
        }
    }
    private void FinishScan()
    {
        EditorApplication.update -= ScanUpdate;
        mIsScanning = false;
        mCurrentScanFile = "";

        BuildDuplicateListFromMap();

        EditorUtility.UnloadUnusedAssetsImmediate();
        GC.Collect();

        mHasAnalyzed = true;

        Debug.Log("[已构建AB重复资源检测] 检测完成, 有效AB: " + mBundleFileList.Count +
            ", Manifest包: " + mManifestBundleCount +
            ", 无资源AB: " + mEmptyBundleCount +
            ", 无效文件: " + mInvalidFileCount +
            ", 重复资源: " + mDuplicateAssetList.Count);

        Repaint();
    }
    private void ReadSingleBundleFile(string filePath, string relativePath)
    {
        AssetBundle bundle = null;
        try
        {
            bundle = AssetBundle.LoadFromFile(filePath);
            if (bundle == null)
            {
                ++mInvalidFileCount;
                return;
            }

            BundleFileInfo bundleInfo = new();
            bundleInfo.mBundlePath = filePath;
            bundleInfo.mBundleRelativePath = relativePath;
            bundleInfo.mAssetNames.addRange(bundle.GetAllAssetNames());
            if (mIncludeSceneBundles)
            {
                bundleInfo.mScenePaths.addRange(bundle.GetAllScenePaths());
            }

            if (IsManifestBundleByAssetNames(bundleInfo.mAssetNames, bundleInfo.mScenePaths))
            {
                ++mManifestBundleCount;
                return;
            }
            if (bundleInfo.mAssetNames.Count == 0 && bundleInfo.mScenePaths.Count == 0)
            {
                ++mEmptyBundleCount;
            }
            AddNamesToMap(bundleInfo.mAssetNames, false, relativePath, mAssetBundleSetMap, mDisplayNameMap, mSceneFlagMap);
            if (mIncludeSceneBundles)
            {
                AddNamesToMap(bundleInfo.mScenePaths, true, relativePath, mAssetBundleSetMap, mDisplayNameMap, mSceneFlagMap);
            }

            bundleInfo.mAssetNames.Sort(StringComparer.OrdinalIgnoreCase);
            bundleInfo.mScenePaths.Sort(StringComparer.OrdinalIgnoreCase);
            mBundleFileList.Add(bundleInfo);
        }
        catch (Exception e)
        {
            ++mInvalidFileCount;
            Debug.LogWarning("[已构建AB重复资源检测] 读取失败: " + filePath + "\n" + e.Message);
        }
        finally
        {
            if (bundle != null)
            {
                bundle.Unload(false);
            }
        }
    }
    private void BuildDuplicateListFromMap()
    {
        mDuplicateAssetList.Clear();
        foreach (var pair in mAssetBundleSetMap)
        {
            if (pair.Value.Count <= 1)
            {
                continue;
            }

            DuplicateBuiltAssetInfo info = new();
            info.mAssetName = mDisplayNameMap.TryGetValue(pair.Key, out string displayName) ? displayName : pair.Key;
            info.mIsScene = mSceneFlagMap.TryGetValue(pair.Key, out bool isScene) && isScene;
            info.mBundleRelativePaths.AddRange(pair.Value.OrderBy(x => x));
            mDuplicateAssetList.Add(info);
        }

        mDuplicateAssetList.Sort((a, b) =>
        {
            int countCompare = b.mBundleRelativePaths.Count.CompareTo(a.mBundleRelativePaths.Count);
            if (countCompare != 0)
            {
                return countCompare;
            }
            return string.CompareOrdinal(a.mAssetName, b.mAssetName);
        });
    }
    private void AddNamesToMap(List<string> assetNames, bool isScene, string bundleRelativePath, Dictionary<string, HashSet<string>> assetBundleSetMap,
                                Dictionary<string, string> displayNameMap, Dictionary<string, bool> sceneFlagMap)
    {
        HashSet<string> uniqueNameSet = new(GetPathComparer());
        foreach (string assetName in assetNames)
        {
            if (assetName.isEmpty())
            {
                continue;
            }
            string normalizedName = NormalizeAssetName(assetName);
            if (normalizedName.isEmpty() || !uniqueNameSet.Add(normalizedName))
            {
                continue;
            }
            assetBundleSetMap.getOrAddNew(normalizedName).Add(bundleRelativePath);
            displayNameMap.TryAdd(normalizedName, assetName.Replace("\\", "/"));
            sceneFlagMap.TryAdd(normalizedName, isScene);
        }
    }
    private bool IsManifestBundleByAssetNames(List<string> assetNames, List<string> scenePaths)
    {
        if (!scenePaths.isEmpty() || assetNames.isEmpty() || assetNames.Count > 2)
        {
            return false;
        }
        for (int i = 0; i < assetNames.Count; ++i)
        {
            string name = assetNames[i];
            if (name.isEmpty())
            {
                continue;
            }
            if (name.IndexOf("assetbundlemanifest", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
        return false;
    }
    private bool ShouldSkipFileByExtension(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(fileName))
        {
            return true;
        }
        if (fileName.endWith(".manifest", false) ||
            fileName.endWith(".meta", false) ||
            fileName.endWith(".txt", false) ||
            fileName.endWith(".json", false) ||
            fileName.endWith(".hash", false) ||
            fileName.endWith(".crc", false) ||
            fileName.endWith(".log", false))
        {
            return true;
        }
        return false;
    }
    private StringComparer GetPathComparer()
    {
        return mIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }
    private string NormalizeAssetName(string assetName)
    {
        string name = assetName.Replace("\\", "/").Trim();
        if (mIgnoreCase)
        {
            name = name.ToLowerInvariant();
        }
        return name;
    }
    private string GetRelativePath(string rootFolder, string filePath)
    {
        string root = rootFolder.Replace("\\", "/").TrimEnd('/');
        string path = filePath.Replace("\\", "/");
        if (path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
        {
            return path[(root.Length + 1)..];
        }

        return Path.GetFileName(filePath);
    }
    private List<DuplicateBuiltAssetInfo> GetFilteredDuplicateAssetList()
    {
        if (string.IsNullOrEmpty(mSearchText))
        {
            return mDuplicateAssetList;
        }

        List<DuplicateBuiltAssetInfo> result = new();
        foreach (DuplicateBuiltAssetInfo info in mDuplicateAssetList)
        {
            if (info.mAssetName != null && info.mAssetName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Add(info);
                continue;
            }
            foreach (string bundlePath in info.mBundleRelativePaths)
            {
                if (bundlePath != null && bundlePath.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(info);
                    break;
                }
            }
        }
        return result;
    }
    private void ExportTxt()
    {
        if (!mHasAnalyzed)
        {
            EditorUtility.DisplayDialog("提示", "请先点击“开始检测”。", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("导出已构建AB重复资源检测结果", Application.dataPath, "BuiltAssetBundleDuplicateAssets.txt", "txt");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        StringBuilder sb = new();

        sb.AppendLine("============================================================");
        sb.AppendLine("Built AssetBundle Duplicate Asset Check");
        sb.AppendLine("Bundle Root Folder: " + mBundleRootFolder);
        sb.AppendLine("Valid Bundle Count: " + mBundleFileList.Count);
        sb.AppendLine("Manifest Bundle Count: " + mManifestBundleCount);
        sb.AppendLine("Empty Bundle Count: " + mEmptyBundleCount);
        sb.AppendLine("Invalid File Count: " + mInvalidFileCount);
        sb.AppendLine("Duplicate Asset Count: " + mDuplicateAssetList.Count);
        sb.AppendLine();

        if (mDuplicateAssetList.Count == 0)
        {
            sb.AppendLine("<None>");
        }
        else
        {
            foreach (DuplicateBuiltAssetInfo info in mDuplicateAssetList)
            {
                sb.AppendLine("============================================================");
                sb.AppendLine("Asset: " + info.mAssetName);
                sb.AppendLine("Type: " + (info.mIsScene ? "Scene" : "Asset"));
                sb.AppendLine("Bundle Count: " + info.mBundleRelativePaths.Count);
                sb.AppendLine("[Bundles]");
                foreach (string bundlePath in info.mBundleRelativePaths)
                {
                    sb.AppendLine("    " + bundlePath);
                }
                sb.AppendLine();
            }
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();

        Debug.Log("[已构建AB重复资源检测] TXT导出完成: " + path);
        EditorUtility.DisplayDialog("完成", "TXT导出完成:\n" + path, "OK");
    }
}
