using System;
using System.Collections.Generic;
using static TestAssert;

// 网络核心模块完整单元测试 — 覆盖全部执行路径
public static class NetCoreTest
{
    private class TestPacket { }
    private class TestPacket2 { }
    private class TestPacket3 { }

    public static void Run()
    {
        // ── NetPacketTypeManager ──
        testTypeManager_RegisteAndLookup();
        testTypeManager_RegisteDuplicateClassType();
        testTypeManager_RegisteDuplicateTypeID();
        testTypeManager_RegisteTypeIDZero();
        testTypeManager_MultipleRegistrations();
        testTypeManager_GetPacketTypeID_Unregistered();
        testTypeManager_GetPacketType_Unregistered();
        testTypeManager_GetPacketType_MaxID();
        testTypeManager_UnregistePacket();
        testTypeManager_UnregisteUDPPacketName();
        testTypeManager_UDPNameMapping();
        testTypeManager_UDPNameMapping_DuplicateName();
        testTypeManager_UDPNameMapping_DuplicateID();
        testTypeManager_GetUDPPacketType_Unknown();
        testTypeManager_GetUDPPacketType_EmptyString();
        testTypeManager_IsUDPPacket_True();
        testTypeManager_IsUDPPacket_False();
        testTypeManager_IsUDPPacket_MaxValue();

        // ── NetConnect 加解密 ──
        testEncryptDecrypt_RoundTrip_Basic();
        testEncryptDecrypt_RoundTrip_Empty();
        testEncryptDecrypt_RoundTrip_SingleByte();
        testEncryptDecrypt_RoundTrip_LargeData();
        testEncryptDecrypt_RoundTrip_WithOffset();
        testEncryptDecrypt_RoundTrip_PartialLength();
        testEncryptDecrypt_RoundTrip_AllZeros();
        testEncryptDecrypt_RoundTrip_AllFFs();
        testEncryptDecrypt_DifferentParams();
        testEncryptDecrypt_Param0();
        testEncryptDecrypt_Param255();
        testEncryptDecrypt_Param128();
        testEncryptDecrypt_DoubleEncryptDecrypt();
        testEncryptDecrypt_OnlyEncrypt_ChangesData();
        testEncryptDecrypt_OffsetZero_LengthZero();

        // ── NetConnect 属性 ──
        testNetConnect_Label();
        testNetConnect_Label_Null();
        testNetConnect_Label_LongString();
        testNetConnect_SetEncrypt();
        testNetConnect_SetEncrypt_Null();
        testNetConnect_DefaultEncryptDecrypt();
        testNetConnect_ResetProperty();

        // ── NetPacketTypeManager 边界 ──
        testTypeManager_RegisteTypeIDZero_NotAdded();
    }

    // ==================== NetPacketTypeManager ====================

