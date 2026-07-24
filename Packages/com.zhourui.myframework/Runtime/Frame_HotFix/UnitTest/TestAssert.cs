using System;
using static MathUtility;

// 简单的测试断言工具类
public static class TestAssert
{
    public static void assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }
    public static void assertEqual<T>(T expected, T actual, string message = "")
    {
        bool eq = (expected == null && actual == null) || (expected != null && expected.Equals(actual));
        if (!eq)
        {
            throw new Exception(message.isEmpty() ? $"Expected [{expected}] but got [{actual}]"
                                                  : $"{message} - Expected [{expected}] but got [{actual}]");
        }
    }
	public static void assertEqual(float expected, float actual, float precision, string message = "")
	{
		if (!isFloatEqual(expected, actual, precision))
		{
			throw new Exception(message.isEmpty() ? $"Expected [{expected}] but got [{actual}]"
					                              : $"{message} - Expected [{expected}] but got [{actual}]");
		}
	}
	public static void assertNotNull(object obj, string message = "")
    {
        if (obj == null)
        {
            throw new Exception(message.isEmpty() ? "Object should not be null" : message);
        }
    }
    public static void assertNull(object obj, string message = "")
    {
        if (obj != null)
        {
            throw new Exception(message.isEmpty() ? "Object should be null" : message);
        }
    }
    public static void assertTrue(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception(message.isEmpty() ? "Condition should be true" : message);
        }
    }
    public static void assertFalse(bool condition, string message = "")
    {
        if (condition)
        {
            throw new Exception(message.isEmpty() ? "Condition should be false" : message);
        }
    }
}