using System.Collections.Generic;
using static TestAssert;

// NetStructByte 单元测试：addParam/read/write/resetProperty/fieldFlag 逻辑
public static class NetStructByteTest
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
		testMHasOptionalParamsNotClearedOnReset();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 用于测试的 NetStructByte 子类
	private class TestNetStructByte : NetStructByte
	{
		public void PublicAddParam(Serializable param, bool isOptional)
		{
			addParam(param, isOptional);
		}
	}

	// 用于测试的简单 Serializable 子类
	private class TestParamByte : Serializable
	{
		public override bool read(SerializerRead reader) { return true; }
		public override void write(SerializerWrite writer) { }
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testAddParamNoOptional()
	{
		var ns = new TestNetStructByte();
		var param = new TestParamByte();
		ns.PublicAddParam(param, false);
		assertFalse(ns.mHasOptionalParams, "非可选参数不应设置 mHasOptionalParams");
		assertEqual(1, ns.mParams.Count, "参数列表应有1个");
		assertFalse(param.mOptional, "参数 mOptional 应为 false");
	}

	private static void testAddParamWithOptional()
	{
		var ns = new TestNetStructByte();
		var param = new TestParamByte();
		ns.PublicAddParam(param, true);
		assertTrue(ns.mHasOptionalParams, "可选参数应设置 mHasOptionalParams");
		assertEqual(1, ns.mParams.Count, "参数列表应有1个");
		assertTrue(param.mOptional, "参数 mOptional 应为 true");
	}

	private static void testAddParamMixedOptional()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		var p2 = new TestParamByte();
		var p3 = new TestParamByte();
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
		var ns = new TestNetStructByte();
		var param = new TestParamByte();
		ns.PublicAddParam(param, true);
		assertTrue(ns.mHasOptionalParams, "可选参数后 mHasOptionalParams=true");
	}

	private static void testMHasOptionalParamsNotSet()
	{
		var ns = new TestNetStructByte();
		var param = new TestParamByte();
		ns.PublicAddParam(param, false);
		assertFalse(ns.mHasOptionalParams, "仅非可选参数 mHasOptionalParams=false");
	}

	private static void testResetProperty()
	{
		var ns = new TestNetStructByte();
		var param = new TestParamByte();
		param.mValid = false;
		ns.PublicAddParam(param, true);
		ns.resetProperty();
		// mHasOptionalParams 不会被 resetProperty 清除
		assertTrue(ns.mHasOptionalParams, "resetProperty 不清除 mHasOptionalParams");
		// param 的 resetProperty 被调用，mValid 应重置为 true
		assertTrue(param.mValid, "resetProperty 应重置 param.mValid=true");
	}

	private static void testResetPropertyResetsParams()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		var p2 = new TestParamByte();
		p1.mValid = false;
		p2.mValid = false;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, false);
		ns.resetProperty();
		assertTrue(p1.mValid, "p1 resetProperty 后 mValid=true");
		assertTrue(p2.mValid, "p2 resetProperty 后 mValid=true");
	}

	private static void testWriteNoOptionalParams()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		ns.PublicAddParam(p1, false);
		var writer = new SerializerWrite();
		ns.write(writer);
		assertFalse(ns.mHasOptionalParams, "write 后 mHasOptionalParams 不变");
	}

	private static void testWriteAllOptionalValid()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		var p2 = new TestParamByte();
		p1.mValid = true;
		p2.mValid = true;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, true);
		var writer = new SerializerWrite();
		ns.write(writer);
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams=true");
	}

	private static void testWriteSomeOptionalInvalid()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		var p2 = new TestParamByte();
		p1.mValid = false;
		p2.mValid = true;
		ns.PublicAddParam(p1, true);
		ns.PublicAddParam(p2, true);
		var writer = new SerializerWrite();
		ns.write(writer);
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams=true");
	}

	private static void testReadNoOptionalParams()
	{
		var ns = new TestNetStructByte();
		var reader = new SerializerRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader);
		assertTrue(result, "无可选参数时 read 返回 true");
	}

	private static void testReadWithOptionalFlag()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		ns.PublicAddParam(p1, true);
		var reader = new SerializerRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader);
		assertTrue(result, "有可选参数但 useFlag=false 时 read 返回 true");
	}

	private static void testReadWithOptionalFlagAllValid()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		p1.mValid = true;
		ns.PublicAddParam(p1, true);
		var reader = new SerializerRead();
		reader.init(new byte[10]);
		bool result = ns.read(reader);
		assertTrue(result, "read 返回 true");
	}

	private static void testAddParamThenReset()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		p1.mValid = false;
		ns.PublicAddParam(p1, true);
		ns.resetProperty();
		assertTrue(p1.mValid, "resetProperty 后 mValid 恢复");
		assertTrue(ns.mHasOptionalParams, "mHasOptionalParams 保持");
	}

	private static void testMultipleAddParam()
	{
		var ns = new TestNetStructByte();
		var paramsList = new List<TestParamByte>();
		for (int i = 0; i < 10; i++)
		{
			var p = new TestParamByte();
			paramsList.Add(p);
			ns.PublicAddParam(p, i % 2 == 0);
		}
		assertEqual(10, ns.mParams.Count, "应有10个参数");
		assertTrue(ns.mHasOptionalParams, "有可选参数 mHasOptionalParams=true");
		for (int i = 0; i < 10; i++)
		{
			assertEqual(i % 2 == 0, paramsList[i].mOptional, "参数 " + i + " mOptional 正确");
		}
	}

	private static void testDefaultMHasOptionalParamsFalse()
	{
		var ns = new TestNetStructByte();
		assertFalse(ns.mHasOptionalParams, "构造后 mHasOptionalParams=false");
	}

	private static void testDefaultMParamsEmpty()
	{
		var ns = new TestNetStructByte();
		assertNotNull(ns.mParams, "mParams 不为 null");
		assertEqual(0, ns.mParams.Count, "构造后 mParams 为空");
	}

	private static void testMHasOptionalParamsNotClearedOnReset()
	{
		var ns = new TestNetStructByte();
		var p1 = new TestParamByte();
		ns.PublicAddParam(p1, true);
		ns.resetProperty();
		assertTrue(ns.mHasOptionalParams, "resetProperty 后 mHasOptionalParams 仍为 true（构造赋值不重置）");
	}
}