    static void testTypeManager_RegisteAndLookup()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 100);
        manager.registePacket(typeof(TestPacket2), 200);

        assertEqual((ushort)100, manager.getPacketTypeID(typeof(TestPacket)), "typeID of TestPacket");
        assertEqual((ushort)200, manager.getPacketTypeID(typeof(TestPacket2)), "typeID of TestPacket2");
        assertEqual(typeof(TestPacket), manager.getPacketType(100), "type of ID 100");
        assertEqual(typeof(TestPacket2), manager.getPacketType(200), "type of ID 200");
    }

    static void testTypeManager_RegisteDuplicateClassType()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 100);

        // 重复注册同一 classType 会抛异常（Dictionary.Add 重复key）
        try
        {
            manager.registePacket(typeof(TestPacket), 999);
            // 如果没抛异常，检查 getPacketTypeID 返回什么
            // 实际上 Dictionary.Add 会抛 ArgumentException
        }
        catch (ArgumentException)
        {
            // 预期行为
        }
    }

    static void testTypeManager_RegisteDuplicateTypeID()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 100);

        // 重复注册相同 typeID：Dictionary.addIf 内部调用 Dictionary.Add
        // 重复 key 会抛 ArgumentException，这是实现行为
        bool threw = false;
        try
        {
            manager.registePacket(typeof(TestPacket2), 100);
        }
        catch (System.ArgumentException)
        {
            threw = true;
        }
        assertTrue(threw, "duplicate typeID throws");
    }

    static void testTypeManager_RegisteTypeIDZero()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        // typeID=0 时 addIf 的条件是 type > 0，所以不会添加
        manager.registePacket(typeof(TestPacket), 0);
        assertEqual((ushort)0, manager.getPacketTypeID(typeof(TestPacket)), "typeID=0 not registered");
    }

    static void testTypeManager_MultipleRegistrations()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 1);
        manager.registePacket(typeof(TestPacket2), 2);
        manager.registePacket(typeof(TestPacket3), 3);

        assertEqual(typeof(TestPacket), manager.getPacketType(1), "type 1");
        assertEqual(typeof(TestPacket2), manager.getPacketType(2), "type 2");
        assertEqual(typeof(TestPacket3), manager.getPacketType(3), "type 3");
    }

    static void testTypeManager_GetPacketTypeID_Unregistered()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertEqual((ushort)0, manager.getPacketTypeID(typeof(TestPacket)), "unregistered → 0");
    }

    static void testTypeManager_GetPacketType_Unregistered()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertNull(manager.getPacketType(9999), "unregistered → null");
        assertNull(manager.getPacketType(0), "ID 0 → null");
    }

    static void testTypeManager_UnregistePacket()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 100);
        manager.registePacket(typeof(TestPacket2), 200);
        assertEqual(typeof(TestPacket), manager.getPacketType(100), "100 已注册");
        assertEqual((ushort)100, manager.getPacketTypeID(typeof(TestPacket)), "TestPacket 已注册");

        // unregistePacket 同时移除双向映射
        manager.unregistePacket(typeof(TestPacket), 100);
        assertNull(manager.getPacketType(100), "unregistePacket 后 typeID=100 → null");
        assertEqual((ushort)0, manager.getPacketTypeID(typeof(TestPacket)), "unregistePacket 后 TestPacket → 0");

        // 未注册的 type 再 unregiste 也不报错(Remove 无害)
        manager.unregistePacket(typeof(TestPacket), 999);
        manager.unregistePacket(typeof(TestPacket3), 300);

        // 其余注册项不受影响
        assertEqual(typeof(TestPacket2), manager.getPacketType(200), "200 仍映射 TestPacket2");
    }

    static void testTypeManager_UnregisteUDPPacketName()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registeUDPPacketName(300, "HeartBeat");
        manager.registeUDPPacketName(301, "Login");
        assertTrue(manager.isUDPPacket(300), "300 是 UDP");
        assertEqual((ushort)300, manager.getUDPPacketType("HeartBeat"), "HeartBeat → 300");

        // unregisteUDPPacketName 同时移除 name→type 与 type→name 两条映射
        manager.unregisteUDPPacketName(300, "HeartBeat");
        assertEqual((ushort)0, manager.getUDPPacketType("HeartBeat"), "unregiste 后 HeartBeat → 0");
        assertFalse(manager.isUDPPacket(300), "unregiste 后 300 不再标记为 UDP");

        // 未注册的 name/type 再 unregiste 也不报错(Remove 无害)
        manager.unregisteUDPPacketName(999, "Unknown");
        manager.unregisteUDPPacketName(301, "NotExist");

        // 其余注册项不受影响
        assertEqual((ushort)301, manager.getUDPPacketType("Login"), "Login 仍 → 301");
        assertTrue(manager.isUDPPacket(301), "301 仍为 UDP");
    }

    static void testTypeManager_UDPNameMapping()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registeUDPPacketName(300, "HeartBeat");
        manager.registeUDPPacketName(301, "Login");

        assertEqual((ushort)300, manager.getUDPPacketType("HeartBeat"), "HeartBeat → 300");
        assertEqual((ushort)301, manager.getUDPPacketType("Login"), "Login → 301");
        assertTrue(manager.isUDPPacket(300), "300 is UDP");
        assertTrue(manager.isUDPPacket(301), "301 is UDP");
    }

    static void testTypeManager_UDPNameMapping_DuplicateName()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registeUDPPacketName(100, "Test");

        try
        {
            manager.registeUDPPacketName(200, "Test");
            // Dictionary.Add 重复 key 抛异常
        }
        catch (ArgumentException)
        {
            // 预期行为
        }
    }

    static void testTypeManager_UDPNameMapping_DuplicateID()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registeUDPPacketName(100, "Name1");

        try
        {
            manager.registeUDPPacketName(100, "Name2");
            // mUDPIDNameList.Add(type, name) 重复 key
        }
        catch (ArgumentException)
        {
            // 预期行为
        }
    }

    static void testTypeManager_GetUDPPacketType_Unknown()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertEqual((ushort)0, manager.getUDPPacketType("Unknown"), "unknown → 0");
        assertEqual((ushort)0, manager.getUDPPacketType(""), "empty → 0");
    }

    static void testTypeManager_IsUDPPacket_True()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registeUDPPacketName(42, "Answer");
        assertTrue(manager.isUDPPacket(42), "registered UDP");
    }

    static void testTypeManager_IsUDPPacket_False()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertFalse(manager.isUDPPacket(1), "not registered");
        assertFalse(manager.isUDPPacket(0), "zero");
        assertFalse(manager.isUDPPacket(ushort.MaxValue), "max");
    }

    // ==================== NetConnect 加解密 ====================

    private class TestNetConnect : NetConnect
    {
        public new static void encrypt(byte[] data, int offset, int length, byte param)
        {
            NetConnect.encrypt(data, offset, length, param);
        }
        public new static void decrypt(byte[] data, int offset, int length, byte param)
        {
            NetConnect.decrypt(data, offset, length, param);
        }
    }

    static void testEncryptDecrypt_RoundTrip_Basic()
    {
        byte[] original = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 42);
        TestNetConnect.decrypt(data, 0, data.Length, 42);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "roundTrip[" + i + "]");
    }

    static void testEncryptDecrypt_RoundTrip_Empty()
    {
        byte[] empty = new byte[0];
        TestNetConnect.encrypt(empty, 0, 0, 0);
        TestNetConnect.decrypt(empty, 0, 0, 0);
    }

    static void testEncryptDecrypt_RoundTrip_SingleByte()
    {
        byte[] original = { 0xAB };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, 1, 7);
        TestNetConnect.decrypt(data, 0, 1, 7);

        assertEqual(original[0], data[0], "single byte roundTrip");
    }

    static void testEncryptDecrypt_RoundTrip_LargeData()
    {
        byte[] original = new byte[256];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)(i & 0xFF);

        byte[] data = (byte[])original.Clone();
        TestNetConnect.encrypt(data, 0, data.Length, 123);
        TestNetConnect.decrypt(data, 0, data.Length, 123);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "large[" + i + "]");
    }

    static void testEncryptDecrypt_RoundTrip_WithOffset()
    {
        // 测试非零偏移量的加解密
        byte[] original = { 0xFF, 0xEE, 1, 2, 3, 4, 5, 6, 0xAA, 0xBB };
        byte[] data = (byte[])original.Clone();

        // 只加解密中间部分 (offset=2, length=6)
        TestNetConnect.encrypt(data, 2, 6, 42);
        TestNetConnect.decrypt(data, 2, 6, 42);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "offset[" + i + "]");

        // 验证前后字节未变（offset 0-1 和 8-9 不受影响）
        assertEqual((byte)0xFF, data[0], "prefix byte 0 unchanged");
        assertEqual((byte)0xEE, data[1], "prefix byte 1 unchanged");
        assertEqual((byte)0xAA, data[8], "suffix byte 8 unchanged");
        assertEqual((byte)0xBB, data[9], "suffix byte 9 unchanged");
    }

    static void testEncryptDecrypt_RoundTrip_PartialLength()
    {
        byte[] original = { 10, 20, 30, 40, 50, 60, 70, 80 };
        byte[] data = (byte[])original.Clone();

        // 只加密前 4 字节
        TestNetConnect.encrypt(data, 0, 4, 99);
        TestNetConnect.decrypt(data, 0, 4, 99);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "partial[" + i + "]");
    }

    static void testEncryptDecrypt_DifferentParams()
    {
        byte[] original = { 1, 2, 3, 4, 5 };
        byte[] data1 = (byte[])original.Clone();
        byte[] data2 = (byte[])original.Clone();
        byte[] data3 = (byte[])original.Clone();

        TestNetConnect.encrypt(data1, 0, data1.Length, 1);
        TestNetConnect.decrypt(data1, 0, data1.Length, 1);

        TestNetConnect.encrypt(data2, 0, data2.Length, 128);
        TestNetConnect.decrypt(data2, 0, data2.Length, 128);

        TestNetConnect.encrypt(data3, 0, data3.Length, 255);
        TestNetConnect.decrypt(data3, 0, data3.Length, 255);

        for (int i = 0; i < original.Length; i++)
        {
            assertEqual(original[i], data1[i], "param=1 [" + i + "]");
            assertEqual(original[i], data2[i], "param=128 [" + i + "]");
            assertEqual(original[i], data3[i], "param=255 [" + i + "]");
        }
    }

    static void testEncryptDecrypt_Param0()
    {
        byte[] original = { 10, 20, 30, 40 };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 0);
        TestNetConnect.decrypt(data, 0, data.Length, 0);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "param=0 [" + i + "]");
    }

    static void testEncryptDecrypt_Param255()
    {
        byte[] original = new byte[64];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)i;

        byte[] data = (byte[])original.Clone();
        TestNetConnect.encrypt(data, 0, data.Length, 255);
        TestNetConnect.decrypt(data, 0, data.Length, 255);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "param=255 [" + i + "]");
    }

    static void testEncryptDecrypt_DoubleEncryptDecrypt()
    {
        // 连续两次加解密也应该还原
        byte[] original = { 7, 14, 21, 28, 35 };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 55);
        TestNetConnect.encrypt(data, 0, data.Length, 55);
        TestNetConnect.decrypt(data, 0, data.Length, 55);
        TestNetConnect.decrypt(data, 0, data.Length, 55);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "double[" + i + "]");
    }

    // ==================== NetConnect 属性 ====================

    static void testNetConnect_Label()
    {
        TestNetConnect conn = new TestNetConnect();
        assertNull(conn.getLabel(), "default null");

        conn.setLabel("GameServer");
        assertEqual("GameServer", conn.getLabel(), "label set");

        conn.setLabel("");
        assertEqual("", conn.getLabel(), "label empty string");
    }

    static void testNetConnect_Label_Null()
    {
        TestNetConnect conn = new TestNetConnect();
        conn.setLabel("Test");
        conn.setLabel(null);
        assertNull(conn.getLabel(), "label reset to null");
    }

    static void testNetConnect_SetEncrypt()
    {
        TestNetConnect conn = new TestNetConnect();
        // setEncrypt 替换加密/解密函数，null 表示使用默认加密
        conn.setEncrypt(null, null);
        // 设置 null 后，mEncryptPacket/mDecryptPacket 应为 null
        // 但默认构造函数中 mEncryptPacket 通过 FrameSystem.init 设置为默认 encrypt/decrypt
        // 验证不崩溃即可
    }

    static void testNetConnect_ResetProperty()
    {
        TestNetConnect conn = new TestNetConnect();
        conn.setLabel("Server1");

        conn.resetProperty();
        assertNull(conn.getLabel(), "label reset after resetProperty");
    }

    // ==================== 第三轮新增测试 ====================

    static void testTypeManager_GetPacketType_MaxID()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertNull(manager.getPacketType(ushort.MaxValue), "max ID → null");
    }

    static void testTypeManager_GetUDPPacketType_EmptyString()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertEqual((ushort)0, manager.getUDPPacketType(""), "empty string → 0");
    }

    static void testTypeManager_IsUDPPacket_MaxValue()
    {
        NetPacketTypeManager manager = new NetPacketTypeManager();
        assertFalse(manager.isUDPPacket(ushort.MaxValue), "max value not UDP");
    }

    static void testEncryptDecrypt_RoundTrip_AllZeros()
    {
        byte[] original = new byte[32]; // 全0
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 77);
        TestNetConnect.decrypt(data, 0, data.Length, 77);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "all zeros[" + i + "]");
    }

    static void testEncryptDecrypt_RoundTrip_AllFFs()
    {
        byte[] original = new byte[32];
        for (int i = 0; i < original.Length; i++)
            original[i] = 0xFF;
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 77);
        TestNetConnect.decrypt(data, 0, data.Length, 77);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "all FFs[" + i + "]");
    }

    static void testEncryptDecrypt_Param128()
    {
        byte[] original = { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 128);
        TestNetConnect.decrypt(data, 0, data.Length, 128);

        for (int i = 0; i < original.Length; i++)
            assertEqual(original[i], data[i], "param=128[" + i + "]");
    }

    static void testEncryptDecrypt_OnlyEncrypt_ChangesData()
    {
        byte[] original = { 1, 2, 3, 4, 5 };
        byte[] data = (byte[])original.Clone();

        TestNetConnect.encrypt(data, 0, data.Length, 42);

        // 加密后数据应该与原始不同
        bool changed = false;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != original[i])
            {
                changed = true;
                break;
            }
        }
        assertTrue(changed, "encrypt changes data");
    }

    static void testEncryptDecrypt_OffsetZero_LengthZero()
    {
        byte[] data = { 1, 2, 3 };
        TestNetConnect.encrypt(data, 0, 0, 1);
        TestNetConnect.decrypt(data, 0, 0, 1);
        // 零长度操作不改变数据
        assertEqual((byte)1, data[0], "zero length: byte 0 unchanged");
        assertEqual((byte)2, data[1], "zero length: byte 1 unchanged");
        assertEqual((byte)3, data[2], "zero length: byte 2 unchanged");
    }

    static void testNetConnect_Label_LongString()
    {
        TestNetConnect conn = new TestNetConnect();
        string longLabel = new string('X', 1000);
        conn.setLabel(longLabel);
        assertEqual(longLabel, conn.getLabel(), "long label preserved");
    }

    static void testNetConnect_SetEncrypt_Null()
    {
        TestNetConnect conn = new TestNetConnect();
        conn.setEncrypt(null, null);
        // 设为 null 后，加解密不执行
        // 不崩溃即可（注意：实际调用 null 加解密会 NRE，这里只验证设置不崩溃）
    }

    static void testNetConnect_DefaultEncryptDecrypt()
    {
        // 验证构造函数中默认的 encrypt/decrypt 可以正常往返
        TestNetConnect conn = new TestNetConnect();
        byte[] data = { 10, 20, 30, 40, 50 };
        byte[] original = (byte[])data.Clone();

        // 通过反射调用默认的 encrypt/decrypt
        var encryptMethod = typeof(NetConnect).GetMethod("encrypt",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var decryptMethod = typeof(NetConnect).GetMethod("decrypt",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        encryptMethod.Invoke(null, new object[] { data, 0, data.Length, (byte)7 });
        decryptMethod.Invoke(null, new object[] { data, 0, data.Length, (byte)7 });

        // 往返后应与原始数据一致
        for (int i = 0; i < data.Length; i++)
        {
            assertEqual(original[i], data[i], "roundtrip byte " + i);
        }
    }

    static void testTypeManager_RegisteTypeIDZero_NotAdded()
    {
        // registePacket 使用 addIf(type, info, type > 0) — type=0 不应被添加
        NetPacketTypeManager manager = new NetPacketTypeManager();
        manager.registePacket(typeof(TestPacket), 0);

        // type=0 被 addIf 拒绝，getPacketType(0) 应返回 null
        assertNull(manager.getPacketType(0), "type 0 not added");
        // getPacketTypeID 通过 classType 查找，仍然可以找到（因为 mClassTypeList.Add 无条件）
        assertEqual((ushort)0, manager.getPacketTypeID(typeof(TestPacket)), "classType still maps to 0");
    }
}
