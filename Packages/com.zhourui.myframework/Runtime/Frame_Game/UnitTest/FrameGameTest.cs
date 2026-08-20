using System;
using System.Collections.Generic;
using UnityEngine;

// Frame_Game 精简层测试运行器(与 Frame_HotFix 的 FrameHotFixTest 同机制)
// 注意: Frame_Game 无 logError/log/logException(仅 Frame_Base 的 logErrorBase), 日志直接用 UnityEngine.Debug
public class FrameGameTest
{
	private static readonly Dictionary<string, Action> sTests = new();
	public static void runAll()
	{
		Register("FrameCallbackTest", FrameCallbackTest.Run);
		Register("MathUtilityTest", MathUtilityTest.Run);
		Register("StringUtilityTest", StringUtilityTest.Run);
		Register("ListExtensionTest", ListExtensionTest.Run);
		Register("DictionaryExtensionTest", DictionaryExtensionTest.Run);
		Register("WidgetUtilityTest", WidgetUtilityTest.Run);
		Register("UnityUtilityTest", UnityUtilityTest.Run);
		Register("StringExtensionTest", StringExtensionTest.Run);
		Register("ResourceLoadInfoTest", ResourceLoadInfoTest.Run);
		Register("LayoutInfoTest", LayoutInfoTest.Run);
		Register("FontSizeInfoTest", FontSizeInfoTest.Run);
		Register("SceneProcedureTest", SceneProcedureTest.Run);
		Register("FileUtilityTest", FileUtilityTest.Run);
		doRunAll(sTests);
	}
	public static void Register(string name, Action run)
	{
		if (sTests.ContainsKey(name))
		{
			Debug.LogError("[TestRunner] duplicate test: " + name);
			return;
		}
		sTests.Add(name, run);
	}
	public static void doRunAll(Dictionary<string, Action> list)
	{
		int pass = 0;
		int fail = 0;
		foreach (var test in list)
		{
			TestResult result = runOne(test.Key, test.Value);
			if (result.mPassed)
			{
				pass++;
			}
			else
			{
				fail++;
			}
		}
		string info = "[TestRunner] total:" + list.Count + ", pass:" + pass + ", fail:" + fail;
		if (fail > 0)
		{
			Debug.LogError(info);
		}
		else
		{
			Debug.Log(info);
		}
	}
	public static TestResult runOne(string name, Action run)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		try
		{
			run();
			sw.Stop();
			return new TestResult(name, true, "", (float)sw.Elapsed.TotalMilliseconds);
		}
		catch (Exception ex)
		{
			sw.Stop();
			Debug.LogException(ex);
			return new TestResult(name, false, ex.Message, (float)sw.Elapsed.TotalMilliseconds);
		}
	}
}
