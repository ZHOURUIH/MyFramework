using static ClassObjectExtension;
using static TestAssert;

// ClassObjectExtension 穷举测试
public static class ClassObjectExtensionTest
{
	public static void Run()
	{
		testIsValid();
		testIsValidNull();
		testIsValidAfterSetDestroy();
		testIsValidNewObject();
	}

	private static void testIsValid()
	{
		// null 引用 → 无效
		ClassObject nullObj = null;
		assertFalse(nullObj.isValid(), "isValid null → false");

		// setDestroy(false) → 有效
		var obj = new TestClassObjExt();
		obj.setDestroy(false);
		assertTrue(obj.isValid(), "isValid setDestroy(false) → true");

		// setDestroy(true) → 无效
		obj.setDestroy(true);
		assertFalse(obj.isValid(), "isValid setDestroy(true) → false");
	}

	private static void testIsValidNull()
	{
		ClassObject obj = null;
		assertFalse(obj.isValid(), "isValid null → false");
	}

	private static void testIsValidAfterSetDestroy()
	{
		var obj = new TestClassObjExt();
		obj.setDestroy(false);
		assertTrue(obj.isValid());
		obj.setDestroy(true);
		assertFalse(obj.isValid());
		obj.setDestroy(false);
		assertTrue(obj.isValid());
	}

	private static void testIsValidNewObject()
	{
		// new 出来的对象 mHasDestroy=true，无效
		var obj = new TestClassObjExt();
		assertFalse(obj.isValid(), "isValid new (isDestroy=true) → false");
	}
}

class TestClassObjExt : ClassObject
{
	public override void resetProperty() { base.resetProperty(); }
}
