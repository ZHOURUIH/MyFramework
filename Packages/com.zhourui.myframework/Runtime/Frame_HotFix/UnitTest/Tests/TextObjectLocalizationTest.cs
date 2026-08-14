using static TestAssert;

// TextObjectLocalization: 文本对象本地化数据(纯 C# ClassObject, 直接 new 可测)
public static class TextObjectLocalizationTest
{
	public static void Run()
	{
		testDefaultValues();
		testFieldAssignment();
		testResetProperty();
	}

	// 构造默认值
	private static void testDefaultValues()
	{
		TextObjectLocalization info = new TextObjectLocalization();
		assertTrue(info.mObject == null, "默认 mObject null");
		assertTrue(info.mText == null, "默认 mText null");
		assertEqual(0, info.mID, "默认 mID 0");
		assertEqual(0, info.mParam.Count, "默认 mParam 空");
		assertTrue(info.mCallback == null, "默认 mCallback null");
	}

	// 字段赋值
	private static void testFieldAssignment()
	{
		TextObjectLocalization info = new TextObjectLocalization();
		info.mText = "hello";
		info.mID = 42;
		info.mParam.Add("param0");
		info.mParam.Add("param1");
		assertEqual("hello", info.mText, "mText 赋值读回");
		assertEqual(42, info.mID, "mID 赋值读回");
		assertEqual(2, info.mParam.Count, "mParam 添加 2 个");
	}

	// resetProperty: 全字段复位
	private static void testResetProperty()
	{
		TextObjectLocalization info = new TextObjectLocalization();
		info.mText = "hello";
		info.mID = 42;
		info.mParam.Add("param0");
		info.resetProperty();
		assertTrue(info.mObject == null, "resetProperty 后 mObject null");
		assertTrue(info.mText == null, "resetProperty 后 mText null");
		assertEqual(0, info.mID, "resetProperty 后 mID 0");
		assertEqual(0, info.mParam.Count, "resetProperty 后 mParam 清空");
		assertTrue(info.mCallback == null, "resetProperty 后 mCallback null");
	}
}
