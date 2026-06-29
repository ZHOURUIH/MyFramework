using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MathUtility;

public class AssetBundleDependencyWindow : GameEditorWindow
{
    private class BundleInfo
    {
        public string mBundleName;                                          // AssetBundle名称
        public int mAssetCount;                                             // 该AB中显式设置的资源数量
        public List<string> mDirectDependencies = new();                    // 该AB直接依赖的AB列表
        public List<string> mAllDependencies = new();                       // 该AB递归依赖到的所有AB列表
        public List<string> mDirectReferencedBy = new();                    // 直接引用该AB的AB列表
        public List<string> mAllReferencedBy = new();                       // 递归引用该AB的所有AB列表
        public List<string> mAssets = new();                                // 该AB中显式包含的资源路径列表
    }

    private class GraphNode
    {
        public string mBundleName;                                          // 节点对应的AssetBundle名称
        public Rect mRect;                                                  // 节点在图空间中的矩形区域
    }

    private readonly Dictionary<string, BundleInfo> mBundleMap = new();     // AB名称到AB信息的映射
    private readonly Dictionary<string, GraphNode> mNodeMap = new();        // AB名称到图节点的映射
    private readonly HashSet<string> mEdges = new();                        // 直接依赖边集合,格式为 from|to
    private readonly HashSet<string> mCycleEdgeSet = new();                 // 处于依赖环中的边集合,格式为 from|to
    private readonly HashSet<string> mSelectedBundleSet = new();            // 当前多选中的AB集合
    private readonly List<List<string>> mDependencyCycleList = new();       // 检测到的依赖环列表
    private Vector2 mPanOffset = new(80.0f, 140.0f);                        // 图视图平移偏移
    private bool mDraggingCanvas;                                           // 是否正在拖动画布
    private Vector2 mLastMousePos;                                          // 拖动画布时上一帧鼠标位置
    private string mSearchText = "";                                        // 顶部AB搜索文本
    private string mSelectedBundle = "";                                    // 当前主选中的AB名称
    private string mDraggingNodeName = "";                                  // 当前正在拖拽的节点AB名称
    private bool mHideIndependentBundle;                                    // 是否隐藏无依赖也无被依赖的独立AB
    private Vector2 mLastNodeMouseScreenPos;                                // 拖拽节点时上一帧鼠标屏幕坐标
    private bool mSelectingNodes;                                           // 是否正在框选节点
    private bool mAppendSelectingNodes;                                     // 框选时是否追加到已有选择
    private Vector2 mSelectStartLocalPos;                                   // 框选起点在图区域内的局部坐标
    private Vector2 mSelectEndLocalPos;                                     // 框选终点在图区域内的局部坐标
    private Vector2 mDependencyCycleScrollPos;                              // 顶部依赖环列表滚动位置
    private Vector2 mSelectedBundleDependencyScrollPos;                     // 右侧选中AB依赖列表滚动位置
    private Rect mSelectedBundleDependencyPanelScreenRect;                  // 右侧依赖面板的屏幕坐标区域,用于阻止点击穿透
    private string mSelectedBundleDependencySearchText = "";                // 右侧依赖列表搜索文本
    private float mZoom = 1.0f;                                             // 图视图缩放比例
    private GUIStyle mNodeStyle;                                            // 普通节点窗口样式
    private GUIStyle mSelectedNodeStyle;                                    // 选中节点窗口样式
    private GUIStyle mNodeNameStyle;                                        // 节点名称文本样式
    private GUIStyle mSmallLabelStyle;                                      // 节点详情小字文本样式
    private const int MAX_NODE_COUNT_PER_COLUMN = 20;                       // 自动排版时每列最多节点数
    private const int MAX_DEPENDENCY_CYCLE_COUNT = 300;                     // 最多记录和显示的依赖环数量
    private const int MAX_DEPENDENCY_CYCLE_DEPTH = 128;                     // 依赖环DFS最大搜索深度
    private const float SAME_FOLDER_COLUMN_SPACE = 300.0f;                  // 同一归类目录下节点列间距
    private const float DIFFERENT_FOLDER_COLUMN_SPACE = 420.0f;             // 不同归类目录之间的列间距
    private const float TOOLBAR_HEIGHT = 260.0f;                            // 顶部工具栏高度
    private const float NODE_WIDTH = 280.0f;                                // 节点宽度
    private const float NODE_HEIGHT = 120.0f;                               // 节点高度
    private const float VERTICAL_SPACE = 155.0f;                            // 自动排版时节点纵向间距
    private const float MIN_ZOOM = 0.1f;                                    // 最小缩放比例
    private const float MAX_ZOOM = 2.0f;                                    // 最大缩放比例
    private const float DETAIL_HIDE_ZOOM = 0.55f;                           // 低于该缩放时隐藏节点详情
    private const float TITLE_ONLY_ZOOM = 0.35f;                            // 低于该缩放时节点标题使用单行/裁剪显示
    private const float SELECTED_DEPENDENCY_PANEL_WIDTH = 390.0f;           // 右侧选中AB依赖面板目标宽度
    private const float SELECTED_DEPENDENCY_PANEL_MARGIN = 8.0f;            // 右侧选中AB依赖面板外边距
    private const float SELECTED_DEPENDENCY_PANEL_CONTENT_PADDING = 8.0f;   // 右侧选中AB依赖面板内容内边距
    private const float SELECTED_DEPENDENCY_PANEL_OPAQUE_ALPHA = 0.88f;     // 选中AB后右侧依赖面板背景不透明度
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
    protected override void onGUI()
    {
        if (mNodeStyle == null)
        {
            InitStyles();
        }

        Rect toolbarRect = new(0.0f, 0.0f, position.width, TOOLBAR_HEIGHT);
        DrawToolbar(toolbarRect);

        Rect graphRect = new(0.0f, TOOLBAR_HEIGHT, position.width, position.height - TOOLBAR_HEIGHT);
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
            "    多选数量: " + mSelectedBundleSet.Count +
            "    依赖环: " + mDependencyCycleList.Count,
            EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();

        DrawDependencyCycleWarning();

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawDependencyCycleWarning()
    {
        if (mBundleMap.Count == 0)
        {
            return;
        }

        if (mDependencyCycleList.Count == 0)
        {
            EditorGUILayout.HelpBox("未发现AB依赖环。", MessageType.Info);
            return;
        }

        string text = "发现AB依赖环: " + mDependencyCycleList.Count + " 个。";
        if (mDependencyCycleList.Count >= MAX_DEPENDENCY_CYCLE_COUNT)
        {
            text += " 当前最多显示前 " + MAX_DEPENDENCY_CYCLE_COUNT + " 个，请优先处理。";
        }
        EditorGUILayout.HelpBox(text, MessageType.Error);

        mDependencyCycleScrollPos = EditorGUILayout.BeginScrollView(mDependencyCycleScrollPos, GUILayout.Height(96.0f));
        for (int i = 0; i < mDependencyCycleList.Count; ++i)
        {
            EditorGUILayout.LabelField((i + 1) + ". " + GetCycleDisplayText(mDependencyCycleList[i]), EditorStyles.miniLabel);
        }
        EditorGUILayout.EndScrollView();
    }

    private void AnalyzeAllAssetBundles()
    {
        mBundleMap.Clear();
        mNodeMap.Clear();
        mEdges.Clear();
        mCycleEdgeSet.Clear();
        mDependencyCycleList.Clear();
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
                EditorUtility.DisplayProgressBar("AB依赖分析", "正在分析: " + bundleName + "\n " + (i + 1) + "/" + bundleCount, progress);

                BundleInfo info = new BundleInfo();
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

            EditorUtility.DisplayProgressBar("AB依赖分析", "正在生成依赖节点和连线...", 0.95f);
            BuildAllNodesAndEdges();

            EditorUtility.DisplayProgressBar("AB依赖分析", "正在检测依赖环...", 0.975f);
            BuildDependencyCycleList();

            EditorUtility.DisplayProgressBar("AB依赖分析", "正在自动排版...", 0.98f);
            AutoLayout();

            Debug.Log("[AB依赖分析] 分析完成, Bundle数量: " + mBundleMap.Count +
                ", 依赖连线: " + mEdges.Count +
                ", 依赖环: " + mDependencyCycleList.Count);
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

    private void BuildDependencyCycleList()
    {
        mDependencyCycleList.Clear();
        mCycleEdgeSet.Clear();

        Dictionary<string, List<string>> graph = new();
        foreach (string bundleName in mBundleMap.Keys)
        {
            graph[bundleName] = new();
        }

        foreach (BundleInfo info in mBundleMap.Values)
        {
            if (!graph.TryGetValue(info.mBundleName, out List<string> depList))
            {
                continue;
            }
            foreach (string dep in info.mDirectDependencies)
            {
                depList.addUniqueIf(dep, mBundleMap.ContainsKey(dep));
            }
        }

        graph.forValue(depList => depList.Sort());

        HashSet<string> cycleKeySet = new();
        foreach (string start in graph.Keys.OrderBy(x => x))
        {
            if (mDependencyCycleList.Count >= MAX_DEPENDENCY_CYCLE_COUNT)
            {
                break;
            }
            List<string> path = new();
            HashSet<string> pathSet = new();
            FindDependencyCycleDFS(start, start, graph, path, pathSet, cycleKeySet);
        }

        mDependencyCycleList.Sort((a, b) =>
        {
            string aText = GetCycleDisplayText(a);
            string bText = GetCycleDisplayText(b);
            return string.CompareOrdinal(aText, bText);
        });

        foreach (List<string> cycle in mDependencyCycleList)
        {
            AddCycleEdges(cycle);
        }
    }

    private void FindDependencyCycleDFS(string start, string current, Dictionary<string, List<string>> graph, List<string> path, HashSet<string> pathSet, HashSet<string> cycleKeySet)
    {
        if (mDependencyCycleList.Count >= MAX_DEPENDENCY_CYCLE_COUNT || path.Count >= MAX_DEPENDENCY_CYCLE_DEPTH)
        {
            return;
        }
        path.Add(current);
        pathSet.Add(current);
        graph.TryGetValue(current, out var depList);
        foreach (string next in depList.safe())
        {
            if (mDependencyCycleList.Count >= MAX_DEPENDENCY_CYCLE_COUNT)
            {
                break;
            }
            if (next == start && path.Count > 1)
            {
                List<string> cycle = new(path) { start };
                if (cycleKeySet.Add(GetCanonicalCycleKey(cycle)))
                {
                    mDependencyCycleList.Add(cycle);
                }
                continue;
            }
            if (pathSet.Contains(next))
            {
                continue;
            }
            FindDependencyCycleDFS(start, next, graph, path, pathSet, cycleKeySet);
        }

        pathSet.Remove(current);
        path.RemoveAt(path.Count - 1);
    }

    private string GetCanonicalCycleKey(List<string> cycle)
    {
        if (cycle == null || cycle.Count <= 1)
        {
            return "";
        }

        List<string> nodes = new(cycle);
        if (nodes.Count > 1 && nodes[0] == nodes[^1])
        {
            nodes.RemoveAt(nodes.Count - 1);
        }
        if (nodes.Count == 0)
        {
            return "";
        }
        int bestIndex = 0;
        for (int i = 1; i < nodes.Count; ++i)
        {
            if (string.CompareOrdinal(nodes[i], nodes[bestIndex]) < 0)
            {
                bestIndex = i;
            }
        }

        StringBuilder sb = new();
        for (int i = 0; i < nodes.Count; ++i)
        {
            int index = (bestIndex + i) % nodes.Count;
            if (i > 0)
            {
                sb.Append("->");
            }
            sb.Append(nodes[index]);
        }
        return sb.ToString();
    }

    private string GetCycleDisplayText(List<string> cycle)
    {
        if (cycle == null || cycle.Count == 0)
        {
            return "";
        }
        StringBuilder sb = new();
        for (int i = 0; i < cycle.Count; ++i)
        {
            if (i > 0)
            {
                sb.Append(" -> ");
            }
            sb.Append(cycle[i]);
        }
        return sb.ToString();
    }

    private void AddCycleEdges(List<string> cycle)
    {
        if (cycle == null || cycle.Count < 2)
        {
            return;
        }
        for (int i = 0; i < cycle.Count - 1; ++i)
        {
            mCycleEdgeSet.Add(cycle[i] + "|" + cycle[i + 1]);
        }
    }

    private bool IsCycleEdge(string from, string to)
    {
        return mCycleEdgeSet.Contains(from + "|" + to);
    }

    private void AddNode(string bundleName)
    {
        if (mNodeMap.ContainsKey(bundleName))
        {
            return;
        }

        GraphNode node = new();
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

        Dictionary<string, string> bundleFolderMap = new Dictionary<string, string>();
        foreach (GraphNode node in mNodeMap.Values)
        {
            bundleFolderMap[node.mBundleName] = GetBundleFolder(node.mBundleName);
        }

        NormalizeBundleFolderMap(bundleFolderMap);

        Dictionary<string, List<GraphNode>> folderNodeMap = new();
        foreach (GraphNode node in mNodeMap.Values)
        {
            string folder = bundleFolderMap.ContainsKey(node.mBundleName) ? bundleFolderMap[node.mBundleName] : "<Unknown>";
            folderNodeMap.getOrAddNew(folder).Add(node);
        }

        var folderList = folderNodeMap.OrderBy(x => x.Key).ToList();
        float curX = 0.0f;
        for (int folderIndex = 0; folderIndex < folderList.Count; ++folderIndex)
        {
            var folderPair = folderList[folderIndex];
            List<GraphNode> nodes = folderPair.Value.OrderByDescending(x =>
            {
                if (mBundleMap.TryGetValue(x.mBundleName, out BundleInfo info))
                {
                    return info.mDirectDependencies.Count + info.mDirectReferencedBy.Count;
                }

                return 0;
            }).ThenBy(x => x.mBundleName).ToList();

            int columnCount = getMax(1, ceil((float)nodes.Count / MAX_NODE_COUNT_PER_COLUMN));
            for (int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
            {
                int columnIndex = nodeIndex / MAX_NODE_COUNT_PER_COLUMN;
                int rowIndex = nodeIndex % MAX_NODE_COUNT_PER_COLUMN;
                float x = curX + columnIndex * SAME_FOLDER_COLUMN_SPACE;
                float y = rowIndex * VERTICAL_SPACE;
                nodes[nodeIndex].mRect.position = new(x, y);
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
            Dictionary<string, int> folderCountMap = new();
            foreach (string folder in bundleFolderMap.Values)
            {
                folderCountMap.TryAdd(folder, 0);
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

        List<string> folderList = new();
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
        return path[..index];
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
            commonLength = getMin(commonLength, parts.Length);
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

        Rect localGraphRect = new Rect(0.0f, 0.0f, graphRect.width, graphRect.height);
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

        UpdateSelectedBundleDependencyPanelScreenRect(localGraphRect);

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
        DrawSelectedBundleDependencyPanel(localGraphRect);
        GUI.EndGroup();
    }

    private void DrawSelectedBundleDependencyPanel(Rect graphRect)
    {
        if (mBundleMap.Count == 0)
        {
            return;
        }

        Rect panelRect = GetSelectedBundleDependencyPanelRect(graphRect);

        string bundleName = mSelectedBundle;
        if (string.IsNullOrEmpty(bundleName) && mSelectedBundleSet.Count == 1)
        {
            bundleName = mSelectedBundleSet.First();
        }

        bool hasSelectedBundle = !string.IsNullOrEmpty(bundleName) && mBundleMap.ContainsKey(bundleName);
        BeginSelectedBundleDependencyPanelArea(panelRect, hasSelectedBundle);

        EditorGUILayout.LabelField("选中AB依赖详情", EditorStyles.boldLabel);

        if (string.IsNullOrEmpty(bundleName))
        {
            EditorGUILayout.HelpBox("点击左侧任意 AssetBundle 节点后，这里会显示它的依赖项。", MessageType.Info);
            GUILayout.EndArea();
            return;
        }

        if (!mBundleMap.TryGetValue(bundleName, out BundleInfo info))
        {
            EditorGUILayout.HelpBox("找不到选中的 AssetBundle 信息:\n" + bundleName, MessageType.Warning);
            GUILayout.EndArea();
            return;
        }

        EditorGUILayout.LabelField("AssetBundle", EditorStyles.miniBoldLabel);
        EditorGUILayout.SelectableLabel(bundleName, EditorStyles.textField, GUILayout.Height(20.0f));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("资源: " + info.mAssetCount, GUILayout.Width(75.0f));
        EditorGUILayout.LabelField("直接依赖: " + info.mDirectDependencies.Count, GUILayout.Width(95.0f));
        EditorGUILayout.LabelField("所有依赖: " + info.mAllDependencies.Count, GUILayout.Width(95.0f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索", GUILayout.Width(36.0f));
        string newSearchText = EditorGUILayout.TextField(mSelectedBundleDependencySearchText);
        if (newSearchText != mSelectedBundleDependencySearchText)
        {
            mSelectedBundleDependencySearchText = newSearchText;
            Repaint();
        }
        if (GUILayout.Button("清空", GUILayout.Width(48.0f)))
        {
            mSelectedBundleDependencySearchText = "";
            GUI.FocusControl(null);
            Repaint();
        }
        EditorGUILayout.EndHorizontal();

        mSelectedBundleDependencyScrollPos = EditorGUILayout.BeginScrollView(mSelectedBundleDependencyScrollPos);

        DrawSelectedBundleStringList("直接依赖 Direct Dependencies", info.mDirectDependencies);
        DrawSelectedBundleStringList("所有依赖 All Dependencies", info.mAllDependencies);
        DrawSelectedBundleStringList("直接被依赖 Direct Referenced By", info.mDirectReferencedBy);
        DrawSelectedBundleStringList("所有被依赖 All Referenced By", info.mAllReferencedBy);

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void BeginSelectedBundleDependencyPanelArea(Rect panelRect, bool hasSelectedBundle)
    {
        if (!hasSelectedBundle)
        {
            GUILayout.BeginArea(panelRect, EditorStyles.helpBox);
            return;
        }

        float colorValue = EditorGUIUtility.isProSkin ? 0.08f : 0.86f;
        Color backgroundColor = new(colorValue, colorValue, colorValue, SELECTED_DEPENDENCY_PANEL_OPAQUE_ALPHA);
        Color borderColor = EditorGUIUtility.isProSkin ? new Color(0.45f, 0.65f, 0.85f, 0.95f) : new Color(0.25f, 0.42f, 0.62f, 0.95f);

        EditorGUI.DrawRect(panelRect, backgroundColor);
        DrawRectBorder(panelRect, borderColor, 1.0f);

        Rect contentRect = new(
            panelRect.x + SELECTED_DEPENDENCY_PANEL_CONTENT_PADDING,
            panelRect.y + SELECTED_DEPENDENCY_PANEL_CONTENT_PADDING,
            panelRect.width - SELECTED_DEPENDENCY_PANEL_CONTENT_PADDING * 2.0f,
            panelRect.height - SELECTED_DEPENDENCY_PANEL_CONTENT_PADDING * 2.0f);

        GUILayout.BeginArea(contentRect);
    }

    private void UpdateSelectedBundleDependencyPanelScreenRect(Rect graphRect)
    {
        if (mBundleMap.Count == 0)
        {
            mSelectedBundleDependencyPanelScreenRect = new Rect();
            return;
        }

        Rect panelRect = GetSelectedBundleDependencyPanelRect(graphRect);
        Vector2 screenPosition = GUIUtility.GUIToScreenPoint(panelRect.position);
        mSelectedBundleDependencyPanelScreenRect = new Rect(screenPosition, panelRect.size);
    }

    private bool IsMouseInSelectedBundleDependencyPanelScreen(Vector2 mouseScreenPosition)
    {
        if (mBundleMap.Count == 0)
        {
            return false;
        }

        return mSelectedBundleDependencyPanelScreenRect.Contains(mouseScreenPosition);
    }

    private Rect GetSelectedBundleDependencyPanelRect(Rect graphRect)
    {
        float width = Mathf.Min(SELECTED_DEPENDENCY_PANEL_WIDTH, Mathf.Max(260.0f, graphRect.width * 0.42f));
        return new Rect(
            graphRect.xMax - width - SELECTED_DEPENDENCY_PANEL_MARGIN,
            graphRect.y + SELECTED_DEPENDENCY_PANEL_MARGIN,
            width,
            graphRect.height - SELECTED_DEPENDENCY_PANEL_MARGIN * 2.0f);
    }

    private bool IsMouseInSelectedBundleDependencyPanel(Rect graphRect, Vector2 mousePosition)
    {
        if (mBundleMap.Count == 0)
        {
            return false;
        }

        Vector2 localMousePosition = mousePosition - graphRect.position;
        Rect localGraphRect = new(0.0f, 0.0f, graphRect.width, graphRect.height);
        return GetSelectedBundleDependencyPanelRect(localGraphRect).Contains(localMousePosition);
    }

    private void DrawSelectedBundleStringList(string title, List<string> list)
    {
        List<string> filteredList = GetSelectedBundleFilteredList(list);

        EditorGUILayout.Space(6.0f);
        EditorGUILayout.LabelField(title + " (" + filteredList.Count + "/" + list.Count + ")", EditorStyles.boldLabel);

        if (filteredList.Count == 0)
        {
            EditorGUILayout.LabelField("    <None>", EditorStyles.miniLabel);
            return;
        }

        for (int i = 0; i < filteredList.Count; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(28.0f));
            EditorGUILayout.SelectableLabel(filteredList[i], EditorStyles.textField, GUILayout.Height(18.0f));
            EditorGUILayout.EndHorizontal();
        }
    }

    private List<string> GetSelectedBundleFilteredList(List<string> list)
    {
        if (string.IsNullOrEmpty(mSelectedBundleDependencySearchText))
        {
            return list;
        }
        List<string> result = new();
        foreach (string item in list)
        {
            if (item != null && item.IndexOf(mSelectedBundleDependencySearchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.Add(item);
            }
        }
        return result;
    }

    private void DrawSelectionRect()
    {
        if (!mSelectingNodes)
        {
            return;
        }
        Rect rect = GetSelectionRect();
        EditorGUI.DrawRect(rect, new(0.25f, 0.65f, 1.0f, 0.16f));
        DrawRectBorder(rect, new(0.25f, 0.75f, 1.0f, 0.9f), 1.0f);
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
        Rect nodeRect = new(0.0f, 0.0f, NODE_WIDTH * mZoom, NODE_HEIGHT * mZoom);
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
            if (IsMouseInSelectedBundleDependencyPanelScreen(GUIUtility.GUIToScreenPoint(e.mousePosition)))
            {
                return;
            }

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
        Color borderColor = selected ? new Color(0.35f, 0.72f, 1.0f, 1.0f) : GetNodeBorderColor(bundleName);
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
            Rect innerRect = new(nodeRect.x + 2.0f * mZoom, nodeRect.y + 2.0f * mZoom, nodeRect.width - 4.0f * mZoom, nodeRect.height - 4.0f * mZoom);
            DrawRectBorder(innerRect, new Color(0.35f, 0.72f, 1.0f, 0.75f), borderWidth);
        }
    }

    private Color GetNodeBorderColor(string bundleName)
    {
        if (mDependencyCycleList.contains(item => item.contains(bundleName)))
        {
            return new Color(1.0f, 0.12f, 0.12f, 1.0f);
        }
        return new Color(0.22f, 0.42f, 0.58f, 1.0f);
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
        foreach (string edge in mEdges)
        {
            string[] parts = edge.Split('|');
            if (parts.Length != 2)
            {
                continue;
            }
            string from = parts[0];
            string to = parts[1];
            if (IsEdgeHighlighted(from, to) || IsCycleEdge(from, to))
            {
                continue;
            }
            DrawSingleEdge(from, to);
        }

        foreach (string edge in mEdges)
        {
            string[] parts = edge.Split('|');
            if (parts.Length != 2)
            {
                continue;
            }
            string from = parts[0];
            string to = parts[1];
            if (!IsCycleEdge(from, to) || IsEdgeHighlighted(from, to))
            {
                continue;
            }
            DrawSingleEdge(from, to);
        }

        foreach (string edge in mEdges)
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
        if (!IsNodeVisible(from) || 
            !IsNodeVisible(to) ||
            !mNodeMap.TryGetValue(from, out GraphNode fromNode) || 
            !mNodeMap.TryGetValue(to, out GraphNode toNode))
        {
            return;
        }
        Rect fromRect = GraphToScreenRect(fromNode.mRect);
        Rect toRect = GraphToScreenRect(toNode.mRect);
        float portOffset = GetEdgePortOffset(from, to);
        float fromY = fromRect.center.y + portOffset;
        float toY = toRect.center.y + portOffset;

        Vector3 start = new(fromRect.xMax, fromY, 0.0f);
        Vector3 end = new(toRect.xMin, toY, 0.0f);
        bool reverseDirection = toRect.center.x < fromRect.center.x;
        if (reverseDirection)
        {
            start = new(fromRect.xMin, fromY, 0.0f);
            end = new(toRect.xMax, toY, 0.0f);
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
        DrawBezierLine(start, end, startTan, endTan, color, Mathf.Clamp(lineWidth * mZoom, 0.8f, 3.6f));
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
        if (!mSelectedBundle.isEmpty() && (from == mSelectedBundle || to == mSelectedBundle))
        {
            return true;
        }
        return mSelectedBundleSet.Contains(from) || mSelectedBundleSet.Contains(to);
    }

    private float GetEdgePortOffset(string from, string to)
    {
        float selectedOffset = 24.0f * mZoom;
        float reverseOffset = 16.0f * mZoom;
        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            if (from == mSelectedBundle)
            {
                return selectedOffset;
            }

            if (to == mSelectedBundle)
            {
                return -selectedOffset;
            }
        }

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
        if (IsCycleEdge(from, to))
        {
            return IsEdgeHighlighted(from, to) ? 5.0f : 3.4f;
        }

        if (string.IsNullOrEmpty(mSelectedBundle) && mSelectedBundleSet.Count == 0)
        {
            return HasReverseEdge(from, to) ? 3.0f : 2.0f;
        }

        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            if (from == mSelectedBundle || to == mSelectedBundle)
            {
                return HasReverseEdge(from, to) ? 4.6f : 4.0f;
            }
        }

        if (mSelectedBundleSet.Contains(from) && mSelectedBundleSet.Contains(to))
        {
            return HasReverseEdge(from, to) ? 3.8f : 3.2f;
        }

        if (mSelectedBundleSet.Contains(from) || mSelectedBundleSet.Contains(to))
        {
            return HasReverseEdge(from, to) ? 3.2f : 2.8f;
        }

        return HasReverseEdge(from, to) ? 1.8f : 1.2f;
    }

    private Color GetEdgeColor(string from, string to)
    {
        if (IsCycleEdge(from, to))
        {
            return new Color(1.0f, 0.05f, 0.05f, 1.0f);
        }

        if (HasReverseEdge(from, to))
        {
            return new Color(1.0f, 0.18f, 0.12f, 1.0f);
        }

        if (string.IsNullOrEmpty(mSelectedBundle) && mSelectedBundleSet.Count == 0)
        {
            return new Color(0.18f, 0.48f, 0.72f, 0.90f);
        }

        if (!string.IsNullOrEmpty(mSelectedBundle))
        {
            if (from == mSelectedBundle)
            {
                return new Color(0.0f, 0.85f, 1.0f, 1.0f);
            }

            if (to == mSelectedBundle)
            {
                return new Color(1.0f, 0.42f, 0.05f, 1.0f);
            }
        }

        if (mSelectedBundleSet.Contains(from) && mSelectedBundleSet.Contains(to))
        {
            return new Color(0.75f, 0.38f, 1.0f, 1.0f);
        }

        if (mSelectedBundleSet.Contains(from))
        {
            return new Color(0.0f, 0.70f, 1.0f, 1.0f);
        }

        if (mSelectedBundleSet.Contains(to))
        {
            return new Color(1.0f, 0.62f, 0.10f, 1.0f);
        }

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
            Handles.DrawLine(new(x, rect.y, 0.0f), new(x, rect.yMax, 0.0f));
        }

        for (int j = 0; j < heightDivs + 2; j++)
        {
            float y = rect.y + j * scaledGridSpacing + offset.y;
            Handles.DrawLine(new(rect.x, y, 0.0f), new(rect.xMax, y, 0.0f));
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

        if (!graphRect.Contains(e.mousePosition) ||
            IsMouseInSelectedBundleDependencyPanel(graphRect, e.mousePosition))
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

        Rect bounds = new();
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

        Rect viewRect = new(0.0f, 0.0f, position.width, position.height - TOOLBAR_HEIGHT);
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
                if (mNodeMap.TryGetValue(bundleName, out GraphNode node))
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

        sb.AppendLine("============================================================");
        sb.AppendLine("[Dependency Cycles]");
        sb.AppendLine("Count: " + mDependencyCycleList.Count);
        if (mDependencyCycleList.Count == 0)
        {
            sb.AppendLine("    <None>");
        }
        else
        {
            foreach (List<string> cycle in mDependencyCycleList)
            {
                sb.AppendLine("    " + GetCycleDisplayText(cycle));
            }
        }
        sb.AppendLine();

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