using System;
using System.IO;
using System.Text;

// Spine 4.2单动画公共数据,每个Skeleton只生成一份,保存源Spine版本、源Skeleton Hash和所有动画共同使用的字符串表。
public class Spine42AnimationCommonData
{
    public int mFileVersion;
    public string mSourceSkeletonName;
    public string mSpineVersion;
    public long mSkeletonHash;
    public string[] mStrings;
}

// Spine 4.2单动画数据,每个文件只保存一个动画名称以及从原始.skel.bytes中复制出的完整动画二进制块。
public class Spine42SingleAnimationData
{
    public int mFileVersion;
    public string mSpineVersion;
    public long mSkeletonHash;
    public string mAnimationName;
    public byte[] mBinaryData;
    public byte[] mBinarySourceData;
    public int mBinaryOffset;
    public int mBinaryLength;
}

// Spine 4.2公共文件和单动画文件读写器,公共字符串表只保存一次,每个.spineanim.bytes只保存一个动画。
public static class Spine42AnimationFile
{
    public const int CURRENT_VERSION = 1;
    private const int MAX_STRING_BYTE_COUNT = 16 * 1024 * 1024;
    private const int MAX_STRING_COUNT = 1024 * 1024;
    private const int MAX_ANIMATION_BYTE_COUNT = 1024 * 1024 * 1024;
    private static readonly byte[] COMMON_MAGIC = new byte[] { 0x53, 0x50, 0x4E, 0x43, 0x4F, 0x4D, 0x34, 0x32 };
    private static readonly byte[] ANIMATION_MAGIC = new byte[] { 0x53, 0x50, 0x4E, 0x41, 0x4E, 0x49, 0x34, 0x32 };
    public static bool isCommonFile(byte[] bytes)
    {
        return hasMagic(bytes, COMMON_MAGIC);
    }
    public static bool isAnimationFile(byte[] bytes)
    {
        return hasMagic(bytes, ANIMATION_MAGIC);
    }
    public static void writeCommon(string filePath, Spine42AnimationCommonData data)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("公共文件输出路径为空", nameof(filePath));
        }
        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            writeCommon(stream, data);
        }
    }
    public static byte[] writeCommonToBytes(Spine42AnimationCommonData data)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            writeCommon(stream, data);
            return stream.ToArray();
        }
    }
    public static void writeCommon(Stream stream, Spine42AnimationCommonData data)
    {
        validateCommonData(data);
        validateWriteStream(stream);
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            writer.Write(COMMON_MAGIC);
            writer.Write(CURRENT_VERSION);
            writeString(writer, data.mSourceSkeletonName);
            writeString(writer, data.mSpineVersion);
            writer.Write(data.mSkeletonHash);
            writer.Write(data.mStrings.Length);
            for (int i = 0; i < data.mStrings.Length; ++i)
            {
                writeString(writer, data.mStrings[i]);
            }
            writer.Flush();
        }
    }
    public static Spine42AnimationCommonData readCommon(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        using (MemoryStream stream = new MemoryStream(bytes, false))
        {
            return readCommon(stream);
        }
    }
    public static Spine42AnimationCommonData readCommon(Stream stream)
    {
        validateReadStream(stream);
        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            verifyMagic(reader, COMMON_MAGIC, "不是有效的Spine 4.2动画公共文件");
            Spine42AnimationCommonData data = new Spine42AnimationCommonData();
            data.mFileVersion = readFileVersion(reader, "动画公共文件");
            data.mSourceSkeletonName = readString(reader);
            data.mSpineVersion = readString(reader);
            data.mSkeletonHash = reader.ReadInt64();
            validateSpineVersion(data.mSpineVersion, "动画公共文件");
            int stringCount = readCount(reader, MAX_STRING_COUNT, "共享字符串数量");
            data.mStrings = new string[stringCount];
            for (int i = 0; i < stringCount; ++i)
            {
                data.mStrings[i] = readString(reader);
            }
            verifyStreamEnd(stream, "动画公共文件");
            return data;
        }
    }
    public static void writeAnimation(string filePath, Spine42SingleAnimationData data)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("单动画文件输出路径为空", nameof(filePath));
        }
        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            writeAnimation(stream, data);
        }
    }
    public static byte[] writeAnimationToBytes(Spine42SingleAnimationData data)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            writeAnimation(stream, data);
            return stream.ToArray();
        }
    }
    public static void writeAnimation(Stream stream, Spine42SingleAnimationData data)
    {
        validateAnimationData(data);
        validateWriteStream(stream);
        getBinaryData(data, out byte[] binaryData, out int binaryOffset, out int binaryLength);
        using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            writer.Write(ANIMATION_MAGIC);
            writer.Write(CURRENT_VERSION);
            writeString(writer, data.mSpineVersion);
            writer.Write(data.mSkeletonHash);
            writeString(writer, data.mAnimationName);
            writer.Write(binaryLength);
            writer.Write(binaryData, binaryOffset, binaryLength);
            writer.Flush();
        }
    }
    public static Spine42SingleAnimationData readAnimation(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        using (MemoryStream stream = new MemoryStream(bytes, false))
        {
            return readAnimation(stream);
        }
    }
    public static Spine42SingleAnimationData readAnimationNoCopy(byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }
        using (MemoryStream stream = new MemoryStream(bytes, false))
        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            verifyMagic(reader, ANIMATION_MAGIC, "不是有效的Spine 4.2单动画文件");
            Spine42SingleAnimationData data = new Spine42SingleAnimationData();
            data.mFileVersion = readFileVersion(reader, "单动画文件");
            data.mSpineVersion = readString(reader);
            data.mSkeletonHash = reader.ReadInt64();
            data.mAnimationName = readString(reader);
            validateSpineVersion(data.mSpineVersion, "单动画文件");
            if (string.IsNullOrEmpty(data.mAnimationName))
            {
                throw new InvalidDataException("单动画文件中的动画名称为空");
            }
            int binaryLength = readCount(reader, MAX_ANIMATION_BYTE_COUNT, "动画二进制长度");
            int binaryOffset = checked((int)stream.Position);
            if (binaryLength != stream.Length - stream.Position)
            {
                throw new InvalidDataException("单动画文件二进制长度不一致,记录:" + binaryLength + ",实际:" + (stream.Length - stream.Position));
            }
            data.mBinarySourceData = bytes;
            data.mBinaryOffset = binaryOffset;
            data.mBinaryLength = binaryLength;
            return data;
        }
    }
    public static Spine42SingleAnimationData readAnimation(Stream stream)
    {
        validateReadStream(stream);
        using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            verifyMagic(reader, ANIMATION_MAGIC, "不是有效的Spine 4.2单动画文件");
            Spine42SingleAnimationData data = new Spine42SingleAnimationData();
            data.mFileVersion = readFileVersion(reader, "单动画文件");
            data.mSpineVersion = readString(reader);
            data.mSkeletonHash = reader.ReadInt64();
            data.mAnimationName = readString(reader);
            validateSpineVersion(data.mSpineVersion, "单动画文件");
            if (string.IsNullOrEmpty(data.mAnimationName))
            {
                throw new InvalidDataException("单动画文件中的动画名称为空");
            }
            int binaryLength = readCount(reader, MAX_ANIMATION_BYTE_COUNT, "动画二进制长度");
            data.mBinaryData = readBytesExactly(reader, binaryLength);
            data.mBinaryOffset = 0;
            data.mBinaryLength = binaryLength;
            verifyStreamEnd(stream, "单动画文件");
            return data;
        }
    }
    private static bool hasMagic(byte[] bytes, byte[] magic)
    {
        if (bytes == null || bytes.Length < magic.Length)
        {
            return false;
        }
        for (int i = 0; i < magic.Length; ++i)
        {
            if (bytes[i] != magic[i])
            {
                return false;
            }
        }
        return true;
    }
    private static void validateCommonData(Spine42AnimationCommonData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        if (string.IsNullOrEmpty(data.mSourceSkeletonName))
        {
            throw new InvalidDataException("源Skeleton名称为空");
        }
        validateSpineVersion(data.mSpineVersion, "动画公共文件");
        if (data.mStrings == null)
        {
            throw new InvalidDataException("共享字符串表为空");
        }
        if (data.mStrings.Length > MAX_STRING_COUNT)
        {
            throw new InvalidDataException("共享字符串数量过多:" + data.mStrings.Length);
        }
    }
    private static void validateAnimationData(Spine42SingleAnimationData data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
        validateSpineVersion(data.mSpineVersion, "单动画文件");
        if (string.IsNullOrEmpty(data.mAnimationName))
        {
            throw new InvalidDataException("动画名称为空");
        }
        getBinaryData(data, out byte[] binaryData, out int binaryOffset, out int binaryLength);
        if (binaryLength <= 0)
        {
            throw new InvalidDataException("动画二进制为空:" + data.mAnimationName);
        }
        if (binaryLength > MAX_ANIMATION_BYTE_COUNT)
        {
            throw new InvalidDataException("动画二进制过大:" + data.mAnimationName + "," + binaryLength);
        }
        if (binaryOffset < 0 || binaryOffset > binaryData.Length - binaryLength)
        {
            throw new InvalidDataException("动画二进制范围非法:" + data.mAnimationName + ",Offset:" + binaryOffset + ",Length:" + binaryLength + ",DataLength:" + binaryData.Length);
        }
    }
    private static void getBinaryData(Spine42SingleAnimationData data, out byte[] binaryData, out int binaryOffset, out int binaryLength)
    {
        if (data.mBinaryData != null)
        {
            binaryData = data.mBinaryData;
            binaryOffset = data.mBinaryOffset;
            binaryLength = data.mBinaryLength > 0 ? data.mBinaryLength : data.mBinaryData.Length - binaryOffset;
            return;
        }
        if (data.mBinarySourceData != null)
        {
            binaryData = data.mBinarySourceData;
            binaryOffset = data.mBinaryOffset;
            binaryLength = data.mBinaryLength;
            return;
        }
        throw new InvalidDataException("动画二进制为空:" + data.mAnimationName);
    }
    private static void validateSpineVersion(string spineVersion, string fileType)
    {
        if (string.IsNullOrEmpty(spineVersion) || !spineVersion.StartsWith("4.2", StringComparison.Ordinal))
        {
            throw new InvalidDataException(fileType + "中的Spine版本不是4.0:" + spineVersion);
        }
    }
    private static void validateReadStream(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (!stream.CanRead)
        {
            throw new ArgumentException("输入流不可读", nameof(stream));
        }
    }
    private static void validateWriteStream(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (!stream.CanWrite)
        {
            throw new ArgumentException("输出流不可写", nameof(stream));
        }
    }
    private static int readFileVersion(BinaryReader reader, string fileType)
    {
        int fileVersion = reader.ReadInt32();
        if (fileVersion != CURRENT_VERSION)
        {
            throw new InvalidDataException("不支持的" + fileType + "版本:" + fileVersion);
        }
        return fileVersion;
    }
    private static void verifyMagic(BinaryReader reader, byte[] expectedMagic, string errorMessage)
    {
        byte[] actualMagic = readBytesExactly(reader, expectedMagic.Length);
        for (int i = 0; i < expectedMagic.Length; ++i)
        {
            if (actualMagic[i] != expectedMagic[i])
            {
                throw new InvalidDataException(errorMessage);
            }
        }
    }
    private static void writeString(BinaryWriter writer, string value)
    {
        if (value == null)
        {
            writer.Write(-1);
            return;
        }
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        if (bytes.Length > MAX_STRING_BYTE_COUNT)
        {
            throw new InvalidDataException("字符串过长:" + bytes.Length);
        }
        writer.Write(bytes.Length);
        writer.Write(bytes);
    }
    private static string readString(BinaryReader reader)
    {
        int byteCount = reader.ReadInt32();
        if (byteCount == -1)
        {
            return null;
        }
        if (byteCount < 0 || byteCount > MAX_STRING_BYTE_COUNT)
        {
            throw new InvalidDataException("非法字符串长度:" + byteCount);
        }
        if (byteCount == 0)
        {
            return string.Empty;
        }
        return Encoding.UTF8.GetString(readBytesExactly(reader, byteCount));
    }
    private static int readCount(BinaryReader reader, int maximum, string name)
    {
        int value = reader.ReadInt32();
        if (value < 0 || value > maximum)
        {
            throw new InvalidDataException("非法" + name + ":" + value);
        }
        return value;
    }
    private static byte[] readBytesExactly(BinaryReader reader, int count)
    {
        byte[] bytes = reader.ReadBytes(count);
        if (bytes.Length != count)
        {
            throw new EndOfStreamException("文件提前结束,需要读取" + count + "字节,实际只读取" + bytes.Length + "字节");
        }
        return bytes;
    }
    private static void verifyStreamEnd(Stream stream, string fileType)
    {
        if (stream.CanSeek && stream.Position != stream.Length)
        {
            throw new InvalidDataException(fileType + "末尾存在未读取数据:" + (stream.Length - stream.Position) + "字节");
        }
    }
}
