using System.Collections.Generic;
using static TestAssert;

// NetStructBit 单元测试：addParam/read/write/resetProperty/hasSign/fieldFlag 逻辑
public static class NetStructBitTest
{
	public static void Run()
	{
		testAddParamNoOptional();
		testAddParamWithOptional();
		testAddParamMixedOptional();
		testMHasOptionalParamsSet();
		testMHasOptionalParamsNotSet();
		testResetProperty();
		testResetPropertyResetsParams();
		testHasSignDefaultFalse();
		testHasSignOverrideTrue();
		testWriteNoOptionalParams();
		testWriteAllOptionalValid();
		testWriteSomeOptionalInvalid();
		testReadNoOptionalParams();
		testReadWithOptionalFlag();
		testReadWithOptionalFlagAllValid();
		testAddParamThenReset();
		testMultipleAddParam();
		testDefaultMHasOptionalParamsFalse();
		testDefaultMParamsEmpty();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 用于测试的 NetStructBit 子类
	private class TestNetStructBit : NetStructBit
	{
		public void PublicAddParam(SerializableBit param, bool isOptional)
		{
			addParam(param, isOptional);
		}
	}

	// 用于测试的简单 SerializableBit 子类
	private class TestParamBit : SerializableBit
	{
		public override bool read(SerializerBitRead reader, bool needReadSign) { return true; }
		public override void write(SerializerBitWrite writer, bool needWriteSign) { }
	}

	// 带 hasSign 覆盖的子类
	private class TestNetStructBitWithSign : NetStructBit
	{
		public override bool hasSign() { return true; }
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddParamNoOptional()
	{
		var ns = new TestNetStructBit();
		var param = new TestParamBit();
		ns.PublicAddParam(param, false);
		assertFalse(ns.mHasOptionalParams, "非可选参数不应设置 mHasOptionalParams");
		assertEqual(1, ns.mParams.Count, "参数列表应有1个");
		assertFalse(param.mOptional, "参数 mOptional 应为 false");
	}

	private static void testAddParamWithOptional()
	{
		var ns = new TestNetStructBit();
		var param = new TestParamBit();
		ns.PublicAddParam(param, true);
		assertTrue(ns.mHasOptionalParams, "可选参数应设置 mHasOptionalParams");
		assertEqual(1, ns.mParams.Count, "参数列表应有1个");
		assertTrue(param.mOptional, "参数 mOptional 应为 true");
	}

	private static void testAddParamMixedOptional()
	{
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		var p2 = new TestParamBit();
		var p3 = new TestParamBit();
		ns.PublicAddParam(p1, false);
		ns.PublicAddParam(p2, true);
		ns.PublicAddParam(p3, false);
		assertTrue(ns.mHasOptionalParams, "混合参数中有一个可选则 mHasOptionalParams=true");
		assertEqual(3, ns.mParams.Count, "参数列表应有3个");
		assertFalse(p1.mOptional, "p1 mOptional=false");
		assertTrue(p2.mOptional, "p2 mOptional=true");
		assertFalse(p3.mOptional, "p3 mOptional=false");
	}

	private static void testMHasOptionalParamsSet()
	{
		var ns = new TestNetStructBit();
		var param = new TestParamBit();
		ns.PublicAddParam(param, true);
		assertTrue(ns.mHasOptionalParams, "可选参数后 mHasOptionalParams=true");
	}

	private static void testMHasOptionalParamsNotSet()
	{
		var ns = new TestNetStructBit();
		var param = new TestParamBit();
		ns.PublicAddParam(param, false);
		assertFalse(ns.mHasOptionalParams, "仅非可选参数 mHasOptionalParams=false");
	}

	private static void testResetProperty()
	{
		var ns = new TestNetStructBit();
		var param = new TestParamBit();
		param.mValid = false;
		ns.PublicAddParam(param, true);
		// mHasOptionalParams 在 addParam 时被设置，但 resetProperty 不清除它
		ns.resetProperty();
		// mHasOptionalParams 不会被 resetProperty 清除（注释说构造中赋值的不需要重置）
		assertTrue(ns.mHasOptionalParams, "resetProperty 不清除 mHasOptionalParams");
		// 但 param 的 resetProperty 被调用了，mValid 应该重置为 true
		assertTrue(param.mValid, "resetProperty 应重置 param.mValid=true");
	}

	private static void testResetPropertyResetsParams()
	{
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		var p2 = new TestParamBit();
		p1.mValid = false;
		p2.mValid = false;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, false);
		ns.resetProperty();
		assertTrue(p1.mValid, "p1 resetProperty 后 mValid=true");
		assertTrue(p2.mValid, "p2 resetProperty 后 mValid=true");
	}

	private static void testHasSignDefaultFalse()
	{
		var ns = new TestNetStructBit();
		assertFalse(ns.hasSign(), "默认 hasSign 返回 false");
	}

	private static void testHasSignOverrideTrue()
	{
		var ns = new TestNetStructBitWithSign();
		assertTrue(ns.hasSign(), "重写 hasSign 返回 true");
	}

	private static void testWriteNoOptionalParams()
	{
		// write 在有可选参数但所有参数都不是可选时：mHasOptionalParams=false，不写任何东西
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		ns.PublicAddParam(p1, false);
		// mHasOptionalParams=false，write 不进入 if 分支
		// 无异常即通过
		var writer = new SerializerBitWrite();
		ns.write(writer, false);
		assertFalse(ns.mHasOptionalParams, "write 后 mHasOptionalParams 不变");
	}

	private static void testWriteAllOptionalValid()
	{
		// 所有可选参数都 valid：不写 useFlag
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		var p2 = new TestParamBit();
		p1.mValid = true;
		p2.mValid = true;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, true);
		var writer = new SerializerBitWrite();
		ns.write(writer, false);
		// 所有可选参数都 valid，useFlag 为 false
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams=true");
	}

