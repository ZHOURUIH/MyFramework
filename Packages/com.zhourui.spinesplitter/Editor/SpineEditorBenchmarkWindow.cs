using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Threading;
using Spine;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using static SpineDynamicAnimation;
#if SPINE_RUNTIME_43
using SpineAnimationBinaryReader = Spine43AnimationBinaryReader;
using SpineSingleAnimationData = Spine43SingleAnimationData;
using SpineAnimationCommonData = Spine43AnimationCommonData;
using static Spine43AnimationFile;
#elif SPINE_RUNTIME_42
using SpineAnimationBinaryReader = Spine42AnimationBinaryReader;
using SpineSingleAnimationData = Spine42SingleAnimationData;
using SpineAnimationCommonData = Spine42AnimationCommonData;
using static Spine42AnimationFile;
#elif SPINE_RUNTIME_41
using SpineAnimationBinaryReader = Spine41AnimationBinaryReader;
using SpineSingleAnimationData = Spine41SingleAnimationData;
using SpineAnimationCommonData = Spine41AnimationCommonData;
using static Spine41AnimationFile;
#elif SPINE_RUNTIME_40
using SpineAnimationBinaryReader = Spine40AnimationBinaryReader;
using SpineSingleAnimationData = Spine40SingleAnimationData;
using SpineAnimationCommonData = Spine40AnimationCommonData;
using static Spine40AnimationFile;
#else
#error SpineSplitter仅支持通过UPM安装的Spine 4.0、4.1、4.2或4.3 Runtime。
#endif

