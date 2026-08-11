using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIImageButton 深度测试(状态切换按钮):
//   init(预加 Image) / isSelected / setSelected(往返+幂等, mUseStateSprite=false 时不触发资源)
//   setNormalSprite(apply=false 避开资源) / setPressSprite / setHoverSprite / setSelectedSprite
//   setSpriteNames(2/3 参) / 触摸生命周期(mUseStateSprite=false 走 base 链路不崩)
public static class MyUGUIImageButtonTest
{
	public static void Run()
	{
		testInitSelectedFalse();
		testSetSelectedToggle();
		testStateSpriteSettersGuard();
		testTouchLifecycleNoStateSprite();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIImageButton(预加 Image)
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createButton(out myUGUIImageButton btn)
	{
		GameObject go = new GameObject("ImageButton");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		btn = new myUGUIImageButton();
		btn.setObject(go);
		btn.init();
		return go;
	}

	// init 后默认未选中
	private static void testInitSelectedFalse()
	{
		GameObject go = createButton(out myUGUIImageButton btn);
		try
		{
			assertFalse(btn.isSelected(), "init 后默认未选中");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSelected/isSelected 往返 + 幂等(mUseStateSprite=false 不触发资源加载)
	private static void testSetSelectedToggle()
	{
		GameObject go = createButton(out myUGUIImageButton btn);
		try
		{
			btn.setSelected(true);
			assertTrue(btn.isSelected(), "setSelected(true) 读回");
			btn.setSelected(true);   // 相同值幂等
			assertTrue(btn.isSelected(), "重复 setSelected(true) 无变化");
			btn.setSelected(false);
			assertFalse(btn.isSelected(), "setSelected(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 状态 sprite setter 守卫式(apply=false 避免触发图集资源加载)
	private static void testStateSpriteSettersGuard()
	{
		GameObject go = createButton(out myUGUIImageButton btn);
		try
		{
			btn.setNormalSprite("Normal", false);   // apply=false: 只存字段不触发 setSpriteName
			btn.setPressSprite("Press");
			btn.setHoverSprite("Hover");
			btn.setSelectedSprite("Selected");
			btn.setSpriteNames("PressA", "HoverA");
			btn.setSpriteNames("PressB", "HoverB", "SelectedB");
			// 全部守卫式调用不崩(无 getter 可断言)
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 触摸生命周期: mUseStateSprite=false(默认) 时走 base 链路, 无资源操作
	private static void testTouchLifecycleNoStateSprite()
	{
		GameObject go = createButton(out myUGUIImageButton btn);
		try
		{
			btn.onTouchEnter(new Vector3(1.0f, 1.0f, 0.0f), 1);
			btn.onTouchDown(new Vector3(1.0f, 1.0f, 0.0f), 1);
			btn.onTouchUp(new Vector3(1.0f, 1.0f, 0.0f), 1);
			btn.onTouchLeave(new Vector3(1.0f, 1.0f, 0.0f), 1);
			// 全程不崩 + 状态保持
			assertFalse(btn.isSelected(), "触摸链路后选中状态不变");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
