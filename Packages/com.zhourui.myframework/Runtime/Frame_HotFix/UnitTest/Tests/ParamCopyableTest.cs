using static TestAssert;

// 补充覆盖 ParamBase / ParamCopyable / ParamCopyableT
public static class ParamCopyableTest
{
	public static void Run()
	{
		testParamBaseLazyParamSet();
		testParamBaseResetKeepsParamSetButResetsContent();
		testParamCopyableTDispatchesTypedCopy();
	}

	private static void testParamBaseLazyParamSet()
	{
		ParamBase param = new();
		assertNull(param.getParamSet(), "未注册前 ParamSet 应为空");
		assertEqual(0, param.getParamCount(), "未注册前参数数量应为0");
		assertFalse(param.setParam(0, "value"), "未注册前设置参数应失败");

		string value = null;
		param.registeParam(v => value = v);
		assertNotNull(param.getParamSet(), "注册后 ParamSet 应被创建");
		assertEqual(1, param.getParamCount(), "注册一个参数后数量应为1");
		assertTrue(param.setParam(0, "hello"), "设置已注册参数应成功");
		assertEqual("hello", value, "回调应收到参数值");
	}

	private static void testParamBaseResetKeepsParamSetButResetsContent()
	{
		ParamBase param = new();
		int callCount = 0;
		param.registeParam((float _) => ++callCount);
		param.resetProperty();
		assertNotNull(param.getParamSet(), "reset 不会销毁 ParamSet 对象");
		assertEqual(0, param.getParamCount(), "reset 后 ParamSet 中的注册项应清空");
		assertFalse(param.setParam(0, "x"), "reset 后原注册项不应继续可用");
		assertEqual(0, callCount, "reset 后设置失败不应触发旧回调");
	}

	private static void testParamCopyableTDispatchesTypedCopy()
	{
		CopyParam source = new();
		source.mValue = 123;
		CopyParam target = new();
		target.initFromCopy(source);
		assertEqual(123, target.mValue, "应通过泛型类型分派到强类型拷贝函数");

		OtherCopyParam other = new();
		target.initFromCopy(other);
		assertTrue(target.mReceivedNull, "传入错误类型时, 强类型参数应为 null");
	}

	private class CopyParam : ParamCopyableT<CopyParam>
	{
		public int mValue;
		public bool mReceivedNull;
        public override void resetProperty()
        {
            base.resetProperty();
			mValue = 0;
			mReceivedNull = false;
        }
		protected override void initFromCopyInternal(CopyParam other)
		{
			mReceivedNull = other == null;
			if (other != null)
			{
				mValue = other.mValue;
			}
		}
	}

	private class OtherCopyParam : ParamCopyable
	{
	}
}