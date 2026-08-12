using UnityEngine;
using static TestAssert;

// ImageNumber 深度测试
// 数字图片组件(Image 子类), 核心是纯逻辑:
//   setNumber: 设置数字字符串(mSpriteList 为空时跳过显示检查)
//   getContentWidth: mNumberWidth*count + mInterval*(count-1)
//                  (未 setSpriteList 时 mNumberWidth=0, 可精确断言间隔贡献)
//   clearNumber/setInterval/setDocking/getNumber/getInterval/getDocking
// setSpriteList 依赖真实 Sprite 资源, 合法跳过
//
// 环境: 裸 GameObject + ImageNumber(Image 子类)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class ImageNumberTest
{
	public static void Run()
	{
		testSetNumber();
		testGetContentWidth();
		testClearNumber();
		testSetDocking();
		testSetInterval();
		testNumberLongNegativeChain();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 ImageNumber
	// ═════════════════════════════════════════════════════════════════
	private static ImageNumber createNumber(out GameObject go)
	{
		go = new GameObject("ImageNumberGO");
		go.AddComponent<RectTransform>();
		return go.AddComponent<ImageNumber>();
	}

	// setNumber: 字符串存储 + 读回
	private static void testSetNumber()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setNumber("12345");
			assertEqual("12345", number.getNumber(), "setNumber 字符串读回");
			// 覆盖
			number.setNumber("999");
			assertEqual("999", number.getNumber(), "setNumber 覆盖读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getContentWidth: 未 setSpriteList 时 mNumberWidth=0 → 只贡献间隔部分
	private static void testGetContentWidth()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setInterval(5);
			number.setNumber("123");   // 3 位
			// mNumberWidth=0 → 0*3 + 5*2 = 10
			assertEqual(10, number.getContentWidth(), "3 位间隔 5 → 内容宽 10");
			number.setNumber("12345");   // 5 位
			assertEqual(20, number.getContentWidth(), "5 位间隔 5 → 内容宽 20");
			number.setInterval(10);
			assertEqual(40, number.getContentWidth(), "5 位间隔 10 → 内容宽 40");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clearNumber: 清空数字
	private static void testClearNumber()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setNumber("123");
			assertEqual("123", number.getNumber(), "清空前有数字");
			number.clearNumber();
			assertNull(number.getNumber(), "clearNumber 后数字为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDocking: 停靠方式读回
	private static void testSetDocking()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setDocking(DOCKING_POSITION.LEFT);
			assertTrue(DOCKING_POSITION.LEFT == number.getDocking(), "setDocking(LEFT) 读回");
			number.setDocking(DOCKING_POSITION.RIGHT);
			assertTrue(DOCKING_POSITION.RIGHT == number.getDocking(), "setDocking(RIGHT) 读回");
			number.setDocking(DOCKING_POSITION.CENTER);
			assertTrue(DOCKING_POSITION.CENTER == number.getDocking(), "setDocking(CENTER) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setInterval: 间隔读回
	private static void testSetInterval()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setInterval(7);
			assertEqual(7, number.getInterval(), "setInterval(7) 读回");
			number.setInterval(0);
			assertEqual(0, number.getInterval(), "setInterval(0) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 加深: 长数字/负数链 + interval 组合宽度
	private static void testNumberLongNegativeChain()
	{
		ImageNumber number = createNumber(out GameObject go);
		try
		{
			number.setNumber("123456789");
			assertEqual("123456789", number.getNumber(), "长数字读回");
			int longWidth = number.getContentWidth();
			number.setNumber("-123");
			assertEqual("-123", number.getNumber(), "负数读回");
			// interval 影响组合宽度
			int width0 = number.getContentWidth();
			number.setInterval(10);
			int width10 = number.getContentWidth();
			assertTrue(width10 > width0, "interval 增大 → 内容宽增大");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
