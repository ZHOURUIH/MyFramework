using UnityEngine;
using UnityEngine.UI;
using static TestAssert;
using static FrameUtility;
using static FrameBaseHotFix;

// myUGUIRawImage 深度测试(RawImage 封装)
//   init(预加组件, 默认材质跳过 MaterialPath 检查) / getMaterial / setMaterial
//   getMaterialName / cull / isCull(CanvasGroup alpha) 
//   setWindowShader/getWindowShader / update(applyShader)
//   setTexture(ResourceRef<Texture>) / getTexture / getTextureSize / getTextureName
//   资源加载路径(setTextureName/setMaterialName)因依赖真实资源跳过
public static class MyUGUIRawImageTest
{
	public static void Run()
	{
		testInitWithComponent();
		testCull();
		testWindowShader();
		testSetTexture();
		testMaterial();
		testTextureNull();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIRawImage(预加 RawImage)
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createRawImage(out myUGUIRawImage img)
	{
		GameObject go = new GameObject("RawImage");
		go.AddComponent<RectTransform>();
		go.AddComponent<RawImage>();
		img = new myUGUIRawImage();
		img.setObject(go);
		img.init();
		return go;
	}

	// init: 预加 RawImage → getMaterial 非 null(默认材质), 无 MaterialPath logError
	private static void testInitWithComponent()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		try
		{
			RawImage comp = go.GetComponent<RawImage>();
			assertNotNull(img.getMaterial(), "init 后 getMaterial 非 null");
			assertTrue(ReferenceEquals(comp.material, img.getMaterial()), "getMaterial 返回 RawImage 的材质");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// cull/isCull: CanvasGroup alpha 0/1 切换
	private static void testCull()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		try
		{
			assertFalse(img.isCull(), "初始不剔除");
			img.cull(true);
			assertTrue(img.isCull(), "cull(true) 后 isCull true");
			CanvasGroup group = go.GetComponent<CanvasGroup>();
			assertNotNull(group, "cull 自动添加 CanvasGroup");
			assertEqual(0.0f, group.alpha, 0.001f, "剔除时 alpha=0");
			img.cull(false);
			assertFalse(img.isCull(), "cull(false) 后 isCull false");
			assertEqual(1.0f, group.alpha, 0.001f, "恢复时 alpha=1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setWindowShader/getWindowShader + update 调 applyShader(基类空实现)
	private static void testWindowShader()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		try
		{
			CLASS(out WindowShader shader);
			try
			{
				img.setWindowShader(shader);
				assertTrue(ReferenceEquals(shader, img.getWindowShader()), "setWindowShader 读回同一对象");
				img.update(0.01f);   // applyShader 空实现, 无副作用
			}
			finally
			{
				UN_CLASS(ref shader);
			}
			// null 传参
			img.setWindowShader(null);
			assertNull(img.getWindowShader(), "setWindowShader(null) 后为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setTexture(ResourceRef<Texture>): 绑定纹理 + 尺寸 + 名字(dirty 刷新)
	private static void testSetTexture()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		Texture2D tex = new Texture2D(2, 3);
		CLASS(out ResourceRef<Texture> refTex);
		try
		{
			refTex.set(tex);
			img.setTexture(refTex);
			assertTrue(ReferenceEquals(tex, img.getTexture()), "setTexture 后 getTexture 同一纹理");
			Vector2 size = img.getTextureSize();
			assertEqual(2.0f, size.x, 0.001f, "纹理宽度 2");
			assertEqual(3.0f, size.y, 0.001f, "纹理高度 3");
			assertEqual(tex.name, img.getTextureName(), "getTextureName 刷新为纹理名");
		}
		finally
		{
			// 平衡 set() 增加的引用计数
			mResourceManager.unload(ref refTex);
			UnityEngine.Object.DestroyImmediate(tex);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setMaterial/getMaterial/getMaterialName: 材质赋值读回
	private static void testMaterial()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		try
		{
			Material mat = img.getMaterial();
			img.setMaterial(mat);
			assertTrue(ReferenceEquals(mat, img.getMaterial()), "setMaterial 后读回同一材质");
			assertEqual(mat.name, img.getMaterialName(), "getMaterialName 返回材质名");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 无纹理时的 null 安全
	private static void testTextureNull()
	{
		GameObject go = createRawImage(out myUGUIRawImage img);
		try
		{
			assertNull(img.getTexture(), "初始无纹理");
			assertTrue(img.getTextureSize().isZero(), "无纹理时尺寸 zero");
			assertNull(img.getTextureName(), "无纹理时名字 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
