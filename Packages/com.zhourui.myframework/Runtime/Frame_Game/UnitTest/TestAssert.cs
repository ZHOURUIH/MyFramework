using System;
using UnityEngine;

// Frame_Game 版简单测试断言工具类(精简: 无 FloatExtension 依赖)
public static class TestAssert
{
	[HideInCallstack]
	public static void assert(bool condition, string message = "")
	{
		if (!condition)
		{
			throw new Exception($"Assertion failed: {message}");
		}
	}
	[HideInCallstack]
	public static void assertEqual<T>(T expected, T actual, string message = "")
	{
		bool eq = (expected == null && actual == null) || (expected != null && expected.Equals(actual));
		if (!eq)
		{
			throw new Exception(string.IsNullOrEmpty(message) ? $"Expected [{expected}] but got [{actual}]"
															  : $"{message} - Expected [{expected}] but got [{actual}]");
		}
	}
	[HideInCallstack]
	public static void assertNotNull(object obj, string message = "")
	{
		if (obj == null)
		{
			throw new Exception(string.IsNullOrEmpty(message) ? "Object should not be null" : message);
		}
	}
	[HideInCallstack]
	public static void assertNull(object obj, string message = "")
	{
		if (obj != null)
		{
			throw new Exception(string.IsNullOrEmpty(message) ? "Object should be null" : message);
		}
	}
	[HideInCallstack]
	public static void assertTrue(bool condition, string message = "")
	{
		if (!condition)
		{
			throw new Exception(string.IsNullOrEmpty(message) ? "Condition should be true" : message);
		}
	}
	[HideInCallstack]
	public static void assertFalse(bool condition, string message = "")
	{
		if (condition)
		{
			throw new Exception(string.IsNullOrEmpty(message) ? "Condition should be false" : message);
		}
	}
}
