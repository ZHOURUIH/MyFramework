#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

// SerializerBitWrite / SerializerBitRead 集成测试
// 序列化器不依赖框架运行时，但放在此处统一管理
public static class SerializerBitTest
{
    public static void Run()
    {
        testWriteReadInt();
        testWriteReadBool();
        testWriteReadFloat();
        testWriteReadLong();
        testWriteReadString();
        testWriteReadMixed();
        testReadList();
    }

    private static void testWriteReadInt()
    {
        var w = new SerializerBitWrite();
        w.write(12345, true);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());
        r.read(out int val, true);

        AssertEqual(12345, val, "Serializer int: 读写不一致");
    }

    private static void testWriteReadBool()
    {
        var w = new SerializerBitWrite();
        w.write(true);
        w.write(false);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        r.read(out bool b1);
        r.read(out bool b2);

        Assert(b1, "Serializer bool: true 应为 true");
        Assert(!b2, "Serializer bool: false 应为 false");
    }

    private static void testWriteReadFloat()
    {
        var w = new SerializerBitWrite();
        w.write(3.14f, false);
        w.write(-2.5f, true);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        r.read(out float f1, false);
        r.read(out float f2, true);

        AssertEqual(3.14f, f1, "Serializer float: 3.14 应一致");
        AssertEqual(-2.5f, f2, "Serializer float: -2.5 应一致");
    }

    private static void testWriteReadLong()
    {
        var w = new SerializerBitWrite();
        w.write(1234567890123L, false);
        w.write(-9876543210987L, true);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        r.read(out long l1, false);
        r.read(out long l2, true);

        AssertEqual(1234567890123L, l1, "Serializer long: 正数应一致");
        AssertEqual(-9876543210987L, l2, "Serializer long: 负数应一致");
    }

    private static void testWriteReadString()
    {
        var w = new SerializerBitWrite();
        w.writeString("Hello World");
        w.writeString("测试字符串");

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        r.readString(out string s1);
        r.readString(out string s2);

        AssertEqual("Hello World", s1, "Serializer string: English 应一致");
        AssertEqual("测试字符串", s2, "Serializer string: 中文应一致");
    }

    private static void testWriteReadMixed()
    {
        var w = new SerializerBitWrite();
        w.write(42, true);
        w.write(3.14f, false);
        w.writeString("mixed");
        w.write(true);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        r.read(out int i, true);
        r.read(out float f, false);
        r.readString(out string s);
        r.read(out bool b);

        AssertEqual(42, i, "Serializer mixed: int 应一致");
        AssertEqual(3.14f, f, "Serializer mixed: float 应一致");
        AssertEqual("mixed", s, "Serializer mixed: string 应一致");
        Assert(b, "Serializer mixed: bool 应一致");
    }

    private static void testReadList()
    {
        var w = new SerializerBitWrite();
        var list = new List<int> { 1, 2, 3, 4, 5 };
        w.writeList(list, false);

        var r = new SerializerBitRead();
        r.init(w.getBuffer(), w.getByteCount());

        List<int> readList = new();
		r.readList(readList, false);

        AssertNotNull(readList, "Serializer list: 不应为 null");
        AssertEqual(5, readList.Count, "Serializer list: 长度应为 5");
        for (int i = 0; i < list.Count; i++)
        {
            AssertEqual(list[i], readList[i], $"Serializer list: 元素 {i} 应一致");
        }
    }

    // Simple assertion methods
    private static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message = "")
    {
        bool eq = (expected == null && actual == null)
               || (expected != null && expected.Equals(actual));
        if (!eq)
        {
            throw new Exception(
                string.IsNullOrEmpty(message)
                    ? $"Expected [{expected}] but got [{actual}]"
                    : $"{message} - Expected [{expected}] but got [{actual}]");
        }
    }

    private static void AssertNotNull(object obj, string message = "")
    {
        if (obj == null)
        {
            throw new Exception(string.IsNullOrEmpty(message) ? "Object should not be null" : message);
        }
    }
}
#endif