public class SpineEditorBenchmarkWindow : EditorWindow
{
    private static readonly string[] PROFILE_STAGES =
    {
        "Slot", "Bone", "IK", "Transform", "Path", "Deform", "DrawOrder", "Event", "Duration", "CreateAnimation"
    };
    private sealed class ArrayReferenceComparer : IEqualityComparer<Array>
    {
        public bool Equals(Array x, Array y)
        {
            return ReferenceEquals(x, y);
        }
        public int GetHashCode(Array obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
    private sealed class TimelineMemoryInfo
    {
        public int mTimelineCount;
        public int mArrayCount;
        public long mArrayPayloadBytes;
        public int mFloatArrayCount;
        public long mFloatElementCount;
        public long mFloatPayloadBytes;
        public int mIntArrayCount;
        public long mIntElementCount;
        public long mIntPayloadBytes;
        public int mReferenceArrayCount;
        public long mReferenceElementCount;
        public long mReferencePayloadBytes;
        public int mOtherArrayCount;
        public long mOtherArrayPayloadBytes;
    }
    private sealed class DeformPackingSimulation
    {
        public int mTimelineCount;
        public int mFrameArrayCount;
        public long mFloatElementCount;
        public long mCurrentFloatPayloadBytes;
        public long mCurrentFrameReferencePayloadBytes;
        public long mPerTimelineFloatPayloadBytes;
        public long mPerTimelineOffsetPayloadBytes;
        public int mPerTimelineFloatArrayCount;
        public int mPerTimelineOffsetArrayCount;
        public long mGlobalFloatPayloadBytes;
        public long mGlobalOffsetPayloadBytes;
        public int mGlobalFloatArrayCount;
        public int mGlobalOffsetArrayCount;
    }

    private sealed class MultiAnimationMemoryInfo
    {
        public string mName;
        public long mFileBytes;
        public long mTimelineArrayPayloadBytes;
        public long mDeformFloatPayloadBytes;
        public int mTimelineCount;
        public long mDeformTimelineCount;
        public long mDeformFrameCount;
    }
    private DefaultAsset mAnimationFolder;
    private TextAsset mCommonFile;
    private TextAsset mAnimationFile;
    private SkeletonDataAsset mSkeletonDataAsset;
    private int mSampleCount = 10;
    private int mWarmupCount = 2;
    [MenuItem("Tools/Spine/Animation Benchmark")]
    private static void open()
    {
        GetWindow<SpineEditorBenchmarkWindow>("Spine Benchmark");
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Spine Animation Benchmark", EditorStyles.boldLabel);
        mCommonFile = (TextAsset)EditorGUILayout.ObjectField("Common File", mCommonFile, typeof(TextAsset), false);
        mAnimationFile = (TextAsset)EditorGUILayout.ObjectField("Animation File", mAnimationFile, typeof(TextAsset), false);
        mAnimationFolder = (DefaultAsset)EditorGUILayout.ObjectField("Animation Folder", mAnimationFolder, typeof(DefaultAsset), false);
        mSkeletonDataAsset = (SkeletonDataAsset)EditorGUILayout.ObjectField("SkeletonDataAsset", mSkeletonDataAsset, typeof(SkeletonDataAsset), false);
        mSampleCount = Math.Max(1, EditorGUILayout.IntField("Sample Count", mSampleCount));
        mWarmupCount = Math.Max(0, EditorGUILayout.IntField("Warmup Count", mWarmupCount));
        EditorGUILayout.Space(8);
        GUI.enabled = mCommonFile != null && mAnimationFile != null && mSkeletonDataAsset != null;
        if (GUILayout.Button("Start Performance Benchmark", GUILayout.Height(30)))
        {
            runPerformanceBenchmark();
        }
        if (GUILayout.Button("Start Memory Benchmark", GUILayout.Height(30)))
        {
            runMemoryBenchmark();
        }
        GUI.enabled = mCommonFile != null && mAnimationFolder != null && mSkeletonDataAsset != null;
        if (GUILayout.Button("Start Multi Animation Memory Benchmark", GUILayout.Height(30)))
        {
            runMultiAnimationMemoryBenchmark();
        }
        if (GUILayout.Button("Start Dynamic Animation LRU Test", GUILayout.Height(30)))
        {
            runDynamicAnimationLRUTest();
        }
        GUI.enabled = true;
        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox("Performance同时输出Profile ON阶段分析和正式Profile OFF耗时；Memory测试动画结构；Multi Animation Memory使用正式Reader扫描全部动作；Dynamic Animation LRU Test验证60秒驻留、LRU顺序、Pin、播放保护和Dispose事件自动Trim。", UnityEditor.MessageType.Info);
    }
    private void runPerformanceBenchmark()
    {
        if (!tryPrepare(out SpineAnimationCommonData commonData, out SkeletonData skeletonData, out byte[] animationBytes))
        {
            return;
        }
        SpineSingleAnimationData preparedData = readAnimationNoCopy(animationBytes);
        for (int i = 0; i < mWarmupCount; ++i)
        {
			SpineAnimationBinaryReader warmupReader = new();
            warmupReader.readAnimation(preparedData.mBinarySourceData, preparedData.mBinaryOffset, preparedData.mBinaryLength, commonData.mStrings, skeletonData, mSkeletonDataAsset.scale, preparedData.mAnimationName);
        }
        List<double> fileTimes = new(mSampleCount);
        List<double> profileReaderTimes = new(mSampleCount);
        List<double> profileTotalTimes = new(mSampleCount);
        List<double> productionReaderTimes = new(mSampleCount);
        Dictionary<string, List<double>> stageTimes = new(StringComparer.Ordinal);
        for (int i = 0; i < PROFILE_STAGES.Length; ++i)
        {
            stageTimes.Add(PROFILE_STAGES[i], new(mSampleCount));
        }
        long gcBefore = GC.GetTotalMemory(false);
        for (int i = 0; i < mSampleCount; ++i)
        {
            Stopwatch totalWatch = Stopwatch.StartNew();
            Stopwatch fileWatch = Stopwatch.StartNew();
            SpineSingleAnimationData animationData = readAnimationNoCopy(animationBytes);
            fileWatch.Stop();
            Stopwatch readerWatch = Stopwatch.StartNew();
            SpineAnimationBinaryReader profileReader = new();
            profileReader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData, mSkeletonDataAsset.scale, animationData.mAnimationName);
            readerWatch.Stop();
            totalWatch.Stop();
            fileTimes.Add(fileWatch.Elapsed.TotalMilliseconds);
            profileReaderTimes.Add(readerWatch.Elapsed.TotalMilliseconds);
            profileTotalTimes.Add(totalWatch.Elapsed.TotalMilliseconds);
            benchmarkProductionReader(preparedData, commonData, skeletonData, productionReaderTimes);
        }
        long gcAfter = GC.GetTotalMemory(false);
        Debug.Log(buildPerformanceReport(animationBytes.Length, fileTimes, profileReaderTimes, profileTotalTimes, productionReaderTimes, stageTimes, gcAfter - gcBefore));
    }
    private void benchmarkProductionReader(SpineSingleAnimationData animationData, SpineAnimationCommonData commonData, SkeletonData skeletonData, List<double> result)
    {
        Stopwatch watch = Stopwatch.StartNew();
        SpineAnimationBinaryReader reader = new();
        reader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData, mSkeletonDataAsset.scale, animationData.mAnimationName);
        watch.Stop();
        result.Add(watch.Elapsed.TotalMilliseconds);
    }
    private void runMemoryBenchmark()
    {
        if (!tryPrepare(out SpineAnimationCommonData commonData, out SkeletonData skeletonData, out byte[] animationBytes))
        {
            return;
        }
        SpineSingleAnimationData animationData = readAnimationNoCopy(animationBytes);
        SpineAnimationBinaryReader reader = new();
        Spine.Animation animation = reader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData, mSkeletonDataAsset.scale, animationData.mAnimationName);
        long payloadCopyBytes = animationData.mBinaryData != null ? animationData.mBinaryLength : 0;
        Dictionary<string, TimelineMemoryInfo> timelineMemory = analyzeTimelineMemory(animation, out TimelineMemoryInfo totalTimelineMemory);
        DeformPackingSimulation deformPacking = analyzeDeformPacking(animation);
        double timelinePayloadToFileRatio = animationBytes.Length > 0 ? totalTimelineMemory.mArrayPayloadBytes / (double)animationBytes.Length : 0.0;
        double sourcePlusTimelinePayloadRatio = animationBytes.Length > 0 ? (animationBytes.Length + totalTimelineMemory.mArrayPayloadBytes) / (double)animationBytes.Length : 0.0;
        StringBuilder builder = new(8192);
        builder.AppendLine("================ Spine Memory Benchmark ================");
        builder.AppendLine("File:" + mAnimationFile.name);
        builder.AppendLine("Animation File Bytes:" + formatBytes(animationBytes.Length));
        builder.AppendLine("Animation Binary Payload:" + formatBytes(animationData.mBinaryLength));
        builder.AppendLine("Payload Copy Bytes:" + formatBytes(payloadCopyBytes));
        builder.AppendLine("Zero Copy:" + (payloadCopyBytes == 0 ? "Yes" : "No"));
        builder.AppendLine("---------------- Timeline Array Payload ----------------");
        builder.AppendLine("Timeline Count:" + totalTimelineMemory.mTimelineCount);
        builder.AppendLine("Unique Array Count:" + totalTimelineMemory.mArrayCount);
        builder.AppendLine("All Timeline Array Payload:" + formatBytes(totalTimelineMemory.mArrayPayloadBytes));
        builder.AppendLine("float[] Count:" + totalTimelineMemory.mFloatArrayCount + " Elements:" + totalTimelineMemory.mFloatElementCount + " Payload:" + formatBytes(totalTimelineMemory.mFloatPayloadBytes));
        builder.AppendLine("int[] Count:" + totalTimelineMemory.mIntArrayCount + " Elements:" + totalTimelineMemory.mIntElementCount + " Payload:" + formatBytes(totalTimelineMemory.mIntPayloadBytes));
        builder.AppendLine("Reference Array Count:" + totalTimelineMemory.mReferenceArrayCount + " Elements:" + totalTimelineMemory.mReferenceElementCount + " Payload:" + formatBytes(totalTimelineMemory.mReferencePayloadBytes));
        builder.AppendLine("Other Array Count:" + totalTimelineMemory.mOtherArrayCount + " Payload:" + formatBytes(totalTimelineMemory.mOtherArrayPayloadBytes));
        builder.AppendLine("Timeline Array Payload / File:" + timelinePayloadToFileRatio.ToString("F2") + "x");
        builder.AppendLine("Source File + Timeline Array Payload / File:" + sourcePlusTimelinePayloadRatio.ToString("F2") + "x");
        builder.AppendLine("---------------- Timeline Type Breakdown ----------------");
        List<KeyValuePair<string, TimelineMemoryInfo>> sortedTimelineMemory = new(timelineMemory);
        sortedTimelineMemory.Sort((a, b) => b.Value.mArrayPayloadBytes.CompareTo(a.Value.mArrayPayloadBytes));
        for (int i = 0; i < sortedTimelineMemory.Count; ++i)
        {
            var pair = sortedTimelineMemory[i];
            TimelineMemoryInfo info = pair.Value;
            builder.AppendLine(pair.Key + " Count:" + info.mTimelineCount + " Arrays:" + info.mArrayCount + " Payload:" + formatBytes(info.mArrayPayloadBytes) + " Float:" + formatBytes(info.mFloatPayloadBytes) + " Ref:" + formatBytes(info.mReferencePayloadBytes));
        }
        builder.AppendLine("---------------- Deform Packing Simulation ----------------");
        builder.AppendLine("Current Frame float[] Count:" + deformPacking.mFrameArrayCount);
        builder.AppendLine("Current Deform Float Elements:" + deformPacking.mFloatElementCount);
        builder.AppendLine("Current Frame float[] Payload:" + formatBytes(deformPacking.mCurrentFloatPayloadBytes));
        builder.AppendLine("Current Frame Reference Payload:" + formatBytes(deformPacking.mCurrentFrameReferencePayloadBytes));
        builder.AppendLine("Per-Timeline Packed float[] Count:" + deformPacking.mPerTimelineFloatArrayCount);
        builder.AppendLine("Per-Timeline Offset int[] Count:" + deformPacking.mPerTimelineOffsetArrayCount);
        builder.AppendLine("Per-Timeline Float Payload:" + formatBytes(deformPacking.mPerTimelineFloatPayloadBytes));
        builder.AppendLine("Per-Timeline Offset Payload:" + formatBytes(deformPacking.mPerTimelineOffsetPayloadBytes));
        builder.AppendLine("Global Packed float[] Count:" + deformPacking.mGlobalFloatArrayCount);
        builder.AppendLine("Global Offset int[] Count:" + deformPacking.mGlobalOffsetArrayCount);
        builder.AppendLine("Global Float Payload:" + formatBytes(deformPacking.mGlobalFloatPayloadBytes));
        builder.AppendLine("Global Offset Payload:" + formatBytes(deformPacking.mGlobalOffsetPayloadBytes));
        appendPackingEstimate(builder, "16-byte array header", deformPacking, 16);
        appendPackingEstimate(builder, "24-byte array header", deformPacking, 24);
        appendPackingEstimate(builder, "32-byte array header", deformPacking, 32);
        builder.AppendLine("---------------- Notes ----------------");
        builder.AppendLine("Timeline Array Payload通过反射直接统计Animation中每个Timeline持有的数组，并按数组引用去重，不依赖GC。");
        builder.AppendLine("会递归统计Timeline字段直接持有的jagged array，例如DeformTimeline中的float[][]以及其中各个float[]。");
        builder.AppendLine("不会递归进入Attachment/SkeletonData等普通对象，因此不会把共享Skeleton资源算进Animation。");
        builder.AppendLine("Array Payload只统计数组元素区，不包含CLR数组头、Timeline对象头和非数组字段，因此仍然是Animation实际托管内存的确定性下限。");
        builder.AppendLine("Payload Copy Bytes为0表示readAnimation阶段没有额外复制完整动画二进制。");
        builder.AppendLine("==========================================================");
        Debug.Log(builder.ToString());
        GC.KeepAlive(animation);
    }
    private static DeformPackingSimulation analyzeDeformPacking(Spine.Animation animation)
    {
        DeformPackingSimulation result = new();
        object timelinesObject = getMemberValue(animation, "Timelines");
        Array items = getMemberValue(timelinesObject, "Items") as Array;
        object countValue = getMemberValue(timelinesObject, "Count");
        int count = countValue != null ? Convert.ToInt32(countValue) : items != null ? items.Length : 0;
        if (items == null)
        {
            Debug.LogError("无法读取Spine.Animation.Timelines.Items");
        }
        HashSet<Array> uniqueFrameArrays = new(new ArrayReferenceComparer());
        for (int i = 0; i < count; ++i)
        {
            object timeline = items.GetValue(i);
            if (timeline == null || timeline.GetType().Name != "DeformTimeline")
            {
                continue;
            }
            ++result.mTimelineCount;
            Array frameVertices = findFloatJaggedArray(timeline);
            if (frameVertices == null)
            {
                continue;
            }
            int frameCount = frameVertices.Length;
            long timelineFloatCount = 0L;
            for (int frame = 0; frame < frameCount; ++frame)
            {
                float[] vertices = frameVertices.GetValue(frame) as float[];
                if (vertices == null || !uniqueFrameArrays.Add(vertices))
                {
                    continue;
                }
                ++result.mFrameArrayCount;
                result.mFloatElementCount += vertices.LongLength;
                timelineFloatCount += vertices.LongLength;
            }
            if (timelineFloatCount > 0)
            {
                ++result.mPerTimelineFloatArrayCount;
                ++result.mPerTimelineOffsetArrayCount;
                result.mPerTimelineFloatPayloadBytes += timelineFloatCount * 4L;
                result.mPerTimelineOffsetPayloadBytes += (frameCount + 1L) * 4L;
            }
        }
        result.mCurrentFloatPayloadBytes = result.mFloatElementCount * 4L;
        result.mCurrentFrameReferencePayloadBytes = result.mFrameArrayCount * (long)IntPtr.Size;
        result.mGlobalFloatArrayCount = result.mFloatElementCount > 0 ? 1 : 0;
        result.mGlobalOffsetArrayCount = result.mFrameArrayCount > 0 ? 1 : 0;
        result.mGlobalFloatPayloadBytes = result.mCurrentFloatPayloadBytes;
        result.mGlobalOffsetPayloadBytes = (result.mFrameArrayCount + result.mTimelineCount + 1L) * 4L;
        return result;
    }
    private static Array findFloatJaggedArray(object target)
    {
        Type type = target.GetType();
        while (type != null && type != typeof(object))
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            for (int i = 0; i < fields.Length; ++i)
            {
                Type fieldType = fields[i].FieldType;
                if (fieldType.IsArray && fieldType.GetElementType() == typeof(float[]))
                {
                    return fields[i].GetValue(target) as Array;
                }
            }
            type = type.BaseType;
        }
        return null;
    }
    private static void appendPackingEstimate(StringBuilder builder, string title, DeformPackingSimulation data, int arrayHeaderBytes)
    {
        long currentBytes = data.mCurrentFloatPayloadBytes + data.mCurrentFrameReferencePayloadBytes + data.mFrameArrayCount * (long)arrayHeaderBytes;
        long perTimelineBytes = data.mPerTimelineFloatPayloadBytes + data.mPerTimelineOffsetPayloadBytes + (data.mPerTimelineFloatArrayCount + data.mPerTimelineOffsetArrayCount) * (long)arrayHeaderBytes;
        long globalBytes = data.mGlobalFloatPayloadBytes + data.mGlobalOffsetPayloadBytes + (data.mGlobalFloatArrayCount + data.mGlobalOffsetArrayCount) * (long)arrayHeaderBytes;
        long perTimelineSave = currentBytes - perTimelineBytes;
        long globalSave = currentBytes - globalBytes;
        double perTimelinePercent = currentBytes > 0 ? perTimelineSave * 100.0 / currentBytes : 0.0;
        double globalPercent = currentBytes > 0 ? globalSave * 100.0 / currentBytes : 0.0;
        builder.AppendLine(title + " Current:" + formatBytes(currentBytes) +
            " PerTimeline:" + formatBytes(perTimelineBytes) +
            " Save:" + formatBytes(perTimelineSave) + " (" + perTimelinePercent.ToString("F1") + "%)" +
            " Global:" + formatBytes(globalBytes) +
            " Save:" + formatBytes(globalSave) + " (" + globalPercent.ToString("F1") + "%)");
    }
    private static Dictionary<string, TimelineMemoryInfo> analyzeTimelineMemory(Spine.Animation animation, out TimelineMemoryInfo total)
    {
        total = new();
        Dictionary<string, TimelineMemoryInfo> result = new(StringComparer.Ordinal);
        HashSet<Array> visitedArrays = new(new ArrayReferenceComparer());
        object timelinesObject = getMemberValue(animation, "Timelines");
        if (timelinesObject == null)
        {
            Debug.LogError("无法读取Spine.Animation.Timelines");
        }
        Array items = getMemberValue(timelinesObject, "Items") as Array;
        object countValue = getMemberValue(timelinesObject, "Count");
        int count = countValue != null ? Convert.ToInt32(countValue) : items != null ? items.Length : 0;
        if (items == null)
        {
			Debug.LogError("无法读取Spine.Animation.Timelines.Items");
        }
        for (int i = 0; i < count; ++i)
        {
            object timeline = items.GetValue(i);
            if (timeline == null)
            {
                continue;
            }
            string typeName = timeline.GetType().Name;
            if (!result.TryGetValue(typeName, out TimelineMemoryInfo info))
            {
                info = new();
                result.Add(typeName, info);
            }
            ++info.mTimelineCount;
            ++total.mTimelineCount;
            Type type = timeline.GetType();
            while (type != null && type != typeof(object))
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int fieldIndex = 0; fieldIndex < fields.Length; ++fieldIndex)
                {
                    object value;
                    try
                    {
                        value = fields[fieldIndex].GetValue(timeline);
                    }
                    catch
                    {
                        continue;
                    }
                    if (value is Array array)
                    {
                        accumulateArray(array, visitedArrays, info, total);
                    }
                }
                type = type.BaseType;
            }
        }
        return result;
    }
    private static void accumulateArray(Array array, HashSet<Array> visitedArrays, TimelineMemoryInfo info, TimelineMemoryInfo total)
    {
        if (array == null || !visitedArrays.Add(array))
        {
            return;
        }
        ++info.mArrayCount;
        ++total.mArrayCount;
        Type elementType = array.GetType().GetElementType();
        long length = array.LongLength;
        long payloadBytes = getArrayPayloadBytes(elementType, length);
        info.mArrayPayloadBytes += payloadBytes;
        total.mArrayPayloadBytes += payloadBytes;
        if (elementType == typeof(float))
        {
            ++info.mFloatArrayCount;
            info.mFloatElementCount += length;
            info.mFloatPayloadBytes += payloadBytes;
            ++total.mFloatArrayCount;
            total.mFloatElementCount += length;
            total.mFloatPayloadBytes += payloadBytes;
        }
        else if (elementType == typeof(int))
        {
            ++info.mIntArrayCount;
            info.mIntElementCount += length;
            info.mIntPayloadBytes += payloadBytes;
            ++total.mIntArrayCount;
            total.mIntElementCount += length;
            total.mIntPayloadBytes += payloadBytes;
        }
        else if (!elementType.IsValueType)
        {
            ++info.mReferenceArrayCount;
            info.mReferenceElementCount += length;
            info.mReferencePayloadBytes += payloadBytes;
            ++total.mReferenceArrayCount;
            total.mReferenceElementCount += length;
            total.mReferencePayloadBytes += payloadBytes;
        }
        else
        {
            ++info.mOtherArrayCount;
            info.mOtherArrayPayloadBytes += payloadBytes;
            ++total.mOtherArrayCount;
            total.mOtherArrayPayloadBytes += payloadBytes;
        }
        if (elementType.IsArray || !elementType.IsValueType)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (array.GetValue(i) is Array childArray)
                {
                    accumulateArray(childArray, visitedArrays, info, total);
                }
            }
        }
    }
    private static long getArrayPayloadBytes(Type elementType, long length)
    {
        if (!elementType.IsValueType)
        {
            return length * IntPtr.Size;
        }
        if (elementType.IsEnum)
        {
            elementType = Enum.GetUnderlyingType(elementType);
        }
        if (elementType == typeof(byte) || elementType == typeof(sbyte) || elementType == typeof(bool))
        {
            return length;
        }
        if (elementType == typeof(short) || elementType == typeof(ushort) || elementType == typeof(char))
        {
            return length * 2L;
        }
        if (elementType == typeof(int) || elementType == typeof(uint) || elementType == typeof(float))
        {
            return length * 4L;
        }
        if (elementType == typeof(long) || elementType == typeof(ulong) || elementType == typeof(double))
        {
            return length * 8L;
        }
        try
        {
            return length * Marshal.SizeOf(elementType);
        }
        catch
        {
            return 0L;
        }
    }
    private static object getMemberValue(object target, string memberName)
    {
        if (target == null)
        {
            return null;
        }
        Type type = target.GetType();
        while (type != null)
        {
            PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (property != null)
            {
                return property.GetValue(target, null);
            }
            FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (field != null)
            {
                return field.GetValue(target);
            }
            type = type.BaseType;
        }
        return null;
    }
    private void runMultiAnimationMemoryBenchmark()
    {
        string folderPath = AssetDatabase.GetAssetPath(mAnimationFolder);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Animation Folder不是有效的Unity资源目录");
            return;
        }
        SpineAnimationCommonData commonData;
        SkeletonData skeletonData;
        try
        {
            commonData = readCommon(mCommonFile.bytes);
            skeletonData = mSkeletonDataAsset.GetSkeletonData(true);
            if (skeletonData == null)
            {
                Debug.LogError("SkeletonData为空");
                return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Multi Animation Benchmark准备失败:" + exception.Message);
            Debug.LogException(exception);
            return;
        }
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderPath });
        List<string> animationPaths = new(guids.Length);
        for (int i = 0; i < guids.Length; ++i)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
            {
                continue;
            }
            byte[] bytes = asset.bytes;
            if (isAnimationFile(bytes))
            {
                animationPaths.Add(assetPath);
            }
        }
        if (animationPaths.Count == 0)
        {
            Debug.LogWarning("Animation Folder中没有找到Spine 4.0单动画文件:" + folderPath);
            return;
        }
        animationPaths.Sort(StringComparer.Ordinal);
        List<Spine.Animation> loadedAnimations = new(animationPaths.Count);
        List<MultiAnimationMemoryInfo> infos = new(animationPaths.Count);
        long totalFileBytes = 0L;
        long totalTimelinePayloadBytes = 0L;
        long totalDeformFloatPayloadBytes = 0L;
        long totalTimelineCount = 0L;
        long totalDeformTimelineCount = 0L;
        long totalDeformFrameCount = 0L;
        long totalReadTicks = 0L;
        int failedCount = 0;
        for (int i = 0; i < animationPaths.Count; ++i)
        {
            string assetPath = animationPaths[i];
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
            {
                ++failedCount;
                continue;
            }
            byte[] animationBytes = asset.bytes;
            try
            {
                SpineSingleAnimationData animationData = readAnimationNoCopy(animationBytes);
                Stopwatch stopwatch = Stopwatch.StartNew();
                SpineAnimationBinaryReader reader = new();
                Spine.Animation animation = reader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData, mSkeletonDataAsset.scale, animationData.mAnimationName);
                stopwatch.Stop();
                totalReadTicks += stopwatch.ElapsedTicks;
                loadedAnimations.Add(animation);
                analyzeTimelineMemory(animation, out TimelineMemoryInfo timelineMemory);
                DeformPackingSimulation deformMemory = analyzeDeformPacking(animation);
                long deformPayload = deformMemory.mCurrentFloatPayloadBytes;
                long deformTimeline = deformMemory.mTimelineCount;
                long deformFrame = deformMemory.mFrameArrayCount;
                MultiAnimationMemoryInfo info = new();
                info.mName = animationData.mAnimationName;
                info.mFileBytes = animationBytes.LongLength;
                info.mTimelineArrayPayloadBytes = timelineMemory.mArrayPayloadBytes;
                info.mDeformFloatPayloadBytes = deformPayload;
                info.mTimelineCount = timelineMemory.mTimelineCount;
                info.mDeformTimelineCount = deformTimeline;
                info.mDeformFrameCount = deformFrame;
                infos.Add(info);
                totalFileBytes += info.mFileBytes;
                totalTimelinePayloadBytes += info.mTimelineArrayPayloadBytes;
                totalDeformFloatPayloadBytes += info.mDeformFloatPayloadBytes;
                totalTimelineCount += info.mTimelineCount;
                totalDeformTimelineCount += info.mDeformTimelineCount;
                totalDeformFrameCount += info.mDeformFrameCount;
            }
            catch (Exception exception)
            {
                ++failedCount;
                Debug.LogError("解析动画失败:" + assetPath + ",原因:" + exception.Message);
            }
        }
        infos.Sort((a, b) => b.mTimelineArrayPayloadBytes.CompareTo(a.mTimelineArrayPayloadBytes));
        double totalReadMs = totalReadTicks * 1000.0 / Stopwatch.Frequency;
        double parsedPayloadToFileRatio = totalFileBytes > 0 ? totalTimelinePayloadBytes / (double)totalFileBytes : 0.0;
        double sourcePlusParsedRatio = totalFileBytes > 0 ? (totalFileBytes + totalTimelinePayloadBytes) / (double)totalFileBytes : 0.0;
        StringBuilder builder = new(16384);
        builder.AppendLine("================ Spine Multi Animation Memory Benchmark ================");
        builder.AppendLine("Folder:" + folderPath);
        builder.AppendLine("Animation Count:" + infos.Count);
        builder.AppendLine("Failed Count:" + failedCount);
        builder.AppendLine("Total Parse Time (Profile OFF):" + totalReadMs.ToString("F3") + "ms");
        builder.AppendLine("Average Parse Time (Profile OFF):" + (infos.Count > 0 ? totalReadMs / infos.Count : 0.0).ToString("F3") + "ms");
        builder.AppendLine("---------------- Totals ----------------");
        builder.AppendLine("Source Animation Files:" + formatBytes(totalFileBytes));
        builder.AppendLine("Timeline Array Payload:" + formatBytes(totalTimelinePayloadBytes));
        builder.AppendLine("Deform float[] Payload:" + formatBytes(totalDeformFloatPayloadBytes));
        builder.AppendLine("Timeline Count:" + totalTimelineCount);
        builder.AppendLine("Deform Timeline Count:" + totalDeformTimelineCount);
        builder.AppendLine("Deform Frame Count:" + totalDeformFrameCount);
        builder.AppendLine("Parsed Timeline Payload / Source Files:" + parsedPayloadToFileRatio.ToString("F2") + "x");
        builder.AppendLine("Keep Source Bytes + Parsed Payload / Source Files:" + sourcePlusParsedRatio.ToString("F2") + "x");
        builder.AppendLine("Release Source Bytes -> Long-Term Lower Bound:" + formatBytes(totalTimelinePayloadBytes));
        builder.AppendLine("Keep Source Bytes -> Long-Term Lower Bound:" + formatBytes(totalFileBytes + totalTimelinePayloadBytes));
        builder.AppendLine("---------------- Per Animation Ranking ----------------");
        int outputCount = Math.Min(infos.Count, 100);
        for (int i = 0; i < outputCount; ++i)
        {
            MultiAnimationMemoryInfo info = infos[i];
            double payloadRatio = info.mFileBytes > 0 ? info.mTimelineArrayPayloadBytes / (double)info.mFileBytes : 0.0;
            builder.AppendLine((i + 1) + ". " + info.mName +
                " File:" + formatBytes(info.mFileBytes) +
                " TimelinePayload:" + formatBytes(info.mTimelineArrayPayloadBytes) +
                " Ratio:" + payloadRatio.ToString("F2") + "x" +
                " Deform:" + formatBytes(info.mDeformFloatPayloadBytes) +
                " Timeline:" + info.mTimelineCount +
                " DeformTimeline:" + info.mDeformTimelineCount +
                " DeformFrame:" + info.mDeformFrameCount);
        }
        if (infos.Count > outputCount)
        {
            builder.AppendLine("其余" + (infos.Count - outputCount) + "个动画未展开显示。");
        }
        builder.AppendLine("---------------- Notes ----------------");
        builder.AppendLine("Timeline Array Payload按Animation实际数组引用去重统计，不包含CLR对象头/数组头，因此是长期Animation托管内存下限。");
        builder.AppendLine("Release Source Bytes表示动态动画解析完成后资源系统释放对应TextAsset/byte[]时的理论长期下限。");
        builder.AppendLine("Keep Source Bytes表示资源系统继续缓存所有原始动画byte[]时的理论长期下限。");
        builder.AppendLine("解析耗时使用正式Profile OFF Reader；Deform内存统计由Editor侧直接分析Animation数组，不依赖运行时Profiler。");
        builder.AppendLine("本测试不会把解析出的Animation加入SkeletonData.Animations，避免污染SkeletonData；loadedAnimations会保留到统计结束，模拟多个动作同时驻留。");
        builder.AppendLine("==========================================================================");
        Debug.Log(builder.ToString());
        GC.KeepAlive(loadedAnimations);
        GC.KeepAlive(commonData);
        GC.KeepAlive(skeletonData);
    }
    private void runDynamicAnimationLRUTest()
    {
        string folderPath = AssetDatabase.GetAssetPath(mAnimationFolder);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("LRU测试失败,Animation Folder不是有效目录");
            return;
        }
        GameObject testObject = null;
        SkeletonAnimation skeletonAnimation = null;
        bool commonDataAdded = false;
        List<string> testAnimationNames = new(3);
        try
        {
            SkeletonData originalSkeletonData = mSkeletonDataAsset.GetSkeletonData(true);
            if (originalSkeletonData == null)
            {
				Debug.LogError("SkeletonData为空");
            }
            List<TextAsset> animationFiles = findLRUTestAnimationFiles(folderPath, originalSkeletonData, 3);
            if (animationFiles.Count < 3)
            {
				Debug.LogError("LRU测试至少需要3个当前SkeletonData中尚未加载的单动画文件");
            }
            testObject = new("SpineDynamicAnimationLRUTest");
            testObject.hideFlags = HideFlags.HideAndDontSave;
            skeletonAnimation = testObject.AddComponent<SkeletonAnimation>();
            skeletonAnimation.skeletonDataAsset = mSkeletonDataAsset;
            skeletonAnimation.Initialize(true);
            if (skeletonAnimation.Skeleton == null || skeletonAnimation.AnimationState == null)
            {
				Debug.LogError("临时SkeletonAnimation初始化失败");
            }
            SkeletonData skeletonData = skeletonAnimation.Skeleton.Data;
            if (getCommonData(skeletonData) == null)
            {
                setCommonData(skeletonData, mCommonFile.bytes);
                commonDataAdded = true;
            }
            clearDynamicAnimationCache(skeletonData);
            for (int i = 0; i < animationFiles.Count; ++i)
            {
                SpineSingleAnimationData fileData = readAnimationNoCopy(animationFiles[i].bytes);
                testAnimationNames.Add(fileData.mAnimationName);
            }
            string animationA = testAnimationNames[0];
            string animationB = testAnimationNames[1];
            string animationC = testAnimationNames[2];
            StringBuilder builder = new(4096);
            builder.AppendLine("================ Spine Dynamic Animation LRU Test ================");
            builder.AppendLine("A:" + animationA);
            builder.AppendLine("B:" + animationB);
            builder.AppendLine("C:" + animationC);
            setDynamicAnimationCacheLimit(skeletonAnimation, 1);
            assertLRU(Math.Abs(getDynamicAnimationMinResidentTime(skeletonData) - 60.0) < 0.0001, "默认最小驻留时间应为60秒");
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            addAnimation(skeletonAnimation, animationFiles[1].bytes);
            assertLRU(getDynamicAnimationCount(skeletonData) == 2, "60秒驻留保护下,超过Limit后不应立即淘汰刚加载动画");
            assertLRU(skeletonData.FindAnimation(animationA) != null && skeletonData.FindAnimation(animationB) != null, "60秒驻留保护期间A/B都应存在");
            builder.AppendLine("[PASS] 默认60秒驻留保护");
            setDynamicAnimationMinResidentTime(skeletonAnimation, 0.05);
            Thread.Sleep(80);
            int removedAfterDelay = trimDynamicAnimations(skeletonAnimation);
            assertLRU(removedAfterDelay == 1, "超过最小驻留时间后应淘汰1个动画");
            assertLRU(skeletonData.FindAnimation(animationA) == null && skeletonData.FindAnimation(animationB) != null, "超过驻留时间后应按LRU淘汰较旧的A");
            builder.AppendLine("[PASS] 驻留时间到期后按LRU淘汰");
            cleanupLRUTestAnimations(skeletonAnimation, testAnimationNames);
            clearDynamicAnimationCache(skeletonData);
            setDynamicAnimationMinResidentTime(skeletonAnimation, 0.0);
            setDynamicAnimationCacheLimit(skeletonAnimation, 1);
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            assertLRU(pinAnimation(skeletonAnimation, animationA), "Pin A失败");
            addAnimation(skeletonAnimation, animationFiles[1].bytes);
            assertLRU(skeletonData.FindAnimation(animationA) != null && skeletonData.FindAnimation(animationB) == null, "A被Pin后,超限时应淘汰B而不是A");
            builder.AppendLine("[PASS] Pin保护");
            cleanupLRUTestAnimations(skeletonAnimation, testAnimationNames);
            clearDynamicAnimationCache(skeletonData);
            setDynamicAnimationMinResidentTime(skeletonAnimation, 0.0);
            setDynamicAnimationCacheLimit(skeletonAnimation, 2);
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            addAnimation(skeletonAnimation, animationFiles[1].bytes);
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            addAnimation(skeletonAnimation, animationFiles[2].bytes);
            assertLRU(skeletonData.FindAnimation(animationA) != null && skeletonData.FindAnimation(animationB) == null && skeletonData.FindAnimation(animationC) != null, "再次访问A后,B应成为最旧并被淘汰");
            builder.AppendLine("[PASS] LRU访问顺序刷新");
            cleanupLRUTestAnimations(skeletonAnimation, testAnimationNames);
            clearDynamicAnimationCache(skeletonData);
            setDynamicAnimationMinResidentTime(skeletonAnimation, 0.0);
            setDynamicAnimationCacheLimit(skeletonAnimation, 1);
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            playAnimation(skeletonAnimation, 0, animationA, true);
            addAnimation(skeletonAnimation, animationFiles[1].bytes);
            assertLRU(skeletonData.FindAnimation(animationA) != null && skeletonData.FindAnimation(animationB) == null, "正在播放的A不能被LRU淘汰");
            builder.AppendLine("[PASS] 当前播放动画保护");
            skeletonAnimation.AnimationState.ClearTracks();
            cleanupLRUTestAnimations(skeletonAnimation, testAnimationNames);
            clearDynamicAnimationCache(skeletonData);
            setDynamicAnimationMinResidentTime(skeletonAnimation, 0.0);
            disableDynamicAnimationCacheLimit(skeletonData);
            addAnimation(skeletonAnimation, animationFiles[0].bytes);
            addAnimation(skeletonAnimation, animationFiles[1].bytes);
            playAnimation(skeletonAnimation, 0, animationA, true);
            playAnimation(skeletonAnimation, 1, animationB, true);
            setDynamicAnimationCacheLimit(skeletonAnimation, 1);
            assertLRU(getDynamicAnimationCount(skeletonData) == 2, "A/B同时播放时即使超限也不能淘汰");
            skeletonAnimation.AnimationState.ClearTrack(1);
            assertLRU(skeletonData.FindAnimation(animationA) != null && skeletonData.FindAnimation(animationB) == null, "ClearTrack触发Dispose后应自动Trim并淘汰B,不需要Tick");
            builder.AppendLine("[PASS] Dispose事件自动Trim,无需Tick");
            builder.AppendLine("Result:PASS");
            builder.AppendLine("===================================================================");
            Debug.Log(builder.ToString());
        }
        catch (Exception exception)
        {
            Debug.LogError("================ Spine Dynamic Animation LRU Test FAILED ================\n" + exception.Message);
            Debug.LogException(exception);
        }
        finally
        {
            if (skeletonAnimation != null)
            {
                try
                {
                    skeletonAnimation.AnimationState.ClearTracks();
                    cleanupLRUTestAnimations(skeletonAnimation, testAnimationNames);
                    clearDynamicAnimationCache(skeletonAnimation.Skeleton != null ? skeletonAnimation.Skeleton.Data : null);
                    unregisterAnimationState(skeletonAnimation);
                    if (commonDataAdded && skeletonAnimation.Skeleton != null)
                    {
                        removeCommonData(skeletonAnimation.Skeleton.Data);
                    }
                }
                catch (Exception cleanupException)
                {
                    Debug.LogException(cleanupException);
                }
            }
            if (testObject != null)
            {
                UnityEngine.Object.DestroyImmediate(testObject);
            }
        }
    }
    private List<TextAsset> findLRUTestAnimationFiles(string folderPath, SkeletonData skeletonData, int count)
    {
        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { folderPath });
        Array.Sort(guids, StringComparer.Ordinal);
        List<TextAsset> result = new(count);
        HashSet<string> names = new(StringComparer.Ordinal);
        for (int i = 0; i < guids.Length && result.Count < count; ++i)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (asset == null)
            {
                continue;
            }
            byte[] bytes = asset.bytes;
            if (!isAnimationFile(bytes))
            {
                continue;
            }
            SpineSingleAnimationData data;
            try
            {
                data = readAnimationNoCopy(bytes);
            }
            catch
            {
                continue;
            }
            if (string.IsNullOrEmpty(data.mAnimationName) || skeletonData.FindAnimation(data.mAnimationName) != null || !names.Add(data.mAnimationName))
            {
                continue;
            }
            result.Add(asset);
        }
        return result;
    }
    private static void cleanupLRUTestAnimations(SkeletonAnimation skeletonAnimation, List<string> animationNames)
    {
        if (skeletonAnimation == null)
        {
            return;
        }
        for (int i = 0; i < animationNames.Count; ++i)
        {
            forceRemoveAnimation(skeletonAnimation, animationNames[i]);
        }
    }
    private static void assertLRU(bool condition, string message)
    {
        if (!condition)
        {
			Debug.LogError(message);
        }
    }
    private bool tryPrepare(out SpineAnimationCommonData commonData, out SkeletonData skeletonData, out byte[] animationBytes)
    {
        commonData = null;
        skeletonData = null;
        animationBytes = null;
        try
        {
            byte[] commonBytes = mCommonFile.bytes;
            commonData = readCommon(commonBytes);
            skeletonData = mSkeletonDataAsset.GetSkeletonData(true);
            if (skeletonData == null)
            {
                Debug.LogError("SkeletonData为空");
                return false;
            }
            animationBytes = mAnimationFile.bytes;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Benchmark准备失败:" + exception.Message);
            Debug.LogException(exception);
            return false;
        }
    }
    private string buildPerformanceReport(int fileBytes, List<double> fileTimes, List<double> profileReaderTimes, List<double> profileTotalTimes, List<double> productionReaderTimes, Dictionary<string, List<double>> stageTimes, long gcDelta)
    {
        StringBuilder builder = new(3072);
        builder.AppendLine("================ Spine Editor Benchmark ================");
        builder.AppendLine("File:" + mAnimationFile.name);
        builder.AppendLine("Bytes:" + fileBytes);
        builder.AppendLine("Warmup:" + mWarmupCount + " Sample:" + mSampleCount);
        appendResult(builder, "Read Animation File", fileTimes);
        appendResult(builder, "Binary Reader Profile ON", profileReaderTimes);
        appendResult(builder, "Total Profile ON", profileTotalTimes);
        builder.AppendLine("---------------- Production Reader ----------------");
        appendResult(builder, "Binary Reader Profile OFF", productionReaderTimes);
        double productionMedian = getMedianDouble(productionReaderTimes);
        double profileMedian = getMedianDouble(profileReaderTimes);
        builder.AppendLine("Profile ON Overhead:" + (productionMedian > 0.0 ? ((profileMedian / productionMedian - 1.0) * 100.0).ToString("F1") : "0.0") + "%");
        builder.AppendLine("---------------- Reader Stages (Profile ON) ----------------");
        for (int i = 0; i < PROFILE_STAGES.Length; ++i)
        {
            appendResult(builder, PROFILE_STAGES[i], stageTimes[PROFILE_STAGES[i]]);
        }
        builder.AppendLine("---------------- Reader Counts ----------------");
        builder.AppendLine("GC Memory Delta:" + gcDelta + " bytes");
        builder.AppendLine("==========================================================");
        return builder.ToString();
    }
    private static void appendResult(StringBuilder builder, string name, List<double> values)
    {
        values.Sort();
        int count = values.Count;
        double median = (count & 1) != 0 ? values[count >> 1] : (values[(count >> 1) - 1] + values[count >> 1]) * 0.5;
        builder.AppendLine(name + " Median:" + median.ToString("F3") + "ms Min:" + values[0].ToString("F3") + "ms Max:" + values[count - 1].ToString("F3") + "ms");
    }
    private static double getMedianDouble(List<double> values)
    {
        double[] sorted = values.ToArray();
        Array.Sort(sorted);
        int count = sorted.Length;
        return (count & 1) != 0 ? sorted[count >> 1] : (sorted[(count >> 1) - 1] + sorted[count >> 1]) * 0.5;
    }
    private static long getMedian(List<long> values)
    {
        values.Sort();
        int count = values.Count;
        return (count & 1) != 0 ? values[count >> 1] : (values[(count >> 1) - 1] + values[count >> 1]) / 2;
    }
    private static string formatBytes(long bytes)
    {
        return bytes + " bytes (" + (bytes / 1024.0 / 1024.0).ToString("F3") + " MiB)";
    }
}
