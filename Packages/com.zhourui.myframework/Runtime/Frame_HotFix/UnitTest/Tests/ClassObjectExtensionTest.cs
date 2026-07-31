using static ClassObjectExtension;
using static TestAssert;

// ClassObjectExtension 扩展方法测试
public static class ClassObjectExtensionTest
{
	public static void Run()
	{
		testIsValid();
	}

	private static void testIsValid()
	{
		// null 引用 → 无效
		ClassObject nullObj = null;
		assertFalse(nullObj.isValid(), "isValid null → false");

		// 未从池中分配(new) → isDestroy=true → 无效
		var obj = new TestClassObjExt();
		assertFalse(obj.isValid(), "isValid new (isDestroy=true) → false");

		// setDestroy(false) 后 → 有效
		obj.setDestroy(false);
		assertTrue(obj.isValid(), "isValid setDestroy(false) → true");

		// setDestroy(true) 后 → 无效
		obj.setDestroy(true);
		assertFalse(obj.isValid(), "isValid setDestroy(true) → false");
	}
}

// 测试用 ClassObject 子类
class TestClassObjExt : ClassObject
{
	public override void resetProperty() { base.resetProperty(); }
}
