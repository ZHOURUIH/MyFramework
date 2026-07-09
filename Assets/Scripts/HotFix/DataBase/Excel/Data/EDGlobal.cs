// auto generate start
using System;
using System.Collections.Generic;
using UnityEngine;

// 全局变量表格
public class EDGlobal : ExcelDataT<EDGlobal>
{
	private static bool mGlobalLoaded;
	private static Vector2Int _TEST_GLOBAL_3;		// 测试全局参数3
	private static Vector2 _TEST_GLOBAL_4;			// 测试全局参数4
	private static Vector3 _TEST_GLOBAL_5;			// 测试全局参数5
	private static Vector3Int _TEST_GLOBAL_6;		// 测试全局参数6
	private static List<int> _TEST_GLOBAL_7;		// 测试全局参数7
	private static List<float> _TEST_GLOBAL_8;		// 测试全局参数8
	private static List<long> _TEST_GLOBAL_9;		// 测试全局参数9
	private static string _TEST_GLOBAL_10;			// 测试全局参数10

	public const int TEST_GLOBAL_0 = 10;			// 测试全局参数0
	public const float TEST_GLOBAL_1 = 3.5f;		// 测试全局参数1
	public const long TEST_GLOBAL_2 = 99999999999;	// 测试全局参数2

	public static Vector2Int TEST_GLOBAL_3{ get { loadAllParam(); return _TEST_GLOBAL_3; } }// 测试全局参数3
	public static Vector2 TEST_GLOBAL_4{ get { loadAllParam(); return _TEST_GLOBAL_4; } }// 测试全局参数4
	public static Vector3 TEST_GLOBAL_5{ get { loadAllParam(); return _TEST_GLOBAL_5; } }// 测试全局参数5
	public static Vector3Int TEST_GLOBAL_6{ get { loadAllParam(); return _TEST_GLOBAL_6; } }// 测试全局参数6
	public static List<int> TEST_GLOBAL_7{ get { loadAllParam(); return _TEST_GLOBAL_7; } }// 测试全局参数7
	public static List<float> TEST_GLOBAL_8{ get { loadAllParam(); return _TEST_GLOBAL_8; } }// 测试全局参数8
	public static List<long> TEST_GLOBAL_9{ get { loadAllParam(); return _TEST_GLOBAL_9; } }// 测试全局参数9
	public static string TEST_GLOBAL_10{ get { loadAllParam(); return _TEST_GLOBAL_10; } }// 测试全局参数10

	public string mParamName;						// 参数名
	public string mParamType;						// 参数类型
	public string mParamValue;						// 参数值
	public override bool read(SerializerRead reader)
	{
		bool result = base.read(reader);
		result = result && reader.readString(out mParamName);
		result = result && reader.readString(out mParamType);
		result = result && reader.readString(out mParamValue);
		return result;
	}
	public static void loadAllParam()
	{
		if (mGlobalLoaded)
		{
			return;
		}
		mGlobalLoaded = true;
		using var a = new DicScope<string, string>(out var paramMap);
		foreach (EDGlobal data in mTable.queryAll())
		{
			paramMap.add(data.mParamName, data.mParamValue.removeAllEmpty());
		}
		_TEST_GLOBAL_3 = paramMap["TEST_GLOBAL_3"].SToV2I();
		_TEST_GLOBAL_4 = paramMap["TEST_GLOBAL_4"].SToV2();
		_TEST_GLOBAL_5 = paramMap["TEST_GLOBAL_5"].SToV3();
		_TEST_GLOBAL_6 = paramMap["TEST_GLOBAL_6"].SToV3I();
		_TEST_GLOBAL_7 = paramMap["TEST_GLOBAL_7"].SToIs();
		_TEST_GLOBAL_8 = paramMap["TEST_GLOBAL_8"].SToFs();
		_TEST_GLOBAL_9 = paramMap["TEST_GLOBAL_9"].SToLs();
	}
}
// auto generate end