using System;
using System.Collections.Generic;
using System.IO;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using static UnityEditor.AssetDatabase;
#if SPINE_RUNTIME_43
using static Spine43AnimationSplitter;
using SpineBinaryScannerVersion = Spine43BinaryScanner;
using static Spine43AnimationFile;
using SpineSingleAnimationDataVersion = Spine43SingleAnimationData;
#elif SPINE_RUNTIME_42
using static Spine42AnimationSplitter;
using SpineBinaryScannerVersion = Spine42BinaryScanner;
using static Spine42AnimationFile;
using SpineSingleAnimationDataVersion = Spine42SingleAnimationData;
#elif SPINE_RUNTIME_41
using static Spine41AnimationSplitter;
using SpineBinaryScannerVersion = Spine41BinaryScanner;
using static Spine41AnimationFile;
using SpineSingleAnimationDataVersion = Spine41SingleAnimationData;
#elif SPINE_RUNTIME_40
using static Spine40AnimationSplitter;
using SpineBinaryScannerVersion = Spine40BinaryScanner;
using static Spine40AnimationFile;
using SpineSingleAnimationDataVersion = Spine40SingleAnimationData;
#else
#error SpineSplitter仅支持通过UPM安装的Spine 4.0、4.1、4.2或4.3 Runtime。
#endif
using static SpineAnimationFileNameUtility;

