#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

// Serializable 单元测试
// mValid / mOptional 默认值 / resetProperty
public static class SerializableTest
{
	public static void Run()
	{
		testDefaultState();
		testResetProperty();
	}

	// ─── 默认状态 ────────────────────────────────────────────────────────

	private static void testDefaultState()
	{
		var s = new TestSerializable();
		assertTrue(s.mValid, "构造后 mValid 应为 true");
		assertFalse(s.mOptional, "构造后 mOptional 应为 false");
	}

	// ─── resetProperty ──────────────────────────────────────────────────

	private static void testResetProperty()
	{
		var s = new TestSerializable();
		s.mValid = false;
		s.resetProperty();
		assertTrue(s.mValid, "resetProperty 后 mValid 应恢复为 true");
	}
}

// 测试专用的 Serializable 具体子类
public class TestSerializable : Serializable
{
	public override bool read(SerializerRead reader) { return true; }
	public override void write(SerializerWrite writer) { }
}
#endif
