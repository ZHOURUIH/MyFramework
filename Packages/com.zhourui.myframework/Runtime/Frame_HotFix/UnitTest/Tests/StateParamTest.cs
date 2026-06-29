using static TestAssert;

// StateParam 单元测试
// 字段读写 / resetProperty
public static class StateParamTest
{
	public static void Run()
	{
		testDefault();
		testSetFloat();
		testResetProperty();
	}

	private static void testDefault()
	{
		var p = new StateParam();
		assertEqual(-1.0f, p.mBuffTime, "默认 mBuffTime 应为 -1.0f");
	}

	private static void testSetFloat()
	{
		var p = new StateParam();
		p.mBuffTime = 5.5f;
		assertEqual(5.5f, p.mBuffTime, "mBuffTime 赋值后应正确返回");
	}

	private static void testResetProperty()
	{
		var p = new StateParam();
		p.mBuffTime = 5.5f;
		p.resetProperty();
		assertEqual(-1.0f, p.mBuffTime, "resetProperty 后 mBuffTime 应恢复为 -1.0f");
	}
}