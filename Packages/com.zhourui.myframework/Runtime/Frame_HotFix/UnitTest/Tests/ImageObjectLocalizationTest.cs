using static TestAssert;

// ImageObjectLocalization: 图片对象本地化数据(纯 C# ClassObject, 直接 new 可测)
public static class ImageObjectLocalizationTest
{
	public static void Run()
	{
		testDefaultValues();
		testFieldAssignment();
		testResetProperty();
	}

	// 构造默认值: 字段均为 null
	private static void testDefaultValues()
	{
		ImageObjectLocalization info = new ImageObjectLocalization();
		assertTrue(info.mObject == null, "默认 mObject null");
		assertTrue(info.mImageNameWithoutSuffix == null, "默认 mImageNameWithoutSuffix null");
	}

	// 字段赋值
	private static void testFieldAssignment()
	{
		ImageObjectLocalization info = new ImageObjectLocalization();
		info.mImageNameWithoutSuffix = "icon_";
		assertEqual("icon_", info.mImageNameWithoutSuffix, "mImageNameWithoutSuffix 赋值读回");
	}

	// resetProperty: 字段复位为 null
	private static void testResetProperty()
	{
		ImageObjectLocalization info = new ImageObjectLocalization();
		info.mImageNameWithoutSuffix = "icon_";
		info.resetProperty();
		assertTrue(info.mObject == null, "resetProperty 后 mObject null");
		assertTrue(info.mImageNameWithoutSuffix == null, "resetProperty 后 mImageNameWithoutSuffix null");
	}
}
