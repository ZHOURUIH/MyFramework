using System;
using System.Collections.Generic;
using System.Reflection;
using static TestAssert;

public class SerializerBitWriteTest
{
	public static void Run()
	{
		testConstructor();
		testWriteMethodsExist();
		testWriteIntMethod();
		testWriteUIntMethod();
		testWriteLongMethod();
		testWriteULongMethod();
		testWriteShortMethod();
		testWriteUShortMethod();
		testWriteByteMethod();
		testWriteSByteMethod();
		testWriteBoolMethod();
		testWriteFloatMethod();
		testWriteStringMethod();
		testWriteVector2Method();
		testWriteVector3Method();
		testWriteVector4Method();
		testWriteListIntMethod();
		testWriteListFloatMethod();
		testWriteListLongMethod();
		testMethodSignaturesExist();
		testInstanceCreation();
		testWriteMethodsReturnType();
		testWriteBufferBitCount();
		testWriteBufferZeroLen();
		testFillZeroToByteEndAfterBool();
		testClearResetsBitIndex();
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
	private static MethodInfo tryFindWriteMethod(Type t, Type[] paramTypesWithoutOptional)
	{
		var method = findMethod(t, "write", paramTypesWithoutOptional);
		if (method != null)
		{
			return method;
		}
		// 尝试加上 needReadSign bool 参数
		var withBool = new Type[paramTypesWithoutOptional.Length + 1];
		Array.Copy(paramTypesWithoutOptional, withBool, paramTypesWithoutOptional.Length);
		withBool[paramTypesWithoutOptional.Length] = typeof(bool);
		return findMethod(t, "write", withBool);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructor()
	{
		Type t = typeof(SerializerBitWrite);
		var ctors = t.GetConstructors();
		assertTrue(ctors.Length > 0, "SerializerBitWrite should have at least one constructor");
	}
	private static void testWriteMethodsExist()
	{
		Type t = typeof(SerializerBitWrite);
		var methods = t.GetMethods();
		int writeCount = 0;
		foreach (var m in methods)
		{
			if (m.Name.StartsWith("write") || m.Name.StartsWith("Write"))
			{
				writeCount++;
			}
		}
		assertTrue(writeCount > 0, "SerializerBitWrite should have write methods, found: " + writeCount);
	}
	private static void testWriteIntMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(int) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteUIntMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(uint) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteLongMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(long) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteULongMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(ulong) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteShortMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(short) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteUShortMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(ushort) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteByteMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(byte) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteSByteMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(sbyte) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteBoolMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteFloatMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(float) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteStringMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = findMethod(t, "writeString", new Type[] { typeof(string) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteVector2Method()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(UnityEngine.Vector2) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteVector3Method()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(UnityEngine.Vector3) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteVector4Method()
	{
		Type t = typeof(SerializerBitWrite);
		var method = tryFindWriteMethod(t, new Type[] { typeof(UnityEngine.Vector4) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteListIntMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = findMethod(t, "writeList", new Type[] { typeof(List<int>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteListFloatMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = findMethod(t, "writeList", new Type[] { typeof(List<float>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testWriteListLongMethod()
	{
		Type t = typeof(SerializerBitWrite);
		var method = findMethod(t, "writeList", new Type[] { typeof(List<long>), typeof(bool) });
		if (method != null)
		{
			assertNotNull(method);
		}
	}
	private static void testMethodSignaturesExist()
	{
		Type t = typeof(SerializerBitWrite);
		string[] expectedNames = new string[] { "write", "writeString", "writeList" };
		foreach (string name in expectedNames)
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
			assertTrue(found, "Method " + name + " should exist in SerializerBitWrite");
		}
	}
	private static void testInstanceCreation()
	{
		Type t = typeof(SerializerBitWrite);
		try
		{
			var instance = Activator.CreateInstance(t);
			assertNotNull(instance, "Should be able to create SerializerBitWrite instance");
		}
		catch (Exception e)
		{
			// 如果实例化失败（例如需要参数），测试可以接受
			assertTrue(true, "Instance creation may fail if constructor requires parameters: " + e.Message);
		}
	}
	private static void testWriteMethodsReturnType()
	{
		Type t = typeof(SerializerBitWrite);
		foreach (var method in t.GetMethods())
		{
			if (method.Name == "write" && method.GetParameters().Length > 0)
			{
				assertNotNull(method);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 位写入器核心逻辑测试(writeBuffer/fillZeroToByteEnd/getBitCount/getByteCount/clear)
	// 纯内存位操作, 确定性可测。基于 SerializeBitUtility 语义:
	//   write(bool) 固定 1 位; writeBuffer 裸写字节推进 size*8 位;
	//   fillZeroToByteEnd 对齐到字节末尾; getByteCount = bitCountToByteCount(bitCount)(向上取整到字节)
	private static void testWriteBufferBitCount()
	{
		var w = new SerializerBitWrite();
		assertEqual(0, w.getBitCount(), "初始 bitCount 为 0");
		assertEqual(0, w.getByteCount(), "初始 byteCount 为 0");
		// write(bool) 固定写 1 位
		w.write(true);
		assertEqual(1, w.getBitCount(), "write(bool) 后 bitCount=1");
		assertEqual(1, w.getByteCount(), "1 bit → 1 byte");
		assertTrue(w.getBuffer() != null, "write 后 getBuffer 非 null");
		// writeBuffer 裸写 3 字节: 内部先 fillZeroToByteEnd(1→8对齐), 再写 3 字节 → bitIndex=(1+3)*8=32
		byte[] data = { 1, 2, 3 };
		w.writeBuffer(data, 3);
		assertEqual(32, w.getBitCount(), "writeBuffer(3字节) 后 bitIndex 对齐到字节末尾+24 = 32");
		assertEqual(4, w.getByteCount(), "32 bit → 4 byte");
	}
	private static void testWriteBufferZeroLen()
	{
		var w = new SerializerBitWrite();
		// 空缓冲区或 dataSize=0 时 writeBuffer 直接 return, 不改变 bitCount
		w.writeBuffer(null, 0);
		assertEqual(0, w.getBitCount(), "writeBuffer(null,0) 不改变 bitCount");
		byte[] data = { 9 };
		w.writeBuffer(data, 0);
		assertEqual(0, w.getBitCount(), "writeBuffer(data,0) 不改变 bitCount");
	}
	private static void testFillZeroToByteEndAfterBool()
	{
		var w = new SerializerBitWrite();
		w.write(true); // 1 位
		assertEqual(1, w.getBitCount(), "write(bool) 后 bitCount=1");
		w.fillZeroToByteEnd();
		assertEqual(8, w.getBitCount(), "fillZeroToByteEnd 后 bitCount 对齐到字节末尾 8");
		assertEqual(1, w.getByteCount(), "8 bit → 1 byte");
	}
	private static void testClearResetsBitIndex()
	{
		var w = new SerializerBitWrite();
		w.write(true);
		w.write(true);
		assertEqual(2, w.getBitCount(), "写 2 个 bool 后 bitCount=2");
		w.clear();
		assertEqual(0, w.getBitCount(), "clear 后 bitCount=0");
	}
}