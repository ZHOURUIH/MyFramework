using static TestAssert;

// Serializable 穷举测试
public static class SerializableTest
{
	public static void Run()
	{
		testDefaultState();
		testResetProperty();
		testSetValid();
		testSetOptional();
		testMultipleInstances();
	}

	private static void testDefaultState()
	{
		var s = new TestSerializable();
		assertTrue(s.mValid, "构造后 mValid=true");
		assertFalse(s.mOptional, "构造后 mOptional=false");
	}

	private static void testResetProperty()
	{
		var s = new TestSerializable();
		s.mValid = false;
		s.mOptional = true;
		s.resetProperty();
		assertTrue(s.mValid, "resetProperty 后 mValid=true");
		assertFalse(s.mOptional, "resetProperty 后 mOptional=false");
	}

	private static void testSetValid()
	{
		var s = new TestSerializable();
		s.mValid = false;
		assertFalse(s.mValid);
		s.mValid = true;
		assertTrue(s.mValid);
	}

	private static void testSetOptional()
	{
		var s = new TestSerializable();
		s.mOptional = true;
		assertTrue(s.mOptional);
		s.mOptional = false;
		assertFalse(s.mOptional);
	}

	private static void testMultipleInstances()
	{
		var a = new TestSerializable();
		var b = new TestSerializable();
		a.mValid = false;
		assertFalse(a.mValid);
		assertTrue(b.mValid); // b 不受影响
	}
}

public class TestSerializable : Serializable
{
	public override bool read(SerializerRead reader) { return true; }
	public override void write(SerializerWrite writer) { }
}
