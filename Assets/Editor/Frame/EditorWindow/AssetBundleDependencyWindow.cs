using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 可以查看AssetBundle资源依赖关系的窗口
public class AssetBundleDependencyWindow : EditorWindow
{
    private class BundleInfo
    {
        public string mBundleName;
        public int mAssetCount;
        public List<string> mDirectDependencies = new();
        public List<string> mAllDependencies = new();
        public List<string> mDirectReferencedBy = new();
        public List<string> mAllReferencedBy = new();
        public List<string> mAssets = new();
    }

    private class GraphNode
    {
        public string mBundleName;
        public Rect mRect;
    }

    private readonly Dictionary<string, BundleInfo> mBundleMap = new();
    private readonly Dictionary<string, GraphNode> mNodeMap = new();
    private readonly HashSet<string> mEdges = new();
    private readonly HashSet<string> mSelectedBundleSet = new();
    private Vector2 mPanOffset = new(80.0f, 140.0f);
    private bool mDraggingCanvas;
    private Vector2 mLastMousePos;
    private string mSearchText = "";
    private string mSelectedBundle = "";
    private string mDraggingNodeName = "";
    private bool mHideIndependentBundle;
    private Vector2 mLastNodeMouseScreenPos;
    private bool mSelectingNodes;
    private bool mAppendSelectingNodes;
    private Vector2 mSelectStartLocalPos;
    private Vector2 mSelectEndLocalPos;
    private float mZoom = 1.0f;
    private GUIStyle mNodeStyle;
    private GUIStyle mSelectedNodeStyle;
    private GUIStyle mNodeNameStyle;
    private GUIStyle mSmallLabelStyle;
    private const int MAX_NODE_COUNT_PER_COLUMN = 20;
    private const float SAME_FOLDER_COLUMN_SPACE = 300.0f;
    private const float DIFFERENT_FOLDER_COLUMN_SPACE = 420.0f;
    private const float TOOLBAR_HEIGHT = 112.0f;
    private const float NODE_WIDTH = 280.0f;
    private const float NODE_HEIGHT = 120.0f;
    private const float HORIZONTAL_SPACE = 360.0f;
    private const float VERTICAL_SPACE = 155.0f;
    private const float MIN_ZOOM = 0.1f;
    private const float MAX_ZOOM = 2.0f;
    private const float DETAIL_HIDE_ZOOM = 0.55f;
    private const float TITLE_ONLY_ZOOM = 0.35f;

    [MenuItem("AssetBundle/AB依赖分析窗口")]
    public static void Open()
    {
        var window = GetWindow<AssetBundleDependencyWindow>();
        window.titleContent = new GUIContent("AB依赖分析");
        window.minSize = new Vector2(1100.0f, 720.0f);
        window.Show();
    }
    public static void OpenFromOtherWindow()
    {
        Open();
    }
    private void OnEnable()
    {
        InitStyles();
    }
    private void InitStyles()
    {
        mNodeStyle = new GUIStyle();
        mNodeStyle.padding = new RectOffset(0, 0, 0, 0);

        mSelectedNodeStyle = new GUIStyle();
        mSelectedNodeStyle.padding = new RectOffset(0, 0, 0, 0);

        mNodeNameStyle = new GUIStyle(EditorStyles.boldLabel);
        mNodeNameStyle.fontSize = 11;
        mNodeNameStyle.wordWrap = true;
        mNodeNameStyle.clipping = TextClipping.Clip;
        mNodeNameStyle.normal.textColor = new Color(0.92f, 0.92f, 0.92f);

        mSmallLabelStyle = new GUIStyle(EditorStyles.miniLabel);
        mSmallLabelStyle.wordWrap = true;
        mSmallLabelStyle.normal.textColor = new Color(0.82f, 0.82f, 0.82f);
    }
    private void OnGUI()
    {
        if (mNodeStyle == null)
        {
            InitStyles();
        }

        Rect toolbarRect = new Rect(0.0f, 0.0f, position.width, TOOLBAR_HEIGHT);
        DrawToolbar(toolbarRect);

        Rect graphRect = new Rect(0.0f, TOOLBAR_HEIGHT, position.width, position.height - TOOLBAR_HEIGHT);
        DrawGraphArea(graphRect);
        HandleCanvasInput(graphRect);
    }
    private void DrawToolbar(Rect toolbarRect)
    {
        GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("分析所有AB依赖", EditorStyles.toolbarButton, GUILayout.Width(130.0f)))
        {
            AnalyzeAllAssetBundles();
        }
        if (GUILayout.Button("导出TXT", EditorStyles.toolbarButton, GUILayout.Width(80.0f)))
        {
            ExportTxt();
        }
        if (GUILayout.Button("自动排版", EditorStyles.toolbarButton, GUILayout.Width(80.0f)))
        {
            AutoLayout();
        }
        if (GUILayout.Button("适配全图", EditorStyles.toolbarButton, GUILayout.Width(80.0f)))
        {
            FitAllNodesToView();
        }
        if (GUILayout.Button("居中", EditorStyles.toolbarButton, GUILayout.Width(60.0f)))
        {
            mZoom = 1.0f;
            mPanOffset = new Vector2(80.0f, 140.0f);
            Repaint();
        }
        GUILayout.Space(16.0f);

        EditorGUILayout.LabelField("搜索", GUILayout.Width(32.0f));
        string newSearchText = EditorGUILayout.TextField(mSearchText, GUILayout.Width(240.0f));
        if (newSearchText != mSearchText)
        {
            mSearchText = newSearchText;
            Repaint();
        }

        if (GUILayout.Button("查找", EditorStyles.toolbarButton, GUILayout.Width(60.0f)))
        {
            SelectFirstMatchedNode();
        }

