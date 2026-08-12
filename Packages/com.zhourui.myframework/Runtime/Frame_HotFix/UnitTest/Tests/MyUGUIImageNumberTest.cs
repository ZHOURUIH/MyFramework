using UnityEngine;
using static TestAssert;

// myUGUIImageNumber 深度测试(图片数字窗口封装)
//   init: 无 ImageNumber 组件自动添加(isNewObject 不 logError)
//   setNumber → mRenderer.setNumber(组件转发) + getNumber 读回
//   setInterval/setDocking → mRenderer 转发
// 环境: 裸 GameObject + RectTransform + myUGUIImageNumber(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUIImageNumberTest
{
	public static void Run()
	{
		testInitAutoAddImageNumber();
		testSetNumberInt();
		testSetNumberLong();
		testInterval();
		testDocking();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIImageNumber createNumber(out GameObject go)
	{
		go = new GameObject("ImageNumberGO");
		go.AddComponent<RectTransform>();
		myUGUIImageNumber num = new myUGUIImageNumber();
		num.setIsNewObject(true);
		num.setObject(go);
		num.init();
		return num;
	}

	// init: 无 ImageNumber 组件 → 自动添加
	private static void testInitAutoAddImageNumber()
	{
		myUGUIImageNumber num = createNumber(out GameObject go);
		try
		{
			assertTrue(go.GetComponent<ImageNumber>() != null, "init 自动添加 ImageNumber");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setNumber(int) → 组件转发
	private static void testSetNumberInt()
	{
		myUGUIImageNumber num = createNumber(out GameObject go);
		try
		{
			num.setNumber(123);
			assertEqual("123", go.GetComponent<ImageNumber>().getNumber(), "setNumber(123) 转发到组件");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setNumber(long)
	private static void testSetNumberLong()
	{
		myUGUIImageNumber num = createNumber(out GameObject go);
		try
		{
			num.setNumber(123456789L);
			assertEqual("123456789", go.GetComponent<ImageNumber>().getNumber(), "setNumber(long) 转发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setInterval → 组件转发
	private static void testInterval()
	{
		myUGUIImageNumber num = createNumber(out GameObject go);
		try
		{
			num.setInterval(5);
			assertEqual(5, go.GetComponent<ImageNumber>().getInterval(), "setInterval(5) 转发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDocking → 组件转发
	private static void testDocking()
	{
		myUGUIImageNumber num = createNumber(out GameObject go);
		try
		{
			num.setDocking(DOCKING_POSITION.RIGHT);
			assertTrue(go.GetComponent<ImageNumber>().getDocking() == DOCKING_POSITION.RIGHT, "setDocking(RIGHT) 转发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
