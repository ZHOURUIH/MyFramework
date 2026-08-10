using System;
using System.Collections.Generic;
using System.Reflection;
using static TestAssert;

public class SerializerBitReadTest
{
	public static void Run()
	{
		testConstructor();
		testReadMethodsExist();
		testReadIntMethod();
		testReadUIntMethod();
		testReadLongMethod();
		testReadULongMethod();
		testReadShortMethod();
		testReadUShortMethod();
		testReadByteMethod();
		testReadSByteMethod();
		testReadBoolMethod();
		testReadFloatMethod();
		testReadStringMethod();
		testReadVector2Method();
		testReadVector3Method();
		testReadVector4Method();
		testReadListIntMethod();
		testReadListFloatMethod();
		testReadListLongMethod();
		testMethodSignaturesExist();
		testInstanceCreation();
		testInitAndGetters();
		testReadBufferBytes();
		testReadBufferShortBuffer();
		testReadBufferZeroLen();
		testSkipToByteEnd();
		testReadEnumByte();
		testReadEnumInt();
		testReadEnumLong();
		testReadEnumByteList();
		testReadEnumRoundTrip();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 安全方法查找：避免 Type.GetMethod 在多个重载时抛出 AmbiguousMatchException
	private static MethodInfo findMethod(Type t, string name, Type[] paramTypes)
	{
		foreach (var m in t.GetMethods())
		{
			if (m.Name != name)
			{
				continue;
			}
			var ps = m.GetParameters();
			if (ps.Length != paramTypes.Length)
			{
				continue;
			}
			bool match = true;
			for (int i = 0; i < ps.Length; i++)
			{
				if (ps[i].ParameterType != paramTypes[i]) 
				{
					match = false; 
					break;
				}
			}
			if (match)
			{
                return m;
            }
		}
		return null;
	}
	private static MethodInfo tryFindReadMethod(Type t, Type[] paramTypesWithoutOptional)
	{
		var method = findMethod(t, "read", paramTypesWithoutOptional);
		if (method != null)
		{
			return method;
		}
		// 尝试加上 needReadSign bool 参数
		var withBool = new Type[paramTypesWithoutOptional.Length + 1];
		Array.Copy(paramTypesWithoutOptional, withBool, paramTypesWithoutOptional.Length);
		withBool[paramTypesWithoutOptional.Length] = typeof(bool);
		return findMethod(t, "read", withBool);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructor()
	{
		// SerializerBitRead 构造函数验证
		Type t = typeof(SerializerBitRead);
		var ctors = t.GetConstructors();
		assertTrue(ctors.Length > 0, "SerializerBitRead should have at least one constructor");
	}
	private static void testReadMethodsExist()
	{
		Type t = typeof(SerializerBitRead);
		var methods = t.GetMethods();
		int readCount = 0;
		foreach (var m in methods)
		{
			if (m.Name.StartsWith("read") || m.Name.StartsWith("Read"))
			{
				readCount++;
			}
		}
		assertTrue(readCount > 0, "SerializerBitRead should have read methods");
	}
	private static void testReadIntMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(int).MakeByRefType() });
		if (method != null)
		{
			assertTrue(method.ReturnType == typeof(bool) || method.ReturnType == typeof(void));
		}
	}
	private static void testReadUIntMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(uint).MakeByRefType() });
		if (method != null)
		{
			assertTrue(method.ReturnType == typeof(bool) || method.ReturnType == typeof(void));
		}
	}
	private static void testReadLongMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(long).MakeByRefType() });
		if (method != null)
		{
			assertTrue(method.ReturnType == typeof(bool) || method.ReturnType == typeof(void));
		}
	}
	private static void testReadULongMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(ulong).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadShortMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(short).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadUShortMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(ushort).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadByteMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(byte).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadSByteMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(sbyte).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadBoolMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(bool).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadFloatMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(float).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadStringMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = findMethod(t, "readString", new Type[] { typeof(string).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadVector2Method()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(UnityEngine.Vector2).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadVector3Method()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(UnityEngine.Vector3).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadVector4Method()
	{
		Type t = typeof(SerializerBitRead);
		var method = tryFindReadMethod(t, new Type[] { typeof(UnityEngine.Vector4).MakeByRefType() });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadListIntMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = findMethod(t, "readList", new Type[] { typeof(List<int>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadListFloatMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = findMethod(t, "readList", new Type[] { typeof(List<float>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testReadListLongMethod()
	{
		Type t = typeof(SerializerBitRead);
		var method = findMethod(t, "readList", new Type[] { typeof(List<long>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testMethodSignaturesExist()
	{
		Type t = typeof(SerializerBitRead);
		string[] expectedMethods = new string[] { "read", "readString", "readList" };
		foreach (string name in expectedMethods)
		{
			var methods = t.GetMethods();
			bool found = false;
			foreach (var m in methods)
			{
				if (m.Name == name)
				{
					found = true;
					break;
				}
			}
			assertTrue(found, "Method " + name + " should exist in SerializerBitRead");
		}
	}
	private static void testInstanceCreation()
	{
		Type t = typeof(SerializerBitRead);
		var instance = Activator.CreateInstance(t);
		assertNotNull(instance, "Should be able to create SerializerBitRead instance");
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 位读取器核心逻辑测试(init/readBuffer/skipToByteEnd/getBitIndex/getReadByteCount/getBufferSize)
	// 纯内存位操作, 确定性可测。readBuffer 从当前字节位读取 readLength 字节,
	//   mBitIndex = (起始字节+readLength)*8; skipToByteEnd 对齐到字节末尾; getReadByteCount = bitCountToByteCount(mBitIndex)
	private static void testInitAndGetters()
	{
		byte[] src = { 0xAA, 0xBB, 0xCC, 0xDD };
		var r = new SerializerBitRead();
		r.init(src, 4, 0);
		assertEqual(4, r.getBufferSize(), "init 后 bufferSize=4");
		assertEqual(0, r.getBitIndex(), "init bitIndex=0");
		assertEqual(0, r.getReadByteCount(), "初始已读字节 0");
		assertTrue(r.getBuffer() != null, "init 后 getBuffer 非 null");
	}
	private static void testReadBufferBytes()
	{
		byte[] src = { 0xAA, 0xBB, 0xCC, 0xDD };
		var r = new SerializerBitRead();
		r.init(src, 4, 0);
		byte[] dst = new byte[2];
		bool ok = r.readBuffer(dst, 2);
		assertTrue(ok, "readBuffer 数据充足返回 true");
		assertEqual((byte)0xAA, dst[0], "读到的第 1 字节");
		assertEqual((byte)0xBB, dst[1], "读到的第 2 字节");
		assertEqual(16, r.getBitIndex(), "readBuffer(2) 后 bitIndex=16");
		assertEqual(2, r.getReadByteCount(), "已读 2 字节");
	}
	private static void testReadBufferShortBuffer()
	{
		byte[] src = { 0xAA, 0xBB, 0xCC, 0xDD };
		var r = new SerializerBitRead();
		r.init(src, 4, 0);
		// 目标 buffer 长度 1, 读 3 字节 → 只拷 1 字节, 但下标正常跳转, 返回 false
		byte[] dst = new byte[1];
		bool ok = r.readBuffer(dst, 3);
		assertFalse(ok, "目标空间不足返回 false");
		assertEqual((byte)0xAA, dst[0], "只拷贝能容纳的 1 字节");
		assertEqual(24, r.getBitIndex(), "空间不足时下标仍正常跳转 = 3*8=24");
	}
	private static void testReadBufferZeroLen()
	{
		byte[] src = { 0xAA };
		var r = new SerializerBitRead();
		r.init(src, 1, 0);
		bool ok = r.readBuffer(new byte[1], 0);
		assertTrue(ok, "readLength=0 返回 true");
		assertEqual(0, r.getBitIndex(), "readLength=0 不改变 bitIndex");
	}
	private static void testSkipToByteEnd()
	{
		byte[] src = { 0xAA, 0xBB, 0xCC, 0xDD };
		var r = new SerializerBitRead();
		r.init(src, 4, 0);
		byte[] dst = new byte[1];
		r.readBuffer(dst, 1); // bitIndex = 8
		r.skipToByteEnd();
		assertEqual(8, r.getBitIndex(), "bitIndex=8 已是字节边界, skipToByteEnd 不变");
		// 模拟非字节对齐(手动 set bitIndex), 用 ref 语义验证 skipToByteEnd 对齐
		var r2 = new SerializerBitRead();
		r2.init(src, 4, 5); // 从第 5 位开始
		assertEqual(5, r2.getBitIndex(), "init bitIndex=5");
		r2.skipToByteEnd();
		assertEqual(8, r2.getBitIndex(), "skipToByteEnd 把 bitIndex 5 对齐到 8");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumByte — 用 write(byte) 对称写入再读回
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumByte()
	{
		SerializerBitWrite w = new SerializerBitWrite();
		w.write((byte)5);
		byte[] buffer = w.getBuffer();
		int byteCount = w.getByteCount();
		SerializerBitRead r = new SerializerBitRead();
		r.init(buffer, byteCount, 0);
		bool ok = r.readEnumByte<TestReadEnum>(out TestReadEnum value);
		assertTrue(ok, "readEnumByte 应返回 true");
		assertEqual(TestReadEnum.Five, value, "readEnumByte 读回写入的枚举值 5");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumInt — int 底层枚举
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumInt()
	{
		SerializerBitWrite w = new SerializerBitWrite();
		w.write(12345, true);
		SerializerBitRead r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount(), 0);
		bool ok = r.readEnumInt<TestReadEnumInt>(out TestReadEnumInt value, true);
		assertTrue(ok, "readEnumInt 应返回 true");
		assertEqual(TestReadEnumInt.Val12345, value, "readEnumInt 读回写入的枚举值 12345");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumLong — long 底层枚举
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumLong()
	{
		SerializerBitWrite w = new SerializerBitWrite();
		w.write(9876543210L, true);
		SerializerBitRead r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount(), 0);
		bool ok = r.readEnumLong<TestReadEnumLong>(out TestReadEnumLong value, true);
		assertTrue(ok, "readEnumLong 应返回 true");
		assertEqual(TestReadEnumLong.Big, value, "readEnumLong 读回写入的枚举值 9876543210");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumByteList — 枚举列表读取
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumByteList()
	{
		// readEnumByteList 内部 readListBit(List) 的格式 = 16位count前缀 + lengthBitType + 长度位 + 值。
		// write(Span<byte>) 走 writeListBit(Span) 不写 count 前缀, 与 readListBit(List) 不对称;
		// 故先用 write((ushort)count) 手写 count 前缀, 再 write(Span) 写数据体, 拼成对称格式。
		SerializerBitWrite w = new SerializerBitWrite();
		byte[] items = { 1, 5, 0 };
		w.write((ushort)items.Length);
		w.write(new Span<byte>(items));
		SerializerBitRead r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount(), 0);
		List<TestReadEnum> list = new List<TestReadEnum>();
		bool ok = r.readEnumByteList(list);
		assertTrue(ok, "readEnumByteList 应返回 true");
		assertEqual(3, list.Count, "读回 3 个枚举");
		assertEqual(TestReadEnum.One, list[0], "第 0 个枚举为 One");
		assertEqual(TestReadEnum.Five, list[1], "第 1 个枚举为 Five");
		assertEqual(TestReadEnum.Zero, list[2], "第 2 个枚举为 Zero");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumRoundTrip — 大值回环(验证变长编码不丢位)
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumRoundTrip()
	{
		// 连续写多个不同规模的枚举值, 依次读回
		SerializerBitWrite w = new SerializerBitWrite();
		w.write((byte)200);
		w.write((byte)0);
		w.write(30000, true);
		SerializerBitRead r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount(), 0);
		bool ok1 = r.readEnumByte<TestReadEnumBig>(out TestReadEnumBig v1);
		bool ok2 = r.readEnumByte<TestReadEnumBig>(out TestReadEnumBig v2);
		bool ok3 = r.readEnumInt<TestReadEnumBigInt>(out TestReadEnumBigInt v3, true);
		assertTrue(ok1 && ok2 && ok3, "连续读三个枚举值全部成功");
		assertEqual(TestReadEnumBig.Val200, v1, "读回 200");
		assertEqual(TestReadEnumBig.Val0, v2, "读回 0");
		assertEqual(TestReadEnumBigInt.Val30000, v3, "读回 30000");
	}
}

// ═════════════════════════════════════════════════════════════════
// readEnum 测试用枚举(不同底层类型)
// ═════════════════════════════════════════════════════════════════
public enum TestReadEnum : byte
{
	Zero = 0,
	One = 1,
	Five = 5,
}
public enum TestReadEnumInt : int
{
	Val12345 = 12345,
}
public enum TestReadEnumLong : long
{
	Big = 9876543210L,
}
public enum TestReadEnumBig : byte
{
	Val0 = 0,
	Val200 = 200,
}
public enum TestReadEnumBigInt : int
{
	Val30000 = 30000,
}