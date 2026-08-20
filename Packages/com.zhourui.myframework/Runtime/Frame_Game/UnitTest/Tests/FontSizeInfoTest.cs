using static TestAssert;

// Frame_Game 精简层 FontSizeInfo 结构体测试(语言+字号)
public static class FontSizeInfoTest
{
	public static void Run()
	{
		testDefaultValues();
		testConstructor();
		testCopyIndependent();
	}

	// 默认字段
	static void testDefaultValues()
	{
		FontSizeInfo info = new FontSizeInfo();
		assertNull(info.mLanguage, "默认语言 null");
		assertEqual(0, info.mFontSize, "默认字号 0");
	}

	// 构造赋值
	static void testConstructor()
	{
		FontSizeInfo info = new FontSizeInfo("zh", 24);
		assertEqual("zh", info.mLanguage, "语言读回");
		assertEqual(24, info.mFontSize, "字号读回");
	}

	// 结构体复制独立
	static void testCopyIndependent()
	{
		FontSizeInfo a = new FontSizeInfo("en", 18);
		FontSizeInfo b = a;
		b.mFontSize = 30;
		assertEqual(18, a.mFontSize, "a 不受影响");
		assertEqual(30, b.mFontSize, "b 独立");
	}
}
