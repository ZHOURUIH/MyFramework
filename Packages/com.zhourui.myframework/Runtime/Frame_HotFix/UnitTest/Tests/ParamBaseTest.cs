using static TestAssert;

// ParamBase 单元测试：registeParam/resetProperty/setParam/getParamCount/getParamSet
public static class ParamBaseTest
{
	public static void Run()
	{
		testDefaultState();
		testRegisteStringParam();
		testRegisteFloatParam();
		testRegisteMultipleParams();
		testGetParamSet();
		testGetParamCountDefault();
		testGetParamCountAfterRegiste();
		testResetProperty();
		testSetParamNull();
		testSetParamValid();
		testRegisteAllParamDefault();
		testCheckDefault();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private class TestParamBase : ParamBase
	{
		public int mStrCount = 0;
		public int mFloatCount = 0;

		public override void resetProperty()
		{
			base.resetProperty();
			mStrCount = 0;
			mFloatCount = 0;
		}

		public override void registeAllParam()
		{
			registeParam((StringCallback)onString);
			registeParam((FloatCallback)onFloat);
		}

		private void onString(string val) { mStrCount++; }
		private void onFloat(float val) { mFloatCount++; }
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testDefaultState()
	{
		var p = new TestParamBase();
		assertNull(p.getParamSet(), "默认 getParamSet=null");
		assertEqual(0, p.getParamCount(), "默认 paramCount=0");
	}

	private static void testRegisteStringParam()
	{
		var p = new TestParamBase();
		p.registeParam((StringCallback)((string val) => { }));
		assertNotNull(p.getParamSet(), "registeParam 后 getParamSet 不为 null");
		assertEqual(1, p.getParamCount(), "注册1个参数后 count=1");
	}

	private static void testRegisteFloatParam()
	{
		var p = new TestParamBase();
		p.registeParam((FloatCallback)((float val) => { }));
		assertNotNull(p.getParamSet(), "registeParam 后 getParamSet 不为 null");
		assertEqual(1, p.getParamCount(), "注册1个 float 参数后 count=1");
	}

	private static void testRegisteMultipleParams()
	{
		var p = new TestParamBase();
		p.registeParam((StringCallback)((string val) => { }));
		p.registeParam((FloatCallback)((float val) => { }));
		p.registeParam((StringCallback)((string val) => { }));
		assertEqual(3, p.getParamCount(), "注册3个参数后 count=3");
	}

	private static void testGetParamSet()
	{
		var p = new TestParamBase();
		assertNull(p.getParamSet(), "注册前 getParamSet=null");
		p.registeParam((StringCallback)((string val) => { }));
		assertNotNull(p.getParamSet(), "注册后 getParamSet!=null");
	}

	private static void testGetParamCountDefault()
	{
		var p = new TestParamBase();
		assertEqual(0, p.getParamCount(), "默认 paramCount=0");
	}

	private static void testGetParamCountAfterRegiste()
	{
		var p = new TestParamBase();
		p.registeParam((StringCallback)((string val) => { }));
		assertEqual(1, p.getParamCount(), "1个 paramCount=1");
		p.registeParam((FloatCallback)((float val) => { }));
		assertEqual(2, p.getParamCount(), "2个 paramCount=2");
	}

	private static void testResetProperty()
	{
		var p = new TestParamBase();
		p.registeParam((StringCallback)((string val) => { }));
		assertNotNull(p.getParamSet(), "reset 前 paramSet 不为 null");
		p.resetProperty();
		assertEqual(0, p.getParamCount(), "resetProperty 后 paramCount=0");
	}

	private static void testSetParamNull()
	{
		var p = new TestParamBase();
		bool result = p.setParam(0, "test");
		assertFalse(result, "无 paramSet 时 setParam 返回 false");
	}

	private static void testSetParamValid()
	{
		var p = new TestParamBase();
		p.registeAllParam();
		bool result = p.setParam(0, "hello");
		assertTrue(result, "setParam(0,string) 应返回 true");
	}

	private static void testRegisteAllParamDefault()
	{
		var p = new TestParamBase();
		p.registeAllParam();
		assertEqual(2, p.getParamCount(), "registeAllParam 注册2个参数");
	}

	private static void testCheckDefault()
	{
		var p = new TestParamBase();
		p.check();
	}
}
