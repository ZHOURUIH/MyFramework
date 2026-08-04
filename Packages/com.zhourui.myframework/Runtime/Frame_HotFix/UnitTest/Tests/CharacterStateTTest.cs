using static TestAssert;

// CharacterStateT 单元测试：setParam/getCustomParam/resetProperty
public static class CharacterStateTTest
{
	public static void Run()
	{
		testSetParamSameType();
		testSetParamDifferentType();
		testGetCustomParamAfterSet();
		testResetProperty();
		testDefaultGetCustomParamNull();
		testSetParamNull();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private class TestStateParam : StateParam { }
	private class TestOtherParam : StateParam { }
	private class TestCharacterState : CharacterStateT<TestStateParam> { }

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testSetParamSameType()
	{
		var state = new TestCharacterState();
		var param = new TestStateParam();
		state.setParam(param);
		assertEqual(param, state.getCustomParam(), "setParam 同类型参数后 getCustomParam 返回该参数");
	}

	private static void testSetParamDifferentType()
	{
		var state = new TestCharacterState();
		var param = new TestOtherParam();
		state.setParam(param);
		assertNull(state.getCustomParam(), "setParam 不同类型参数后 getCustomParam 返回 null");
	}

	private static void testGetCustomParamAfterSet()
	{
		var state = new TestCharacterState();
		var param = new TestStateParam();
		state.setParam(param);
		var result = state.getCustomParam();
		assertNotNull(result, "setParam 后 getCustomParam 不为 null");
		assertEqual(param, result, "getCustomParam 返回正确的参数");
	}

	private static void testResetProperty()
	{
		var state = new TestCharacterState();
		var param = new TestStateParam();
		state.setParam(param);
		assertNotNull(state.getCustomParam(), "reset 前 customParam 不为 null");
		state.resetProperty();
		assertNull(state.getCustomParam(), "resetProperty 后 customParam=null");
	}

	private static void testDefaultGetCustomParamNull()
	{
		var state = new TestCharacterState();
		assertNull(state.getCustomParam(), "默认 getCustomParam=null");
	}

	private static void testSetParamNull()
	{
		var state = new TestCharacterState();
		state.setParam(null);
		assertNull(state.getCustomParam(), "setParam(null) 后 getCustomParam=null");
	}
}
