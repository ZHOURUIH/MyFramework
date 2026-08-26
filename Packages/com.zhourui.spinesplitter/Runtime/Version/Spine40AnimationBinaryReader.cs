#if SPINE_RUNTIME_40
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Spine;

// 读取由SpineAnimationSubsetWindow生成的Spine 4.0单动画二进制,不修改Spine官方代码。
public class Spine40AnimationBinaryReader
{
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
    private const int SLOT_ATTACHMENT = 0;
    private const int SLOT_RGBA = 1;
    private const int SLOT_RGB = 2;
    private const int SLOT_RGBA2 = 3;
    private const int SLOT_RGB2 = 4;
    private const int SLOT_ALPHA = 5;
    private const int PATH_POSITION = 0;
    private const int PATH_SPACING = 1;
    private const int PATH_MIX = 2;
    private const int CURVE_STEPPED = 1;
    private const int CURVE_BEZIER = 2;
    private float mScale;
    public Animation readAnimation(byte[] binaryData, string[] strings, SkeletonData skeletonData, float scale, string expectedAnimationName)
    {
        if (binaryData == null || binaryData.Length == 0)
        {
            throw new ArgumentException("动画二进制为空", nameof(binaryData));
        }
        if (strings == null)
        {
            throw new ArgumentNullException(nameof(strings));
        }
        if (skeletonData == null)
        {
            throw new ArgumentNullException(nameof(skeletonData));
        }
        mScale = scale;
        using (MemoryStream stream = new MemoryStream(binaryData, false))
        {
            Spine40AnimationBinaryInput input = new Spine40AnimationBinaryInput(stream, strings);
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
            if (stream.Position != stream.Length)
            {
                throw new InvalidDataException("动画二进制没有完全读取:" + animationName + ",剩余字节:" + (stream.Length - stream.Position));
            }
            return animation;
        }
    }
    private Animation readAnimationBody(string name, Spine40AnimationBinaryInput input, SkeletonData skeletonData)
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
            float time = input.readFloat();
            float mix = input.readFloat();
            float softness = input.readFloat() * scale;
            for (int frame = 0, bezier = 0; ; frame++)
            {
                timeline.SetFrame(frame, time, mix, softness, input.readSByte(), input.readBoolean(), input.readBoolean());
                if (frame == frameLast)
                {
                    break;
                }
                float time2 = input.readFloat();
                float mix2 = input.readFloat();
                float softness2 = input.readFloat() * scale;
                switch (input.readByte())
                {
                    case CURVE_STEPPED:
                        timeline.SetStepped(frame);
                        break;
                    case CURVE_BEZIER:
                        setBezier(input, timeline, bezier++, frame, 0, time, time2, mix, mix2, 1);
                        setBezier(input, timeline, bezier++, frame, 1, time, time2, softness, softness2, scale);
                        break;
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
        // Deform timelines.
        for (int i = 0, n = input.readInt(true); i < n; i++)
        {
            Skin skin = skeletonData.Skins.Items[input.readInt(true)];
            for (int ii = 0, nn = input.readInt(true); ii < nn; ii++)
            {
                int slotIndex = input.readInt(true);
                for (int iii = 0, nnn = input.readInt(true); iii < nnn; iii++)
                {
                    String attachmentName = input.readStringRef();
                    VertexAttachment attachment = (VertexAttachment)skin.GetAttachment(slotIndex, attachmentName);
                    if (attachment == null)
                    {
                        throw new SerializationException("Vertex attachment not found: " + attachmentName);
                    }
                    bool weighted = attachment.Bones != null;
                    float[] vertices = attachment.Vertices;
                    int deformLength = weighted ? (vertices.Length / 3) << 1 : vertices.Length;
                    int frameCount = input.readInt(true);
                    int frameLast = frameCount - 1;
                    DeformTimeline timeline = new DeformTimeline(frameCount, input.readInt(true), slotIndex, attachment);
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
                e.String = input.readBoolean() ? input.readString() : eventData.String;
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
    private Timeline readTimeline(Spine40AnimationBinaryInput input, CurveTimeline1 timeline, float scale)
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
    private Timeline readTimeline(Spine40AnimationBinaryInput input, CurveTimeline2 timeline, float scale)
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
    void setBezier(Spine40AnimationBinaryInput input, CurveTimeline timeline, int bezier, int frame, int value, float time1, float time2, float value1, float value2, float scale)
    {
        timeline.SetBezier(bezier, frame, value, time1, value1, input.readFloat(), input.readFloat() * scale, input.readFloat(), input.readFloat() * scale, time2, value2);
    }
    private class Spine40AnimationBinaryInput
    {
        private readonly byte[] mFloatBytes = new byte[4];
        private readonly Stream mStream;
        private readonly string[] mStrings;
        public Spine40AnimationBinaryInput(Stream stream, string[] strings)
        {
            mStream = stream ?? throw new ArgumentNullException(nameof(stream));
            mStrings = strings ?? throw new ArgumentNullException(nameof(strings));
        }
        public int read()
        {
            int value = mStream.ReadByte();
            if (value < 0)
            {
                throw new EndOfStreamException();
            }
            return value;
        }
        public byte readByte()
        {
            return (byte)read();
        }
        public sbyte readSByte()
        {
            return (sbyte)read();
        }
        public bool readBoolean()
        {
            return read() != 0;
        }
        public float readFloat()
        {
            readFully(mFloatBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(mFloatBytes);
            }
            float value = BitConverter.ToSingle(mFloatBytes, 0);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(mFloatBytes);
            }
            return value;
        }
        public int readInt(bool optimizePositive)
        {
            int b = read();
            int result = b & 0x7F;
            if ((b & 0x80) != 0)
            {
                b = read();
                result |= (b & 0x7F) << 7;
                if ((b & 0x80) != 0)
                {
                    b = read();
                    result |= (b & 0x7F) << 14;
                    if ((b & 0x80) != 0)
                    {
                        b = read();
                        result |= (b & 0x7F) << 21;
                        if ((b & 0x80) != 0)
                        {
                            result |= (read() & 0x7F) << 28;
                        }
                    }
                }
            }
            return optimizePositive ? result : (result >> 1) ^ -(result & 1);
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
            byte[] bytes = new byte[byteCount];
            readFully(bytes, 0, byteCount);
            return Encoding.UTF8.GetString(bytes, 0, byteCount);
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
        private void readFully(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                int count = mStream.Read(buffer, offset, length);
                if (count <= 0)
                {
                    throw new EndOfStreamException();
                }
                offset += count;
                length -= count;
            }
        }
    }
}
#endif