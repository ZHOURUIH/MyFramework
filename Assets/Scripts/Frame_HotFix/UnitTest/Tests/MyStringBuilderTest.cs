#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;

// MyStringBuilder 测试
// 覆盖：add / clear / remove / insert / replace / replaceAll /
//        endWith / lastIndexOf / indexOf / addRepeat / addIf /
//        colorString / addLine / Length / ToString
public static class MyStringBuilderTest
{
    public static void Run()
    {
        testClearAndLength();
        testAddTypes();
        testAddStrings();
        testAddIf();
        testAddRepeat();
        testInsert();
        testRemove();
        testReplace();
        testReplaceAll();
        testEndWith();
        testLastIndexOf();
        testIndexOf();
        testColorString();
        testAddLine();
        testChaining();
        testEdgeCases();
    }

    // 获取一个 CLASS 池化的 MyStringBuilder
    public static MyStringBuilder getBuilder()
    {
        var builder = CLASS<MyStringBuilder>();
        builder.clear();
        return builder;
    }

    // ─── clear / Length ──────────────────────────────────────────────────

    private static void testClearAndLength()
    {
        var b = getBuilder();
        AssertEqual(0, b.Length, "clear 后 Length 应为 0");

        b.add("test");
        Assert(b.Length > 0, "add 后 Length 应 > 0");

        b.clear();
        AssertEqual(0, b.Length, "clear 后 Length 应归零");
    }

    // ─── 添加各种类型 ─────────────────────────────────────────────────────

    private static void testAddTypes()
    {
        var b = getBuilder();
        b.add(123);
        Assert(b.ToString().Contains("123"), "add(int) 应包含数字");

        b.clear();
        b.add(3.14f);
        Assert(b.ToString().Contains("3.14"), "add(float) 应包含浮点数");

        b.clear();
        b.add(true);
        Assert(b.ToString().Contains("true"), "add(bool) 应包含布尔值");
    }

    // ─── 添加字符串 ───────────────────────────────────────────────────────

    private static void testAddStrings()
    {
        var b = getBuilder();
        b.add("Hello");
        b.add(" ");
        b.add("World");
        AssertEqual("Hello World", b.ToString(), "add(string) 应拼接字符串");
    }

    // ─── addIf ───────────────────────────────────────────────────────────

    private static void testAddIf()
    {
        var b = getBuilder();
        b.addIf("yes", true);
        b.addIf("no", false);
        AssertEqual("yes", b.ToString(), "addIf: 只有 true 条件才添加");
    }

    // ─── addRepeat ───────────────────────────────────────────────────────

    private static void testAddRepeat()
    {
        var b = getBuilder();
        b.addRepeat("ab", 3);
        AssertEqual("ababab", b.ToString(), "addRepeat: 应重复指定次数");
    }

    // ─── insert ──────────────────────────────────────────────────────────

    private static void testInsert()
    {
        var b = getBuilder();
        b.add("World");
        b.insert(0, "Hello ");
        AssertEqual("Hello World", b.ToString(), "insert 应在指定位置插入");
    }

    // ─── remove ──────────────────────────────────────────────────────────

    private static void testRemove()
    {
        var b = getBuilder();
        b.add("Hello World");
        b.remove(5, 6); // 移除 " World"
        AssertEqual("Hello", b.ToString(), "remove 应删除指定范围");
    }

    // ─── replace ─────────────────────────────────────────────────────────

    private static void testReplace()
    {
        var b = getBuilder();
        b.add("Hello World");
        b.replace(6, 5, "Universe"); // 替换 "World" 为 "Universe"
        AssertEqual("Hello Universe", b.ToString(), "replace 应替换指定范围");
    }

    // ─── replaceAll ──────────────────────────────────────────────────────

    private static void testReplaceAll()
    {
        var b = getBuilder();
        b.add("apple apple banana");
        b.replaceAll("apple", "orange");
        AssertEqual("orange orange banana", b.ToString(), "replaceAll 应替换所有匹配");
    }

    // ─── endWith ─────────────────────────────────────────────────────────

    private static void testEndWith()
    {
        var b = getBuilder();
        b.add("Hello World");
        Assert(b.endWith('d'), "endWith: 应以 'd' 结尾");
        Assert(!b.endWith('H'), "endWith: 不应以 'H' 结尾");
    }

    // ─── lastIndexOf ─────────────────────────────────────────────────────

    private static void testLastIndexOf()
    {
        var b = getBuilder();
        b.add("abc def abc");
        int idx = b.lastIndexOf('a');
        AssertEqual(8, idx, "lastIndexOf 应返回最后出现的位置");
    }

    // ─── indexOf ─────────────────────────────────────────────────────────

    private static void testIndexOf()
    {
        var b = getBuilder();
        b.add("abc def abc");
        int idx = b.indexOf('d');
        AssertEqual(4, idx, "indexOf 应返回首次出现的位置");
    }

    // ─── colorString ─────────────────────────────────────────────────────

    private static void testColorString()
    {
        var b = getBuilder();
        b.colorString("red", "FF0000");
        string result = b.ToString();
        Assert(result.Contains("red"), "colorString 应包含文本");
        Assert(result.Contains("FF0000"), "colorString 应包含颜色值");
    }

    // ─── addLine ─────────────────────────────────────────────────────────

    private static void testAddLine()
    {
        var b = getBuilder();
        b.addLine("first");
        b.addLine("second");
        string result = b.ToString();
        Assert(result.Contains("\n"), "addLine 应添加换行符");
    }

    // ─── 链式调用 ─────────────────────────────────────────────────────────

    private static void testChaining()
    {
        var b = getBuilder();
        b.add("a").add("b").add("c");
        AssertEqual("abc", b.ToString(), "链式调用应正常工作");
    }

    // ─── 边界情况 ─────────────────────────────────────────────────────────

    private static void testEdgeCases()
    {
        var b = getBuilder();
        // 空字符串
        b.add("");
        AssertEqual(0, b.Length, "add 空字符串不应增加长度");

        // 越界操作应安全
        try
        {
            b.insert(100, "test");
            // 如果没抛异常，应该没插入
            AssertEqual(0, b.Length, "越界 insert 不应改变内容");
        }
        catch
        {
            // 允许抛异常
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
}
#endif
