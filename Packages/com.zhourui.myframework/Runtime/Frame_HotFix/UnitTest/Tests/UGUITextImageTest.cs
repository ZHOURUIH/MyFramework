using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUITextImage 深度测试(Text 子类, 支持 <quad> 图片标记)
//   setCreateImage/setDestroyImage: 图片创建/销毁回调存储
//   SetVerticesDirty: 无回调时直接 return(空安全); 有回调时触发图片重建
// 环境: 裸 GameObject + UGUITextImage(AddComponent, Text 子类)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class UGUITextImageTest
{
	public static void Run()
	{
		testAddComponentSafe();
		testCallbackStorage();
		testSetVerticesDirtyNoCallback();
		testSetVerticesDirtyWithCallback();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static TextImage createTextImage(out GameObject go)
	{
		go = new GameObject("TextImageGO");
		go.AddComponent<RectTransform>();
		return go.AddComponent<TextImage>();
	}

	// AddComponent 安全(Text 子类)
	private static void testAddComponentSafe()
	{
		TextImage ti = createTextImage(out GameObject go);
		try
		{
			assertTrue(ti != null, "AddComponent<TextImage> 成功");
			assertTrue(go.GetComponent<Text>() != null, "TextImage 是 Text 子类");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 回调存储(setCreateImage/setDestroyImage 后可触发重建)
	private static void testCallbackStorage()
	{
		TextImage ti = createTextImage(out GameObject go);
		try
		{
			bool createCalled = false;
			bool destroyCalled = false;
			// ImageFunction = () => myUGUIImage(无参返回); ImageCallback = (myUGUIImage)(一参)
			// 文本为空 → mRegex 无匹配 → 不调用回调, 仅验证签名与存储
			ti.setCreateImage(() => { createCalled = true; return null; });
			ti.setDestroyImage((image) => destroyCalled = true);
			ti.SetVerticesDirty();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 无回调时 SetVerticesDirty 空安全
	private static void testSetVerticesDirtyNoCallback()
	{
		TextImage ti = createTextImage(out GameObject go);
		try
		{
			ti.SetVerticesDirty();   // 无回调, 直接 return
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 有回调时 SetVerticesDirty 不崩溃
	private static void testSetVerticesDirtyWithCallback()
	{
		TextImage ti = createTextImage(out GameObject go);
		try
		{
			// 有 quad 标签 → SetVerticesDirty 会真实调用 mCreateImage() 并解引用返回值(必须非 null)
			ti.setCreateImage(() =>
			{
				GameObject imgGo = new GameObject("QuadImgGO");
				imgGo.AddComponent<RectTransform>();
				myUGUIImage img = new myUGUIImage();
				img.setIsNewObject(true);
				img.setObject(imgGo);
				img.init();
				return img;
			});
			ti.setDestroyImage((image) =>
			{
				if (image != null)
				{
					UnityEngine.Object.DestroyImmediate(image.getGameObject());
				}
			});
			ti.text = "测试<quad width=10 sprite=icon_1/>文本";
			ti.SetVerticesDirty();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
