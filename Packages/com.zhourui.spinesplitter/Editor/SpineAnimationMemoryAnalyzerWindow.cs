#if SPINE_RUNTIME_43 || SPINE_RUNTIME_42 || SPINE_RUNTIME_41 || SPINE_RUNTIME_40
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using SpineAnimation = Spine.Animation;
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

// Spine动画内存分析窗口。
// 用于统计SkeletonDataAsset中每个动画包含的Timeline、关键帧、Deform、
// DrawOrder、Event以及解析后的估算内存。
public class SpineAnimationMemoryAnalyzerWindow : EditorWindow
{
    protected const int OBJECT_HEADER_SIZE_64 = 16;
    protected const int ARRAY_HEADER_SIZE_64 = 24;
    protected const int STRING_HEADER_SIZE_64 = 24;
    protected const int POINTER_SIZE_64 = 8;
    protected const int MEMORY_ALIGN_SIZE = 8;
    protected SkeletonDataAsset mSkeletonDataAsset;
    protected readonly List<AnimationAnalyzeResult> mAnalyzeResults = new List<AnimationAnalyzeResult>();
    protected Vector2 mScrollPosition;
    protected string mSearchText = string.Empty;
    protected string mSkeletonName = string.Empty;
    protected string mSkeletonVersion = string.Empty;
    protected string mSkeletonFilePath = string.Empty;
    protected long mSkeletonFileSize;
    protected long mTotalEstimatedAnimationBytes;
    protected int mTotalAnimationCount;
    protected int mTotalTimelineCount;
    protected int mTotalFrameCount;
    protected long mTotalDeformFloatCount;
    [MenuItem("Tools/Spine/动画内存分析")]
    protected static void openWindow()
    {
        var window = GetWindow<SpineAnimationMemoryAnalyzerWindow>("Spine动画内存分析");
        window.minSize = new Vector2(1100.0f, 600.0f);
        window.tryUseSelectedAsset();
        window.Show();
    }
    protected void OnEnable()
    {
        tryUseSelectedAsset();
    }
    protected void OnSelectionChange()
    {
        if (Selection.activeObject is SkeletonDataAsset skeletonDataAsset)
        {
            mSkeletonDataAsset = skeletonDataAsset;
            Repaint();
        }
    }
    protected void OnGUI()
    {
        drawAssetArea();
        EditorGUILayout.Space(6.0f);
        drawOperationArea();
        EditorGUILayout.Space(6.0f);
        if (mAnalyzeResults.Count == 0)
        {
            EditorGUILayout.HelpBox("请选择一个SkeletonDataAsset，然后点击“分析动画数据”。",UnityEditor.MessageType.Info);
            return;
        }
        drawSummaryArea();
        EditorGUILayout.Space(6.0f);
        drawSearchArea();
        EditorGUILayout.Space(4.0f);
        drawResultTable();
    }
    protected void drawAssetArea()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("分析对象", EditorStyles.boldLabel);
        mSkeletonDataAsset = (SkeletonDataAsset)EditorGUILayout.ObjectField("SkeletonDataAsset", mSkeletonDataAsset, typeof(SkeletonDataAsset), false);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("使用当前选中资源", GUILayout.Width(150.0f)))
        {
            tryUseSelectedAsset();
        }
        if (mSkeletonDataAsset != null && GUILayout.Button("在Project中定位", GUILayout.Width(150.0f)))
        {
            Selection.activeObject = mSkeletonDataAsset;
            EditorGUIUtility.PingObject(mSkeletonDataAsset);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    protected void drawOperationArea()
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUI.BeginDisabledGroup(mSkeletonDataAsset == null);
        if (GUILayout.Button("分析动画数据", GUILayout.Height(30.0f)))
        {
            analyzeSkeletonData();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(mAnalyzeResults.Count == 0);
        if (GUILayout.Button("导出TXT", GUILayout.Width(120.0f), GUILayout.Height(30.0f)))
        {
            exportTxt();
        }
        if (GUILayout.Button("导出CSV", GUILayout.Width(120.0f), GUILayout.Height(30.0f)))
        {
            exportCsv();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
    protected void drawSummaryArea()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("汇总", EditorStyles.boldLabel);
        drawSummaryLine("骨架名称", mSkeletonName);
        drawSummaryLine("Spine版本", string.IsNullOrEmpty(mSkeletonVersion) ? "未知" : mSkeletonVersion);
        drawSummaryLine("源文件", string.IsNullOrEmpty(mSkeletonFilePath) ? "未知" : mSkeletonFilePath);
        drawSummaryLine("源文件大小", getMemoryText(mSkeletonFileSize));
        drawSummaryLine("动画数量", mTotalAnimationCount.ToString());
        drawSummaryLine("Timeline数量", mTotalTimelineCount.ToString());
        drawSummaryLine("关键帧总数", mTotalFrameCount.ToString());
        drawSummaryLine("Deform浮点数", mTotalDeformFloatCount.ToString("N0"));
        drawSummaryLine("动画解析内存估算", getMemoryText(mTotalEstimatedAnimationBytes));

        float percent = mSkeletonFileSize > 0 ? mTotalEstimatedAnimationBytes * 100.0f / mSkeletonFileSize : 0.0f;
        drawSummaryLine("估算内存 / skel文件", mSkeletonFileSize > 0 ? percent.ToString("F2") + "%" : "未知");
        EditorGUILayout.Space(4.0f);
        EditorGUILayout.HelpBox(
            "“动画解析内存估算”只统计Animation、Timeline及其帧数组、曲线、" +
            "Deform、DrawOrder和Event等数据，不包含纹理、材质、Atlas、骨骼、" +
            "Slot、Skin和Attachment。\n" +
            "该数值用于比较动画之间的相对大小，不等同于Memory Profiler中的精确内存。",
            UnityEditor.MessageType.Warning);
        EditorGUILayout.EndVertical();
    }
    protected void drawSummaryLine(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(180.0f));
        EditorGUILayout.SelectableLabel(value, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        EditorGUILayout.EndHorizontal();
    }
    protected void drawSearchArea()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("搜索动画", GUILayout.Width(70.0f));
        mSearchText = EditorGUILayout.TextField(mSearchText);

        if (GUILayout.Button("清空", GUILayout.Width(70.0f)))
        {
            mSearchText = string.Empty;
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
    }
    protected void drawResultTable()
    {
        EditorGUILayout.BeginVertical("box");
        drawTableHeader();
        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);
        for (int i = 0; i < mAnalyzeResults.Count; ++i)
        {
            AnimationAnalyzeResult result = mAnalyzeResults[i];
            if (!string.IsNullOrEmpty(mSearchText) && result.mAnimationName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }
            drawTableRow(result, i);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    protected void drawTableHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        drawHeaderLabel("排名", 45.0f);
        drawHeaderLabel("动画名称", 260.0f);
        drawHeaderLabel("时长", 65.0f);
        drawHeaderLabel("Timeline", 70.0f);
        drawHeaderLabel("关键帧", 75.0f);
        drawHeaderLabel("float数组", 75.0f);
        drawHeaderLabel("float数量", 90.0f);
        drawHeaderLabel("Deform帧", 80.0f);
        drawHeaderLabel("Deform float", 100.0f);
        drawHeaderLabel("DrawOrder帧", 95.0f);
        drawHeaderLabel("Event帧", 75.0f);
        drawHeaderLabel("估算内存", 100.0f);

        EditorGUILayout.EndHorizontal();
    }
    protected void drawHeaderLabel(string text, float width)
    {
        GUILayout.Label(text, EditorStyles.miniBoldLabel, GUILayout.Width(width));
    }
    protected void drawTableRow(AnimationAnalyzeResult result, int index)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight + 6.0f));
        
        drawRowLabel((index + 1).ToString(), 45.0f);
        drawRowLabel(result.mAnimationName, 260.0f);
        drawRowLabel(result.mDuration.ToString("F3"), 65.0f);
        drawRowLabel(result.mTimelineCount.ToString(), 70.0f);
        drawRowLabel(result.mFrameCount.ToString(), 75.0f);
        drawRowLabel(result.mFloatArrayCount.ToString(), 75.0f);
        drawRowLabel(result.mFloatElementCount.ToString("N0"), 90.0f);
        drawRowLabel(result.mDeformFrameCount.ToString(), 80.0f);
        drawRowLabel(result.mDeformFloatCount.ToString("N0"), 100.0f);
        drawRowLabel(result.mDrawOrderFrameCount.ToString(), 95.0f);
        drawRowLabel(result.mEventFrameCount.ToString(), 75.0f);
        drawRowLabel(getMemoryText(result.mEstimatedBytes), 100.0f);

        EditorGUILayout.EndHorizontal();
    }
    protected void drawRowLabel(string text, float width)
    {
        GUILayout.Label(text, EditorStyles.miniLabel, GUILayout.Width(width));
    }
    protected void tryUseSelectedAsset()
    {
        if (Selection.activeObject is SkeletonDataAsset skeletonDataAsset)
        {
            mSkeletonDataAsset = skeletonDataAsset;
        }
    }
    protected void analyzeSkeletonData()
    {
        if (mSkeletonDataAsset == null)
        {
            Debug.LogError("没有选择SkeletonDataAsset");
            return;
        }

        try
        {
            EditorUtility.DisplayProgressBar("Spine动画内存分析", "正在读取SkeletonData...", 0.0f);
            SkeletonData skeletonData = getSkeletonData(mSkeletonDataAsset);
            if (skeletonData == null)
            {
                Debug.LogError("无法从SkeletonDataAsset中获取SkeletonData:" + GetAssetPath(mSkeletonDataAsset));
                return;
            }

            clearAnalyzeResult();
            mSkeletonName = getStringMemberValue(skeletonData, "Name");
            mSkeletonVersion = getStringMemberValue(skeletonData, "Version");
            findSkeletonSourceFile();
            ExposedList<SpineAnimation> animations = skeletonData.Animations;
            if (animations == null || animations.Count == 0)
            {
                Debug.LogError("当前SkeletonData中没有动画");
                return;
            }
            mTotalAnimationCount = animations.Count;
            for (int i = 0; i < animations.Count; ++i)
            {
                EditorUtility.DisplayProgressBar("Spine动画内存分析", "正在分析动画:" + animations.Items[i].Name, (float)i / animations.Count);
                AnimationAnalyzeResult result = analyzeAnimation(animations.Items[i]);
                mAnalyzeResults.Add(result);
                mTotalEstimatedAnimationBytes += result.mEstimatedBytes;
                mTotalTimelineCount += result.mTimelineCount;
                mTotalFrameCount += result.mFrameCount;
                mTotalDeformFloatCount += result.mDeformFloatCount;
            }
            mAnalyzeResults.Sort((left, right) => right.mEstimatedBytes.CompareTo(left.mEstimatedBytes));
            printAnalyzeSummary();
            Repaint();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    protected AnimationAnalyzeResult analyzeAnimation(SpineAnimation animation)
    {
        AnimationAnalyzeResult result = new AnimationAnalyzeResult
        {
            mAnimationName = animation.Name,
            mDuration = animation.Duration
        };

        HashSet<object> animationVisitedObjects = new HashSet<object>(ReferenceEqualityComparer.mInstance);
        result.mEstimatedBytes += estimateShallowObject(animation);
        addStringMemory(animation.Name, result, animationVisitedObjects);
        ExposedList<Timeline> timelines = animation.Timelines;
        if (timelines == null)
        {
            return result;
        }
        result.mTimelineCount = timelines.Count;
        if (animationVisitedObjects.Add(timelines))
        {
            result.mEstimatedBytes += estimateShallowObject(timelines);
        }
        if (timelines.Items != null && animationVisitedObjects.Add(timelines.Items))
        {
            result.mEstimatedBytes += estimateArrayShallow(timelines.Items);
        }
        HashSet<string> uniquePropertyIDs = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < timelines.Count; ++i)
        {
            object timeline = timelines.Items[i];
            if (timeline == null)
            {
                continue;
            }

            TimelineAnalyzeResult timelineResult = analyzeTimeline(timeline);
            result.mEstimatedBytes += timelineResult.mEstimatedBytes;
            result.mFrameCount += timelineResult.mFrameCount;
            result.mFloatArrayCount += timelineResult.mFloatArrayCount;
            result.mFloatElementCount += timelineResult.mFloatElementCount;
            result.mIntArrayCount += timelineResult.mIntArrayCount;
            result.mIntElementCount += timelineResult.mIntElementCount;
            string timelineTypeName = timeline.GetType().Name;
            if (timelineTypeName.IndexOf("Deform", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.mDeformTimelineCount++;
                result.mDeformFrameCount += timelineResult.mFrameCount;
                result.mDeformFloatCount += countPrimitiveArrayElements(getFieldValue(timeline, "vertices"), typeof(float));
            }
            if (timelineTypeName.IndexOf("DrawOrder", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.mDrawOrderTimelineCount++;
                result.mDrawOrderFrameCount += timelineResult.mFrameCount;
                result.mDrawOrderIntCount += countPrimitiveArrayElements(getFieldValue(timeline, "drawOrders"), typeof(int));
            }
            if (timelineTypeName.IndexOf("EventTimeline", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result.mEventTimelineCount++;
                result.mEventFrameCount += getArrayLength(getFieldValue(timeline, "events"));
            }
            collectPropertyIDs(timeline, uniquePropertyIDs);
            TimelineTypeAnalyzeResult typeResult;
            if (!result.mTimelineTypes.TryGetValue(timelineTypeName, out typeResult))
            {
                typeResult = new TimelineTypeAnalyzeResult
                {
                    mTypeName = timelineTypeName
                };
                result.mTimelineTypes.Add(timelineTypeName, typeResult);
            }
            typeResult.mTimelineCount++;
            typeResult.mFrameCount += timelineResult.mFrameCount;
            typeResult.mEstimatedBytes += timelineResult.mEstimatedBytes;
            typeResult.mFloatElementCount += timelineResult.mFloatElementCount;
            typeResult.mIntElementCount += timelineResult.mIntElementCount;
        }

        // Animation内部还有一个HashSet保存Timeline属性ID。
        // 不重复计算字符串本身，只估算HashSet的桶和Entry空间。
        result.mEstimatedBytes += estimateHashSetStorage(uniquePropertyIDs.Count);
        return result;
    }
    protected TimelineAnalyzeResult analyzeTimeline(object timeline)
    {
        TimelineAnalyzeResult result = new TimelineAnalyzeResult
        {
            mFrameCount = getTimelineFrameCount(timeline)
        };

        HashSet<object> visitedObjects = new HashSet<object>(ReferenceEqualityComparer.mInstance);
        visitedObjects.Add(timeline);
        result.mEstimatedBytes += estimateShallowObject(timeline);
        List<FieldInfo> fields = getAllInstanceFields(timeline.GetType());
        for (int i = 0; i < fields.Count; ++i)
        {
            FieldInfo field = fields[i];
            object value;
            try
            {
                value = field.GetValue(timeline);
            }
            catch
            {
                continue;
            }
            scanReferencedValue(value, result, visitedObjects);
        }
        return result;
    }
    protected void scanReferencedValue(object value, TimelineAnalyzeResult result, HashSet<object> visitedObjects)
    {
        if (value == null)
        {
            return;
        }

        Type type = value.GetType();
        if (type.IsValueType)
        {
            return;
        }
        if (value is string stringValue)
        {
            if (visitedObjects.Add(stringValue))
            {
                result.mEstimatedBytes += estimateString(stringValue);
                result.mStringCount++;
            }
            return;
        }

        if (value is Array array)
        {
            scanArray(array, result, visitedObjects);
            return;
        }
        if (isSpineEventObject(type))
        {
            scanSimpleOwnedObject(value, result, visitedObjects);
            return;
        }
        if (isExposedList(type))
        {
            scanExposedList(value, result, visitedObjects);
        }
        // Attachment、BoneData、SlotData、EventData等属于SkeletonData共享对象，
        // 不递归统计，避免把基础骨架数据错误算到单个动画中。
    }
    protected void scanArray(Array array, TimelineAnalyzeResult result, HashSet<object> visitedObjects)
    {
        if (array == null || !visitedObjects.Add(array))
        {
            return;
        }
        result.mEstimatedBytes += estimateArrayShallow(array);
        Type elementType = array.GetType().GetElementType();
        if (elementType == null)
        {
            return;
        }
        if (elementType == typeof(float))
        {
            result.mFloatArrayCount++;
            result.mFloatElementCount += array.LongLength;
            return;
        }
        if (elementType == typeof(int))
        {
            result.mIntArrayCount++;
            result.mIntElementCount += array.LongLength;
            return;
        }
        if (elementType == typeof(short) ||
            elementType == typeof(ushort) ||
            elementType == typeof(byte) ||
            elementType == typeof(sbyte) ||
            elementType == typeof(long) ||
            elementType == typeof(ulong) ||
            elementType == typeof(double) ||
            elementType == typeof(bool) ||
            elementType == typeof(char))
        {
            return;
        }

        if (elementType == typeof(string))
        {
            result.mStringArrayCount++;
            foreach (object item in array)
            {
                if (item is string stringItem && visitedObjects.Add(stringItem))
                {
                    result.mEstimatedBytes += estimateString(stringItem);
                    result.mStringCount++;
                }
            }
            return;
        }
        if (elementType.IsValueType)
        {
            return;
        }
        foreach (object item in array)
        {
            if (item == null)
            {
                continue;
            }

            if (item is Array nestedArray)
            {
                scanArray(nestedArray, result, visitedObjects);
            }
            else if (item is string stringItem)
            {
                if (visitedObjects.Add(stringItem))
                {
                    result.mEstimatedBytes += estimateString(stringItem);
                    result.mStringCount++;
                }
            }
            else if (isSpineEventObject(item.GetType()))
            {
                scanSimpleOwnedObject(item, result, visitedObjects);
            }
        }
    }
    protected void scanSimpleOwnedObject(object target, TimelineAnalyzeResult result, HashSet<object> visitedObjects)
    {
        if (target == null || !visitedObjects.Add(target))
        {
            return;
        }

        result.mEstimatedBytes += estimateShallowObject(target);
        List<FieldInfo> fields = getAllInstanceFields(target.GetType());
        for (int i = 0; i < fields.Count; ++i)
        {
            FieldInfo field = fields[i];
            object value;
            try
            {
                value = field.GetValue(target);
            }
            catch
            {
                continue;
            }

            if (value is string stringValue && visitedObjects.Add(stringValue))
            {
                result.mEstimatedBytes += estimateString(stringValue);
                result.mStringCount++;
            }
            else if (value is Array array)
            {
                scanArray(array, result, visitedObjects);
            }
        }
    }
    protected void scanExposedList(object exposedList, TimelineAnalyzeResult result, HashSet<object> visitedObjects)
    {
        if (exposedList == null || !visitedObjects.Add(exposedList))
        {
            return;
        }
        result.mEstimatedBytes += estimateShallowObject(exposedList);
        object items = getFieldValue(exposedList, "Items") ?? getFieldValue(exposedList, "items");
        if (items is Array array)
        {
            scanArray(array, result, visitedObjects);
        }
    }
    protected SkeletonData getSkeletonData(SkeletonDataAsset skeletonDataAsset)
    {
        MethodInfo[] methods = skeletonDataAsset.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        // 优先调用GetSkeletonData(bool quiet)。
        for (int i = 0; i < methods.Length; ++i)
        {
            MethodInfo method = methods[i];
            if (method.Name != "GetSkeletonData")
            {
                continue;
            }
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(bool))
            {
                return method.Invoke(skeletonDataAsset, new object[] { false }) as SkeletonData;
            }
        }

        // 兼容部分旧版本的无参数接口。
        for (int i = 0; i < methods.Length; ++i)
        {
            MethodInfo method = methods[i];
            if (method.Name == "GetSkeletonData" && method.GetParameters().Length == 0)
            {
                return method.Invoke(skeletonDataAsset, null) as SkeletonData;
            }
        }
        return null;
    }
    protected int getTimelineFrameCount(object timeline)
    {
        if (timeline == null)
        {
            return 0;
        }
        object frameCountValue = getPropertyValue(timeline, "FrameCount");
        if (frameCountValue is int frameCount)
        {
            return frameCount;
        }
        Array frames = getPropertyValue(timeline, "Frames") as Array;
        if (frames == null)
        {
            frames = getFieldValue(timeline, "frames") as Array;
        }
        if (frames == null)
        {
            return 0;
        }
        int frameEntries = 1;
        object frameEntriesValue = getPropertyValue(timeline, "FrameEntries");
        if (frameEntriesValue is int entries && entries > 0)
        {
            frameEntries = entries;
        }
        else
        {
            FieldInfo entriesField = findFieldInHierarchy(timeline.GetType(), "ENTRIES", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (entriesField != null)
            {
                try
                {
                    object value = entriesField.GetValue(null);
                    if (value is int staticEntries && staticEntries > 0)
                    {
                        frameEntries = staticEntries;
                    }
                }
                catch
                {
                    frameEntries = 1;
                }
            }
        }
        return frameEntries > 0 ? frames.Length / frameEntries : frames.Length;
    }
    protected void collectPropertyIDs(object timeline, HashSet<string> result)
    {
        object propertyIDs = getPropertyValue(timeline, "PropertyIds");
        if (propertyIDs == null)
        {
            propertyIDs = getPropertyValue(timeline, "PropertyIDs");
        }
        if (propertyIDs == null)
        {
            propertyIDs = getFieldValue(timeline, "propertyIds");
        }
        if (!(propertyIDs is Array array))
        {
            return;
        }
        foreach (object item in array)
        {
            if (item is string propertyID && !string.IsNullOrEmpty(propertyID))
            {
                result.Add(propertyID);
            }
        }
    }
    protected long countPrimitiveArrayElements(object value, Type primitiveType)
    {
        if (!(value is Array array))
        {
            return 0;
        }
        Type elementType = array.GetType().GetElementType();
        if (elementType == primitiveType)
        {
            return array.LongLength;
        }
        long count = 0;
        foreach (object item in array)
        {
            if (item is Array nestedArray)
            {
                count += countPrimitiveArrayElements(nestedArray, primitiveType);
            }
        }
        return count;
    }
    protected int getArrayLength(object value)
    {
        return value is Array array ? array.Length : 0;
    }
    protected object getFieldValue(object target, string fieldName)
    {
        if (target == null)
        {
            return null;
        }
        FieldInfo field = findFieldInHierarchy(target.GetType(), fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null)
        {
            return null;
        }
        try
        {
            return field.GetValue(target);
        }
        catch
        {
            return null;
        }
    }
    protected object getPropertyValue(object target, string propertyName)
    {
        if (target == null)
        {
            return null;
        }
        Type type = target.GetType();
        while (type != null && type != typeof(object))
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | 
                                                     BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                try
                {
                    return property.GetValue(target, null);
                }
                catch
                {
                    return null;
                }
            }
            type = type.BaseType;
        }
        return null;
    }
    protected string getStringMemberValue(object target, string memberName)
    {
        object value = getPropertyValue(target, memberName);
        if (value == null)
        {
            value = getFieldValue(target, char.ToLowerInvariant(memberName[0]) + memberName.Substring(1));
        }
        return value != null ? value.ToString() : string.Empty;
    }
    protected FieldInfo findFieldInHierarchy(Type type, string fieldName, BindingFlags bindingFlags)
    {
        while (type != null && type != typeof(object))
        {
            FieldInfo field = type.GetField(fieldName, bindingFlags | BindingFlags.DeclaredOnly);
            if (field != null)
            {
                return field;
            }
            type = type.BaseType;
        }
        return null;
    }
    protected List<FieldInfo> getAllInstanceFields(Type type)
    {
        List<FieldInfo> fields = new List<FieldInfo>();
        while (type != null && type != typeof(object))
        {
            FieldInfo[] currentFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | 
                                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (int i = 0; i < currentFields.Length; ++i)
            {
                if (!currentFields[i].IsStatic)
                {
                    fields.Add(currentFields[i]);
                }
            }
            type = type.BaseType;
        }
        return fields;
    }
    protected long estimateShallowObject(object target)
    {
        if (target == null)
        {
            return 0;
        }
        Type type = target.GetType();
        if (type == typeof(string))
        {
            return estimateString((string)target);
        }
        if (type.IsArray)
        {
            return estimateArrayShallow((Array)target);
        }
        long size = OBJECT_HEADER_SIZE_64;
        List<FieldInfo> fields = getAllInstanceFields(type);
        for (int i = 0; i < fields.Count; ++i)
        {
            Type fieldType = fields[i].FieldType;
            size += fieldType.IsValueType ? getValueTypeSize(fieldType) : POINTER_SIZE_64;
        }
        return alignMemory(size);
    }
    protected long estimateArrayShallow(Array array)
    {
        if (array == null)
        {
            return 0;
        }
        Type elementType = array.GetType().GetElementType();
        long elementSize = elementType != null && elementType.IsValueType ? getValueTypeSize(elementType) : POINTER_SIZE_64;
        return alignMemory(ARRAY_HEADER_SIZE_64 + array.LongLength * elementSize);
    }
    protected long estimateString(string value)
    {
        if (value == null)
        {
            return 0;
        }
        return alignMemory(STRING_HEADER_SIZE_64 + (value.Length + 1L) * sizeof(char));
    }
    protected long estimateHashSetStorage(int count)
    {
        if (count <= 0)
        {
            return 0;
        }
        // 粗略估算HashSet对象、bucket数组和entry数组。
        // Entry通常包含hashCode、next和对象引用。
        return alignMemory(64L + count * 24L);
    }
    protected long getValueTypeSize(Type type)
    {
        if (type.IsEnum)
        {
            return getValueTypeSize(Enum.GetUnderlyingType(type));
        }

        if (type == typeof(bool) ||
            type == typeof(byte) ||
            type == typeof(sbyte))
        {
            return 1;
        }

        if (type == typeof(char) ||
            type == typeof(short) ||
            type == typeof(ushort))
        {
            return 2;
        }

        if (type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(float))
        {
            return 4;
        }

        if (type == typeof(long) ||
            type == typeof(ulong) ||
            type == typeof(double) ||
            type == typeof(IntPtr) ||
            type == typeof(UIntPtr))
        {
            return 8;
        }

        try
        {
            return Marshal.SizeOf(type);
        }
        catch
        {
            return POINTER_SIZE_64;
        }
    }
    protected long alignMemory(long value)
    {
        long remain = value % MEMORY_ALIGN_SIZE;
        return remain == 0 ? value : value + MEMORY_ALIGN_SIZE - remain;
    }
    protected bool isSpineEventObject(Type type)
    {
        return type != null && type.Namespace == "Spine" && type.Name == "Event";
    }
    protected bool isExposedList(Type type)
    {
        if (type == null)
        {
            return false;
        }
        return type.IsGenericType && type.GetGenericTypeDefinition().Name.StartsWith("ExposedList", StringComparison.Ordinal);
    }
    protected void addStringMemory(string value, AnimationAnalyzeResult result, HashSet<object> visitedObjects)
    {
        if (value == null || !visitedObjects.Add(value))
        {
            return;
        }
        result.mEstimatedBytes += estimateString(value);
    }
    protected void findSkeletonSourceFile()
    {
        mSkeletonFilePath = string.Empty;
        mSkeletonFileSize = 0;
        SerializedObject serializedObject = new SerializedObject(mSkeletonDataAsset);
        string[] possibleFieldNames =
        {
            "skeletonJSON",
            "skeletonDataFile",
            "skeletonFile"
        };

        TextAsset skeletonFile = null;
        for (int i = 0; i < possibleFieldNames.Length; ++i)
        {
            SerializedProperty property = serializedObject.FindProperty(possibleFieldNames[i]);
            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
            {
                continue;
            }
            skeletonFile = property.objectReferenceValue as TextAsset;
            if (skeletonFile != null)
            {
                break;
            }
        }
        if (skeletonFile == null)
        {
            object value = getFieldValue(mSkeletonDataAsset, "skeletonJSON");
            skeletonFile = value as TextAsset;
        }
        if (skeletonFile == null)
        {
            return;
        }
        mSkeletonFilePath = GetAssetPath(skeletonFile);
        if (string.IsNullOrEmpty(mSkeletonFilePath))
        {
            return;
        }
        FileInfo fileInfo = new FileInfo(mSkeletonFilePath);
        if (fileInfo.Exists)
        {
            mSkeletonFileSize = fileInfo.Length;
        }
    }
    protected void printAnalyzeSummary()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spine动画内存分析完成");
        builder.AppendLine("SkeletonDataAsset:" + GetAssetPath(mSkeletonDataAsset));
        builder.AppendLine("skel文件:" + mSkeletonFilePath);
        builder.AppendLine("skel大小:" + getMemoryText(mSkeletonFileSize));
        builder.AppendLine("动画数量:" + mTotalAnimationCount);
        builder.AppendLine("动画估算内存:" + getMemoryText(mTotalEstimatedAnimationBytes));
        builder.AppendLine();
        builder.AppendLine("体积最大的前20个动画:");
        int count = Mathf.Min(20, mAnalyzeResults.Count);
        for (int i = 0; i < count; ++i)
        {
            AnimationAnalyzeResult result = mAnalyzeResults[i];
            builder.Append(i + 1)
                   .Append(". ")
                   .Append(result.mAnimationName)
                   .Append(", 内存:")
                   .Append(getMemoryText(result.mEstimatedBytes))
                   .Append(", Timeline:")
                   .Append(result.mTimelineCount)
                   .Append(", 帧:")
                   .Append(result.mFrameCount)
                   .Append(", Deform float:")
                   .Append(result.mDeformFloatCount)
                   .AppendLine();
        }
        Debug.Log(builder.ToString(), mSkeletonDataAsset);
    }
    protected void exportTxt()
    {
        string defaultName = sanitizeFileName(mSkeletonDataAsset.name) + "_动画内存分析.txt";
        string path = EditorUtility.SaveFilePanel("导出Spine动画内存分析", Application.dataPath + "/../", defaultName, "txt");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Spine动画内存分析");
        builder.AppendLine("SkeletonDataAsset\t" + GetAssetPath(mSkeletonDataAsset));
        builder.AppendLine("SkeletonName\t" + mSkeletonName);
        builder.AppendLine("SpineVersion\t" + mSkeletonVersion);
        builder.AppendLine("SkeletonFile\t" + mSkeletonFilePath);
        builder.AppendLine("SkeletonFileBytes\t" + mSkeletonFileSize);
        builder.AppendLine("SkeletonFileSize\t" + getMemoryText(mSkeletonFileSize));
        builder.AppendLine("AnimationCount\t" + mTotalAnimationCount);
        builder.AppendLine("TimelineCount\t" + mTotalTimelineCount);
        builder.AppendLine("FrameCount\t" + mTotalFrameCount);
        builder.AppendLine("DeformFloatCount\t" + mTotalDeformFloatCount);
        builder.AppendLine("EstimatedAnimationBytes\t" + mTotalEstimatedAnimationBytes);
        builder.AppendLine("EstimatedAnimationSize\t" + getMemoryText(mTotalEstimatedAnimationBytes));
        builder.AppendLine();
        builder.AppendLine(
            "排名\t动画名称\t时长\tTimeline数量\t关键帧数量\t" +
            "float数组数量\tfloat数量\tint数组数量\tint数量\t" +
            "Deform Timeline\tDeform帧\tDeform float\t" +
            "DrawOrder帧\tDrawOrder int\tEvent帧\t估算字节\t估算大小");

        for (int i = 0; i < mAnalyzeResults.Count; ++i)
        {
            AnimationAnalyzeResult result = mAnalyzeResults[i];
            builder.Append(i + 1).Append('\t')
                   .Append(result.mAnimationName).Append('\t')
                   .Append(result.mDuration.ToString("F6")).Append('\t')
                   .Append(result.mTimelineCount).Append('\t')
                   .Append(result.mFrameCount).Append('\t')
                   .Append(result.mFloatArrayCount).Append('\t')
                   .Append(result.mFloatElementCount).Append('\t')
                   .Append(result.mIntArrayCount).Append('\t')
                   .Append(result.mIntElementCount).Append('\t')
                   .Append(result.mDeformTimelineCount).Append('\t')
                   .Append(result.mDeformFrameCount).Append('\t')
                   .Append(result.mDeformFloatCount).Append('\t')
                   .Append(result.mDrawOrderFrameCount).Append('\t')
                   .Append(result.mDrawOrderIntCount).Append('\t')
                   .Append(result.mEventFrameCount).Append('\t')
                   .Append(result.mEstimatedBytes).Append('\t')
                   .Append(getMemoryText(result.mEstimatedBytes))
                   .AppendLine();
            List<TimelineTypeAnalyzeResult> typeResults = new List<TimelineTypeAnalyzeResult>(result.mTimelineTypes.Values);
            typeResults.Sort((left, right) => right.mEstimatedBytes.CompareTo(left.mEstimatedBytes));
            for (int j = 0; j < typeResults.Count; ++j)
            {
                TimelineTypeAnalyzeResult typeResult = typeResults[j];
                builder.Append("\tTimeline类型\t")
                       .Append(typeResult.mTypeName)
                       .Append("\t数量:")
                       .Append(typeResult.mTimelineCount)
                       .Append("\t帧:")
                       .Append(typeResult.mFrameCount)
                       .Append("\tfloat:")
                       .Append(typeResult.mFloatElementCount)
                       .Append("\tint:")
                       .Append(typeResult.mIntElementCount)
                       .Append("\t估算:")
                       .Append(getMemoryText(typeResult.mEstimatedBytes))
                       .AppendLine();
            }
        }
        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
        refreshAssetDatabaseIfNeeded(path);
        Debug.Log("已导出Spine动画内存分析:" + path);
    }
    protected void exportCsv()
    {
        string defaultName = sanitizeFileName(mSkeletonDataAsset.name) + "_动画内存分析.csv";
        string path = EditorUtility.SaveFilePanel("导出Spine动画内存分析", Application.dataPath + "/../", defaultName, "csv");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(
            "排名,动画名称,时长,Timeline数量,关键帧数量," +
            "float数组数量,float数量,int数组数量,int数量," +
            "Deform Timeline数量,Deform帧数量,Deform float数量," +
            "DrawOrder帧数量,DrawOrder int数量,Event帧数量," +
            "估算字节,估算大小");

        for (int i = 0; i < mAnalyzeResults.Count; ++i)
        {
            AnimationAnalyzeResult result = mAnalyzeResults[i];
            builder.Append(i + 1).Append(',')
                   .Append(toCsvText(result.mAnimationName)).Append(',')
                   .Append(result.mDuration.ToString("F6")).Append(',')
                   .Append(result.mTimelineCount).Append(',')
                   .Append(result.mFrameCount).Append(',')
                   .Append(result.mFloatArrayCount).Append(',')
                   .Append(result.mFloatElementCount).Append(',')
                   .Append(result.mIntArrayCount).Append(',')
                   .Append(result.mIntElementCount).Append(',')
                   .Append(result.mDeformTimelineCount).Append(',')
                   .Append(result.mDeformFrameCount).Append(',')
                   .Append(result.mDeformFloatCount).Append(',')
                   .Append(result.mDrawOrderFrameCount).Append(',')
                   .Append(result.mDrawOrderIntCount).Append(',')
                   .Append(result.mEventFrameCount).Append(',')
                   .Append(result.mEstimatedBytes).Append(',')
                   .Append(toCsvText(getMemoryText(result.mEstimatedBytes)))
                   .AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
        refreshAssetDatabaseIfNeeded(path);
        Debug.Log("已导出Spine动画内存分析:" + path);
    }
    protected string toCsvText(string value)
    {
        if (value == null)
        {
            return "\"\"";
        }
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
    protected string sanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalidChars.Length; ++i)
        {
            fileName = fileName.Replace(invalidChars[i], '_');
        }
        return fileName;
    }
    protected void refreshAssetDatabaseIfNeeded(string path)
    {
        string normalizedPath = path.Replace('\\', '/');
        string normalizedAssetPath = Application.dataPath.Replace('\\', '/');
        if (normalizedPath.StartsWith(normalizedAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            Refresh();
        }
    }
    protected void clearAnalyzeResult()
    {
        mAnalyzeResults.Clear();
        mSkeletonName = string.Empty;
        mSkeletonVersion = string.Empty;
        mSkeletonFilePath = string.Empty;
        mSkeletonFileSize = 0;
        mTotalEstimatedAnimationBytes = 0;
        mTotalAnimationCount = 0;
        mTotalTimelineCount = 0;
        mTotalFrameCount = 0;
        mTotalDeformFloatCount = 0;
    }
    protected class AnimationAnalyzeResult
    {
        public string mAnimationName;
        public float mDuration;
        public int mTimelineCount;
        public int mFrameCount;
        public int mFloatArrayCount;
        public long mFloatElementCount;
        public int mIntArrayCount;
        public long mIntElementCount;
        public int mDeformTimelineCount;
        public int mDeformFrameCount;
        public long mDeformFloatCount;
        public int mDrawOrderTimelineCount;
        public int mDrawOrderFrameCount;
        public long mDrawOrderIntCount;
        public int mEventTimelineCount;
        public int mEventFrameCount;
        public long mEstimatedBytes;
        public readonly Dictionary<string, TimelineTypeAnalyzeResult> mTimelineTypes = new Dictionary<string, TimelineTypeAnalyzeResult>();
    }
    protected class TimelineAnalyzeResult
    {
        public int mFrameCount;
        public int mFloatArrayCount;
        public long mFloatElementCount;
        public int mIntArrayCount;
        public long mIntElementCount;
        public int mStringArrayCount;
        public int mStringCount;
        public long mEstimatedBytes;
    }
    protected class TimelineTypeAnalyzeResult
    {
        public string mTypeName;
        public int mTimelineCount;
        public int mFrameCount;
        public long mFloatElementCount;
        public long mIntElementCount;
        public long mEstimatedBytes;
    }
    protected sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer mInstance = new ReferenceEqualityComparer();
        public new bool Equals(object left, object right)
        {
            return ReferenceEquals(left, right);
        }
        public int GetHashCode(object target)
        {
            return target == null ? 0 : RuntimeHelpers.GetHashCode(target);
        }
    }
}
#endif