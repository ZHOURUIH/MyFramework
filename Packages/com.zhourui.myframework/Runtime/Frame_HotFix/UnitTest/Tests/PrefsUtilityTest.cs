using UnityEngine;
using static PrefsUtility;
using static TestAssert;

// PrefsUtility 全部函数单元测试 (PlayerPrefs 封装)
public static class PrefsUtilityTest
{
	public static void Run()
	{
		testPrefsGetSetBool();
		testPrefsGetSetInt();
		testPrefsGetSetFloat();
		testPrefsGetSetString();
		testPrefsHasKey();
		testPrefsDeleteKey();
		testPrefsDefaultValues();
	}

	private static void testPrefsGetSetBool()
	{
		string key = "test_bool";
		PlayerPrefs.DeleteKey(key);
		// 默认值
		assertFalse(prefsGetBool(key), "未设置时应返回默认 false");
		assertTrue(prefsGetBool(key, true), "未设置时应返回指定默认 true");
		// 设置 true
		prefsSetBool(key, true, true);
		assertTrue(prefsGetBool(key), "设置 true 后应返回 true");
		// 设置 false
		prefsSetBool(key, false);
		assertFalse(prefsGetBool(key, true), "设置 false 后应返回 false");
		// save=false 也应能读到
		prefsSetBool(key, true, false);
		assertTrue(prefsGetBool(key), "save=false 也应能读到值");
		PlayerPrefs.DeleteKey(key);
	}

	private static void testPrefsGetSetInt()
	{
		string key = "test_int";
		PlayerPrefs.DeleteKey(key);
		assertEqual(0, prefsGetInt(key), "未设置时应返回 0");
		assertEqual(42, prefsGetInt(key, 42), "未设置时应返回默认 42");
		prefsSetInt(key, 12345);
		assertEqual(12345, prefsGetInt(key), "设置后应返回 12345");
		prefsSetInt(key, -7);
		assertEqual(-7, prefsGetInt(key), "设置负数应返回 -7");
		PlayerPrefs.DeleteKey(key);
	}

	private static void testPrefsGetSetFloat()
	{
		string key = "test_float";
		PlayerPrefs.DeleteKey(key);
		assertEqual(0.0f, prefsGetFloat(key), 0.0001f, "未设置时应返回 0");
		assertEqual(3.5f, prefsGetFloat(key, 3.5f), 0.0001f, "未设置时应返回默认 3.5");
		prefsSetFloat(key, 1.25f);
		assertEqual(1.25f, prefsGetFloat(key), 0.0001f, "设置后应返回 1.25");
		prefsSetFloat(key, -9.75f);
		assertEqual(-9.75f, prefsGetFloat(key), 0.0001f, "设置负数应返回 -9.75");
		PlayerPrefs.DeleteKey(key);
	}

	private static void testPrefsGetSetString()
	{
		string key = "test_string";
		PlayerPrefs.DeleteKey(key);
		assertEqual("", prefsGetString(key), "未设置时应返回空字符串");
		prefsSetString(key, "hello");
		assertEqual("hello", prefsGetString(key), "设置后应返回 hello");
		prefsSetString(key, "");
		assertEqual("", prefsGetString(key), "设置空串应返回空串");
		PlayerPrefs.DeleteKey(key);
	}

	private static void testPrefsHasKey()
	{
		string key = "test_haskey";
		PlayerPrefs.DeleteKey(key);
		assertFalse(prefsHasKey(key), "未设置时 hasKey 应为 false");
		prefsSetInt(key, 1);
		assertTrue(prefsHasKey(key), "设置后 hasKey 应为 true");
		PlayerPrefs.DeleteKey(key);
		assertFalse(prefsHasKey(key), "删除后 hasKey 应为 false");
	}

	private static void testPrefsDeleteKey()
	{
		string key = "test_delete";
		prefsSetString(key, "value");
		assertTrue(prefsHasKey(key), "删除前应存在");
		prefsDeleteKey(key);
		assertFalse(prefsHasKey(key), "删除后不应存在");
	}

	private static void testPrefsDefaultValues()
	{
		// 不同类型 Key 不互相污染
		string baseKey = "test_type_";
		PlayerPrefs.DeleteKey(baseKey + "i");
		PlayerPrefs.DeleteKey(baseKey + "f");
		PlayerPrefs.DeleteKey(baseKey + "s");
		prefsSetInt(baseKey + "i", 10);
		prefsSetFloat(baseKey + "f", 0.5f);
		prefsSetString(baseKey + "s", "str");
		assertEqual(10, prefsGetInt(baseKey + "i"), "int 值正确");
		assertEqual(0.5f, prefsGetFloat(baseKey + "f"), 0.0001f, "float 值正确");
		assertEqual("str", prefsGetString(baseKey + "s"), "string 值正确");
		PlayerPrefs.DeleteKey(baseKey + "i");
		PlayerPrefs.DeleteKey(baseKey + "f");
		PlayerPrefs.DeleteKey(baseKey + "s");
	}
}
