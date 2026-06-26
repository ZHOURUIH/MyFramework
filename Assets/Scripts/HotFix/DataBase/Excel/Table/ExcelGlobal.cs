// auto generate start
using UnityEngine;
using System.Collections.Generic;
using static StringUtility;

public class ExcelGlobal : ExcelTableT<EDGlobal>
{
	public const int TEST_GLOBAL_0 = 10;							// 测试全局参数0
	public const float TEST_GLOBAL_1 = 3.5f;						// 测试全局参数1
	public const long TEST_GLOBAL_2 = 99999999999;					// 测试全局参数2
	public static Vector2Int TEST_GLOBAL_3;							// 测试全局参数3
	public static Vector2 TEST_GLOBAL_4;							// 测试全局参数4
	public static Vector3 TEST_GLOBAL_5;							// 测试全局参数5
	public static Vector3Int TEST_GLOBAL_6;							// 测试全局参数6
	public static List<int> TEST_GLOBAL_7;							// 测试全局参数7
	public static List<float> TEST_GLOBAL_8;						// 测试全局参数8
	public static List<long> TEST_GLOBAL_9;							// 测试全局参数9
	public static string TEST_GLOBAL_10;							// 测试全局参数10
	protected override void onOpenFile()
	{
		using var a = new DicScope<string, string>(out var paramMap);
		foreach (EDGlobal data in queryAll())
		{
			paramMap.add(data.mParamName, data.mParamValue.removeAllEmpty());
		}
		TEST_GLOBAL_3 = SToV2I(paramMap["TEST_GLOBAL_3"]);
		TEST_GLOBAL_4 = SToV2(paramMap["TEST_GLOBAL_4"]);
		TEST_GLOBAL_5 = SToV3(paramMap["TEST_GLOBAL_5"]);
		TEST_GLOBAL_6 = SToV3I(paramMap["TEST_GLOBAL_6"]);
		TEST_GLOBAL_7 = SToIs(paramMap["TEST_GLOBAL_7"]);
		TEST_GLOBAL_8 = SToFs(paramMap["TEST_GLOBAL_8"]);
		TEST_GLOBAL_9 = SToLLs(paramMap["TEST_GLOBAL_9"]);
	}
}
// auto generate end