        GUILayout.Space(16.0f);
        bool newHideIndependentBundle = GUILayout.Toggle(mHideIndependentBundle, "隐藏独立AB", EditorStyles.toolbarButton, GUILayout.Width(90.0f));
        if (newHideIndependentBundle != mHideIndependentBundle)
        {
            mHideIndependentBundle = newHideIndependentBundle;
            ClearHiddenSelectedBundles();
            Repaint();
        }

        GUILayout.Space(16.0f);
        EditorGUILayout.LabelField("缩放:" + (mZoom * 100.0f).ToString("F0") + "%", GUILayout.Width(80.0f));

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            "打开窗口不会自动分析。点击“分析所有AB依赖”后，才会读取当前工程 AssetBundleName 并生成节点图。点击“导出TXT”后，才会生成依赖文本。",
            EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            "Bundle数量: " + mBundleMap.Count +
            "    节点: " + mNodeMap.Count +
            "    依赖连线: " + mEdges.Count +
            "    当前选中: " + (string.IsNullOrEmpty(mSelectedBundle) ? "<None>" : mSelectedBundle) +
            "    多选数量: " + mSelectedBundleSet.Count,
            EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }
    private void AnalyzeAllAssetBundles()
    {
        mBundleMap.Clear();
        mNodeMap.Clear();
        mEdges.Clear();
        mSelectedBundle = "";
        mSelectedBundleSet.Clear();
        mDraggingNodeName = "";
        mSelectingNodes = false;

        try
        {
            EditorUtility.DisplayProgressBar("AB依赖分析", "正在获取所有 AssetBundleName...", 0.0f);
            string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
            if (allBundleNames == null || allBundleNames.Length == 0)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "当前工程没有设置任何 AssetBundleName。", "OK");
                return;
            }

            int bundleCount = allBundleNames.Length;
            for (int i = 0; i < bundleCount; ++i)
            {
                string bundleName = allBundleNames[i];
                float progress = bundleCount > 0 ? (float)i / bundleCount : 1.0f;
                // 不需要每个都刷新也可以,这里简单处理,如果数量特别多可以改成每隔几个刷新一次
                EditorUtility.DisplayProgressBar("AB依赖分析", $"正在分析: {bundleName}\n {i + 1}/{bundleCount}", progress);

                BundleInfo info = new();
                info.mBundleName = bundleName;
                string[] assets = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                if (assets != null)
                {
                    info.mAssets.AddRange(assets);
                    info.mAssetCount = assets.Length;
                }

                string[] directDeps = AssetDatabase.GetAssetBundleDependencies(bundleName, false);
                if (directDeps != null)
                {
                    info.mDirectDependencies.AddRange(directDeps);
                }

                string[] allDeps = AssetDatabase.GetAssetBundleDependencies(bundleName, true);
                if (allDeps != null)
                {
                    info.mAllDependencies.AddRange(allDeps);
                }
                mBundleMap[bundleName] = info;
            }

            EditorUtility.DisplayProgressBar("AB依赖分析", "正在计算反向依赖关系...", 0.92f);
            BuildReferencedBy();
            EditorUtility.DisplayProgressBar("AB依赖分析", "正在生成依赖节点和连线...", 0.96f);
            BuildAllNodesAndEdges();
            EditorUtility.DisplayProgressBar("AB依赖分析", "正在自动排版...", 0.98f);
            AutoLayout();
            Debug.Log("[AB依赖分析] 分析完成, Bundle数量: " + mBundleMap.Count + ", 依赖连线: " + mEdges.Count);
            Repaint();
        }
        catch (Exception e)
        {
            Debug.LogError("[AB依赖分析] 分析失败: " + e);
            EditorUtility.DisplayDialog("错误", "AB依赖分析失败:\n" + e.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    private void BuildReferencedBy()
    {
        foreach (BundleInfo info in mBundleMap.Values)
        {
            info.mDirectReferencedBy.Clear();
            info.mAllReferencedBy.Clear();
        }

        foreach (BundleInfo info in mBundleMap.Values)
        {
            foreach (string dep in info.mDirectDependencies)
            {
                if (mBundleMap.TryGetValue(dep, out BundleInfo depInfo) && !depInfo.mDirectReferencedBy.Contains(info.mBundleName))
                {
                    depInfo.mDirectReferencedBy.Add(info.mBundleName);
                }
            }

            foreach (string dep in info.mAllDependencies)
            {
                if (mBundleMap.TryGetValue(dep, out BundleInfo depInfo) && !depInfo.mAllReferencedBy.Contains(info.mBundleName))
                {
                    depInfo.mAllReferencedBy.Add(info.mBundleName);
                }
            }
        }

        foreach (BundleInfo info in mBundleMap.Values)
        {
            info.mDirectReferencedBy.Sort();
            info.mAllReferencedBy.Sort();
            info.mDirectDependencies.Sort();
            info.mAllDependencies.Sort();
            info.mAssets.Sort();
        }
    }
    private void BuildAllNodesAndEdges()
    {
        mNodeMap.Clear();
        mEdges.Clear();

        foreach (string bundleName in mBundleMap.Keys)
        {
            AddNode(bundleName);
        }

        foreach (BundleInfo info in mBundleMap.Values)
        {
            foreach (string dep in info.mDirectDependencies)
            {
                if (!mBundleMap.ContainsKey(dep))
                {
                    continue;
                }
                AddNode(dep);
                AddEdge(info.mBundleName, dep);
            }
        }
    }
    private void AddNode(string bundleName)
    {
        if (mNodeMap.ContainsKey(bundleName))
        {
            return;
        }

        GraphNode node = new GraphNode();
        node.mBundleName = bundleName;
        node.mRect = new Rect(0.0f, 0.0f, NODE_WIDTH, NODE_HEIGHT);
        mNodeMap.Add(bundleName, node);
    }
    private void AddEdge(string from, string to)
    {
        mEdges.Add(from + "|" + to);
    }
    private void AutoLayout()
    {
        if (mNodeMap.Count == 0)
        {
            return;
        }

        Dictionary<string, string> bundleFolderMap = new();
        foreach (GraphNode node in mNodeMap.Values)
        {
            bundleFolderMap[node.mBundleName] = GetBundleFolder(node.mBundleName);
        }

        NormalizeBundleFolderMap(bundleFolderMap);

        Dictionary<string, List<GraphNode>> folderNodeMap = new();
        foreach (GraphNode node in mNodeMap.Values)
        {
            string folder = bundleFolderMap.ContainsKey(node.mBundleName) ? bundleFolderMap[node.mBundleName] : "<Unknown>";
            if (!folderNodeMap.TryGetValue(folder, out var nodeList))
            {
                nodeList = new List<GraphNode>();
                folderNodeMap.Add(folder, nodeList);
            }
            nodeList.Add(node);
        }

        List<KeyValuePair<string, List<GraphNode>>> folderList = folderNodeMap.OrderBy(x => x.Key).ToList();
        float curX = 0.0f;
        for (int folderIndex = 0; folderIndex < folderList.Count; ++folderIndex)
        {
            var folderPair = folderList[folderIndex];
            List<GraphNode> nodes = folderPair.Value
                .OrderByDescending(x =>
                {
                    if (mBundleMap.TryGetValue(x.mBundleName, out BundleInfo info))
                    {
                        return info.mDirectDependencies.Count + info.mDirectReferencedBy.Count;
                    }
                    return 0;
                })
                .ThenBy(x => x.mBundleName)
                .ToList();

            int columnCount = Mathf.CeilToInt((float)nodes.Count / MAX_NODE_COUNT_PER_COLUMN);
            columnCount = Mathf.Max(1, columnCount);
            for (int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
            {
                int columnIndex = nodeIndex / MAX_NODE_COUNT_PER_COLUMN;
                int rowIndex = nodeIndex % MAX_NODE_COUNT_PER_COLUMN;
                float x = curX + columnIndex * SAME_FOLDER_COLUMN_SPACE;
                float y = rowIndex * VERTICAL_SPACE;
                nodes[nodeIndex].mRect.position = new Vector2(x, y);
            }

            curX += (columnCount - 1) * SAME_FOLDER_COLUMN_SPACE + DIFFERENT_FOLDER_COLUMN_SPACE;
        }

        Repaint();
    }
    private void NormalizeBundleFolderMap(Dictionary<string, string> bundleFolderMap)
    {
        if (bundleFolderMap.Count == 0)
        {
            return;
        }

        for (int i = 0; i < 5; ++i)
        {
            Dictionary<string, int> folderCountMap = new Dictionary<string, int>();
            foreach (string folder in bundleFolderMap.Values)
            {
                if (!folderCountMap.ContainsKey(folder))
                {
                    folderCountMap.Add(folder, 0);
                }
                ++folderCountMap[folder];
            }

            int singleFolderCount = 0;
            foreach (int count in folderCountMap.Values)
            {
                if (count == 1)
                {
                    ++singleFolderCount;
                }
            }

            if (singleFolderCount < folderCountMap.Count * 0.7f)
            {
                break;
            }

            List<string> bundleList = bundleFolderMap.Keys.ToList();
            bool changed = false;
            foreach (string bundleName in bundleList)
            {
                string parentFolder = GetParentFolder(bundleFolderMap[bundleName]);
                if (!string.IsNullOrEmpty(parentFolder) && parentFolder != bundleFolderMap[bundleName])
                {
                    bundleFolderMap[bundleName] = parentFolder;
                    changed = true;
                }
            }

            if (!changed)
            {
                break;
            }
        }
    }
    private string GetBundleFolder(string bundleName)
    {
        if (!mBundleMap.TryGetValue(bundleName, out BundleInfo info))
        {
            return "<Unknown>";
        }
        if (info.mAssets.Count == 0)
        {
            return "<Empty>";
        }

        List<string> folderList = new List<string>();
        foreach (string assetPath in info.mAssets)
        {
            string folder = GetAssetFolder(assetPath);
            if (!string.IsNullOrEmpty(folder) && !folderList.Contains(folder))
            {
                folderList.Add(folder);
            }
        }

        if (folderList.Count == 0)
        {
            return "<Unknown>";
        }
        if (folderList.Count == 1)
        {
            return folderList[0];
        }

        return GetCommonFolder(folderList);
    }
    private string GetAssetFolder(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return "";
        }

        string path = assetPath.Replace("\\", "/");
        int index = path.LastIndexOf('/');
        if (index < 0)
        {
            return "";
        }

        return path.Substring(0, index);
    }
    private string GetParentFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return "";
        }
        if (folder == "<Unknown>" || folder == "<Empty>" || folder == "<Root>")
        {
            return folder;
        }

        string path = folder.Replace("\\", "/");
        int index = path.LastIndexOf('/');
        if (index < 0)
        {
            return path;
        }

        return path[..index];
    }
    private string GetCommonFolder(List<string> folderList)
    {
        if (folderList == null || folderList.Count == 0)
        {
            return "<Unknown>";
        }

        string[] commonParts = folderList[0].Split('/');
        int commonLength = commonParts.Length;
        for (int i = 1; i < folderList.Count; ++i)
        {
            string[] parts = folderList[i].Split('/');
            commonLength = Mathf.Min(commonLength, parts.Length);
            for (int j = 0; j < commonLength; ++j)
            {
                if (commonParts[j] != parts[j])
                {
                    commonLength = j;
                    break;
                }
            }
        }

        if (commonLength <= 0)
        {
            return "<Root>";
        }

        StringBuilder sb = new();
        for (int i = 0; i < commonLength; ++i)
        {
            if (i > 0)
            {
                sb.Append("/");
            }
            sb.Append(commonParts[i]);
        }
        return sb.ToString();
    }
    private Vector2 GraphToScreen(Vector2 graphPos)
    {
        return graphPos * mZoom + mPanOffset;
    }
    private Vector2 ScreenToGraph(Vector2 screenPos)
    {
        return (screenPos - mPanOffset) / mZoom;
    }
    private Rect GraphToScreenRect(Rect graphRect)
    {
        return new Rect(GraphToScreen(graphRect.position), graphRect.size * mZoom);
    }
    private void DrawGraphArea(Rect graphRect)
    {
        GUI.Box(graphRect, GUIContent.none);
        GUI.BeginGroup(graphRect);

        Rect localGraphRect = new(0.0f, 0.0f, graphRect.width, graphRect.height);
        if (mBundleMap.Count == 0)
        {
            GUI.Label(new Rect(20.0f, 20.0f, localGraphRect.width - 40.0f, 40.0f), "尚未分析。点击上方“分析所有AB依赖”开始。");
            GUI.EndGroup();
            return;
        }

        Handles.BeginGUI();
        DrawGrid(localGraphRect, 20.0f, new Color(0, 0, 0, 0.10f));
        DrawGrid(localGraphRect, 100.0f, new Color(0, 0, 0, 0.20f));
        DrawEdges();
        Handles.EndGUI();

        BeginWindows();
        List<GraphNode> nodes = mNodeMap.Values.ToList();
        for (int i = 0; i < nodes.Count; ++i)
        {
            GraphNode node = nodes[i];
            if (!IsNodeVisible(node.mBundleName))
            {
                continue;
            }

            Rect drawRect = GraphToScreenRect(node.mRect);
            GUIStyle style = IsBundleSelected(node.mBundleName) ? mSelectedNodeStyle : mNodeStyle;
            GUI.Window(i, drawRect, DrawNodeWindow, "", style);
        }
        EndWindows();

        DrawSelectionRect();
        DrawZoomInfo(localGraphRect);
        GUI.EndGroup();
    }
    private void DrawSelectionRect()
    {
        if (!mSelectingNodes)
        {
            return;
        }

        Rect rect = GetSelectionRect();
        Color fillColor = new(0.25f, 0.65f, 1.0f, 0.16f);
        Color borderColor = new(0.25f, 0.75f, 1.0f, 0.9f);

        EditorGUI.DrawRect(rect, fillColor);
        DrawRectBorder(rect, borderColor, 1.0f);
    }
    private Rect GetSelectionRect()
    {
        float xMin = Mathf.Min(mSelectStartLocalPos.x, mSelectEndLocalPos.x);
        float yMin = Mathf.Min(mSelectStartLocalPos.y, mSelectEndLocalPos.y);
        float xMax = Mathf.Max(mSelectStartLocalPos.x, mSelectEndLocalPos.x);
        float yMax = Mathf.Max(mSelectStartLocalPos.y, mSelectEndLocalPos.y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
    private void DrawZoomInfo(Rect graphRect)
    {
        GUI.Label(new Rect(graphRect.x + 10.0f, graphRect.yMax - 24.0f, 200.0f, 20.0f), "Zoom: " + (mZoom * 100.0f).ToString("F0") + "%", EditorStyles.miniLabel);
    }
    private bool IsIndependentBundle(string bundleName)
    {
        if (!mBundleMap.TryGetValue(bundleName, out BundleInfo info))
        {
            return false;
        }

        return info.mDirectDependencies.Count == 0 && info.mDirectReferencedBy.Count == 0;
    }
    private bool IsNodeVisible(string bundleName)
    {
        if (mHideIndependentBundle && IsIndependentBundle(bundleName))
        {
            return false;
        }

        return IsNodeVisibleBySearch(bundleName);
    }
    private void ClearHiddenSelectedBundles()
    {
        if (!mHideIndependentBundle)
        {
            return;
        }

        List<string> selectedList = mSelectedBundleSet.ToList();
        foreach (string bundleName in selectedList)
        {
            if (IsIndependentBundle(bundleName))
            {
                mSelectedBundleSet.Remove(bundleName);
            }
        }

        if (!string.IsNullOrEmpty(mSelectedBundle) && IsIndependentBundle(mSelectedBundle))
        {
            mSelectedBundle = mSelectedBundleSet.Count > 0 ? mSelectedBundleSet.First() : "";
        }
    }
    private bool IsNodeVisibleBySearch(string bundleName)
    {
        if (string.IsNullOrEmpty(mSearchText))
        {
            return true;
        }
        return bundleName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0 || bundleName == mSelectedBundle;
    }
    private bool IsBundleSelected(string bundleName)
    {
        return mSelectedBundleSet.Contains(bundleName);
    }
    private void DrawNodeWindow(int id)
    {
        List<GraphNode> nodes = mNodeMap.Values.ToList();
        if (id < 0 || id >= nodes.Count)
        {
            return;
        }

        GraphNode node = nodes[id];
        Rect nodeRect = new Rect(0.0f, 0.0f, NODE_WIDTH * mZoom, NODE_HEIGHT * mZoom);
        DrawNodeBox(nodeRect, node.mBundleName);

        GUIStyle titleStyle = new(mNodeNameStyle);
        titleStyle.fontSize = Mathf.Max(7, Mathf.RoundToInt(11.0f * mZoom));
        titleStyle.wordWrap = mZoom >= TITLE_ONLY_ZOOM;

        GUIStyle infoStyle = new(mSmallLabelStyle);
        infoStyle.fontSize = Mathf.Max(7, Mathf.RoundToInt(10.0f * mZoom));

        float padding = 10.0f * mZoom;
        float titleY = 8.0f * mZoom;
        float titleHeight = mZoom < DETAIL_HIDE_ZOOM ? NODE_HEIGHT * mZoom - titleY * 2.0f : 38.0f * mZoom;
        Rect titleRect = new(padding, titleY, NODE_WIDTH * mZoom - padding * 2.0f, titleHeight);
        GUI.Label(titleRect, node.mBundleName, titleStyle);

        if (mZoom >= DETAIL_HIDE_ZOOM && mBundleMap.TryGetValue(node.mBundleName, out BundleInfo info))
        {
            float lineHeight = 16.0f * mZoom;
            float startY = 52.0f * mZoom;
            GUI.Label(new Rect(padding, startY, NODE_WIDTH * mZoom - padding * 2.0f, lineHeight), "AssetCount: " + info.mAssetCount, infoStyle);
            GUI.Label(new Rect(padding, startY + lineHeight, NODE_WIDTH * mZoom - padding * 2.0f, lineHeight), "DirectDep: " + info.mDirectDependencies.Count, infoStyle);
            GUI.Label(new Rect(padding, startY + lineHeight * 2.0f, NODE_WIDTH * mZoom - padding * 2.0f, lineHeight), "ReferencedBy: " + info.mDirectReferencedBy.Count, infoStyle);
        }

        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            mSelectedBundle = node.mBundleName;
            if (e.control || e.command)
            {
                if (mSelectedBundleSet.Contains(node.mBundleName))
                {
                    mSelectedBundleSet.Remove(node.mBundleName);
                }
                else
                {
                    mSelectedBundleSet.Add(node.mBundleName);
                }
            }
            else if (!mSelectedBundleSet.Contains(node.mBundleName))
            {
                mSelectedBundleSet.Clear();
                mSelectedBundleSet.Add(node.mBundleName);
            }

            mDraggingNodeName = node.mBundleName;
            mLastNodeMouseScreenPos = GUIUtility.GUIToScreenPoint(e.mousePosition);
            Repaint();
            e.Use();
        }
    }
    private void DrawNodeBox(Rect nodeRect, string bundleName)
    {
        bool selected = IsBundleSelected(bundleName);
        Color backgroundColor = GetNodeBackgroundColor(bundleName);
        Color borderColor = selected ? new Color(0.35f, 0.72f, 1.0f, 1.0f) : new Color(0.22f, 0.42f, 0.58f, 1.0f);
        Color titleColor = selected ? new Color(0.18f, 0.45f, 0.70f, 1.0f) : GetNodeTitleColor(bundleName);

        EditorGUI.DrawRect(nodeRect, backgroundColor);

        if (mZoom >= DETAIL_HIDE_ZOOM)
        {
            float titleHeight = 24.0f * mZoom;
            EditorGUI.DrawRect(new Rect(nodeRect.x, nodeRect.y, nodeRect.width, titleHeight), titleColor);
        }

        float borderWidth = Mathf.Clamp(2.0f * mZoom, 1.0f, 2.0f);
        DrawRectBorder(nodeRect, borderColor, borderWidth);

        if (selected)
        {
            Rect innerRect = new Rect(nodeRect.x + 2.0f * mZoom, nodeRect.y + 2.0f * mZoom, nodeRect.width - 4.0f * mZoom, nodeRect.height - 4.0f * mZoom);
            DrawRectBorder(innerRect, new Color(0.35f, 0.72f, 1.0f, 0.75f), borderWidth);
        }
    }
    private void DrawRectBorder(Rect rect, Color color, float width)
    {
        EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, rect.width, width), color);
        EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMax - width, rect.width, width), color);
        EditorGUI.DrawRect(new Rect(rect.xMin, rect.yMin, width, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - width, rect.yMin, width, rect.height), color);
    }
    private void DrawEdges()
    {
        List<string> edgeList = mEdges.ToList();

        foreach (string edge in edgeList)
        {
            string[] parts = edge.Split('|');
            if (parts.Length != 2)
            {
                continue;
            }

            string from = parts[0];
            string to = parts[1];
            if (IsEdgeHighlighted(from, to))
            {
                continue;
            }

            DrawSingleEdge(from, to);
        }

        foreach (string edge in edgeList)
        {
            string[] parts = edge.Split('|');
            if (parts.Length != 2)
            {
                continue;
            }

            string from = parts[0];
            string to = parts[1];
            if (!IsEdgeHighlighted(from, to))
            {
                continue;
            }

            DrawSingleEdge(from, to);
        }
    }
    private void DrawSingleEdge(string from, string to)
    {
        if (!IsNodeVisible(from) || !IsNodeVisible(to))
        {
            return;
        }
        if (!mNodeMap.TryGetValue(from, out GraphNode fromNode) || !mNodeMap.TryGetValue(to, out GraphNode toNode))
        {
            return;
        }

        Rect fromRect = GraphToScreenRect(fromNode.mRect);
        Rect toRect = GraphToScreenRect(toNode.mRect);

        float portOffset = GetEdgePortOffset(from, to);
        float fromY = fromRect.center.y + portOffset;
        float toY = toRect.center.y + portOffset;

        Vector3 start = new Vector3(fromRect.xMax, fromY, 0.0f);
        Vector3 end = new Vector3(toRect.xMin, toY, 0.0f);

        bool reverseDirection = toRect.center.x < fromRect.center.x;
        if (reverseDirection)
        {
            start = new Vector3(fromRect.xMin, fromY, 0.0f);
            end = new Vector3(toRect.xMax, toY, 0.0f);
        }

        float tangentLength = Mathf.Max(30.0f, 80.0f * mZoom);
        Vector3 startTan = start + Vector3.right * tangentLength;
        Vector3 endTan = end + Vector3.left * tangentLength;

        if (reverseDirection)
        {
            startTan = start + Vector3.left * tangentLength;
            endTan = end + Vector3.right * tangentLength;
        }

        Color color = GetEdgeColor(from, to);
        float lineWidth = GetEdgeWidth(from, to);
        DrawBezierLine(start, end, startTan, endTan, color, Mathf.Clamp(lineWidth * mZoom, 0.8f, 2.6f));
        DrawArrow(end, endTan, color);
    }
    private void DrawBezierLine(Vector3 start, Vector3 end, Vector3 startTan, Vector3 endTan, Color color, float width)
    {
        const int SEGMENT_COUNT = 32;
        Vector3[] points = new Vector3[SEGMENT_COUNT + 1];

        for (int i = 0; i <= SEGMENT_COUNT; ++i)
        {
            float t = (float)i / SEGMENT_COUNT;
            points[i] = EvaluateBezier(start, end, startTan, endTan, t);
        }

        Handles.color = color;
        Handles.DrawAAPolyLine(width, points);
        Handles.color = Color.white;
    }
    private Vector3 EvaluateBezier(Vector3 start, Vector3 end, Vector3 startTan, Vector3 endTan, float t)
    {
        float oneMinusT = 1.0f - t;
        return oneMinusT * oneMinusT * oneMinusT * start +
            3.0f * oneMinusT * oneMinusT * t * startTan +
            3.0f * oneMinusT * t * t * endTan +
            t * t * t * end;
    }
    private bool IsEdgeHighlighted(string from, string to)
    {
        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            if (from == mSelectedBundle || to == mSelectedBundle)
            {
                return true;
            }
        }

        return mSelectedBundleSet.Contains(from) || mSelectedBundleSet.Contains(to);
    }
    private float GetEdgePortOffset(string from, string to)
    {
        float selectedOffset = 24.0f * mZoom;
        float reverseOffset = 16.0f * mZoom;

        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            // 当前选中节点依赖的包：选中节点 -> 子依赖，走偏下端口
            if (from == mSelectedBundle)
            {
                return selectedOffset;
            }

            // 依赖当前选中节点的包：父依赖 -> 选中节点，走偏上端口
            if (to == mSelectedBundle)
            {
                return -selectedOffset;
            }
        }

        // 如果两个包之间存在双向依赖，则上下错开，避免完全重叠
        if (HasReverseEdge(from, to))
        {
            return string.CompareOrdinal(from, to) < 0 ? reverseOffset : -reverseOffset;
        }

        return 0.0f;
    }
    private bool HasReverseEdge(string from, string to)
    {
        return mEdges.Contains(to + "|" + from);
    }
    private float GetEdgeWidth(string from, string to)
    {
        if (string.IsNullOrEmpty(mSelectedBundle) && mSelectedBundleSet.Count == 0)
        {
            return 2.0f;
        }

        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            if (from == mSelectedBundle || to == mSelectedBundle)
            {
                return 4.0f;
            }
        }

        if (mSelectedBundleSet.Contains(from) && mSelectedBundleSet.Contains(to))
        {
            return 3.2f;
        }

        if (mSelectedBundleSet.Contains(from) || mSelectedBundleSet.Contains(to))
        {
            return 2.8f;
        }

        return 1.2f;
    }
    private Color GetEdgeColor(string from, string to)
    {
        if (string.IsNullOrEmpty(mSelectedBundle) && mSelectedBundleSet.Count == 0)
        {
            return new Color(0.18f, 0.48f, 0.72f, 0.90f);
        }

        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            // 当前选中节点依赖的包：选中节点 -> 子依赖
            if (from == mSelectedBundle)
            {
                return new Color(0.0f, 0.85f, 1.0f, 1.0f);
            }

            // 依赖当前选中节点的包：父依赖 -> 选中节点
            if (to == mSelectedBundle)
            {
                return new Color(1.0f, 0.42f, 0.05f, 1.0f);
            }
        }

        // 多选节点之间的依赖线
        if (mSelectedBundleSet.Contains(from) && mSelectedBundleSet.Contains(to))
        {
            return new Color(0.75f, 0.38f, 1.0f, 1.0f);
        }

        // 多选节点依赖出去的线
        if (mSelectedBundleSet.Contains(from))
        {
            return new Color(0.0f, 0.70f, 1.0f, 1.0f);
        }

        // 依赖多选节点的线
        if (mSelectedBundleSet.Contains(to))
        {
            return new Color(1.0f, 0.62f, 0.10f, 1.0f);
        }

        // 其他不相关依赖线：不要用太低 alpha，直接用暗色
        return new Color(0.10f, 0.24f, 0.34f, 0.75f);
    }
    private void DrawArrow(Vector3 end, Vector3 endTan, Color color)
    {
        Vector3 dir = (end - endTan).normalized;
        if (dir.sqrMagnitude < 0.001f)
        {
            return;
        }

        Vector3 normal = new(-dir.y, dir.x, 0.0f);
        float size = Mathf.Clamp(10.0f * mZoom, 4.0f, 10.0f);
        Vector3 p1 = end;
        Vector3 p2 = end - dir * size + normal * size * 0.45f;
        Vector3 p3 = end - dir * size - normal * size * 0.45f;

        Handles.color = color;
        Handles.DrawAAConvexPolygon(p1, p2, p3);
        Handles.color = Color.white;
    }
    private void DrawGrid(Rect rect, float gridSpacing, Color gridColor)
    {
        float scaledGridSpacing = gridSpacing * mZoom;
        if (scaledGridSpacing < 8.0f)
        {
            return;
        }

        int widthDivs = Mathf.CeilToInt(rect.width / scaledGridSpacing);
        int heightDivs = Mathf.CeilToInt(rect.height / scaledGridSpacing);
        Handles.color = gridColor;
        Vector3 offset = new(mPanOffset.x % scaledGridSpacing, mPanOffset.y % scaledGridSpacing, 0.0f);
        for (int i = 0; i < widthDivs + 2; i++)
        {
            float x = rect.x + i * scaledGridSpacing + offset.x;
            Handles.DrawLine(new Vector3(x, rect.y, 0.0f), new Vector3(x, rect.yMax, 0.0f));
        }
        for (int j = 0; j < heightDivs + 2; j++)
        {
            float y = rect.y + j * scaledGridSpacing + offset.y;
            Handles.DrawLine(new Vector3(rect.x, y, 0.0f), new Vector3(rect.xMax, y, 0.0f));
        }
        Handles.color = Color.white;
    }
    private void HandleCanvasInput(Rect graphRect)
    {
        Event e = Event.current;

        if (!string.IsNullOrEmpty(mDraggingNodeName))
        {
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                Vector2 curMouseScreenPos = GUIUtility.GUIToScreenPoint(e.mousePosition);
                Vector2 delta = curMouseScreenPos - mLastNodeMouseScreenPos;
                mLastNodeMouseScreenPos = curMouseScreenPos;

                if (IsBundleSelected(mDraggingNodeName))
                {
                    foreach (string bundleName in mSelectedBundleSet)
                    {
                        if (mNodeMap.TryGetValue(bundleName, out GraphNode selectedNode))
                        {
                            selectedNode.mRect.position += delta / mZoom;
                        }
                    }
                }
                else if (mNodeMap.TryGetValue(mDraggingNodeName, out GraphNode draggingNode))
                {
                    draggingNode.mRect.position += delta / mZoom;
                }

                Repaint();
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                mDraggingNodeName = "";
                e.Use();
                return;
            }
        }

        if (mSelectingNodes)
        {
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                mSelectEndLocalPos = e.mousePosition - graphRect.position;
                Repaint();
                e.Use();
                return;
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                mSelectEndLocalPos = e.mousePosition - graphRect.position;
                ApplySelectionRect();
                mSelectingNodes = false;
                Repaint();
                e.Use();
                return;
            }
        }

        if (!graphRect.Contains(e.mousePosition))
        {
            return;
        }

        Vector2 localMousePosition = e.mousePosition - graphRect.position;
        if (e.type == EventType.ScrollWheel)
        {
            Vector2 graphPosBeforeZoom = ScreenToGraph(localMousePosition);
            float zoomDelta = -e.delta.y * 0.08f;
            float newZoom = Mathf.Clamp(mZoom * (1.0f + zoomDelta), MIN_ZOOM, MAX_ZOOM);
            if (!Mathf.Approximately(newZoom, mZoom))
            {
                mZoom = newZoom;
                mPanOffset = localMousePosition - graphPosBeforeZoom * mZoom;
                Repaint();
            }

            e.Use();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            mSelectingNodes = true;
            mAppendSelectingNodes = e.control || e.command;
            mSelectStartLocalPos = localMousePosition;
            mSelectEndLocalPos = localMousePosition;
            if (!mAppendSelectingNodes)
            {
                mSelectedBundleSet.Clear();
                mSelectedBundle = "";
            }
            Repaint();
            e.Use();
            return;
        }

        if (e.type == EventType.MouseDown && e.button == 2)
        {
            mDraggingCanvas = true;
            mLastMousePos = e.mousePosition;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && mDraggingCanvas)
        {
            Vector2 delta = e.mousePosition - mLastMousePos;
            mPanOffset += delta;
            mLastMousePos = e.mousePosition;
            Repaint();
            e.Use();
        }

        if (e.type == EventType.MouseUp && e.button == 2)
        {
            mDraggingCanvas = false;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 0 && e.alt)
        {
            mPanOffset += e.delta;
            Repaint();
            e.Use();
        }
    }
    private void ApplySelectionRect()
    {
        Rect selectRect = GetSelectionRect();
        if (!mAppendSelectingNodes)
        {
            mSelectedBundleSet.Clear();
        }

        foreach (GraphNode node in mNodeMap.Values)
        {
            if (!IsNodeVisible(node.mBundleName))
            {
                continue;
            }

            Rect nodeRect = GraphToScreenRect(node.mRect);
            if (selectRect.Overlaps(nodeRect))
            {
                mSelectedBundleSet.Add(node.mBundleName);
                mSelectedBundle = node.mBundleName;
            }
        }

        if (mSelectedBundleSet.Count == 0)
        {
            mSelectedBundle = "";
        }
    }
    private void FitAllNodesToView()
    {
        if (mNodeMap.Count == 0)
        {
            return;
        }

        Rect bounds = new Rect();
        bool first = true;
        foreach (GraphNode node in mNodeMap.Values)
        {
            if (!IsNodeVisible(node.mBundleName))
            {
                continue;
            }

            if (first)
            {
                bounds = node.mRect;
                first = false;
            }
            else
            {
                bounds = Union(bounds, node.mRect);
            }
        }

        if (first)
        {
            return;
        }

        Rect viewRect = new Rect(0.0f, 0.0f, position.width, position.height - TOOLBAR_HEIGHT);
        float padding = 80.0f;
        float zoomX = (viewRect.width - padding) / bounds.width;
        float zoomY = (viewRect.height - padding) / bounds.height;
        mZoom = Mathf.Clamp(Mathf.Min(zoomX, zoomY), MIN_ZOOM, MAX_ZOOM);
        mPanOffset = viewRect.center - bounds.center * mZoom;
        Repaint();
    }
    private Rect Union(Rect a, Rect b)
    {
        float xMin = Mathf.Min(a.xMin, b.xMin);
        float yMin = Mathf.Min(a.yMin, b.yMin);
        float xMax = Mathf.Max(a.xMax, b.xMax);
        float yMax = Mathf.Max(a.yMax, b.yMax);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }
    private void SelectFirstMatchedNode()
    {
        if (string.IsNullOrEmpty(mSearchText))
        {
            return;
        }

        foreach (string bundleName in mNodeMap.Keys.OrderBy(x => x))
        {
            if (IsNodeVisible(bundleName) && bundleName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mSelectedBundle = bundleName;
                mSelectedBundleSet.Clear();
                mSelectedBundleSet.Add(bundleName);
                GraphNode node;
                if (mNodeMap.TryGetValue(bundleName, out node))
                {
                    Rect viewRect = new(0.0f, 0.0f, position.width, position.height - TOOLBAR_HEIGHT);
                    mPanOffset = viewRect.center - node.mRect.center * mZoom;
                }
                Repaint();
                return;
            }
        }
    }
    private void ExportTxt()
    {
        if (mBundleMap.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "请先点击“分析所有AB依赖”。", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanel("导出AB依赖文本", Application.dataPath, "AssetBundleDependencies_Detail.txt", "txt");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        StringBuilder sb = new();
        foreach (BundleInfo info in mBundleMap.Values.OrderBy(x => x.mBundleName))
        {
            sb.AppendLine("============================================================");
            sb.AppendLine("Bundle: " + info.mBundleName);
            sb.AppendLine("Asset Count: " + info.mAssetCount);
            sb.AppendLine();

            AppendList(sb, "[Direct Dependencies]", info.mDirectDependencies);
            AppendList(sb, "[All Dependencies]", info.mAllDependencies);
            AppendList(sb, "[Direct Referenced By]", info.mDirectReferencedBy);
            AppendList(sb, "[All Referenced By]", info.mAllReferencedBy);
            AppendList(sb, "[Assets]", info.mAssets);

            sb.AppendLine();
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();

        Debug.Log("[AB依赖分析] TXT导出完成: " + path);
        EditorUtility.DisplayDialog("完成", "TXT导出完成:\n" + path, "OK");
    }
    private void AppendList(StringBuilder sb, string title, List<string> list)
    {
        sb.AppendLine(title + " " + list.Count);
        if (list.Count == 0)
        {
            sb.AppendLine("    <None>");
        }
        else
        {
            foreach (string item in list)
            {
                sb.AppendLine("    " + item);
            }
        }
        sb.AppendLine();
    }
    private Color GetNodeBackgroundColor(string bundleName)
    {
        string n = bundleName.ToLower();
        if (IsBundleSelected(bundleName))
        {
            return new Color(0.20f, 0.30f, 0.38f, 1.0f);
        }
        if (n.StartsWith("res_prefabs_"))
        {
            return new Color(0.16f, 0.23f, 0.30f, 1.0f);
        }
        if (n.StartsWith("resatlassprites_"))
        {
            return new Color(0.15f, 0.26f, 0.19f, 1.0f);
        }
        if (n.StartsWith("assetbundle_activity_"))
        {
            return new Color(0.30f, 0.23f, 0.14f, 1.0f);
        }
        if (n.StartsWith("sharedarts_"))
        {
            return new Color(0.24f, 0.19f, 0.32f, 1.0f);
        }
        if (n.StartsWith("animation"))
        {
            return new Color(0.31f, 0.18f, 0.18f, 1.0f);
        }
        if (n.StartsWith("assetbundle_spineres_"))
        {
            return new Color(0.30f, 0.27f, 0.15f, 1.0f);
        }
        if (n.StartsWith("_scenes"))
        {
            return new Color(0.32f, 0.18f, 0.27f, 1.0f);
        }
        return new Color(0.20f, 0.20f, 0.20f, 1.0f);
    }
    private Color GetNodeTitleColor(string bundleName)
    {
        string n = bundleName.ToLower();
        if (n.StartsWith("res_prefabs_"))
        {
            return new Color(0.12f, 0.20f, 0.28f, 1.0f);
        }
        if (n.StartsWith("resatlassprites_"))
        {
            return new Color(0.10f, 0.22f, 0.14f, 1.0f);
        }
        if (n.StartsWith("assetbundle_activity_"))
        {
            return new Color(0.25f, 0.18f, 0.10f, 1.0f);
        }
        if (n.StartsWith("sharedarts_"))
        {
            return new Color(0.20f, 0.15f, 0.28f, 1.0f);
        }
        if (n.StartsWith("animation"))
        {
            return new Color(0.26f, 0.13f, 0.13f, 1.0f);
        }
        if (n.StartsWith("assetbundle_spineres_"))
        {
            return new Color(0.25f, 0.22f, 0.10f, 1.0f);
        }
        if (n.StartsWith("_scenes"))
        {
            return new Color(0.27f, 0.13f, 0.22f, 1.0f);
        }
        return new Color(0.16f, 0.16f, 0.16f, 1.0f);
    }
}