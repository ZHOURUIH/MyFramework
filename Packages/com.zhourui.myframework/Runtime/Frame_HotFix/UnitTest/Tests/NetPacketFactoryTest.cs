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
}
