using static TestAssert;

// SafeListModify / SafeDictionaryModify / SafeHashSetModify 结构体测试
public static class SafeModifyTest
{
	public static void Run()
	{
		testSafeListModifyAdd();
		testSafeListModifyRemove();
		testSafeListModifyFields();
		testSafeDictionaryModifyAdd();
		testSafeDictionaryModifyRemove();
		testSafeDictionaryModifyFields();
		testSafeHashSetModifyAdd();
		testSafeHashSetModifyRemove();
		testSafeHashSetModifyFields();
		testSafeListModifyInt();
		testSafeListModifyString();
		testSafeDictionaryModifyStringInt();
		testSafeHashSetModifyInt();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// SafeListModify
	private static void testSafeListModifyAdd()
	{
		var mod = new SafeListModify<int>(42, true, -1);
		assertEqual(42, mod.mValue, "mValue=42");
		assertTrue(mod.mAdd, "mAdd=true");
		assertEqual(-1, mod.mRemoveIndex, "mRemoveIndex=-1");
	}

	private static void testSafeListModifyRemove()
	{
		var mod = new SafeListModify<string>("test", false, 3);
		assertEqual("test", mod.mValue, "mValue=test");
		assertFalse(mod.mAdd, "mAdd=false");
		assertEqual(3, mod.mRemoveIndex, "mRemoveIndex=3");
	}

	private static void testSafeListModifyFields()
	{
		var mod = new SafeListModify<float>(1.5f, true, 0);
		assertEqual(1.5f, mod.mValue, "mValue=1.5");
		assertTrue(mod.mAdd, "mAdd=true");
		assertEqual(0, mod.mRemoveIndex, "mRemoveIndex=0");
	}

	private static void testSafeListModifyInt()
	{
		var add = new SafeListModify<int>(10, true, -1);
		assertEqual(10, add.mValue, "int add mValue=10");
		assertTrue(add.mAdd, "int add mAdd=true");

		var remove = new SafeListModify<int>(20, false, 5);
		assertEqual(20, remove.mValue, "int remove mValue=20");
		assertFalse(remove.mAdd, "int remove mAdd=false");
		assertEqual(5, remove.mRemoveIndex, "int remove index=5");
	}

	private static void testSafeListModifyString()
	{
		var mod = new SafeListModify<string>("hello", false, 2);
		assertEqual("hello", mod.mValue, "string mValue");
		assertFalse(mod.mAdd, "string mAdd=false");
		assertEqual(2, mod.mRemoveIndex, "string index=2");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// SafeDictionaryModify
	private static void testSafeDictionaryModifyAdd()
	{
		var mod = new SafeDictionaryModify<int, string>(1, "value");
		assertEqual(1, mod.mKey, "add mKey=1");
		assertEqual("value", mod.mValue, "add mValue=value");
		assertTrue(mod.mAdd, "add mAdd=true");
	}

	private static void testSafeDictionaryModifyRemove()
	{
		var mod = new SafeDictionaryModify<int, string>(42);
		assertEqual(42, mod.mKey, "remove mKey=42");
		assertEqual(default(string), mod.mValue, "remove mValue=default");
		assertFalse(mod.mAdd, "remove mAdd=false");
	}

	private static void testSafeDictionaryModifyFields()
	{
		var mod = new SafeDictionaryModify<string, int>("key", 100);
		assertEqual("key", mod.mKey, "mKey=key");
		assertEqual(100, mod.mValue, "mValue=100");
		assertTrue(mod.mAdd, "mAdd=true");
	}

	private static void testSafeDictionaryModifyStringInt()
	{
		var add = new SafeDictionaryModify<string, int>("mykey", 999);
		assertEqual("mykey", add.mKey, "string key");
		assertEqual(999, add.mValue, "int value");
		assertTrue(add.mAdd, "add=true");

		var remove = new SafeDictionaryModify<string, int>("delkey");
		assertEqual("delkey", remove.mKey, "remove key");
		assertEqual(0, remove.mValue, "remove value=default(0)");
		assertFalse(remove.mAdd, "remove=false");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// SafeHashSetModify
	private static void testSafeHashSetModifyAdd()
	{
		var mod = new SafeHashSetModify<int>(42, true);
		assertEqual(42, mod.mValue, "add mValue=42");
		assertTrue(mod.mAdd, "add mAdd=true");
	}

	private static void testSafeHashSetModifyRemove()
	{
		var mod = new SafeHashSetModify<string>("item", false);
		assertEqual("item", mod.mValue, "remove mValue=item");
		assertFalse(mod.mAdd, "remove mAdd=false");
	}

	private static void testSafeHashSetModifyFields()
	{
		var mod = new SafeHashSetModify<float>(3.14f, true);
		assertEqual(3.14f, mod.mValue, "mValue=3.14");
		assertTrue(mod.mAdd, "mAdd=true");
	}

	private static void testSafeHashSetModifyInt()
	{
		var add = new SafeHashSetModify<int>(1, true);
		assertEqual(1, add.mValue, "int add");
		assertTrue(add.mAdd, "int add flag");

		var remove = new SafeHashSetModify<int>(2, false);
		assertEqual(2, remove.mValue, "int remove");
		assertFalse(remove.mAdd, "int remove flag");
	}
}
