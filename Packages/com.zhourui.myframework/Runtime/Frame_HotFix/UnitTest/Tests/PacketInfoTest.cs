using System;
using static TestAssert;

// 消息包信息类的单元测试 — 覆盖纯数据类 PacketSendInfo / PacketReceiveInfo / PacketRegisterInfo
// 均为网络通信的消息中转/注册结构,不涉及真实连接,可靠可测
public static class PacketInfoTest
{
	public static void Run()
	{
		// ── PacketSendInfo 待发送消息信息 ──
		testPacketSendInfo_ConstructorAssigns();
		testPacketSendInfo_FieldMutation();
		testPacketSendInfo_StructCopyValueSemantics();
		testPacketSendInfo_EmptyBytes();

		// ── PacketReceiveInfo 接收消息信息 ──
		testPacketReceiveInfo_ConstructorAssigns();
		testPacketReceiveInfo_FieldMutation();
		testPacketReceiveInfo_StructCopyValueSemantics();
		testPacketReceiveInfo_ZeroType();

		// ── PacketRegisterInfo 注册信息 ──
		testPacketRegisterInfo_FieldAssignment();
		testPacketRegisterInfo_DefaultValues();
		testPacketRegisterInfo_ClassReferenceSemantics();
		testPacketRegisterInfo_NullType();
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketSendInfo — 构造函数赋值
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketSendInfo_ConstructorAssigns()
	{
		byte[] data = { 1, 2, 3, 4 };
		PacketSendInfo info = new PacketSendInfo(data, 4, true, 0x1234);
		assertEqual(data, info.mData, "mData 应引用传入的数组");
		assertEqual(4, info.mDataSize, "mDataSize 应为构造传入值");
		assertTrue(info.mDataNeedDestroy, "mDataNeedDestroy 应为 true");
		assertEqual(0x1234, info.mPacketType, "mPacketType 应为构造传入值");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketSendInfo — 字段可修改
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketSendInfo_FieldMutation()
	{
		PacketSendInfo info = new PacketSendInfo(null, 0, false, 0);
		byte[] newData = { 9 };
		info.mData = newData;
		info.mDataSize = 7;
		info.mDataNeedDestroy = true;
		info.mPacketType = 88;
		assertEqual(newData, info.mData, "mData 修改生效");
		assertEqual(7, info.mDataSize, "mDataSize 修改生效");
		assertTrue(info.mDataNeedDestroy, "mDataNeedDestroy 修改生效");
		assertEqual(88, info.mPacketType, "mPacketType 修改生效");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketSendInfo — struct 值语义(赋值是拷贝)
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketSendInfo_StructCopyValueSemantics()
	{
		byte[] data = { 5, 6 };
		PacketSendInfo a = new PacketSendInfo(data, 2, false, 10);
		PacketSendInfo b = a;   // struct 拷贝
		// 修改 b 的标量字段不应影响 a
		b.mDataSize = 99;
		b.mPacketType = 999;
		assertEqual(2, a.mDataSize, "a.mDataSize 不受 b 修改影响(值语义)");
		assertEqual(10, a.mPacketType, "a.mPacketType 不受 b 修改影响(值语义)");
		// 引用类型字段 mData 仍是同一数组(浅拷贝)
		assertEqual(data, a.mData, "a.mData 引用不变");
		assertEqual(data, b.mData, "b.mData 与 a 共享同一数组");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketSendInfo — 空字节数组
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketSendInfo_EmptyBytes()
	{
		byte[] empty = new byte[0];
		PacketSendInfo info = new PacketSendInfo(empty, 0, false, 0);
		assertEqual(0, info.mData.Length, "空数组长度为 0");
		assertEqual(empty, info.mData, "mData 引用空数组");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketReceiveInfo — 构造函数赋值
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketReceiveInfo_ConstructorAssigns()
	{
		byte[] data = { 10, 20, 30 };
		ulong flag = 0b101;
		PacketReceiveInfo info = new PacketReceiveInfo(data, flag, 3, 42u, 0xABCD, true);
		assertEqual(data, info.mPacketData, "mPacketData 应引用传入数组");
		assertEqual(flag, info.mFieldFlag, "mFieldFlag 应为构造传入值");
		assertEqual(3, info.mPacketSize, "mPacketSize 应为构造传入值");
		assertEqual(42u, info.mSequence, "mSequence 应为构造传入值");
		assertEqual((ushort)0xABCD, info.mType, "mType 应为构造传入值");
		assertTrue(info.mHasSign, "mHasSign 应为 true");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketReceiveInfo — 字段可修改
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketReceiveInfo_FieldMutation()
	{
		PacketReceiveInfo info = new PacketReceiveInfo(null, 0, 0, 0u, 0, false);
		byte[] newData = { 0xFF };
		info.mPacketData = newData;
		info.mFieldFlag = ulong.MaxValue;
		info.mPacketSize = 1;
		info.mSequence = 123456u;
		info.mType = 7;
		info.mHasSign = true;
		assertEqual(newData, info.mPacketData, "mPacketData 修改生效");
		assertEqual(ulong.MaxValue, info.mFieldFlag, "mFieldFlag 修改生效");
		assertEqual(1, info.mPacketSize, "mPacketSize 修改生效");
		assertEqual(123456u, info.mSequence, "mSequence 修改生效");
		assertEqual((ushort)7, info.mType, "mType 修改生效");
		assertTrue(info.mHasSign, "mHasSign 修改生效");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketReceiveInfo — struct 值语义
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketReceiveInfo_StructCopyValueSemantics()
	{
		byte[] data = { 1 };
		PacketReceiveInfo a = new PacketReceiveInfo(data, 3ul, 1, 5u, 2, true);
		PacketReceiveInfo b = a;
		b.mSequence = 999u;
		b.mType = 66;
		assertEqual(5u, a.mSequence, "a.mSequence 不受 b 影响(值语义)");
		assertEqual((ushort)2, a.mType, "a.mType 不受 b 影响(值语义)");
		assertEqual(data, a.mPacketData, "a.mPacketData 引用不变");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketReceiveInfo — 消息类型为 0
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketReceiveInfo_ZeroType()
	{
		PacketReceiveInfo info = new PacketReceiveInfo(new byte[0], 0ul, 0, 0u, 0, false);
		assertEqual((ushort)0, info.mType, "mType 可为 0");
		assertFalse(info.mHasSign, "mHasSign 默认 false");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketRegisterInfo — 字段赋值
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketRegisterInfo_FieldAssignment()
	{
		PacketRegisterInfo info = new PacketRegisterInfo();
		info.mTypeID = 0x55AA;
		info.mClassType = typeof(string);
		assertEqual((ushort)0x55AA, info.mTypeID, "mTypeID 赋值生效");
		assertEqual(typeof(string), info.mClassType, "mClassType 赋值生效");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketRegisterInfo — 默认值
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketRegisterInfo_DefaultValues()
	{
		PacketRegisterInfo info = new PacketRegisterInfo();
		assertEqual((ushort)0, info.mTypeID, "mTypeID 默认 0");
		assertNull(info.mClassType, "mClassType 默认 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketRegisterInfo — class 引用语义(赋值共享引用)
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketRegisterInfo_ClassReferenceSemantics()
	{
		PacketRegisterInfo a = new PacketRegisterInfo();
		a.mTypeID = 10;
		PacketRegisterInfo b = a;   // class 引用共享
		b.mTypeID = 20;
		assertEqual(20, a.mTypeID, "class 引用共享, 修改 b 影响 a");
		assertEqual(b, a, "b 与 a 是同一引用");
	}

	// ═════════════════════════════════════════════════════════════════
	// PacketRegisterInfo — 可赋 null 类型
	// ═════════════════════════════════════════════════════════════════
	private static void testPacketRegisterInfo_NullType()
	{
		PacketRegisterInfo info = new PacketRegisterInfo();
		info.mClassType = null;
		assertNull(info.mClassType, "mClassType 可设为 null");
	}
}
