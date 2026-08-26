using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Spine 4.2 JSON源数据解析工具。仅Editor使用，不进入运行时。
public static class Spine42JsonUtility
{
    private static readonly byte[] ANIMATION_JSON_MAGIC = new byte[] { 0x53, 0x50, 0x4A, 0x41, 0x4E, 0x49, 0x34, 0x32 }; // SPJANI42
    public sealed class SourceData
    {
        public JObject mRoot;
        public JObject mAnimations;
        public string mVersion;
        public string mSkeletonHashText;
        public long mSkeletonHash;
    }
    public static bool isJson(byte[] bytes)
    {
        if (bytes == null)
        {
            return false;
        }
        int index = 0;
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            index = 3;
        }
        while (index < bytes.Length)
        {
            byte value = bytes[index++];
            if (value == (byte)' ' || value == (byte)'\t' || value == (byte)'\r' || value == (byte)'\n')
            {
                continue;
            }
            return value == (byte)'{';
        }
        return false;
    }
    public static SourceData parse(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            throw new ArgumentException("Spine 4.2 JSON数据为空", nameof(bytes));
        }
        string json = Encoding.UTF8.GetString(bytes);
        if (json.Length > 0 && json[0] == '\uFEFF')
        {
            json = json.Substring(1);
        }
        JObject root = JObject.Parse(json);
        JObject skeleton = root["skeleton"] as JObject;
        if (skeleton == null)
        {
            throw new Exception("Spine 4.2 JSON缺少skeleton节点");
        }
        string version = skeleton.Value<string>("spine");
        if (string.IsNullOrEmpty(version) || !version.StartsWith("4.2", StringComparison.Ordinal))
        {
            throw new Exception("当前JSON不是Spine 4.2资源,实际版本:" + version);
        }
        string hashText = skeleton.Value<string>("hash") ?? string.Empty;
        JObject animations = root["animations"] as JObject;
        if (animations == null)
        {
            animations = new JObject();
        }
        SourceData data = new SourceData();
        data.mRoot = root;
        data.mAnimations = animations;
        data.mVersion = version;
        data.mSkeletonHashText = hashText;
        data.mSkeletonHash = SpineSkeletonHashUtility.getStableHash(hashText);
        return data;
    }
    public static SpineBinaryScanResult scan(byte[] bytes)
    {
        SourceData source = parse(bytes);
        SpineBinaryScanResult result = new SpineBinaryScanResult();
        result.mSourceFormat = SpineSourceDataFormat.Json;
        result.mSkeletonHash = source.mSkeletonHash;
        result.mVersion = source.mVersion;
        result.mStrings = new string[0];
        result.mFileLength = bytes.LongLength;
        JArray slots = source.mRoot["slots"] as JArray;
        result.mSlotCount = slots != null ? slots.Count : 0;
        byte[] baseBytes = createAnimationlessSkeletonBytes(source);
        result.mAnimationCountPosition = baseBytes.LongLength;
        result.mAnimationDataPosition = baseBytes.LongLength;
        long position = baseBytes.LongLength;
        int index = 0;
        foreach (JProperty property in source.mAnimations.Properties())
        {
            byte[] payload = getAnimationPayloadBytes(property);
            SpineAnimationBinaryRange range = new SpineAnimationBinaryRange();
            range.mIndex = index++;
            range.mName = property.Name;
            range.mStartPosition = position;
            position += payload.LongLength;
            range.mEndPosition = position;
            result.mAnimations.Add(range);
        }
        return result;
    }
    public static byte[] createAnimationlessSkeletonBytes(SourceData source)
    {
        JObject root = (JObject)source.mRoot.DeepClone();
        root.Remove("animations");
        return Encoding.UTF8.GetBytes(root.ToString(Formatting.None));
    }
    public static byte[] getAnimationPayloadBytes(SourceData source, string animationName)
    {
        JProperty property = source.mAnimations.Property(animationName, StringComparison.Ordinal);
        if (property == null)
        {
            throw new Exception("Spine 4.2 JSON中没有找到动画:" + animationName);
        }
        return getAnimationPayloadBytes(property);
    }
    public static byte[] getAnimationPayloadBytes(JProperty property)
    {
        byte[] jsonBytes = Encoding.UTF8.GetBytes(property.Value.ToString(Formatting.None));
        byte[] payload = new byte[ANIMATION_JSON_MAGIC.Length + jsonBytes.Length];
        Buffer.BlockCopy(ANIMATION_JSON_MAGIC, 0, payload, 0, ANIMATION_JSON_MAGIC.Length);
        Buffer.BlockCopy(jsonBytes, 0, payload, ANIMATION_JSON_MAGIC.Length, jsonBytes.Length);
        return payload;
    }
}
