using UnityEngine;
using static TestAssert;

// ImageXBR4 纯函数测试：distYCbCr/reduce/isPixelEqual/IsBlendingNeeded/tex2D/scaleTexture
public static class ImageXBR4Test
{
	public static void Run()
	{
		testReduceBlack();
		testReduceWhite();
		testReduceRed();
		testReduceGreen();
		testReduceBlue();
		testReduceGrey();
		testDistYCbCrSame();
		testDistYCbCrDifferent();
		testDistYCbCrBlackWhite();
		testIsPixelEqualSame();
		testIsPixelEqualDifferent();
		testIsPixelEqualClose();
		testIsBlendingNeededNone();
		testIsBlendingNeededNormal();
		testIsBlendingNeededDominant();
		testIsBlendingNeededMixed();
		testTex2DCenter();
		testTex2DCorner();
		testScaleTexture();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 因为 distYCbCr/reduce/isPixelEqual/IsBlendingNeeded 是 protected static，
	// 我们通过反射调用它们
	private static System.Reflection.MethodInfo sDistYCbCrMethod;
	private static System.Reflection.MethodInfo sReduceMethod;
	private static System.Reflection.MethodInfo sIsPixelEqualMethod;
	private static System.Reflection.MethodInfo sIsBlendingNeededMethod;
	private static System.Reflection.MethodInfo sTex2DMethod;
	private static System.Reflection.MethodInfo sScaleTextureMethod;

	private static void initReflection()
	{
		if (sDistYCbCrMethod == null)
		{
			var type = typeof(ImageXBR4);
			var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
			sDistYCbCrMethod = type.GetMethod("distYCbCr", flags);
			sReduceMethod = type.GetMethod("reduce", flags);
			sIsPixelEqualMethod = type.GetMethod("isPixelEqual", flags);
			sIsBlendingNeededMethod = type.GetMethod("IsBlendingNeeded", flags);
			sTex2DMethod = type.GetMethod("tex2D", flags);
			sScaleTextureMethod = type.GetMethod("scaleTexture", flags);
		}
	}

	private static float callDistYCbCr(Vector3 a, Vector3 b)
	{
		initReflection();
		return (float)sDistYCbCrMethod.Invoke(null, new object[] { a, b });
	}

	private static float callReduce(Vector3 color)
	{
		initReflection();
		return (float)sReduceMethod.Invoke(null, new object[] { color });
	}

	private static bool callIsPixelEqual(Vector3 a, Vector3 b)
	{
		initReflection();
		return (bool)sIsPixelEqualMethod.Invoke(null, new object[] { a, b });
	}

	private static bool callIsBlendingNeeded(Vector4 blend)
	{
		initReflection();
		return (bool)sIsBlendingNeededMethod.Invoke(null, new object[] { blend });
	}

	private static Color callTex2D(Color[] pixels, int width, int height, Vector2 texCoord)
	{
		initReflection();
		return (Color)sTex2DMethod.Invoke(null, new object[] { pixels, width, height, texCoord });
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testReduceBlack()
	{
		float result = callReduce(new Vector3(0, 0, 0));
		assertEqual(0.0f, result, "黑色 reduce 为 0");
	}

	private static void testReduceWhite()
	{
		float result = callReduce(new Vector3(1, 1, 1));
		float expected = new Vector3(1, 1, 1).dot(new Vector3(65536.0f, 256.0f, 1.0f));
		assertEqual(expected, result, "白色 reduce = 65536+256+1");
	}

	private static void testReduceRed()
	{
		float result = callReduce(new Vector3(1, 0, 0));
		assertEqual(65536.0f, result, "红色 reduce = 65536");
	}

	private static void testReduceGreen()
	{
		float result = callReduce(new Vector3(0, 1, 0));
		assertEqual(256.0f, result, "绿色 reduce = 256");
	}

	private static void testReduceBlue()
	{
		float result = callReduce(new Vector3(0, 0, 1));
		assertEqual(1.0f, result, "蓝色 reduce = 1");
	}

	private static void testReduceGrey()
	{
		float result = callReduce(new Vector3(0.5f, 0.5f, 0.5f));
		// reduce 内部使用 (int) 强转: (int)(0.5*65536 + 0.5*256 + 0.5) = (int)32896.5 = 32896
		float expected = (int)(0.5f * 65536.0f + 0.5f * 256.0f + 0.5f * 1.0f);
		assertEqual(expected, result, "灰色 reduce 正确（(int)强转）");
	}

	private static void testDistYCbCrSame()
	{
		var color = new Vector3(0.5f, 0.5f, 0.5f);
		float dist = callDistYCbCr(color, color);
		assertEqual(0.0f, dist, "相同颜色距离为0");
	}

	private static void testDistYCbCrDifferent()
	{
		var black = new Vector3(0, 0, 0);
		var white = new Vector3(1, 1, 1);
		float dist = callDistYCbCr(black, white);
		assertTrue(dist > 0, "不同颜色距离大于0");
	}

	private static void testDistYCbCrBlackWhite()
	{
		var black = new Vector3(0, 0, 0);
		var white = new Vector3(1, 1, 1);
		float dist = callDistYCbCr(black, white);
		// Y = dot((1,1,1), (0.2627,0.6780,0.0593)) = 1.0
		float expectedY = 1.0f;
		float scaleB = 0.5f / (1.0f - 0.0593f);
		float scaleR = 0.5f / (1.0f - 0.2627f);
		float Cb = scaleB * (1.0f - expectedY);
		float Cr = scaleR * (1.0f - expectedY);
		float expected = Mathf.Sqrt(expectedY * expectedY + Cb * Cb + Cr * Cr);
		// scaleB/scaleR 有浮点精度误差，expected ≈ 1.0，dist ≈ 0.9999999
		assertTrue(Mathf.Abs(expected - dist) < 0.0001f, "黑白距离正确(浮点精度)");
	}

	private static void testIsPixelEqualSame()
	{
		var color = new Vector3(0.3f, 0.5f, 0.7f);
		assertTrue(callIsPixelEqual(color, color), "相同颜色应相等");
	}

	private static void testIsPixelEqualDifferent()
	{
		var black = new Vector3(0, 0, 0);
		var white = new Vector3(1, 1, 1);
		assertFalse(callIsPixelEqual(black, white), "黑白不应相等");
	}

	private static void testIsPixelEqualClose()
	{
		// 两个非常接近的颜色应该被视为相等
		var a = new Vector3(0.5f, 0.5f, 0.5f);
		var b = new Vector3(0.5f, 0.5001f, 0.5f);
		assertTrue(callIsPixelEqual(a, b), "接近的颜色应相等");
	}

	private static void testIsBlendingNeededNone()
	{
		var blend = new Vector4(0, 0, 0, 0);
		assertFalse(callIsBlendingNeeded(blend), "全部 BLEND_NONE 不需要混合");
	}

	private static void testIsBlendingNeededNormal()
	{
		var blend = new Vector4(0, 1, 0, 0);
		assertTrue(callIsBlendingNeeded(blend), "有 BLEND_NORMAL 需要混合");
	}

	private static void testIsBlendingNeededDominant()
	{
		var blend = new Vector4(0, 0, 2, 0);
		assertTrue(callIsBlendingNeeded(blend), "有 BLEND_DOMINANT 需要混合");
	}

	private static void testIsBlendingNeededMixed()
	{
		var blend = new Vector4(1, 0, 0, 0);
		assertTrue(callIsBlendingNeeded(blend), "任意非零需要混合");
	}

	private static void testTex2DCenter()
	{
		var pixels = new Color[] {
			new Color(1,0,0), new Color(0,1,0),
			new Color(0,0,1), new Color(1,1,1)
		};
		var result = callTex2D(pixels, 2, 2, new Vector2(0.5f, 0.5f));
		// x = (int)(0.5*2) = 1, y = (int)(0.5*2) = 1, index = 1+1*2 = 3 => white
		assertEqual(new Color(1, 1, 1), result, "中心 (0.5,0.5) 采样到 index=3 白色");
	}

	private static void testTex2DCorner()
	{
		var pixels = new Color[] {
			new Color(1,0,0), new Color(0,1,0),
			new Color(0,0,1), new Color(1,1,1)
		};
		// (1,1) 被 saturate 后为 (1,1)，clamp 后 x=1, y=1 => index=1+1*2=3 => white
		var result = callTex2D(pixels, 2, 2, new Vector2(1.0f, 1.0f));
		assertEqual(new Color(1, 1, 1), result, "角落 (1,1) 采样到右下角像素");
	}

	private static void testScaleTexture()
	{
		var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		tex.SetPixels(new Color[] {
			new Color(1,0,0,1), new Color(0,1,0,1),
			new Color(0,0,1,1), new Color(1,1,1,1)
		});
		tex.Apply();

		// 通过反射调用 scaleTexture
		initReflection();
		var args = new object[] { tex, 2, null };
		sScaleTextureMethod.Invoke(null, args);
		var scaledPixels = (Color[])args[2];

		// 缩放后应为 4x4 = 16 像素
		assertEqual(16, scaledPixels.Length, "2x2 放大2倍后16个像素");

		// originY = (int)(i / (scaledHeight * height)) = (int)(i/8), i<4 → originY=0
		// originX = (int)(j / (scaledWidth * width)) = (int)(j/8), j<4 → originX=0
		// 所有像素都采样自 pixels[0] = 红色
		assertEqual(new Color(1, 0, 0, 1), scaledPixels[0], "(0,0) 红色");
		assertEqual(new Color(1, 0, 0, 1), scaledPixels[15], "(3,3) 也是红色");

		Object.DestroyImmediate(tex);
	}
}
