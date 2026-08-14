using UnityEngine;
using static TestAssert;

// WindowShaderHSLOffset: HSL 偏移 shader 参数类(纯 C# ClassObject, 构造只调 Shader.PropertyToID), 直接 new 可测
public static class WindowShaderHSLOffsetTest
{
	public static void Run()
	{
		testDefaultAndGetSet();
		testResetProperty();
		testApplyShaderNullSafe();
	}

	// 默认值 + set/get 读写(Vector3 偏移与 Texture 引用)
	private static void testDefaultAndGetSet()
	{
		WindowShaderHSLOffset shader = new WindowShaderHSLOffset();
		Vector3 offset = shader.getHSLOffset();
		assertEqual(0.0f, offset.x, 0.0001f, "默认 HSL 偏移 x 0");
		assertEqual(0.0f, offset.y, 0.0001f, "默认 HSL 偏移 y 0");
		assertEqual(0.0f, offset.z, 0.0001f, "默认 HSL 偏移 z 0");
		assertTrue(shader.getHSLTexture() == null, "默认 HSL 纹理 null");

		shader.setHSLOffset(new Vector3(0.1f, 0.2f, 0.3f));
		Vector3 read = shader.getHSLOffset();
		assertEqual(0.1f, read.x, 0.0001f, "setHSLOffset x 读回");
		assertEqual(0.2f, read.y, 0.0001f, "setHSLOffset y 读回");
		assertEqual(0.3f, read.z, 0.0001f, "setHSLOffset z 读回");

		Texture2D tex = new Texture2D(2, 2);
		try
		{
			shader.setHSLTexture(tex);
			assertTrue(ReferenceEquals(tex, shader.getHSLTexture()), "setHSLTexture 引用存储");
			shader.setHSLTexture(null);
			assertTrue(shader.getHSLTexture() == null, "setHSLTexture(null) 清空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(tex);
		}
	}

	// resetProperty: HSL 偏移复位 0, 纹理置 null
	private static void testResetProperty()
	{
		WindowShaderHSLOffset shader = new WindowShaderHSLOffset();
		shader.setHSLOffset(new Vector3(1, 2, 3));
		Texture2D tex = new Texture2D(2, 2);
		shader.setHSLTexture(tex);
		shader.resetProperty();
		Vector3 offset = shader.getHSLOffset();
		assertEqual(0.0f, offset.x, 0.0001f, "resetProperty 后 HSL 偏移 x 0");
		assertTrue(shader.getHSLTexture() == null, "resetProperty 后纹理 null");
		UnityEngine.Object.DestroyImmediate(tex);
	}

	// applyShader(null): 空材质安全(内部 mat != null 判空)
	private static void testApplyShaderNullSafe()
	{
		WindowShaderHSLOffset shader = new WindowShaderHSLOffset();
		shader.setHSLOffset(new Vector3(0.5f, 0, 0));
		shader.applyShader(null);
		// 无异常即通过
	}
}
