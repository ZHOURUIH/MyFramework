using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// SequenceSpritePreviewBase 序列帧预览基类的静态工具方法
// 覆盖: setImage/getImage(组件读写 Sprite)、getSpriteSetName(纯字符串解析)
public static class SequenceSpritePreviewBaseTest
{
	public static void Run()
	{
		testGetSpriteSetName();
		testSetImageAndGetImage();
	}

	// getSpriteSetName: 取最后一个'_'之前的部分
	private static void testGetSpriteSetName()
	{
		assertTrue(SequenceSpritePreviewBase.getSpriteSetName("") == "", "空字符串返回空");
		assertTrue(SequenceSpritePreviewBase.getSpriteSetName("abc") == "", "无下划线返回空");
		assertTrue(SequenceSpritePreviewBase.getSpriteSetName("abc_def") == "abc", "取前缀");
		assertTrue(SequenceSpritePreviewBase.getSpriteSetName("a_b_c_10") == "a_b_c", "取最后一个下划线前缀");
	}

	// setImage/getImage: 通过 Image 组件读写 Sprite
	private static void testSetImageAndGetImage()
	{
		GameObject go = new GameObject("SeqPreview");
		go.AddComponent<RectTransform>();
		Image image = go.AddComponent<Image>();
		image.sprite = null;
		Sprite sprite = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
		try
		{
			SequenceSpritePreviewBase.setImage(image, sprite);
			Sprite got = SequenceSpritePreviewBase.getImage(image);
			assertTrue(got != null, "setImage 后 getImage 返回非空 Sprite");

			// null 组件: setImage 直接返回, getImage 返回 null
			SequenceSpritePreviewBase.setImage(null, sprite);
			Sprite gotNull = SequenceSpritePreviewBase.getImage(null);
			assertTrue(gotNull == null, "getImage(null) 返回 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(sprite);
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
