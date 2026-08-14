using static TestAssert;

// myUGUINumber: 数字窗口——无图集环境下仅可测字段读写/截断/边界计算
// (核心 refreshNumber 依赖图集 Sprite, 合法跳过; 不调 init, 因 setMaxCount 依赖 LayoutScript)
public static class MyUGUINumberTest
{
	public static void Run()
	{
		testDefaultValues();
		testSetInterval();
		testSetDockingPosition();
		testSetDirectionNoThrow();
		testSetNumberTruncateToMaxCount();
		testSetNumberInt();
		testContentWidthEmpty();
		testContentHeightEmpty();
		testGetNumberStyle();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 构造默认值
	private static void testDefaultValues()
	{
		myUGUINumber number = new myUGUINumber();
		assertEqual("", number.getNumber(), "默认数字为空");
		assertEqual(5, number.getInterval(), "默认间隔 5");
		assertEqual(DOCKING_POSITION.LEFT, number.getDockingPosition(), "默认左停靠");
		assertEqual(0, number.getMaxCount(), "未 init 时 maxCount 默认 0");
	}

	// setInterval/getInterval 读写
	private static void testSetInterval()
	{
		myUGUINumber number = new myUGUINumber();
		number.setInterval(10);
		assertEqual(10, number.getInterval(), "setInterval(10) 后读回 10");
		number.setInterval(0);
		assertEqual(0, number.getInterval(), "setInterval(0) 后读回 0");
		number.setInterval(-3);
		assertEqual(-3, number.getInterval(), "setInterval(-3) 后读回 -3");
	}

	// setDockingPosition/getDockingPosition 读写
	private static void testSetDockingPosition()
	{
		myUGUINumber number = new myUGUINumber();
		number.setDockingPosition(DOCKING_POSITION.CENTER);
		assertEqual(DOCKING_POSITION.CENTER, number.getDockingPosition(), "CENTER 读回");
		number.setDockingPosition(DOCKING_POSITION.RIGHT);
		assertEqual(DOCKING_POSITION.RIGHT, number.getDockingPosition(), "RIGHT 读回");
		number.setDockingPosition(DOCKING_POSITION.TOP);
		assertEqual(DOCKING_POSITION.TOP, number.getDockingPosition(), "TOP 读回");
		number.setDockingPosition(DOCKING_POSITION.BOTTOM);
		assertEqual(DOCKING_POSITION.BOTTOM, number.getDockingPosition(), "BOTTOM 读回");
	}

	// setDirection: 无 getter, 验证不抛异常(mSpriteList 空时 refreshNumber 提前返回)
	private static void testSetDirectionNoThrow()
	{
		myUGUINumber number = new myUGUINumber();
		number.setDirection(NUMBER_DIRECTION.VERTICAL);
		number.setDirection(NUMBER_DIRECTION.HORIZONTAL);
		// 无异常即通过
	}

	// setNumber: 超过 maxCount 截断(mMaxCount=0 时任意数字被截断为空)
	private static void testSetNumberTruncateToMaxCount()
	{
		myUGUINumber number = new myUGUINumber();
		number.setNumber("12345");
		assertEqual("", number.getNumber(), "mMaxCount=0 时任意数字被截断为空");
	}

	// setNumber(int) 重载不抛(截断为空)
	private static void testSetNumberInt()
	{
		myUGUINumber number = new myUGUINumber();
		number.setNumber(123);
		assertEqual("", number.getNumber(), "int 重载同样被截断为空");
	}

	// 空列表空数字: getContentWidth = 0 + interval*(0-1) = -interval
	private static void testContentWidthEmpty()
	{
		myUGUINumber number = new myUGUINumber();
		assertEqual(-5, number.getContentWidth(), "空内容宽度 = -interval(5)");
		assertEqual(-5, number.getAllSpriteWidth(), "空内容图片宽度 = -interval(5)");
	}

	private static void testContentHeightEmpty()
	{
		myUGUINumber number = new myUGUINumber();
		assertEqual(-5, number.getContentHeight(), "空内容高度 = -interval(5)");
		assertEqual(-5, number.getAllSpriteHeight(), "空内容图片高度 = -interval(5)");
	}

	// getNumberStyle: 纯字段返回, 直接 new 后默认 null(未设置数字图集名)
	private static void testGetNumberStyle()
	{
		myUGUINumber number = new myUGUINumber();
		assertTrue(number.getNumberStyle() == null, "直接 new 后 getNumberStyle 默认 null");
	}
}