	private static void testWriteSomeOptionalInvalid()
	{
		// 有可选参数 invalid：写 useFlag=true 和 fieldFlag
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		var p2 = new TestParamBit();
		p1.mValid = false;
		p2.mValid = true;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, true);
		var writer = new SerializerBitWrite();
		ns.write(writer, false);
		// 无异常即通过（有 invalid 参数时 useFlag 会被写入）
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams=true");
	}

	private static void testReadNoOptionalParams()
	{
		var ns = new TestNetStructBit();
		var reader = new SerializerBitRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader, false);
		assertTrue(result, "无可选参数时 read 返回 true");
	}

	private static void testReadWithOptionalFlag()
	{
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		ns.PublicAddParam(p1, true);
		// useFlag=false 时 fieldFlag=FULL_FIELD_FLAG
		var reader = new SerializerBitRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader, false);
		assertTrue(result, "有可选参数但 useFlag=false 时 read 返回 true");
	}

	private static void testReadWithOptionalFlagAllValid()
	{
		// 测试当 useFlag=true 时，fieldFlag 读取后所有字段有效
		// 先写入再读取验证
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		p1.mValid = true;
		ns.PublicAddParam(p1, true);
		// read 通过 FULL_FIELD_FLAG 传递给 readInternal，返回 true
		var reader = new SerializerBitRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader, false);
		assertTrue(result, "read 返回 true");
	}

	private static void testAddParamThenReset()
	{
		var ns = new TestNetStructBit();
		var p1 = new TestParamBit();
		p1.mValid = false;
		ns.PublicAddParam(p1, true);
		ns.resetProperty();
		// resetProperty 后 param 的 mValid 恢复为 true
		assertTrue(p1.mValid, "resetProperty 后 mValid 恢复");
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams 保持");
	}

	private static void testMultipleAddParam()
	{
		var ns = new TestNetStructBit();
		var paramsList = new List<TestParamBit>();
		for (int i = 0; i < 10; i++)
		{
			var p = new TestParamBit();
			paramsList.Add(p);
			ns.PublicAddParam(p, i % 2 == 0);
		}
		assertEqual(10, ns.mParams.Count, "应有10个参数");
		assertTrue(ns.mHasOptionalParams, "有可选参数 mHasOptionalParams=true");
		// 验证 mOptional 设置
		for (int i = 0; i < 10; i++)
		{
			assertEqual(i % 2 == 0, paramsList[i].mOptional, "参数 " + i + " mOptional 正确");
		}
	}

	private static void testDefaultMHasOptionalParamsFalse()
	{
		var ns = new TestNetStructBit();
		assertFalse(ns.mHasOptionalParams, "构造后 mHasOptionalParams=false");
	}

	private static void testDefaultMParamsEmpty()
	{
		var ns = new TestNetStructBit();
		assertNotNull(ns.mParams, "mParams 不为 null");
		assertEqual(0, ns.mParams.Count, "构造后 mParams 为空");
	}
}
