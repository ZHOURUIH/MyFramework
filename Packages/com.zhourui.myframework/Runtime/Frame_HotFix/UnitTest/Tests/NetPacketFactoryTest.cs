using System;
using static TestAssert;
using static FrameBaseHotFix;

// NetPacketFactory 单元测试 — 消息包工厂的创建/销毁逻辑
// 框架环境已完全初始化, mNetPacketFactory / mNetPacketTypeManager 可用
// 只测试不触发 logError 的正常路径:
//   createSocketPacket(Type, typeID) 显式传入 typeID 避免依赖注册表
//   createSocketPacket(ushort type) 通过注册表解析类型
//   destroyPacket 归还对象池
public static class NetPacketFactoryTest
{
	// 测试用消息包
	private class TestFactoryPacket : NetPacketByte { }

	public static void Run()
	{
		// 环境前置: 工厂单例应可用
		if (mNetPacketFactory == null)
		{
			assertTrue(false, "mNetPacketFactory 应为框架单例可用");
			return;
		}
		testCreateByTypeAndID();
		testCreateByTypeID_Registed();
		testDestroyPacket();
		testCreateDestroyReuse();
		testSetConnectAndType();
		testDestroyNull();
		testCreateDifferentTypes();
		testReuseSameInstance();
		testTypeResetOnReuse();
		testTypeIDZero();
		testSetConnectRoundTrip();
		testCreateDestroyManyMixedCycles();
		testPacketTypeIndependent();
		testDestroyThenRecreateType();
		testSetConnectNullSafe();
		testCreateManySameType();
		testPacketTypeMaxValue();
		testTypeIDHighBits();
	}

