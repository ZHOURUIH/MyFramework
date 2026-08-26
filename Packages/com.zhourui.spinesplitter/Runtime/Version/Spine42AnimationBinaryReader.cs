#if SPINE_RUNTIME_42
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Spine;

// 读取Spine 4.2拆分单动画。二进制payload走Direct Reader，JSON payload复用官方SkeletonJson动画解析逻辑。
public class Spine42AnimationBinaryReader
{
    private static readonly byte[] ANIMATION_JSON_MAGIC = new byte[] { 0x53, 0x50, 0x4A, 0x41, 0x4E, 0x49, 0x34, 0x32 }; // SPJANI42
    private static readonly MethodInfo mSkeletonJsonReadAnimation = typeof(SkeletonJson).GetMethod("ReadAnimation", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(Dictionary<string, object>), typeof(string), typeof(SkeletonData) }, null);
    private const int BONE_ROTATE = 0;
    private const int BONE_TRANSLATE = 1;
    private const int BONE_TRANSLATEX = 2;
    private const int BONE_TRANSLATEY = 3;
    private const int BONE_SCALE = 4;
    private const int BONE_SCALEX = 5;
    private const int BONE_SCALEY = 6;
    private const int BONE_SHEAR = 7;
    private const int BONE_SHEARX = 8;
    private const int BONE_SHEARY = 9;
    private const int BONE_INHERIT = 10;
    private const int SLOT_ATTACHMENT = 0;
    private const int SLOT_RGBA = 1;
    private const int SLOT_RGB = 2;
    private const int SLOT_RGBA2 = 3;
    private const int SLOT_RGB2 = 4;
    private const int SLOT_ALPHA = 5;
    private const int PATH_POSITION = 0;
    private const int PATH_SPACING = 1;
    private const int PATH_MIX = 2;
    private const int PHYSICS_INERTIA = 0;
    private const int PHYSICS_STRENGTH = 1;
    private const int PHYSICS_DAMPING = 2;
    private const int PHYSICS_MASS = 4;
    private const int PHYSICS_WIND = 5;
    private const int PHYSICS_GRAVITY = 6;
    private const int PHYSICS_MIX = 7;
    private const int PHYSICS_RESET = 8;
    private const int ATTACHMENT_DEFORM = 0;
    private const int ATTACHMENT_SEQUENCE = 1;
    private const int CURVE_STEPPED = 1;
    private const int CURVE_BEZIER = 2;
    private float mScale;
    public Animation readAnimation(byte[] binaryData, string[] strings, SkeletonData skeletonData, float scale, string expectedAnimationName)
    {
        if (binaryData == null)
        {
            throw new ArgumentNullException(nameof(binaryData));
        }
        return readAnimation(binaryData, 0, binaryData.Length, strings, skeletonData, scale, expectedAnimationName);
    }
    public Animation readAnimation(byte[] binaryData, int binaryOffset, int binaryLength, string[] strings, SkeletonData skeletonData, float scale, string expectedAnimationName)
    {
        if (binaryData == null)
        {
            throw new ArgumentNullException(nameof(binaryData));
        }
        if (binaryLength <= 0)
        {
            throw new ArgumentException("动画二进制为空", nameof(binaryLength));
        }
        if (binaryOffset < 0 || binaryOffset > binaryData.Length - binaryLength)
        {
            throw new ArgumentOutOfRangeException(nameof(binaryOffset), "动画二进制范围非法,Offset:" + binaryOffset + ",Length:" + binaryLength + ",DataLength:" + binaryData.Length);
        }
        if (strings == null)
        {
            throw new ArgumentNullException(nameof(strings));
        }
        if (skeletonData == null)
        {
            throw new ArgumentNullException(nameof(skeletonData));
        }
        if (isJsonPayload(binaryData, binaryOffset, binaryLength))
        {
            return readJsonAnimation(binaryData, binaryOffset, binaryLength, skeletonData, scale, expectedAnimationName);
        }
        mScale = scale;
        Spine42AnimationBinaryInput input = new Spine42AnimationBinaryInput(binaryData, binaryOffset, binaryLength, strings);
        string animationName = input.readString();
        if (string.IsNullOrEmpty(animationName))
        {
            throw new InvalidDataException("单动画二进制中的动画名称为空");
        }
        if (!string.IsNullOrEmpty(expectedAnimationName) && !string.Equals(animationName, expectedAnimationName, StringComparison.Ordinal))
        {
            throw new InvalidDataException("动画名称不一致,文件记录:" + expectedAnimationName + ",二进制记录:" + animationName);
        }
        Animation animation = readAnimationBody(animationName, input, skeletonData);
        if (input.Position != input.Length)
        {
            throw new InvalidDataException("动画二进制没有完全读取:" + animationName + ",剩余字节:" + (input.Length - input.Position));
        }
        return animation;
    }
    private static bool isJsonPayload(byte[] data, int offset, int length)
    {
        if (length < ANIMATION_JSON_MAGIC.Length) return false;
        for (int i = 0; i < ANIMATION_JSON_MAGIC.Length; ++i)
        {
            if (data[offset + i] != ANIMATION_JSON_MAGIC[i]) return false;
        }
        return true;
    }
    private static Animation readJsonAnimation(byte[] data, int offset, int length, SkeletonData skeletonData, float scale, string expectedAnimationName)
    {
        if (mSkeletonJsonReadAnimation == null)
        {
            throw new MissingMethodException("Spine 4.2 SkeletonJson.ReadAnimation不存在,请确认Spine Runtime版本与资源版本一致");
        }
        offset += ANIMATION_JSON_MAGIC.Length;
        length -= ANIMATION_JSON_MAGIC.Length;
        string jsonText = Encoding.UTF8.GetString(data, offset, length);
        object jsonObject;
        using (StringReader stringReader = new StringReader(jsonText))
        {
            jsonObject = Spine.Json.Deserialize(stringReader);
        }
        Dictionary<string, object> animationMap = jsonObject as Dictionary<string, object>;
        if (animationMap == null)
        {
            throw new InvalidDataException("Spine 4.2单动画JSON格式无效:" + expectedAnimationName);
        }
        SkeletonJson skeletonJson = new SkeletonJson(new Atlas[0]);
        skeletonJson.Scale = scale;
        int oldAnimationCount = skeletonData.Animations.Count;
        try
        {
            mSkeletonJsonReadAnimation.Invoke(skeletonJson, new object[] { animationMap, expectedAnimationName, skeletonData });
        }
        catch (TargetInvocationException exception)
        {
            throw new InvalidDataException("Spine 4.2单动画JSON解析失败:" + expectedAnimationName, exception.InnerException ?? exception);
        }
        if (skeletonData.Animations.Count != oldAnimationCount + 1)
        {
            throw new InvalidDataException("Spine 4.2单动画JSON解析后Animation数量异常:" + expectedAnimationName);
        }
        Animation animation = skeletonData.Animations.Items[oldAnimationCount];
        skeletonData.Animations.Remove(animation);
        if (animation == null || (!string.IsNullOrEmpty(expectedAnimationName) && !string.Equals(animation.Name, expectedAnimationName, StringComparison.Ordinal)))
        {
            throw new InvalidDataException("Spine 4.2单动画JSON名称不一致:" + expectedAnimationName);
        }
        return animation;
    }
    private Animation readAnimationBody(string name, Spine42AnimationBinaryInput input, SkeletonData skeletonData)
    {
        var timelines = new ExposedList<Timeline>(input.readInt(true));
        float scale = mScale;
        // Slot timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int slotIndex = input.readInt(true);
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                int timelineType = input.readByte();
                int frameCount = input.readInt(true);
                int frameLast = frameCount - 1;
                switch (timelineType)
                {
                    case SLOT_ATTACHMENT:
                        {
                            AttachmentTimeline timeline = new AttachmentTimeline(frameCount, slotIndex);
                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                timeline.SetFrame(frame, input.readFloat(), input.readStringRef());
                            }
                            timelines.Add(timeline);
                            break;
                        }
                    case SLOT_RGBA:
                        {
                            RGBATimeline timeline = new RGBATimeline(frameCount, input.readInt(true), slotIndex);
                            float time = input.readFloat();
                            float r = input.read() / 255f;
                            float g = input.read() / 255f;
                            float b = input.read() / 255f;
                            float a = input.read() / 255f;
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                timeline.SetFrame(frame, time, r, g, b, a);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                float r2 = input.read() / 255f;
                                float g2 = input.read() / 255f;
                                float b2 = input.read() / 255f;
                                float a2 = input.read() / 255f;
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
                                        setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
                                        setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
                                        setBezier(input, timeline, bezier++, frame, 3, time, time2, a, a2, 1);
                                        break;
                                }
                                time = time2;
                                r = r2;
                                g = g2;
                                b = b2;
                                a = a2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                    case SLOT_RGB:
                        {
                            RGBTimeline timeline = new RGBTimeline(frameCount, input.readInt(true), slotIndex);
                            float time = input.readFloat();
                            float r = input.read() / 255f;
                            float g = input.read() / 255f;
                            float b = input.read() / 255f;
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                timeline.SetFrame(frame, time, r, g, b);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                float r2 = input.read() / 255f;
                                float g2 = input.read() / 255f;
                                float b2 = input.read() / 255f;
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, r, r2, 1);
                                        setBezier(input, timeline, bezier++, frame, 1, time, time2, g, g2, 1);
                                        setBezier(input, timeline, bezier++, frame, 2, time, time2, b, b2, 1);
                                        break;
                                }
                                time = time2;
                                r = r2;
                                g = g2;
                                b = b2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                    case SLOT_RGBA2:
                        {
                            RGBA2Timeline timeline = new RGBA2Timeline(frameCount, input.readInt(true), slotIndex);
                            float time = input.readFloat();
                            float r = input.read() / 255f;
                            float g = input.read() / 255f;
                            float b = input.read() / 255f;
                            float a = input.read() / 255f;
                            float r2 = input.read() / 255f;
                            float g2 = input.read() / 255f;
                            float b2 = input.read() / 255f;
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                timeline.SetFrame(frame, time, r, g, b, a, r2, g2, b2);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                float nr = input.read() / 255f;
                                float ng = input.read() / 255f;
                                float nb = input.read() / 255f;
                                float na = input.read() / 255f;
                                float nr2 = input.read() / 255f;
                                float ng2 = input.read() / 255f;
                                float nb2 = input.read() / 255f;
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
                                        setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
                                        setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
                                        setBezier(input, timeline, bezier++, frame, 3, time, time2, a, na, 1);
                                        setBezier(input, timeline, bezier++, frame, 4, time, time2, r2, nr2, 1);
                                        setBezier(input, timeline, bezier++, frame, 5, time, time2, g2, ng2, 1);
                                        setBezier(input, timeline, bezier++, frame, 6, time, time2, b2, nb2, 1);
                                        break;
                                }
                                time = time2;
                                r = nr;
                                g = ng;
                                b = nb;
                                a = na;
                                r2 = nr2;
                                g2 = ng2;
                                b2 = nb2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                    case SLOT_RGB2:
                        {
                            RGB2Timeline timeline = new RGB2Timeline(frameCount, input.readInt(true), slotIndex);
                            float time = input.readFloat();
                            float r = input.read() / 255f;
                            float g = input.read() / 255f;
                            float b = input.read() / 255f;
                            float r2 = input.read() / 255f;
                            float g2 = input.read() / 255f;
                            float b2 = input.read() / 255f;
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                timeline.SetFrame(frame, time, r, g, b, r2, g2, b2);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                float nr = input.read() / 255f;
                                float ng = input.read() / 255f;
                                float nb = input.read() / 255f;
                                float nr2 = input.read() / 255f;
                                float ng2 = input.read() / 255f;
                                float nb2 = input.read() / 255f;
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, r, nr, 1);
                                        setBezier(input, timeline, bezier++, frame, 1, time, time2, g, ng, 1);
                                        setBezier(input, timeline, bezier++, frame, 2, time, time2, b, nb, 1);
                                        setBezier(input, timeline, bezier++, frame, 3, time, time2, r2, nr2, 1);
                                        setBezier(input, timeline, bezier++, frame, 4, time, time2, g2, ng2, 1);
                                        setBezier(input, timeline, bezier++, frame, 5, time, time2, b2, nb2, 1);
                                        break;
                                }
                                time = time2;
                                r = nr;
                                g = ng;
                                b = nb;
                                r2 = nr2;
                                g2 = ng2;
                                b2 = nb2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                    case SLOT_ALPHA:
                        {
                            AlphaTimeline timeline = new AlphaTimeline(frameCount, input.readInt(true), slotIndex);
                            float time = input.readFloat();
                            float a = input.read() / 255f;
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                timeline.SetFrame(frame, time, a);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                float a2 = input.read() / 255f;
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, a, a2, 1);
                                        break;
                                }
                                time = time2;
                                a = a2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                }
            }
        }
        // Bone timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int boneIndex = input.readInt(true);
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                int type = input.readByte();
                int frameCount = input.readInt(true);
                if (type == BONE_INHERIT)
                {
                    InheritTimeline timeline = new InheritTimeline(frameCount, boneIndex);
                    for (int frame = 0; frame < frameCount; ++frame)
                    {
                        float time = input.readFloat();
                        int inheritIndex = input.readByte();
                        if (inheritIndex < 0 || inheritIndex >= InheritEnum.Values.Length)
                        {
                            throw new SerializationException("Unknown inherit value: " + inheritIndex);
                        }
                        timeline.SetFrame(frame, time, InheritEnum.Values[inheritIndex]);
                    }
                    timelines.Add(timeline);
                    continue;
                }
                int bezierCount = input.readInt(true);
                switch (type)
                {
                    case BONE_ROTATE:
                        timelines.Add(readTimeline(input, new RotateTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_TRANSLATE:
                        timelines.Add(readTimeline(input, new TranslateTimeline(frameCount, bezierCount, boneIndex), scale));
                        break;
                    case BONE_TRANSLATEX:
                        timelines.Add(readTimeline(input, new TranslateXTimeline(frameCount, bezierCount, boneIndex), scale));
                        break;
                    case BONE_TRANSLATEY:
                        timelines.Add(readTimeline(input, new TranslateYTimeline(frameCount, bezierCount, boneIndex), scale));
                        break;
                    case BONE_SCALE:
                        timelines.Add(readTimeline(input, new ScaleTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_SCALEX:
                        timelines.Add(readTimeline(input, new ScaleXTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_SCALEY:
                        timelines.Add(readTimeline(input, new ScaleYTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_SHEAR:
                        timelines.Add(readTimeline(input, new ShearTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_SHEARX:
                        timelines.Add(readTimeline(input, new ShearXTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    case BONE_SHEARY:
                        timelines.Add(readTimeline(input, new ShearYTimeline(frameCount, bezierCount, boneIndex), 1));
                        break;
                    default:
                        throw new SerializationException("Unknown bone timeline type: " + type);
                }
            }
        }
        // IK constraint timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int index = input.readInt(true);
            int frameCount = input.readInt(true);
            int frameLast = frameCount - 1;
            IkConstraintTimeline timeline = new IkConstraintTimeline(frameCount, input.readInt(true), index);
            int flags = input.read();
            float time = input.readFloat();
            float mix = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1.0f) : 0.0f;
            float softness = (flags & 4) != 0 ? input.readFloat() * scale : 0.0f;
            for (int frame = 0, bezier = 0; ; frame++)
            {
                timeline.SetFrame(frame, time, mix, softness, (flags & 8) != 0 ? 1 : -1, (flags & 16) != 0, (flags & 32) != 0);
                if (frame == frameLast)
                {
                    break;
                }
                flags = input.read();
                float time2 = input.readFloat();
                float mix2 = (flags & 1) != 0 ? ((flags & 2) != 0 ? input.readFloat() : 1.0f) : 0.0f;
                float softness2 = (flags & 4) != 0 ? input.readFloat() * scale : 0.0f;
                if ((flags & 64) != 0)
                {
                    timeline.SetStepped(frame);
                }
                else if ((flags & 128) != 0)
                {
                    setBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
                    setBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
                }
                time = time2;
                mix = mix2;
                softness = softness2;
            }
            timelines.Add(timeline);
        }
        // Transform constraint timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int index = input.readInt(true);
            int frameCount = input.readInt(true);
            int frameLast = frameCount - 1;
            TransformConstraintTimeline timeline = new TransformConstraintTimeline(frameCount, input.readInt(true), index);
            float time = input.readFloat();
            float mixRotate = input.readFloat();
            float mixX = input.readFloat();
            float mixY = input.readFloat();
            float mixScaleX = input.readFloat();
            float mixScaleY = input.readFloat();
            float mixShearY = input.readFloat();
            for (int frame = 0, bezier = 0; ; frame++)
            {
                timeline.SetFrame(frame, time, mixRotate, mixX, mixY, mixScaleX, mixScaleY, mixShearY);
                if (frame == frameLast)
                {
                    break;
                }
                float time2 = input.readFloat();
                float mixRotate2 = input.readFloat();
                float mixX2 = input.readFloat();
                float mixY2 = input.readFloat();
                float mixScaleX2 = input.readFloat();
                float mixScaleY2 = input.readFloat();
                float mixShearY2 = input.readFloat();
                switch (input.readByte())
                {
                    case CURVE_STEPPED:
                        timeline.SetStepped(frame);
                        break;
                    case CURVE_BEZIER:
                        setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
                        setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
                        setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
                        setBezier(input, timeline, bezier++, frame, 3, time, time2, mixScaleX, mixScaleX2, 1);
                        setBezier(input, timeline, bezier++, frame, 4, time, time2, mixScaleY, mixScaleY2, 1);
                        setBezier(input, timeline, bezier++, frame, 5, time, time2, mixShearY, mixShearY2, 1);
                        break;
                }
                time = time2;
                mixRotate = mixRotate2;
                mixX = mixX2;
                mixY = mixY2;
                mixScaleX = mixScaleX2;
                mixScaleY = mixScaleY2;
                mixShearY = mixShearY2;
            }
            timelines.Add(timeline);
        }
        // Path constraint timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int index = input.readInt(true);
            PathConstraintData data = skeletonData.PathConstraints.Items[index];
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                switch (input.readByte())
                {
                    case PATH_POSITION:
                        timelines.Add(readTimeline(input, new PathConstraintPositionTimeline(input.readInt(true), input.readInt(true), index), data.PositionMode == PositionMode.Fixed ? scale : 1));
                        break;
                    case PATH_SPACING:
                        timelines.Add(readTimeline(input, new PathConstraintSpacingTimeline(input.readInt(true), input.readInt(true), index), data.SpacingMode == SpacingMode.Length || data.SpacingMode == SpacingMode.Fixed ? scale : 1));
                        break;
                    case PATH_MIX:
                        PathConstraintMixTimeline timeline = new PathConstraintMixTimeline(input.readInt(true), input.readInt(true), index);
                        float time = input.readFloat();
                        float mixRotate = input.readFloat();
                        float mixX = input.readFloat();
                        float mixY = input.readFloat();
                        for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++)
                        {
                            timeline.SetFrame(frame, time, mixRotate, mixX, mixY);
                            if (frame == frameLast)
                            {
                                break;
                            }
                            float time2 = input.readFloat();
                            float mixRotate2 = input.readFloat();
                            float mixX2 = input.readFloat();
                            float mixY2 = input.readFloat();
                            switch (input.readByte())
                            {
                                case CURVE_STEPPED:
                                    timeline.SetStepped(frame);
                                    break;
                                case CURVE_BEZIER:
                                    setBezier(input, timeline, bezier++, frame, 0, time, time2, mixRotate, mixRotate2, 1);
                                    setBezier(input, timeline, bezier++, frame, 1, time, time2, mixX, mixX2, 1);
                                    setBezier(input, timeline, bezier++, frame, 2, time, time2, mixY, mixY2, 1);
                                    break;
                            }
                            time = time2;
                            mixRotate = mixRotate2;
                            mixX = mixX2;
                            mixY = mixY2;
                        }
                        timelines.Add(timeline);
                        break;
                }
            }
        }
        // Physics timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            int index = input.readInt(true) - 1;
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                int type = input.readByte();
                int frameCount = input.readInt(true);
                if (type == PHYSICS_RESET)
                {
                    PhysicsConstraintResetTimeline timeline = new PhysicsConstraintResetTimeline(frameCount, index);
                    for (int frame = 0; frame < frameCount; ++frame)
                    {
                        timeline.SetFrame(frame, input.readFloat());
                    }
                    timelines.Add(timeline);
                    continue;
                }
                int bezierCount = input.readInt(true);
                switch (type)
                {
                    case PHYSICS_INERTIA:
                        timelines.Add(readTimeline(input, new PhysicsConstraintInertiaTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_STRENGTH:
                        timelines.Add(readTimeline(input, new PhysicsConstraintStrengthTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_DAMPING:
                        timelines.Add(readTimeline(input, new PhysicsConstraintDampingTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_MASS:
                        timelines.Add(readTimeline(input, new PhysicsConstraintMassTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_WIND:
                        timelines.Add(readTimeline(input, new PhysicsConstraintWindTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_GRAVITY:
                        timelines.Add(readTimeline(input, new PhysicsConstraintGravityTimeline(frameCount, bezierCount, index), 1));
                        break;
                    case PHYSICS_MIX:
                        timelines.Add(readTimeline(input, new PhysicsConstraintMixTimeline(frameCount, bezierCount, index), 1));
                        break;
                    default:
                        throw new SerializationException("Unknown physics timeline type: " + type);
                }
            }
        }
        // Attachment timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            Skin skin = skeletonData.Skins.Items[input.readInt(true)];
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                int slotIndex = input.readInt(true);
                for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++)
                {
                    string attachmentName = input.readStringRef();
                    Attachment attachment = skin.GetAttachment(slotIndex, attachmentName);
                    if (attachment == null)
                    {
                        throw new SerializationException("Timeline attachment not found: " + attachmentName);
                    }
                    int timelineType = input.readByte();
                    int frameCount = input.readInt(true);
                    int frameLast = frameCount - 1;
                    switch (timelineType)
                    {
                        case ATTACHMENT_DEFORM:
                        {
                            VertexAttachment vertexAttachment = attachment as VertexAttachment;
                            if (vertexAttachment == null)
                            {
                                throw new SerializationException("Deform timeline attachment is not VertexAttachment: " + attachmentName);
                            }
                            bool weighted = vertexAttachment.Bones != null;
                            float[] vertices = vertexAttachment.Vertices;
                            int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;
                            DeformTimeline timeline = new DeformTimeline(frameCount, input.readInt(true), slotIndex, vertexAttachment);
                            float time = input.readFloat();
                            for (int frame = 0, bezier = 0; ; frame++)
                            {
                                float[] deform;
                                int end = input.readInt(true);
                                if (end == 0)
                                {
                                    deform = weighted ? new float[deformLength] : vertices;
                                }
                                else
                                {
                                    deform = new float[deformLength];
                                    int start = input.readInt(true);
                                    end += start;
                                    if (scale == 1)
                                    {
                                        for (int v = start; v < end; v++)
                                        {
                                            deform[v] = input.readFloat();
                                        }
                                    }
                                    else
                                    {
                                        for (int v = start; v < end; v++)
                                        {
                                            deform[v] = input.readFloat() * scale;
                                        }
                                    }
                                    if (!weighted)
                                    {
                                        for (int v = 0, vn = deform.Length; v < vn; v++)
                                        {
                                            deform[v] += vertices[v];
                                        }
                                    }
                                }
                                timeline.SetFrame(frame, time, deform);
                                if (frame == frameLast)
                                {
                                    break;
                                }
                                float time2 = input.readFloat();
                                switch (input.readByte())
                                {
                                    case CURVE_STEPPED:
                                        timeline.SetStepped(frame);
                                        break;
                                    case CURVE_BEZIER:
                                        setBezier(input, timeline, bezier++, frame, 0, time, time2, 0, 1, 1);
                                        break;
                                }
                                time = time2;
                            }
                            timelines.Add(timeline);
                            break;
                        }
                        case ATTACHMENT_SEQUENCE:
                        {
                            SequenceTimeline timeline = new SequenceTimeline(frameCount, slotIndex, attachment);
                            for (int frame = 0; frame < frameCount; frame++)
                            {
                                float time = input.readFloat();
                                int modeAndIndex = input.readInt32();
                                timeline.SetFrame(frame, time, (SequenceMode)(modeAndIndex & 0xF), modeAndIndex >> 4, input.readFloat());
                            }
                            timelines.Add(timeline);
                            break;
                        }
                        default:
                            throw new SerializationException("Unknown attachment timeline type: " + timelineType);
                    }
                }
            }
        }
        // Draw order timeline.
        int drawOrderCount = input.readInt(true);
        if (drawOrderCount > 0)
        {
            DrawOrderTimeline timeline = new DrawOrderTimeline(drawOrderCount);
            int slotCount = skeletonData.Slots.Count;
            for (int i = 0; i < drawOrderCount; i++)
            {
                float time = input.readFloat();
                int offsetCount = input.readInt(true);
                int[] drawOrder = new int[slotCount];
                for (int ii = slotCount - 1; ii >= 0; ii--)
                {
                    drawOrder[ii] = -1;
                }
                int[] unchanged = new int[slotCount - offsetCount];
                int originalIndex = 0;
                int unchangedIndex = 0;
                for (int ii = 0; ii < offsetCount; ii++)
                {
                    int slotIndex = input.readInt(true);
                    // Collect unchanged items.
                    while (originalIndex != slotIndex)
                    {
                        unchanged[unchangedIndex++] = originalIndex++;
                    }
                    // Set changed items.
                    drawOrder[originalIndex + input.readInt(true)] = originalIndex++;
                }
                // Collect remaining unchanged items.
                while (originalIndex < slotCount)
                {
                    unchanged[unchangedIndex++] = originalIndex++;
                }
                // Fill in unchanged items.
                for (int ii = slotCount - 1; ii >= 0; ii--)
                {
                    if (drawOrder[ii] == -1)
                    {
                        drawOrder[ii] = unchanged[--unchangedIndex];
                    }
                }
                timeline.SetFrame(i, time, drawOrder);
            }
            timelines.Add(timeline);
        }
        // Event timeline.
        int eventCount = input.readInt(true);
        if (eventCount > 0)
        {
            EventTimeline timeline = new EventTimeline(eventCount);
            for (int i = 0; i < eventCount; i++)
            {
                float time = input.readFloat();
                EventData eventData = skeletonData.Events.Items[input.readInt(true)];
                Event e = new Event(time, eventData);
                e.Int = input.readInt(false);
                e.Float = input.readFloat();
                e.String = input.readString();
                if (e.String == null)
                {
                    e.String = eventData.String;
                }
                if (e.Data.AudioPath != null)
                {
                    e.Volume = input.readFloat();
                    e.Balance = input.readFloat();
                }
                timeline.SetFrame(i, e);
            }
            timelines.Add(timeline);
        }
        float duration = 0;
        var items = timelines.Items;
        for (int i = 0, n = timelines.Count; i < n; i++)
        {
            duration = Math.Max(duration, items[i].Duration);
        }
        return new Animation(name, timelines, duration);
    }
    // 读取曲线时间轴。
    private Timeline readTimeline(Spine42AnimationBinaryInput input, CurveTimeline1 timeline, float scale)
    {
        float time = input.readFloat();
        float value = input.readFloat() * scale;
        for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++)
        {
            timeline.SetFrame(frame, time, value);
            if (frame == frameLast)
            {
                break;
            }
            float time2 = input.readFloat();
            float value2 = input.readFloat() * scale;
            switch (input.readByte())
            {
                case CURVE_STEPPED:
                    timeline.SetStepped(frame);
                    break;
                case CURVE_BEZIER:
                    setBezier(input, timeline, bezier++, frame, 0, time, time2, value, value2, scale);
                    break;
            }
            time = time2;
            value = value2;
        }
        return timeline;
    }
    // 读取曲线时间轴。
    private Timeline readTimeline(Spine42AnimationBinaryInput input, CurveTimeline2 timeline, float scale)
    {
        float time = input.readFloat();
        float value1 = input.readFloat() * scale;
        float value2 = input.readFloat() * scale;
        for (int frame = 0, bezier = 0, frameLast = timeline.FrameCount - 1; ; frame++)
        {
            timeline.SetFrame(frame, time, value1, value2);
            if (frame == frameLast)
            {
                break;
            }
            float time2 = input.readFloat();
            float nvalue1 = input.readFloat() * scale;
            float nvalue2 = input.readFloat() * scale;
            switch (input.readByte())
            {
                case CURVE_STEPPED:
                    timeline.SetStepped(frame);
                    break;
                case CURVE_BEZIER:
                    setBezier(input, timeline, bezier++, frame, 0, time, time2, value1, nvalue1, scale);
                    setBezier(input, timeline, bezier++, frame, 1, time, time2, value2, nvalue2, scale);
                    break;
            }
            time = time2;
            value1 = nvalue1;
            value2 = nvalue2;
        }
        return timeline;
    }
    // 读取曲线时间轴。
    void setBezier(Spine42AnimationBinaryInput input, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2, float value1, float value2, float scale)
    {
        timeline.SetBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale, time2, value2);
    }
    [StructLayout(LayoutKind.Explicit)]
    private struct IntFloatUnion
    {
        [FieldOffset(0)] public int mInt;
        [FieldOffset(0)] public float mFloat;
    }
    private class Spine42AnimationBinaryInput
    {
        private readonly byte[] mData;
        private readonly string[] mStrings;
        private readonly int mStart;
        private readonly int mEnd;
        private int mPosition;
        public int Position => mPosition - mStart;
        public int Length => mEnd - mStart;
        public Spine42AnimationBinaryInput(byte[] data, int offset, int length, string[] strings)
        {
            mData = data ?? throw new ArgumentNullException(nameof(data));
            mStrings = strings ?? throw new ArgumentNullException(nameof(strings));
            if (length <= 0 || offset < 0 || offset > data.Length - length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "二进制读取范围非法,Offset:" + offset + ",Length:" + length + ",DataLength:" + data.Length);
            }
            mStart = offset;
            mPosition = offset;
            mEnd = offset + length;
        }
        public int read()
        {
            if ((uint)mPosition >= (uint)mEnd)
            {
                throw new EndOfStreamException();
            }
            return mData[mPosition++];
        }
        public byte readByte()
        {
            if ((uint)mPosition >= (uint)mEnd)
            {
                throw new EndOfStreamException();
            }
            return mData[mPosition++];
        }
        public sbyte readSByte()
        {
            if ((uint)mPosition >= (uint)mEnd)
            {
                throw new EndOfStreamException();
            }
            return (sbyte)mData[mPosition++];
        }
        public bool readBoolean()
        {
            if ((uint)mPosition >= (uint)mEnd)
            {
                throw new EndOfStreamException();
            }
            return mData[mPosition++] != 0;
        }
        public float readFloat()
        {
            int position = mPosition;
            if ((uint)position > (uint)(mEnd - 4))
            {
                throw new EndOfStreamException();
            }
            byte[] data = mData;
            int bits = (data[position] << 24) | (data[position + 1] << 16) | (data[position + 2] << 8) | data[position + 3];
            mPosition = position + 4;
            IntFloatUnion value = new IntFloatUnion { mInt = bits };
            return value.mFloat;
        }
        public int readInt32()
        {
            int position = mPosition;
            if ((uint)position > (uint)(mEnd - 4))
            {
                throw new EndOfStreamException();
            }
            byte[] data = mData;
            int value = (data[position] << 24) | (data[position + 1] << 16) | (data[position + 2] << 8) | data[position + 3];
            mPosition = position + 4;
            return value;
        }
        public int readInt(bool optimizePositive)
        {
            int position = mPosition;
            byte[] data = mData;
            if ((uint)position >= (uint)mEnd)
            {
                throw new EndOfStreamException();
            }
            int b = data[position++];
            int result = b & 0x7F;
            if ((b & 0x80) != 0)
            {
                if ((uint)position >= (uint)mEnd)
                {
                    throw new EndOfStreamException();
                }
                b = data[position++];
                result |= (b & 0x7F) << 7;
                if ((b & 0x80) != 0)
                {
                    if ((uint)position >= (uint)mEnd)
                    {
                        throw new EndOfStreamException();
                    }
                    b = data[position++];
                    result |= (b & 0x7F) << 14;
                    if ((b & 0x80) != 0)
                    {
                        if ((uint)position >= (uint)mEnd)
                        {
                            throw new EndOfStreamException();
                        }
                        b = data[position++];
                        result |= (b & 0x7F) << 21;
                        if ((b & 0x80) != 0)
                        {
                            if ((uint)position >= (uint)mEnd)
                            {
                                throw new EndOfStreamException();
                            }
                            result |= (data[position++] & 0x7F) << 28;
                        }
                    }
                }
            }
            mPosition = position;
            return optimizePositive ? result : (int)((uint)result >> 1) ^ -(result & 1);
        }
        public string readString()
        {
            int byteCount = readInt(true);
            if (byteCount == 0)
            {
                return null;
            }
            if (byteCount == 1)
            {
                return string.Empty;
            }
            --byteCount;
            int position = mPosition;
            if (byteCount < 0 || position > mEnd - byteCount)
            {
                throw new EndOfStreamException();
            }
            string value = Encoding.UTF8.GetString(mData, position, byteCount);
            mPosition = position + byteCount;
            return value;
        }
        public string readStringRef()
        {
            int index = readInt(true);
            if (index == 0)
            {
                return null;
            }
            if (index < 1 || index > mStrings.Length)
            {
                throw new InvalidDataException("共享字符串索引越界:" + index + ",数量:" + mStrings.Length);
            }
            return mStrings[index - 1];
        }
    }
}
#endif
