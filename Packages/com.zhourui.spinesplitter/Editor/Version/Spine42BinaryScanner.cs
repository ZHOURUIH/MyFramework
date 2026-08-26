using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Spine 4.2二进制文件扫描器,用于在不修改Spine官方库且不创建SkeletonData、Animation、Timeline等运行时对象的情况下解析并跳过.skel.bytes中的数据,记录生成基础Skeleton文件和独立动作包所需的信息。
public class Spine42BinaryScanner
{
    protected const int ATTACHMENT_REGION = 0;
    protected const int ATTACHMENT_BOUNDING_BOX = 1;
    protected const int ATTACHMENT_MESH = 2;
    protected const int ATTACHMENT_LINKED_MESH = 3;
    protected const int ATTACHMENT_PATH = 4;
    protected const int ATTACHMENT_POINT = 5;
    protected const int ATTACHMENT_CLIPPING = 6;
    protected const int SLOT_ATTACHMENT = 0;
    protected const int SLOT_RGBA = 1;
    protected const int SLOT_RGB = 2;
    protected const int SLOT_RGBA2 = 3;
    protected const int SLOT_RGB2 = 4;
    protected const int SLOT_ALPHA = 5;
    protected const int ATTACHMENT_DEFORM = 0;
    protected const int ATTACHMENT_SEQUENCE = 1;
    protected const int BONE_ROTATE = 0;
    protected const int BONE_TRANSLATE = 1;
    protected const int BONE_TRANSLATE_X = 2;
    protected const int BONE_TRANSLATE_Y = 3;
    protected const int BONE_SCALE = 4;
    protected const int BONE_SCALE_X = 5;
    protected const int BONE_SCALE_Y = 6;
    protected const int BONE_SHEAR = 7;
    protected const int BONE_SHEAR_X = 8;
    protected const int BONE_SHEAR_Y = 9;
    protected const int BONE_INHERIT = 10;
    protected const int PATH_POSITION = 0;
    protected const int PATH_SPACING = 1;
    protected const int PATH_MIX = 2;
    protected const int PHYSICS_INERTIA = 0;
    protected const int PHYSICS_STRENGTH = 1;
    protected const int PHYSICS_DAMPING = 2;
    protected const int PHYSICS_MASS = 4;
    protected const int PHYSICS_WIND = 5;
    protected const int PHYSICS_GRAVITY = 6;
    protected const int PHYSICS_MIX = 7;
    protected const int PHYSICS_RESET = 8;
    protected const int CURVE_LINEAR = 0;
    protected const int CURVE_STEPPED = 1;
    protected const int CURVE_BEZIER = 2;
    public SpineBinaryScanResult scan(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        using (MemoryStream stream = new MemoryStream(bytes, false))
        {
            Spine42Input input = new Spine42Input(stream);
            SpineBinaryScanResult result = new SpineBinaryScanResult();
            result.mFileLength = bytes.LongLength;
            scanSkeleton(input, result);
            if (input.mPosition != input.mLength)
            {
                throw createFormatException(input, "扫描结束后仍存在未读取数据，剩余字节:" + (input.mLength - input.mPosition));
            }
            return result;
        }
    }
    protected void scanSkeleton(Spine42Input input, SpineBinaryScanResult result)
    {
        result.mSkeletonHash = input.readInt64();
        result.mVersion = input.readString();
        if (string.IsNullOrEmpty(result.mVersion))
        {
            throw createFormatException(input, "无法读取Spine版本");
        }
        if (!result.mVersion.StartsWith("4.2", StringComparison.Ordinal))
        {
            throw createFormatException(input, "当前扫描器只支持Spine 4.2，文件版本:" + result.mVersion);
        }
        input.skipFloat(5);
        bool nonessential = input.readBoolean();
        if (nonessential)
        {
            input.skipFloat(1);
            input.skipString();
            input.skipString();
        }
        int stringCount = input.readVarInt(true);
        result.mStrings = new string[stringCount];
        for (int i = 0; i < stringCount; ++i)
        {
            result.mStrings[i] = input.readString();
        }
        skipBones(input, nonessential);
        result.mSlotCount = skipSlots(input, nonessential);
        skipIkConstraints(input);
        skipTransformConstraints(input);
        skipPathConstraints(input);
        skipPhysicsConstraints(input);
        skipSkin(input, true, nonessential);
        int skinCount = input.readVarInt(true);
        for (int i = 0; i < skinCount; ++i)
        {
            skipSkin(input, false, nonessential);
        }
        bool[] eventHasAudio = skipEvents(input);
        result.mAnimationCountPosition = input.mPosition;
        int animationCount = input.readVarInt(true);
        result.mAnimationDataPosition = input.mPosition;
        for (int i = 0; i < animationCount; ++i)
        {
            long startPosition = input.mPosition;
            string animationName = input.readString();
            if (animationName == null)
            {
                throw createFormatException(input, "第" + i + "个动画名称为null");
            }
            skipAnimation(input, result.mSlotCount, eventHasAudio);
            SpineAnimationBinaryRange range = new SpineAnimationBinaryRange();
            range.mIndex = i;
            range.mName = animationName;
            range.mStartPosition = startPosition;
            range.mEndPosition = input.mPosition;
            result.mAnimations.Add(range);
        }
    }
    protected void skipBones(Spine42Input input, bool nonessential)
    {
        int boneCount = input.readVarInt(true);
        for (int i = 0; i < boneCount; ++i)
        {
            input.skipString();
            if (i != 0)
            {
                input.skipVarInt();
            }
            input.skipFloat(8);
            input.skipVarInt();
            input.skipBoolean();
            if (nonessential)
            {
                input.skipInt32();
                input.skipString();
                input.skipBoolean();
            }
        }
    }
    protected int skipSlots(Spine42Input input, bool nonessential)
    {
        int slotCount = input.readVarInt(true);
        for (int i = 0; i < slotCount; ++i)
        {
            input.skipString();
            input.skipVarInt();
            input.skipInt32();
            input.skipInt32();
            input.skipStringRef();
            input.skipVarInt();
            if (nonessential)
            {
                input.skipBoolean();
            }
        }
        return slotCount;
    }
    protected void skipIkConstraints(Spine42Input input)
    {
        int count = input.readVarInt(true);
        for (int i = 0; i < count; ++i)
        {
            input.skipString();
            input.skipVarInt();
            skipIndexArray(input);
            input.skipVarInt();
            int flags = input.readByte();
            if ((flags & 32) != 0 && (flags & 64) != 0)
            {
                input.skipFloat(1);
            }
            if ((flags & 128) != 0)
            {
                input.skipFloat(1);
            }
        }
    }
    protected void skipTransformConstraints(Spine42Input input)
    {
        int count = input.readVarInt(true);
        for (int i = 0; i < count; ++i)
        {
            input.skipString();
            input.skipVarInt();
            skipIndexArray(input);
            input.skipVarInt();
            int flags = input.readByte();
            if ((flags & 8) != 0) input.skipFloat(1);
            if ((flags & 16) != 0) input.skipFloat(1);
            if ((flags & 32) != 0) input.skipFloat(1);
            if ((flags & 64) != 0) input.skipFloat(1);
            if ((flags & 128) != 0) input.skipFloat(1);
            flags = input.readByte();
            if ((flags & 1) != 0) input.skipFloat(1);
            if ((flags & 2) != 0) input.skipFloat(1);
            if ((flags & 4) != 0) input.skipFloat(1);
            if ((flags & 8) != 0) input.skipFloat(1);
            if ((flags & 16) != 0) input.skipFloat(1);
            if ((flags & 32) != 0) input.skipFloat(1);
            if ((flags & 64) != 0) input.skipFloat(1);
        }
    }
    protected void skipPathConstraints(Spine42Input input)
    {
        int count = input.readVarInt(true);
        for (int i = 0; i < count; ++i)
        {
            input.skipString();
            input.skipVarInt();
            input.skipBoolean();
            skipIndexArray(input);
            input.skipVarInt();
            int flags = input.readByte();
            if ((flags & 128) != 0)
            {
                input.skipFloat(1);
            }
            input.skipFloat(5);
        }
    }
    protected void skipPhysicsConstraints(Spine42Input input)
    {
        int count = input.readVarInt(true);
        for (int i = 0; i < count; ++i)
        {
            input.skipString();
            input.skipVarInt();
            input.skipVarInt();
            int flags = input.readByte();
            if ((flags & 2) != 0) input.skipFloat(1);
            if ((flags & 4) != 0) input.skipFloat(1);
            if ((flags & 8) != 0) input.skipFloat(1);
            if ((flags & 16) != 0) input.skipFloat(1);
            if ((flags & 32) != 0) input.skipFloat(1);
            if ((flags & 64) != 0) input.skipFloat(1);
            input.skipByte();
            input.skipFloat(3);
            if ((flags & 128) != 0) input.skipFloat(1);
            input.skipFloat(2);
            flags = input.readByte();
            if ((flags & 128) != 0) input.skipFloat(1);
        }
    }
    protected void skipSkin(Spine42Input input, bool defaultSkin, bool nonessential)
    {
        int slotCount;
        if (defaultSkin)
        {
            slotCount = input.readVarInt(true);
            if (slotCount == 0)
            {
                return;
            }
        }
        else
        {
            input.skipString();
            if (nonessential)
            {
                input.skipInt32();
            }
            skipIndexArray(input);
            skipIndexArray(input);
            skipIndexArray(input);
            skipIndexArray(input);
            skipIndexArray(input);
            slotCount = input.readVarInt(true);
        }
        for (int i = 0; i < slotCount; ++i)
        {
            input.skipVarInt();
            int attachmentCount = input.readVarInt(true);
            for (int j = 0; j < attachmentCount; ++j)
            {
                input.skipStringRef();
                skipAttachment(input, nonessential);
            }
        }
    }
    protected void skipIndexArray(Spine42Input input)
    {
        int count = input.readVarInt(true);
        for (int i = 0; i < count; ++i)
        {
            input.skipVarInt();
        }
    }
    protected void skipAttachment(Spine42Input input, bool nonessential)
    {
        int flags = input.readByte();
        int attachmentType = flags & 7;
        if ((flags & 8) != 0)
        {
            input.skipStringRef();
        }
        switch (attachmentType)
        {
            case ATTACHMENT_REGION:
                if ((flags & 16) != 0) input.skipStringRef();
                if ((flags & 32) != 0) input.skipInt32();
                if ((flags & 64) != 0) skipSequence(input);
                if ((flags & 128) != 0) input.skipFloat(1);
                input.skipFloat(6);
                break;
            case ATTACHMENT_BOUNDING_BOX:
                skipVertices(input, (flags & 16) != 0);
                if (nonessential) input.skipInt32();
                break;
            case ATTACHMENT_MESH:
                {
                    if ((flags & 16) != 0) input.skipStringRef();
                    if ((flags & 32) != 0) input.skipInt32();
                    if ((flags & 64) != 0) skipSequence(input);
                    int hullLength = input.readVarInt(true);
                    int vertexCount = skipVertices(input, (flags & 128) != 0);
                    int verticesLength = vertexCount << 1;
                    input.skipFloat(verticesLength);
                    int triangleCount = (verticesLength - hullLength - 2) * 3;
                    if (triangleCount < 0)
                    {
                        throw createFormatException(input, "Mesh三角形数量非法:" + triangleCount);
                    }
                    skipVarIntArray(input, triangleCount);
                    if (nonessential)
                    {
                        skipVarIntArray(input, input.readVarInt(true));
                        input.skipFloat(2);
                    }
                    break;
                }
            case ATTACHMENT_LINKED_MESH:
                if ((flags & 16) != 0) input.skipStringRef();
                if ((flags & 32) != 0) input.skipInt32();
                if ((flags & 64) != 0) skipSequence(input);
                input.skipVarInt();
                input.skipStringRef();
                if (nonessential) input.skipFloat(2);
                break;
            case ATTACHMENT_PATH:
                {
                    int vertexCount = skipVertices(input, (flags & 64) != 0);
                    input.skipFloat(vertexCount / 3);
                    if (nonessential) input.skipInt32();
                    break;
                }
            case ATTACHMENT_POINT:
                input.skipFloat(3);
                if (nonessential) input.skipInt32();
                break;
            case ATTACHMENT_CLIPPING:
                input.skipVarInt();
                skipVertices(input, (flags & 16) != 0);
                if (nonessential) input.skipInt32();
                break;
            default:
                throw createFormatException(input, "未知Attachment类型:" + attachmentType);
        }
    }
    protected void skipSequence(Spine42Input input)
    {
        input.skipVarInt();
        input.skipVarInt();
        input.skipVarInt();
        input.skipVarInt();
    }
    protected int skipVertices(Spine42Input input, bool weighted)
    {
        int vertexCount = input.readVarInt(true);
        if (!weighted)
        {
            input.skipFloat(vertexCount << 1);
            return vertexCount;
        }
        for (int i = 0; i < vertexCount; ++i)
        {
            int boneCount = input.readVarInt(true);
            for (int j = 0; j < boneCount; ++j)
            {
                input.skipVarInt();
                input.skipFloat(3);
            }
        }
        return vertexCount;
    }
    protected void skipVarIntArray(Spine42Input input, int count)
    {
        if (count < 0)
        {
            throw createFormatException(input, "VarInt数组数量非法:" + count);
        }
        for (int i = 0; i < count; ++i)
        {
            input.skipVarInt();
        }
    }
    protected bool[] skipEvents(Spine42Input input)
    {
        int eventCount = input.readVarInt(true);
        bool[] eventHasAudio = new bool[eventCount];
        for (int i = 0; i < eventCount; ++i)
        {
            input.skipString();
            input.readVarInt(false);
            input.skipFloat(1);
            input.skipString();
            bool audioPathNotNull = input.skipStringAndReturnNotNull();
            eventHasAudio[i] = audioPathNotNull;
            if (audioPathNotNull)
            {
                input.skipFloat(2);
            }
        }
        return eventHasAudio;
    }
    protected void skipAnimation(Spine42Input input, int slotCount, bool[] eventHasAudio)
    {
        input.skipVarInt();
        skipSlotTimelines(input);
        skipBoneTimelines(input);
        skipIkConstraintTimelines(input);
        skipTransformConstraintTimelines(input);
        skipPathConstraintTimelines(input);
        skipPhysicsTimelines(input);
        skipAttachmentTimelines(input);
        skipDrawOrderTimeline(input, slotCount);
        skipEventTimeline(input, eventHasAudio);
    }
    protected void skipSlotTimelines(Spine42Input input)
    {
        int slotGroupCount = input.readVarInt(true);
        for (int i = 0; i < slotGroupCount; ++i)
        {
            input.skipVarInt();
            int timelineCount = input.readVarInt(true);
            for (int j = 0; j < timelineCount; ++j)
            {
                int timelineType = input.readByte();
                int frameCount = input.readVarInt(true);
                switch (timelineType)
                {
                    case SLOT_ATTACHMENT:
                        for (int frame = 0; frame < frameCount; ++frame)
                        {
                            input.skipFloat(1);
                            input.skipStringRef();
                        }
                        break;
                    case SLOT_RGBA:
                        input.skipVarInt();
                        skipByteValueCurveTimeline(input, frameCount, 4);
                        break;
                    case SLOT_RGB:
                        input.skipVarInt();
                        skipByteValueCurveTimeline(input, frameCount, 3);
                        break;
                    case SLOT_RGBA2:
                        input.skipVarInt();
                        skipByteValueCurveTimeline(input, frameCount, 7);
                        break;
                    case SLOT_RGB2:
                        input.skipVarInt();
                        skipByteValueCurveTimeline(input, frameCount, 6);
                        break;
                    case SLOT_ALPHA:
                        input.skipVarInt();
                        skipByteValueCurveTimeline(input, frameCount, 1);
                        break;
                    default:
                        throw createFormatException(input, "未知Slot Timeline类型:" + timelineType);
                }
            }
        }
    }
    protected void skipByteValueCurveTimeline(Spine42Input input, int frameCount, int valueCount)
    {
        if (frameCount <= 0)
        {
            return;
        }
        input.skipFloat(1);
        input.skipByte(valueCount);
        for (int frame = 0; frame < frameCount - 1; ++frame)
        {
            input.skipFloat(1);
            input.skipByte(valueCount);
            skipCurve(input, valueCount);
        }
    }
    protected void skipBoneTimelines(Spine42Input input)
    {
        int boneGroupCount = input.readVarInt(true);
        for (int i = 0; i < boneGroupCount; ++i)
        {
            input.skipVarInt();
            int timelineCount = input.readVarInt(true);
            for (int j = 0; j < timelineCount; ++j)
            {
                int timelineType = input.readByte();
                int frameCount = input.readVarInt(true);
                if (timelineType == BONE_INHERIT)
                {
                    for (int frame = 0; frame < frameCount; ++frame)
                    {
                        input.skipFloat(1);
                        input.skipByte();
                    }
                    continue;
                }
                input.skipVarInt();
                switch (timelineType)
                {
                    case BONE_ROTATE:
                    case BONE_TRANSLATE_X:
                    case BONE_TRANSLATE_Y:
                    case BONE_SCALE_X:
                    case BONE_SCALE_Y:
                    case BONE_SHEAR_X:
                    case BONE_SHEAR_Y:
                        skipFloatValueCurveTimeline(input, frameCount, 1);
                        break;
                    case BONE_TRANSLATE:
                    case BONE_SCALE:
                    case BONE_SHEAR:
                        skipFloatValueCurveTimeline(input, frameCount, 2);
                        break;
                    default:
                        throw createFormatException(input, "未知Bone Timeline类型:" + timelineType);
                }
            }
        }
    }
    protected void skipFloatValueCurveTimeline(Spine42Input input, int frameCount, int valueCount)
    {
        if (frameCount <= 0)
        {
            return;
        }
        input.skipFloat(1 + valueCount);
        for (int frame = 0; frame < frameCount - 1; ++frame)
        {
            input.skipFloat(1 + valueCount);
            skipCurve(input, valueCount);
        }
    }
    protected void skipIkConstraintTimelines(Spine42Input input)
    {
        int timelineCount = input.readVarInt(true);
        for (int i = 0; i < timelineCount; ++i)
        {
            input.skipVarInt();
            int frameCount = input.readVarInt(true);
            input.skipVarInt();
            if (frameCount <= 0)
            {
                continue;
            }
            int flags = input.readByte();
            input.skipFloat(1);
            if ((flags & 1) != 0 && (flags & 2) != 0) input.skipFloat(1);
            if ((flags & 4) != 0) input.skipFloat(1);
            for (int frame = 0; frame < frameCount - 1; ++frame)
            {
                flags = input.readByte();
                input.skipFloat(1);
                if ((flags & 1) != 0 && (flags & 2) != 0) input.skipFloat(1);
                if ((flags & 4) != 0) input.skipFloat(1);
                if ((flags & 128) != 0)
                {
                    input.skipFloat(8);
                }
            }
        }
    }
    protected void skipTransformConstraintTimelines(Spine42Input input)
    {
        int timelineCount = input.readVarInt(true);
        for (int i = 0; i < timelineCount; ++i)
        {
            input.skipVarInt();
            int frameCount = input.readVarInt(true);
            input.skipVarInt();
            if (frameCount <= 0)
            {
                continue;
            }
            input.skipFloat(7);
            for (int frame = 0; frame < frameCount - 1; ++frame)
            {
                input.skipFloat(7);
                skipCurve(input, 6);
            }
        }
    }
    protected void skipPathConstraintTimelines(Spine42Input input)
    {
        int pathGroupCount = input.readVarInt(true);
        for (int i = 0; i < pathGroupCount; ++i)
        {
            input.skipVarInt();
            int timelineCount = input.readVarInt(true);
            for (int j = 0; j < timelineCount; ++j)
            {
                int timelineType = input.readByte();
                switch (timelineType)
                {
                    case PATH_POSITION:
                    case PATH_SPACING:
                        {
                            int frameCount = input.readVarInt(true);
                            input.skipVarInt();
                            skipFloatValueCurveTimeline(input, frameCount, 1);
                            break;
                        }
                    case PATH_MIX:
                        {
                            int frameCount = input.readVarInt(true);
                            input.skipVarInt();
                            if (frameCount <= 0)
                            {
                                break;
                            }
                            input.skipFloat(4);
                            for (int frame = 0; frame < frameCount - 1; ++frame)
                            {
                                input.skipFloat(4);
                                skipCurve(input, 3);
                            }
                            break;
                        }
                    default:
                        throw createFormatException(input, "未知Path Timeline类型:" + timelineType);
                }
            }
        }
    }
    protected void skipPhysicsTimelines(Spine42Input input)
    {
        int physicsGroupCount = input.readVarInt(true);
        for (int i = 0; i < physicsGroupCount; ++i)
        {
            input.skipVarInt();
            int timelineCount = input.readVarInt(true);
            for (int j = 0; j < timelineCount; ++j)
            {
                int timelineType = input.readByte();
                int frameCount = input.readVarInt(true);
                if (timelineType == PHYSICS_RESET)
                {
                    input.skipFloat(frameCount);
                    continue;
                }
                input.skipVarInt();
                switch (timelineType)
                {
                    case PHYSICS_INERTIA:
                    case PHYSICS_STRENGTH:
                    case PHYSICS_DAMPING:
                    case PHYSICS_MASS:
                    case PHYSICS_WIND:
                    case PHYSICS_GRAVITY:
                    case PHYSICS_MIX:
                        skipFloatValueCurveTimeline(input, frameCount, 1);
                        break;
                    default:
                        throw createFormatException(input, "未知Physics Timeline类型:" + timelineType);
                }
            }
        }
    }
    protected void skipAttachmentTimelines(Spine42Input input)
    {
        int skinGroupCount = input.readVarInt(true);
        for (int i = 0; i < skinGroupCount; ++i)
        {
            input.skipVarInt();
            int slotGroupCount = input.readVarInt(true);
            for (int j = 0; j < slotGroupCount; ++j)
            {
                input.skipVarInt();
                int attachmentCount = input.readVarInt(true);
                for (int k = 0; k < attachmentCount; ++k)
                {
                    input.skipStringRef();
                    int timelineType = input.readByte();
                    int frameCount = input.readVarInt(true);
                    switch (timelineType)
                    {
                        case ATTACHMENT_DEFORM:
                            input.skipVarInt();
                            if (frameCount <= 0)
                            {
                                break;
                            }
                            input.skipFloat(1);
                            for (int frame = 0; frame < frameCount; ++frame)
                            {
                                int valueCount = input.readVarInt(true);
                                if (valueCount != 0)
                                {
                                    input.skipVarInt();
                                    input.skipFloat(valueCount);
                                }
                                if (frame == frameCount - 1)
                                {
                                    break;
                                }
                                input.skipFloat(1);
                                skipCurve(input, 1);
                            }
                            break;
                        case ATTACHMENT_SEQUENCE:
                            for (int frame = 0; frame < frameCount; ++frame)
                            {
                                input.skipFloat(1);
                                input.skipInt32();
                                input.skipFloat(1);
                            }
                            break;
                        default:
                            throw createFormatException(input, "未知Attachment Timeline类型:" + timelineType);
                    }
                }
            }
        }
    }
    protected void skipDrawOrderTimeline(Spine42Input input, int slotCount)
    {
        int frameCount = input.readVarInt(true);
        for (int frame = 0; frame < frameCount; ++frame)
        {
            input.skipFloat(1);
            int offsetCount = input.readVarInt(true);
            if (offsetCount > slotCount)
            {
                throw createFormatException(input, "DrawOrder offsetCount超过Slot数量:" + offsetCount + " > " + slotCount);
            }
            for (int i = 0; i < offsetCount; ++i)
            {
                input.skipVarInt();
                input.readVarInt(false);
            }
        }
    }
    protected void skipEventTimeline(Spine42Input input, bool[] eventHasAudio)
    {
        int frameCount = input.readVarInt(true);
        for (int frame = 0; frame < frameCount; ++frame)
        {
            input.skipFloat(1);
            int eventIndex = input.readVarInt(true);
            if (eventIndex < 0 || eventIndex >= eventHasAudio.Length)
            {
                throw createFormatException(input, "Event索引越界:" + eventIndex);
            }
            input.readVarInt(false);
            input.skipFloat(1);
            input.skipString();
            if (eventHasAudio[eventIndex])
            {
                input.skipFloat(2);
            }
        }
    }
    protected void skipCurve(Spine42Input input, int valueCount)
    {
        int curveType = input.readByte();
        switch (curveType)
        {
            case CURVE_LINEAR:
            case CURVE_STEPPED:
                return;
            case CURVE_BEZIER:
                input.skipFloat(valueCount * 4);
                return;
            default:
                throw createFormatException(input, "未知Curve类型:" + curveType);
        }
    }
    protected Exception createFormatException(Spine42Input input, string message)
    {
        return new InvalidDataException(message + "，当前位置:" + input.mPosition + "/" + input.mLength);
    }
    protected class Spine42Input
    {
        protected readonly Stream mInput;
        protected readonly byte[] mNumberBuffer = new byte[8];
        public long mPosition
        {
            get
            {
                return mInput.Position;
            }
        }
        public long mLength
        {
            get
            {
                return mInput.Length;
            }
        }
        public Spine42Input(Stream input)
        {
            mInput = input ?? throw new ArgumentNullException(nameof(input));
            if (!mInput.CanRead || !mInput.CanSeek)
            {
                throw new ArgumentException("输入流必须支持读取和定位", nameof(input));
            }
        }
        public int readByte()
        {
            int value = mInput.ReadByte();
            if (value < 0)
            {
                throw new EndOfStreamException();
            }
            return value;
        }
        public bool readBoolean()
        {
            return readByte() != 0;
        }
        public float readFloat()
        {
            readFully(mNumberBuffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(mNumberBuffer, 0, 4);
            }
            return BitConverter.ToSingle(mNumberBuffer, 0);
        }
        public int readInt32()
        {
            readFully(mNumberBuffer, 0, 4);
            return (mNumberBuffer[0] << 24) | (mNumberBuffer[1] << 16) | (mNumberBuffer[2] << 8) | mNumberBuffer[3];
        }
        public long readInt64()
        {
            readFully(mNumberBuffer, 0, 8);
            return ((long)mNumberBuffer[0] << 56) | ((long)mNumberBuffer[1] << 48) | ((long)mNumberBuffer[2] << 40) | ((long)mNumberBuffer[3] << 32) | ((long)mNumberBuffer[4] << 24) | ((long)mNumberBuffer[5] << 16) | ((long)mNumberBuffer[6] << 8) | mNumberBuffer[7];
        }
        public int readVarInt(bool optimizePositive)
        {
            int b = readByte();
            int result = b & 0x7F;
            if ((b & 0x80) != 0)
            {
                b = readByte();
                result |= (b & 0x7F) << 7;
                if ((b & 0x80) != 0)
                {
                    b = readByte();
                    result |= (b & 0x7F) << 14;
                    if ((b & 0x80) != 0)
                    {
                        b = readByte();
                        result |= (b & 0x7F) << 21;
                        if ((b & 0x80) != 0)
                        {
                            result |= (readByte() & 0x7F) << 28;
                        }
                    }
                }
            }
            return optimizePositive ? result : ((int)((uint)result >> 1) ^ -(result & 1));
        }
        public string readString()
        {
            int byteCount = readVarInt(true);
            switch (byteCount)
            {
                case 0:
                    return null;
                case 1:
                    return string.Empty;
            }
            --byteCount;
            byte[] bytes = new byte[byteCount];
            readFully(bytes, 0, byteCount);
            return Encoding.UTF8.GetString(bytes, 0, byteCount);
        }
        public void skipString()
        {
            int byteCount = readVarInt(true);
            if (byteCount > 1)
            {
                skipByte(byteCount - 1L);
            }
        }
        public bool skipStringAndReturnNotNull()
        {
            int byteCount = readVarInt(true);
            if (byteCount == 0)
            {
                return false;
            }
            if (byteCount > 1)
            {
                skipByte(byteCount - 1L);
            }
            return true;
        }
        public void skipStringRef()
        {
            skipVarInt();
        }
        public void skipVarInt()
        {
            for (int i = 0; i < 5; ++i)
            {
                if ((readByte() & 0x80) == 0)
                {
                    return;
                }
            }
            throw new InvalidDataException("VarInt长度超过5字节");
        }
        public void skipBoolean()
        {
            skipByte();
        }
        public void skipInt32()
        {
            skipByte(4);
        }
        public void skipFloat(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            skipByte(count * 4L);
        }
        public void skipByte()
        {
            skipByte(1);
        }
        public void skipByte(long count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            long targetPosition = mInput.Position + count;
            if (targetPosition < mInput.Position || targetPosition > mInput.Length)
            {
                throw new EndOfStreamException();
            }
            mInput.Position = targetPosition;
        }
        protected void readFully(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                int readCount = mInput.Read(buffer, offset, length);
                if (readCount <= 0)
                {
                    throw new EndOfStreamException();
                }
                offset += readCount;
                length -= readCount;
            }
        }
    }
}