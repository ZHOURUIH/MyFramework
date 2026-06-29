using static TestAssert;

// Undo 抽象类单元测试
// 创建具体子类、调用 undo 方法
public static class UndoTest
{
	public static void Run()
	{
		testCreate();
		testUndo();
	}

	// ─── 创建 ────────────────────────────────────────────────────────────

	private static void testCreate()
	{
		var u = new TestUndo();
		assertNotNull(u, "TestUndo 实例不应为空");
	}

	// ─── undo 调用 ───────────────────────────────────────────────────────

	private static void testUndo()
	{
		var u = new TestUndo();
		assertFalse(u.mUndoCalled, "初始状态 mUndoCalled 应为 false");
		u.undo();
		assertTrue(u.mUndoCalled, "调用 undo 后 mUndoCalled 应为 true");
	}
}

// 测试专用的 Undo 具体子类
public class TestUndo : MyUndo
{
	public bool mUndoCalled;
    public override void resetProperty()
    {
        base.resetProperty();
		mUndoCalled = false;
    }
	public override void undo()
	{
		mUndoCalled = true;
	}
}