	// ═════════════════════════════════════════════════════════════════
	// createSocketPacket(Type, typeID) — 显式 typeID, 不依赖注册表
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateByTypeAndID()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 100);
		assertNotNull(packet, "createSocketPacket(Type, typeID) 应创建实例");
		assertEqual((ushort)100, packet.getPacketType(), "packetType 应为传入的 typeID");
		assertTrue(packet is TestFactoryPacket, "实例类型应为 TestFactoryPacket");
		mNetPacketFactory.destroyPacket(packet);
	}

	// ═════════════════════════════════════════════════════════════════
	// createSocketPacket(ushort type) — 通过注册表解析类型
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateByTypeID_Registed()
	{
		// 临时注册一个类型到全局注册表 mNetPacketTypeManager, 用后必须 unregistePacket 清理
		// 否则会污染全局单例, 影响其他测试(注册表支持移除)
		// 使用唯一 typeID 且仅在未注册时才注册, 避免重复 Add 抛异常
		ushort typeID = 0x55AA;
		bool registed = false;
		if (mNetPacketTypeManager != null && mNetPacketTypeManager.getPacketType(typeID) == null)
		{
			mNetPacketTypeManager.registePacket(typeof(TestFactoryPacket), typeID);
			registed = true;
		}
		try
		{
			NetPacket packet = mNetPacketFactory.createSocketPacket(typeID);
			assertNotNull(packet, "通过注册表 typeID 应能创建实例");
			if (packet != null)
			{
				assertEqual(typeID, packet.getPacketType(), "packetType 应为注册的 typeID");
				assertTrue(packet is TestFactoryPacket, "实例类型应匹配注册的类");
				mNetPacketFactory.destroyPacket(packet);
			}
		}
		finally
		{
			// 用后必须移除全局注册, 避免污染 mNetPacketTypeManager 单例
			if (registed && mNetPacketTypeManager != null)
			{
				mNetPacketTypeManager.unregistePacket(typeof(TestFactoryPacket), typeID);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyPacket — 归还后应可再次创建
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyPacket()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 200);
		assertNotNull(packet, "创建成功");
		mNetPacketFactory.destroyPacket(packet);
		// destroy 后对象被归还池, 引用置空由调用方负责, 此处仅验证不抛异常
		assertTrue(true, "destroyPacket 正常执行不抛异常");
	}

	// ═════════════════════════════════════════════════════════════════
	// 创建-销毁-再创建 — 对象池复用不应抛异常
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateDestroyReuse()
	{
		for (int i = 0; i < 50; ++i)
		{
			NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), (ushort)(300 + i));
			assertNotNull(packet, "循环创建第 " + i + " 次应成功");
			mNetPacketFactory.destroyPacket(packet);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setConnect/setPacketType — 实例状态设置
	// ═════════════════════════════════════════════════════════════════
	private static void testSetConnectAndType()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 400);
		assertNotNull(packet, "创建成功");
		packet.setPacketType(0x1234);
		assertEqual((ushort)0x1234, packet.getPacketType(), "setPacketType 后 getPacketType 应返回新值");
		mNetPacketFactory.destroyPacket(packet);
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyPacket(null): 空安全
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyNull()
	{
		mNetPacketFactory.destroyPacket(null);
		// 无异常即通过
	}

	// ═════════════════════════════════════════════════════════════════
	// 多种类型创建均成功
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateDifferentTypes()
	{
		NetPacket bytePacket = mNetPacketFactory.createSocketPacket(typeof(NetPacketByte), 500);
		NetPacket factoryPacket = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 501);
		assertTrue(bytePacket is NetPacketByte, "NetPacketByte 类型创建成功");
		assertTrue(factoryPacket is TestFactoryPacket, "TestFactoryPacket 类型创建成功");
		mNetPacketFactory.destroyPacket(bytePacket);
		mNetPacketFactory.destroyPacket(factoryPacket);
	}

	// ═════════════════════════════════════════════════════════════════
	// 销毁后创建复用同一实例(对象池复用)
	// ═════════════════════════════════════════════════════════════════
	private static void testReuseSameInstance()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 600);
		NetPacket saved = packet;
		mNetPacketFactory.destroyPacket(packet);
		NetPacket reused = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 601);
		assertTrue(ReferenceEquals(reused, saved), "销毁后创建复用同一实例");
		mNetPacketFactory.destroyPacket(reused);
	}

	// ═════════════════════════════════════════════════════════════════
	// 复用后 packetType 重设为新 typeID(不残留旧值)
	// ═════════════════════════════════════════════════════════════════
	private static void testTypeResetOnReuse()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 700);
		mNetPacketFactory.destroyPacket(packet);
		// 复用时传入新 typeID
		NetPacket reused = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 701);
		assertEqual((ushort)701, reused.getPacketType(), "复用后 packetType 重设为新 typeID");
		mNetPacketFactory.destroyPacket(reused);
	}

	// ═════════════════════════════════════════════════════════════════
	// typeID=0: 查注册表, TestFactoryPacket 未注册 → typeID 保持 0
	// ═════════════════════════════════════════════════════════════════
	private static void testTypeIDZero()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 0);
		assertNotNull(packet, "typeID=0 创建成功");
		assertEqual((ushort)0, packet.getPacketType(), "未注册类型 typeID=0 时 packetType 0");
		mNetPacketFactory.destroyPacket(packet);
	}

	// ═════════════════════════════════════════════════════════════════
	// setConnect/getConnect 往返
	// ═════════════════════════════════════════════════════════════════
	private static void testSetConnectRoundTrip()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 800);
		NetConnect connect = new NetConnect();
		packet.setConnect(connect);
		assertTrue(ReferenceEquals(connect, packet.getConnect()), "setConnect 后 getConnect 同一引用");
		mNetPacketFactory.destroyPacket(packet);
	}

	// 混合类型多轮创建/销毁
	private static void testCreateDestroyManyMixedCycles()
	{
		for (int i = 0; i < 10; ++i)
		{
			Type type = (i % 2 == 0) ? typeof(TestFactoryPacket) : typeof(NetPacketByte);
			NetPacket packet = mNetPacketFactory.createSocketPacket(type, (ushort)(900 + i));
			assertNotNull(packet, "第 " + i + " 轮混合创建成功");
			mNetPacketFactory.destroyPacket(packet);
		}
	}

	// 两 packet 各自 typeID 独立
	private static void testPacketTypeIndependent()
	{
		NetPacket a = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 1000);
		NetPacket b = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 1001);
		assertEqual((ushort)1000, a.getPacketType(), "a typeID");
		assertEqual((ushort)1001, b.getPacketType(), "b typeID");
		mNetPacketFactory.destroyPacket(a);
		mNetPacketFactory.destroyPacket(b);
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 销毁后重建 typeID 保留(显式 typeID 每次创建设置)
	private static void testDestroyThenRecreateType()
	{
		NetPacket a = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 2000);
		mNetPacketFactory.destroyPacket(a);
		NetPacket b = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 2001);
		assertEqual((ushort)2001, b.getPacketType(), "重建后 typeID 为新的显式值");
		mNetPacketFactory.destroyPacket(b);
	}

	// setConnect(null) 空安全
	private static void testSetConnectNullSafe()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 2100);
		packet.setConnect(null);
		assertNull(packet.getConnect(), "setConnect(null) 后 getConnect null");
		mNetPacketFactory.destroyPacket(packet);
	}

	// 同类型多次创建 typeID 独立
	private static void testCreateManySameType()
	{
		for (int i = 0; i < 5; ++i)
		{
			NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), (ushort)(2200 + i));
			assertEqual((ushort)(2200 + i), packet.getPacketType(), "第 " + i + " 个 typeID");
			mNetPacketFactory.destroyPacket(packet);
		}
	}

	// typeID 最大值
	private static void testPacketTypeMaxValue()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), ushort.MaxValue);
		assertEqual(ushort.MaxValue, packet.getPacketType(), "typeID 最大值读回");
		mNetPacketFactory.destroyPacket(packet);
	}

	// typeID 高位值
	private static void testTypeIDHighBits()
	{
		NetPacket packet = mNetPacketFactory.createSocketPacket(typeof(TestFactoryPacket), 0xABCD);
		assertEqual((ushort)0xABCD, packet.getPacketType(), "typeID 高位值读回");
		mNetPacketFactory.destroyPacket(packet);
	}
}