// Spine动画拆分窗口,用于通过当前版本Scanner扫描完整Skeleton源数据,生成最小化基础Skeleton文件。
// 普通版本会移除全部动画;Spine 4.3带Slider Constraint时会保留Slider加载阶段必须引用的动画,其余动画分别生成为独立.bytes文件。
public class SpineAnimationSubsetWindow : EditorWindow
{
    protected enum AnimationSortType
    {
        None,
        Name,
        SplitStatus,
        Size,
    }
    protected class AnimationSplitFileInfo
    {
        public string mDisplayName;
        public string mAssetPath;
    }
    protected class AnimationItem
    {
        public SpineAnimationBinaryRange mRange;
        public readonly List<AnimationSplitFileInfo> mSplitFiles = new List<AnimationSplitFileInfo>();
    }
    protected SkeletonDataAsset mSkeletonDataAsset;
    protected byte[] mSourceBytes;
    protected SpineBinaryScanResult mScanResult;
    protected readonly List<AnimationItem> mAnimationItems = new List<AnimationItem>();
    protected string mSourceAssetPath = string.Empty;
    protected string mSourceSkeletonDataAssetPath = string.Empty;
    protected string mSourceSkeletonDataAssetName = string.Empty;
    protected string mSearchText = string.Empty;
    protected bool mVerifyAfterGenerate = true;
    protected bool mHideSplitAnimations;
    protected UnityEngine.Object mIgnoredSelectionObject;
    protected int mMatchedSplitFileCount;
    protected int mInvalidSplitFileCount;
    protected AnimationSortType mAnimationSortType = AnimationSortType.None;
    protected bool mAnimationSortAscending;
    protected GUIStyle mEvenRowStyle;
    protected GUIStyle mOddRowStyle;
    protected GUIStyle mUnsplitStatusStyle;
    protected GUIStyle mSplitStatusStyle;
    protected GUIStyle mDuplicateSplitStatusStyle;
    protected GUIStyle mHeaderRowStyle;
    protected GUIStyle mHeaderButtonStyle;
    protected GUIStyle mHeaderLabelStyle;
    protected GUIStyle mRowLabelStyle;
    protected Texture2D mEvenRowTexture;
    protected Texture2D mOddRowTexture;
    protected Vector2 mWindowScrollPosition;
    protected Vector2 mAnimationScrollPosition;
    protected const float ANIMATION_INDEX_COLUMN_WIDTH = 50.0f;
    protected const float ANIMATION_STATUS_COLUMN_WIDTH = 260.0f;
    protected const float ANIMATION_SIZE_COLUMN_WIDTH = 125.0f;
    protected const float ANIMATION_SCROLLBAR_WIDTH = 16.0f;
    protected const float ANIMATION_LIST_PADDING_WIDTH = 14.0f;
    [MenuItem("Tools/Spine/Spine动画拆分")]
    protected static void openWindow()
    {
        var window = GetWindow<SpineAnimationSubsetWindow>("Spine动画拆分");
        window.minSize = new Vector2(1100.0f, 620.0f);
        window.tryUseSelectedAsset();
        window.Show();
    }
    protected void OnEnable()
    {
        tryUseSelectedAsset();
    }
    protected void OnDisable()
    {
        destroyRowTexture(ref mEvenRowTexture);
        destroyRowTexture(ref mOddRowTexture);
        mEvenRowStyle = null;
        mOddRowStyle = null;
        mUnsplitStatusStyle = null;
        mSplitStatusStyle = null;
        mDuplicateSplitStatusStyle = null;
        mHeaderRowStyle = null;
        mHeaderButtonStyle = null;
        mHeaderLabelStyle = null;
        mRowLabelStyle = null;
    }
    protected void OnSelectionChange()
    {
        if (mIgnoredSelectionObject != null)
        {
            if (Selection.activeObject == mIgnoredSelectionObject)
            {
                mIgnoredSelectionObject = null;
                return;
            }
            mIgnoredSelectionObject = null;
        }
        if (Selection.activeObject is SkeletonDataAsset skeletonDataAsset)
        {
            if (mSkeletonDataAsset != skeletonDataAsset)
            {
                mSkeletonDataAsset = skeletonDataAsset;
                clearScanResult();
            }
            Repaint();
        }
    }
    protected void OnGUI()
    {
        ensureStyles();
        float leftPanelWidth = Mathf.Clamp(position.width * 0.38f, 380.0f, 520.0f);
        float rightPanelWidth = Mathf.Max(300.0f, position.width - leftPanelWidth - 5.0f);
        EditorGUILayout.BeginHorizontal();
        drawLeftPanel(leftPanelWidth);
        GUILayout.Space(5.0f);
        drawRightPanel(rightPanelWidth);
        EditorGUILayout.EndHorizontal();
    }
    protected void drawLeftPanel(float panelWidth)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
        mWindowScrollPosition = EditorGUILayout.BeginScrollView(mWindowScrollPosition, GUILayout.Width(panelWidth), GUILayout.ExpandHeight(true));
        drawAssetArea();
        EditorGUILayout.Space(5.0f);
        drawScanArea();
        if (mScanResult == null)
        {
            EditorGUILayout.HelpBox("选择SkeletonDataAsset后点击“分析动画数据”。", UnityEditor.MessageType.Info);
        }
        else
        {
            EditorGUILayout.Space(5.0f);
            drawSummaryArea();
            EditorGUILayout.Space(5.0f);
            drawFilterArea();
            EditorGUILayout.Space(5.0f);
            drawGenerateArea();
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    protected void drawRightPanel(float panelWidth)
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (mScanResult == null)
        {
            EditorGUILayout.HelpBox("扫描完成后，动画列表会显示在右侧。", UnityEditor.MessageType.Info);
        }
        else
        {
            drawSplitStatusArea();
            EditorGUILayout.Space(5.0f);
            drawAnimationList(panelWidth);
        }
        EditorGUILayout.EndVertical();
    }
    protected void drawAssetArea()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("源Skeleton资源", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        var newAsset = (SkeletonDataAsset)EditorGUILayout.ObjectField("SkeletonDataAsset", mSkeletonDataAsset, typeof(SkeletonDataAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            mSkeletonDataAsset = newAsset;
            clearScanResult();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("使用当前选中资源", GUILayout.Width(150.0f)))
        {
            tryUseSelectedAsset();
            clearScanResult();
        }
        if (mSkeletonDataAsset != null && GUILayout.Button("在Project中定位", GUILayout.Width(150.0f)))
        {
            Selection.activeObject = mSkeletonDataAsset;
            EditorGUIUtility.PingObject(mSkeletonDataAsset);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    protected void drawScanArea()
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUI.BeginDisabledGroup(mSkeletonDataAsset == null);
        if (GUILayout.Button("分析动画数据", GUILayout.Height(30.0f)))
        {
            scanAnimations();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
    protected void drawSummaryArea()
    {
        long baseSkeletonSize = getBaseSkeletonEstimatedSize();
        long allAnimationBytes = 0;
        for (int i = 0; i < mScanResult.mAnimations.Count; ++i)
        {
            allAnimationBytes += mScanResult.mAnimations[i].mLength;
        }
        int requiredAnimationCount = getRequiredAnimationCount();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("扫描结果", EditorStyles.boldLabel);
        drawInfoLine("Spine版本", mScanResult.mVersion);
        drawInfoLine("SkeletonDataAsset", mSourceSkeletonDataAssetPath);
        drawInfoLine("源文件", mSourceAssetPath);
        drawInfoLine("源文件大小", getMemoryText(mSourceBytes.LongLength));
        drawInfoLine("基础Skeleton数据", getMemoryText(mScanResult.mAnimationCountPosition));
        drawInfoLine("全部动画数据", getMemoryText(allAnimationBytes));
        drawInfoLine("动画总数", mAnimationItems.Count.ToString());
        if (requiredAnimationCount > 0)
        {
            drawInfoLine("基础Skeleton必须保留动画", requiredAnimationCount.ToString());
        }
        drawInfoLine("基础Skeleton预计大小", getMemoryText(baseSkeletonSize));
        if (requiredAnimationCount == 0)
        {
            EditorGUILayout.HelpBox("运行时基础Skeleton不包含动画。Binary源会重写动画数量；JSON源会移除animations节点；全部动画分别保存为独立.bytes文件。", UnityEditor.MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("当前Spine文件包含Slider Constraint。Slider在Skeleton加载阶段必须引用动画,因此基础Skeleton会自动保留它依赖的" + requiredAnimationCount + "个动画并重映射索引,其余动画仍拆为独立.bytes文件。", UnityEditor.MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }
    protected int getRequiredAnimationCount()
    {
        if (mScanResult == null || mScanResult.mRequiredAnimationIndices == null || mScanResult.mRequiredAnimationIndices.Length == 0)
        {
            return 0;
        }
        HashSet<int> indices = new HashSet<int>();
        for (int i = 0; i < mScanResult.mRequiredAnimationIndices.Length; ++i)
        {
            indices.Add(mScanResult.mRequiredAnimationIndices[i]);
        }
        return indices.Count;
    }
    protected long getBaseSkeletonEstimatedSize()
    {
        if (mScanResult == null)
        {
            return 0;
        }
        if (mScanResult.mSourceFormat == SpineSourceDataFormat.Json)
        {
            return mScanResult.mAnimationCountPosition;
        }
        List<int> requiredIndices = new List<int>();
        HashSet<int> unique = new HashSet<int>();
        if (mScanResult.mRequiredAnimationIndices != null)
        {
            for (int i = 0; i < mScanResult.mRequiredAnimationIndices.Length; ++i)
            {
                int index = mScanResult.mRequiredAnimationIndices[i];
                if (unique.Add(index)) requiredIndices.Add(index);
            }
        }
        requiredIndices.Sort();
        Dictionary<int, int> remap = new Dictionary<int, int>();
        long size = mScanResult.mAnimationCountPosition + getPositiveVarIntSize(requiredIndices.Count);
        for (int i = 0; i < requiredIndices.Count; ++i)
        {
            int index = requiredIndices[i];
            remap.Add(index, i);
            size += mScanResult.mAnimations[index].mLength;
        }
        if (mScanResult.mRequiredAnimationIndices != null)
        {
            for (int i = 0; i < mScanResult.mRequiredAnimationIndices.Length; ++i)
            {
                size += getPositiveVarIntSize(remap[mScanResult.mRequiredAnimationIndices[i]]);
            }
        }
        return size;
    }
    protected void drawSplitStatusArea()
    {
        int splitAnimationCount = getSplitAnimationCount();
        int duplicateSplitAnimationCount = getDuplicateSplitAnimationCount();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("分割状态", EditorStyles.boldLabel, GUILayout.Width(65.0f));
        GUILayout.Label("匹配文件:" + mMatchedSplitFileCount, EditorStyles.miniLabel, GUILayout.Width(90.0f));
        GUILayout.Label("已分割:" + splitAnimationCount, EditorStyles.miniLabel, GUILayout.Width(80.0f));
        GUILayout.Label("未分割:" + (mAnimationItems.Count - splitAnimationCount), EditorStyles.miniLabel, GUILayout.Width(80.0f));
        GUILayout.Label("重复:" + duplicateSplitAnimationCount, EditorStyles.miniLabel, GUILayout.Width(70.0f));
        if (mInvalidSplitFileCount > 0)
        {
            GUILayout.Label("读取失败:" + mInvalidSplitFileCount, EditorStyles.miniLabel, GUILayout.Width(85.0f));
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("刷新分割状态", GUILayout.Width(110.0f), GUILayout.Height(22.0f)))
        {
            refreshSplitStatus();
        }
        EditorGUILayout.EndHorizontal();
        string text = "开启后,右侧列表中不会显示已经生成独立单动画文件的动画。";
        mHideSplitAnimations = EditorGUILayout.ToggleLeft(new GUIContent("隐藏已分割动画", text), mHideSplitAnimations);
        EditorGUILayout.EndVertical();
    }
    protected void drawInfoLine(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(190.0f));
        EditorGUILayout.SelectableLabel(value, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.EndHorizontal();
    }
    protected void drawFilterArea()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("动画筛选", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索", GUILayout.Width(45.0f));
        mSearchText = EditorGUILayout.TextField(mSearchText);
        if (GUILayout.Button("清空", GUILayout.Width(60.0f)))
        {
            mSearchText = string.Empty;
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    protected void drawAnimationList(float panelWidth)
    {
        if (mAnimationSortType == AnimationSortType.None)
        {
            mAnimationItems.Sort(compareAnimationItem);
        }
        else
        {
            mAnimationItems.Sort(compareDisplayAnimationItem);
        }
        panelWidth -= ANIMATION_INDEX_COLUMN_WIDTH;
        panelWidth -= ANIMATION_STATUS_COLUMN_WIDTH;
        panelWidth -= ANIMATION_SIZE_COLUMN_WIDTH;
        panelWidth -= ANIMATION_SCROLLBAR_WIDTH;
        panelWidth -= ANIMATION_LIST_PADDING_WIDTH;
        float nameColumnWidth = Mathf.Max(120.0f, panelWidth);
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        EditorGUILayout.BeginHorizontal(mHeaderRowStyle);
        GUILayout.Label("序号", mHeaderLabelStyle, GUILayout.Width(ANIMATION_INDEX_COLUMN_WIDTH));
        if (GUILayout.Button(getSortHeaderText("动画名称", AnimationSortType.Name), mHeaderButtonStyle, GUILayout.Width(nameColumnWidth)))
        {
            changeAnimationSort(AnimationSortType.Name);
        }
        if (GUILayout.Button(getSortHeaderText("分割状态", AnimationSortType.SplitStatus), mHeaderButtonStyle, GUILayout.Width(ANIMATION_STATUS_COLUMN_WIDTH)))
        {
            changeAnimationSort(AnimationSortType.SplitStatus);
        }
        if (GUILayout.Button(getSortHeaderText("原始二进制大小", AnimationSortType.Size), mHeaderButtonStyle, GUILayout.Width(ANIMATION_SIZE_COLUMN_WIDTH)))
        {
            changeAnimationSort(AnimationSortType.Size);
        }
        GUILayout.Space(ANIMATION_SCROLLBAR_WIDTH);
        EditorGUILayout.EndHorizontal();
        mAnimationScrollPosition = EditorGUILayout.BeginScrollView(mAnimationScrollPosition, false, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        int visibleRowIndex = 0;
        for (int i = 0; i < mAnimationItems.Count; ++i)
        {
            AnimationItem item = mAnimationItems[i];
            if (!isItemVisible(item))
            {
                continue;
            }
            EditorGUILayout.BeginHorizontal(visibleRowIndex % 2 == 0 ? mEvenRowStyle : mOddRowStyle);
            GUILayout.Label((item.mRange.mIndex + 1).ToString(), mRowLabelStyle, GUILayout.Width(ANIMATION_INDEX_COLUMN_WIDTH));
            GUILayout.Label(item.mRange.mName, mRowLabelStyle, GUILayout.Width(nameColumnWidth));
            drawSplitStatusLabel(item);
            GUILayout.Label(getMemoryText(item.mRange.mLength), mRowLabelStyle, GUILayout.Width(ANIMATION_SIZE_COLUMN_WIDTH));
            EditorGUILayout.EndHorizontal();
            ++visibleRowIndex;
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    protected string getSortHeaderText(string title, AnimationSortType sortType)
    {
        if (mAnimationSortType != sortType)
        {
            return title;
        }
        return title + (mAnimationSortAscending ? " ▲" : " ▼");
    }
    protected void changeAnimationSort(AnimationSortType sortType)
    {
        if (mAnimationSortType != sortType)
        {
            mAnimationSortType = sortType;
            mAnimationSortAscending = false;
        }
        else if (!mAnimationSortAscending)
        {
            mAnimationSortAscending = true;
        }
        else
        {
            mAnimationSortType = AnimationSortType.None;
            mAnimationSortAscending = false;
        }
        mAnimationScrollPosition.y = 0.0f;
        Repaint();
    }
    protected void drawSplitStatusLabel(AnimationItem item)
    {
        string statusText = getSplitStatusText(item);
        string tooltip = getSplitStatusTooltip(item);
        GUIStyle style;
        if (item.mSplitFiles.Count == 0)
        {
            style = mUnsplitStatusStyle;
        }
        else if (item.mSplitFiles.Count == 1)
        {
            style = mSplitStatusStyle;
        }
        else
        {
            style = mDuplicateSplitStatusStyle;
        }
        Color oldGUIColor = GUI.color;
        Color oldContentColor = GUI.contentColor;
        bool oldGUIEnabled = GUI.enabled;
        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.enabled = true;
        GUILayout.Label(new GUIContent(statusText, tooltip), style, GUILayout.Width(ANIMATION_STATUS_COLUMN_WIDTH));
        GUI.enabled = oldGUIEnabled;
        GUI.contentColor = oldContentColor;
        GUI.color = oldGUIColor;
    }
    protected void drawGenerateArea()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("生成设置", EditorStyles.boldLabel);
        mVerifyAfterGenerate = EditorGUILayout.Toggle("生成后验证文件", mVerifyAfterGenerate);
        EditorGUILayout.Space(6.0f);
        if (GUILayout.Button("生成基础Skeleton并一键分割全部动画", GUILayout.Height(40.0f)))
        {
            generateAllSingleAnimationFiles();
        }
        EditorGUILayout.EndVertical();
    }
    protected void ensureStyles()
    {
        if (mEvenRowStyle != null &&
            mOddRowStyle != null &&
            mUnsplitStatusStyle != null &&
            mSplitStatusStyle != null &&
            mDuplicateSplitStatusStyle != null &&
            mHeaderRowStyle != null &&
            mHeaderButtonStyle != null &&
            mHeaderLabelStyle != null &&
            mRowLabelStyle != null)
        {
            return;
        }
        initRowStyles();
    }
    protected void initRowStyles()
    {
        destroyRowTexture(ref mEvenRowTexture);
        destroyRowTexture(ref mOddRowTexture);
        mEvenRowTexture = createColorTexture(EditorGUIUtility.isProSkin ? new Color(0.235f, 0.235f, 0.235f, 1.0f) : new Color(0.88f, 0.88f, 0.88f, 1.0f));
        mOddRowTexture = createColorTexture(EditorGUIUtility.isProSkin ? new Color(0.195f, 0.195f, 0.195f, 1.0f) : new Color(0.94f, 0.94f, 0.94f, 1.0f));
        mEvenRowStyle = createRowStyle(mEvenRowTexture);
        mOddRowStyle = createRowStyle(mOddRowTexture);
        mUnsplitStatusStyle = createStatusStyle(EditorGUIUtility.isProSkin ? new Color(0.82f, 0.82f, 0.82f, 1.0f) : new Color(0.30f, 0.30f, 0.30f, 1.0f));
        mSplitStatusStyle = createStatusStyle(EditorGUIUtility.isProSkin ? new Color(0.40f, 0.85f, 0.40f, 1.0f) : new Color(0.0f, 0.45f, 0.0f, 1.0f));
        mDuplicateSplitStatusStyle = createStatusStyle(EditorGUIUtility.isProSkin ? new Color(1.0f, 0.55f, 0.35f, 1.0f) : new Color(0.75f, 0.15f, 0.05f, 1.0f));
        mHeaderRowStyle = new GUIStyle(EditorStyles.toolbar);
        mHeaderRowStyle.margin = new RectOffset(0, 0, 0, 0);
        mHeaderRowStyle.padding = new RectOffset(0, 0, 0, 0);
        mHeaderButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
        mHeaderButtonStyle.margin = new RectOffset(0, 0, 0, 0);
        mHeaderButtonStyle.padding = new RectOffset(4, 4, 0, 0);
        mHeaderLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
        mHeaderLabelStyle.margin = new RectOffset(0, 0, 0, 0);
        mHeaderLabelStyle.padding = new RectOffset(4, 4, 0, 0);
        mHeaderLabelStyle.alignment = TextAnchor.MiddleLeft;
        mRowLabelStyle = new GUIStyle(EditorStyles.miniLabel);
        mRowLabelStyle.margin = new RectOffset(0, 0, 0, 0);
        mRowLabelStyle.padding = new RectOffset(4, 4, 0, 0);
        mRowLabelStyle.alignment = TextAnchor.MiddleLeft;
    }
    protected Texture2D createColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply(false, true);
        return texture;
    }
    protected GUIStyle createRowStyle(Texture2D background)
    {
        GUIStyle style = new GUIStyle();
        style.normal.background = background;
        style.hover.background = background;
        style.active.background = background;
        style.focused.background = background;
        style.onNormal.background = background;
        style.onHover.background = background;
        style.onActive.background = background;
        style.onFocused.background = background;
        style.fixedHeight = EditorGUIUtility.singleLineHeight + 6.0f;
        style.margin = new RectOffset(0, 0, 0, 0);
        style.padding = new RectOffset(0, 0, 3, 3);
        style.stretchWidth = true;
        style.stretchHeight = false;
        return style;
    }
    protected GUIStyle createStatusStyle(Color color)
    {
        GUIStyle sourceStyle = GUI.skin != null && GUI.skin.label != null ? GUI.skin.label : EditorStyles.label;
        GUIStyle style = new GUIStyle(sourceStyle);
        style.fontSize = EditorStyles.miniLabel.fontSize;
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
        style.clipping = TextClipping.Clip;
        style.margin = new RectOffset(0, 0, 0, 0);
        style.padding = new RectOffset(4, 4, 0, 0);
        return style;
    }
    protected void destroyRowTexture(ref Texture2D texture)
    {
        if (texture != null)
        {
            DestroyImmediate(texture);
            texture = null;
        }
    }
    protected void tryUseSelectedAsset()
    {
        if (Selection.activeObject is SkeletonDataAsset skeletonDataAsset)
        {
            mSkeletonDataAsset = skeletonDataAsset;
        }
    }
    protected void clearScanResult()
    {
        mSourceBytes = null;
        mScanResult = null;
        mSourceAssetPath = string.Empty;
        mSourceSkeletonDataAssetPath = string.Empty;
        mSourceSkeletonDataAssetName = string.Empty;
        mAnimationItems.Clear();
        mMatchedSplitFileCount = 0;
        mInvalidSplitFileCount = 0;
    }
    protected void scanAnimations()
    {
        if (mSkeletonDataAsset == null)
        {
            Debug.LogError("没有选择SkeletonDataAsset");
            return;
        }
        if (mSkeletonDataAsset.skeletonJSON == null)
        {
            Debug.LogError("SkeletonDataAsset没有设置skeletonJSON", mSkeletonDataAsset);
            return;
        }
        mSourceSkeletonDataAssetPath = GetAssetPath(mSkeletonDataAsset);
        if (string.IsNullOrEmpty(mSourceSkeletonDataAssetPath))
        {
            Debug.LogError("无法获取SkeletonDataAsset资源路径", mSkeletonDataAsset);
            return;
        }
        mSourceSkeletonDataAssetName = Path.GetFileNameWithoutExtension(mSourceSkeletonDataAssetPath);
        mSourceAssetPath = GetAssetPath(mSkeletonDataAsset.skeletonJSON);
        if (!isSourceSkeletonAssetPath(mSourceAssetPath))
        {
            Debug.LogError("当前Skeleton源文件不是本Runtime版本支持的可拆分资源:" + mSourceAssetPath, mSkeletonDataAsset.skeletonJSON);
            return;
        }
        try
        {
            EditorUtility.DisplayProgressBar("扫描Spine动画", "正在分析动画数据...", 0.2f);
            mSourceBytes = mSkeletonDataAsset.skeletonJSON.bytes;
			SpineBinaryScannerVersion scanner = new SpineBinaryScannerVersion();
            mScanResult = scanner.scan(mSourceBytes);
            mAnimationItems.Clear();
            for (int i = 0; i < mScanResult.mAnimations.Count; ++i)
            {
                AnimationItem item = new AnimationItem();
                item.mRange = mScanResult.mAnimations[i];
                if ("_" + item.mRange.mName == COMMON_SUFFIX)
                {
                    string info = "发现了有一个动画名为" + item.mRange.mName + ",这与动画公共数据重名了,会导致错误";
                    Debug.LogError(info);
                    EditorUtility.DisplayDialog("动画名字错误", info, "确定");
                }
                mAnimationItems.Add(item);
            }
            refreshSplitStatus();
            Debug.Log("Spine动画扫描完成" + "\n文件:" + mSourceAssetPath +
                "\n版本:" + mScanResult.mVersion +
                "\n动画数量:" + mAnimationItems.Count +
                "\n公共数据:" + getMemoryText(mScanResult.mAnimationCountPosition) +
                "\n动画数据:" + getMemoryText(getTotalAnimationDataSize()), mSkeletonDataAsset);
            Repaint();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            clearScanResult();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    protected long getTotalAnimationDataSize()
    {
        if (mScanResult == null) return 0L;
        long total = 0L;
        for (int i = 0; i < mScanResult.mAnimations.Count; ++i) total += mScanResult.mAnimations[i].mLength;
        return total;
    }
    protected bool isItemVisible(AnimationItem item)
    {
        if (mHideSplitAnimations && item.mSplitFiles.Count > 0)
        {
            return false;
        }
        if (string.IsNullOrEmpty(mSearchText))
        {
            return true;
        }
        return item.mRange.mName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }
    protected void refreshSplitStatus()
    {
        if (mScanResult == null || mAnimationItems.Count == 0)
        {
            return;
        }
        for (int i = 0; i < mAnimationItems.Count; ++i)
        {
            mAnimationItems[i].mSplitFiles.Clear();
        }
        mMatchedSplitFileCount = 0;
        mInvalidSplitFileCount = 0;
        Dictionary<string, AnimationItem> itemByName = new Dictionary<string, AnimationItem>(StringComparer.Ordinal);
        for (int i = 0; i < mAnimationItems.Count; ++i)
        {
            itemByName[mAnimationItems[i].mRange.mName] = mAnimationItems[i];
        }
        List<string> candidatePaths = new List<string>();
        string sourceDirectory = Path.GetDirectoryName(mSourceAssetPath).Replace('\\', '/');
        string skeletonResourceName = getSkeletonResourceName(mSourceSkeletonDataAssetName);
        string outputDirectoryAssetPath = combineAssetPath(sourceDirectory, getAnimationDirectoryName(skeletonResourceName));
        string outputDirectoryAbsolutePath = assetPathToAbsolutePath(outputDirectoryAssetPath);
        if (Directory.Exists(outputDirectoryAbsolutePath))
        {
            string[] splitFiles = Directory.GetFiles(outputDirectoryAbsolutePath, "*.bytes", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < splitFiles.Length; ++i)
            {
                candidatePaths.Add(combineAssetPath(outputDirectoryAssetPath, Path.GetFileName(splitFiles[i])));
            }
        }
        try
        {
            for (int i = 0; i < candidatePaths.Count; ++i)
            {
                string assetPath = candidatePaths[i];
                EditorUtility.DisplayProgressBar("刷新动画分割状态", assetPath, candidatePaths.Count == 0 ? 1.0f : (float)i / candidatePaths.Count);
                try
                {
                    collectSingleAnimationSplitStatus(assetPath, itemByName);
                }
                catch (Exception exception)
                {
                    ++mInvalidSplitFileCount;
                    Debug.LogWarning("读取Spine单动画文件失败:" + assetPath + "\n" + exception.Message);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        Repaint();
    }
    protected void collectSingleAnimationSplitStatus(string assetPath, Dictionary<string, AnimationItem> itemByName)
    {
        byte[] bytes = File.ReadAllBytes(assetPathToAbsolutePath(assetPath));
        if (!isAnimationFile(bytes))
        {
            return;
        }
        SpineSingleAnimationDataVersion animationData = readAnimationNoCopy(bytes);
        if (animationData.mSkeletonHash != mScanResult.mSkeletonHash ||
            !string.Equals(animationData.mSpineVersion, mScanResult.mVersion, StringComparison.Ordinal))
        {
            return;
        }
        if (!itemByName.TryGetValue(animationData.mAnimationName, out AnimationItem item))
        {
            return;
        }
        AnimationSplitFileInfo splitFile = new AnimationSplitFileInfo();
        splitFile.mDisplayName = "单动画:" + animationData.mAnimationName;
        splitFile.mAssetPath = assetPath;
        addSplitFile(item, splitFile);
        ++mMatchedSplitFileCount;
    }
    protected void addSplitFile(AnimationItem item, AnimationSplitFileInfo splitFile)
    {
        for (int i = 0; i < item.mSplitFiles.Count; ++i)
        {
            if (string.Equals(item.mSplitFiles[i].mAssetPath, splitFile.mAssetPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }
        item.mSplitFiles.Add(splitFile);
    }
    protected int getSplitAnimationCount()
    {
        int count = 0;
        for (int i = 0; i < mAnimationItems.Count; ++i)
        {
            if (mAnimationItems[i].mSplitFiles.Count > 0)
            {
                ++count;
            }
        }
        return count;
    }
    protected int getDuplicateSplitAnimationCount()
    {
        int count = 0;
        for (int i = 0; i < mAnimationItems.Count; ++i)
        {
            if (mAnimationItems[i].mSplitFiles.Count > 1)
            {
                ++count;
            }
        }
        return count;
    }
    protected string getSplitStatusText(AnimationItem item)
    {
        if (item.mSplitFiles.Count == 0)
        {
            return "未分割";
        }
        List<string> displayNames = new List<string>();
        for (int i = 0; i < item.mSplitFiles.Count; ++i)
        {
            displayNames.Add(item.mSplitFiles[i].mDisplayName);
        }
        return item.mSplitFiles.Count == 1 ? displayNames[0] : "重复:" + string.Join(" | ", displayNames);
    }
    protected string getSplitStatusTooltip(AnimationItem item)
    {
        if (item.mSplitFiles.Count == 0)
        {
            return "该动画尚未存在于任何已识别的分割文件中";
        }
        List<string> lines = new List<string>();
        for (int i = 0; i < item.mSplitFiles.Count; ++i)
        {
            lines.Add(item.mSplitFiles[i].mDisplayName + "\n" + item.mSplitFiles[i].mAssetPath);
        }
        return string.Join("\n\n", lines);
    }
    protected void generateAllSingleAnimationFiles()
    {
        if (mScanResult == null || mSourceBytes == null || string.IsNullOrEmpty(mSourceAssetPath))
        {
            Debug.LogError("请先扫描完整Skeleton动画数据");
            return;
        }
        string confirmMessage = "即将按当前Skeleton源文件重新生成基础Skeleton、公共数据和全部" + mAnimationItems.Count + "个单动画文件。" +
            "\n源SkeletonDataAsset:" + mSourceSkeletonDataAssetPath +
            "\n源文件:" + mSourceAssetPath +
            "\n\n动画新增会自动生成,动画删除或改名产生的旧文件会自动清理。";
        if (!EditorUtility.DisplayDialog("一键分割Spine动画", confirmMessage, "开始生成", "取消"))
        {
            return;
        }
        SpineAnimationSplitResult result = split(mSourceAssetPath, mVerifyAfterGenerate, true);
        if (!result.mSuccess)
        {
            return;
        }
        scanAnimations();
        UnityEngine.Object selectionObject = LoadAssetAtPath<SkeletonDataAsset>(result.mGeneratedSkeletonDataAssetPath);
        if (selectionObject == null)
        {
            selectionObject = LoadAssetAtPath<TextAsset>(result.mGeneratedSkeletonAssetPath);
        }
        if (selectionObject != null)
        {
            mIgnoredSelectionObject = selectionObject;
            Selection.activeObject = selectionObject;
            EditorGUIUtility.PingObject(selectionObject);
        }
    }
    protected int compareDisplayAnimationItem(AnimationItem left, AnimationItem right)
    {
        int result = 0;
        if (mAnimationSortType == AnimationSortType.Name)
        {
            result = string.Compare(left.mRange.mName, right.mRange.mName, StringComparison.OrdinalIgnoreCase);
        }
        else if (mAnimationSortType == AnimationSortType.SplitStatus)
        {
            result = compareSplitStatus(left, right);
        }
        else if (mAnimationSortType == AnimationSortType.Size)
        {
            result = left.mRange.mLength.CompareTo(right.mRange.mLength);
        }
        if (result != 0)
        {
            return mAnimationSortAscending ? result : -result;
        }
        return left.mRange.mIndex.CompareTo(right.mRange.mIndex);
    }
    protected int compareSplitStatus(AnimationItem left, AnimationItem right)
    {
        int leftStatus = left.mSplitFiles.Count == 0 ? 0 : left.mSplitFiles.Count == 1 ? 1 : 2;
        int rightStatus = right.mSplitFiles.Count == 0 ? 0 : right.mSplitFiles.Count == 1 ? 1 : 2;
        int result = leftStatus.CompareTo(rightStatus);
        if (result == 0)
        {
            result = string.Compare(getSplitStatusText(left), getSplitStatusText(right), StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }
    protected int compareAnimationItem(AnimationItem left, AnimationItem right)
    {
        return left.mRange.mIndex.CompareTo(right.mRange.mIndex);
    }
    protected int getPositiveVarIntSize(int value)
    {
        uint unsignedValue = unchecked((uint)value);
        int size = 1;
        while ((unsignedValue & ~0x7FU) != 0)
        {
            ++size;
            unsignedValue >>= 7;
        }
        return size;
    }
    protected string assetPathToAbsolutePath(string assetPath)
    {
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
    }
}
