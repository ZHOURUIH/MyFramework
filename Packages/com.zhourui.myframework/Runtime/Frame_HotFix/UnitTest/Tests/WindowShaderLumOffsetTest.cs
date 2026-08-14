using static TestAssert;

// WindowShaderLumOffset: 亮度偏移 shader 参数类(纯 C# ClassObject, 构造只调 Shader.PropertyToID), 直接 new 可测
public static class WindowShaderLumOffsetTest
{
	public static void Run()
	{
		testDefaultAndGetSet();
		testResetProperty();
		testApplyShaderNullSafe();
	}

	// 默认值 + setLumOffset/getLumOffset 读写
	private static void testDefaultAndGetSet()
	{
		WindowShaderLumOffset shader = new WindowShaderLumOffset();
		assertEqual(0.0f, shader.getLumOffset(), 0.0001f, "默认亮度偏移 0");
		shader.setLumOffset(0.5f);
		assertEqual(0.5f, shader.getLumOffset(), 0.0001f, "setLumOffset(0.5) 读回");
		shader.setLumOffset(-0.3f);
		assertEqual(-0.3f, shader.getLumOffset(), 0.0001f, "setLumOffset(-0.3) 读回");
	}

	// resetProperty: 亮度偏移复位为 0
	private static void testResetProperty()
	{
		WindowShaderLumOffset shader = new WindowShaderLumOffset();
		shader.setLumOffset(0.8f);
		shader.resetProperty();
		assertEqual(0.0f, shader.getLumOffset(), 0.0001f, "resetProperty 后亮度偏移 0");
	}

	// applyShader(null): 空材质安全(内部 mat != null 判空)
	private static void testApplyShaderNullSafe()
	{
		WindowShaderLumOffset shader = new WindowShaderLumOffset();
		shader.setLumOffset(0.5f);
		shader.applyShader(null);
		// 无异常即通过
	}
}
