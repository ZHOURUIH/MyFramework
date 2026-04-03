#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;

// FrameUtility 中与数值/字符串格式化相关的纯函数（不依赖场景）
public static class FrameUtilityStringStrictTests
{
    public static void Run()
    {
        testFixedAndPercent();
        testToPercentFloat();
        testToPercentString();
        testToProbabilityFloat();
        testToProbabilityString();
    }

    private static void testFixedAndPercent()
    {
        AssertEqual("", fixedAndPercent(0, 0f), "fap 0,0");
        AssertEqual("10", fixedAndPercent(10, 0f), "fap int only");
        Assert(fixedAndPercent(0, 0.1f).Length > 0, "fap percent only");
        Assert(fixedAndPercent(5, 0.25f).Contains("+"), "fap combined has +");
        AssertEqual("100", fixedAndPercent(100, 0f), "fap int 100");
    }

    private static void testToPercentFloat()
    {
        Assert(toPercent(0.5f).EndsWith("%"), "pct float half");
        Assert(toPercent(1f).StartsWith("100"), "pct float full");
        Assert(toPercent(0f).Contains("0"), "pct float zero");
    }

    private static void testToPercentString()
    {
        Assert(toPercent("0.5").EndsWith("%"), "pct str half");
        Assert(toPercent("1", 0).Contains("100"), "pct str 1");
    }

    private static void testToProbabilityFloat()
    {
        Assert(toProbability(100f).EndsWith("%"), "prob 100 万分比");
        Assert(toProbability(0f).Contains("0"), "prob 0");
    }

    private static void testToProbabilityString()
    {
        Assert(toProbability("50").EndsWith("%"), "prob str");
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

    private static void AssertNull(object obj, string message = "")
    {
        if (obj != null)
        {
            throw new Exception(string.IsNullOrEmpty(message) ? "Object should be null" : message);
        }
    }
}
#endif