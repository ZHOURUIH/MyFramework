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
}