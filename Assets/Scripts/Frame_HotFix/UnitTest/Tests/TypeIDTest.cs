#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;

public class TypeIDTest
{
	public static void Run()
	{
		testDifferentTypesGetDifferentIDs();
		testSameTypeAlwaysSameID();
		testGlobalCounterIncrements();
	}

	// 测试不同类型获得不同 ID
	private static void testDifferentTypesGetDifferentIDs()
	{
		int idInt = TypeID<int>.ID;
		int idFloat = TypeID<float>.ID;
		int idString = TypeID<string>.ID;
		assertTrue(idInt != idFloat);
		assertTrue(idInt != idString);
		assertTrue(idFloat != idString);
	}

	// 测试同一类型 ID 恒定
	private static void testSameTypeAlwaysSameID()
	{
		int id1 = TypeID<int>.ID;
		int id2 = TypeID<int>.ID;
		assertEqual(id1, id2);
	}

	// 测试全局计数器递增
	private static void testGlobalCounterIncrements()
	{
		int before = TypeID.mGlobalCounter;
		int idNew = TypeID<TypeIDTest>.ID;
		assertTrue(idNew > 0);
	}
}
#endif
