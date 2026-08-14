using static TestAssert;

// myUGUIImagePro: 带 shader 的 Image 封装, 大部分方法依赖 mImage/资源管理器(环境依赖不测),
// 只测纯字段存储的 setWindowShader/getWindowShader(直接 new 不 init)
public static class MyUGUIImageProTest
{
	public static void Run()
	{
		testWindowShaderDefault();
		testSetWindowShader();
	}

	// 构造默认: getWindowShader 为 null(未设置 shader)
	private static void testWindowShaderDefault()
	{
		myUGUIImagePro img = new myUGUIImagePro();
		assertTrue(img.getWindowShader() == null, "直接 new 后 getWindowShader 默认 null");
	}

	// setWindowShader/getWindowShader: 纯字段存储(设置后启用更新 mNeedUpdate=true, 无副作用)
	private static void testSetWindowShader()
	{
		myUGUIImagePro img = new myUGUIImagePro();
		WindowShader shader = new WindowShader();
		img.setWindowShader(shader);
		assertTrue(ReferenceEquals(shader, img.getWindowShader()), "setWindowShader 引用存储");
		// 置 null 清空
		img.setWindowShader(null);
		assertTrue(img.getWindowShader() == null, "setWindowShader(null) 清空");
	}
}
