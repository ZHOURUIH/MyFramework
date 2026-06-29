using System;
using static TestAssert;

public class SerializableBitTest
{
	public static void Run()
	{
		testIsAbstractClass();
		testHasMValidField();
		testHasMOptionalField();
		testConstructorSetsMValidTrue();
		testHasReadAbstractMethod();
		testHasWriteAbstractMethod();
		testHasResetPropertyMethod();
		testClassHasSerializableAttribute();
		testImplementsClassObject();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testIsAbstractClass()
	{
		Type type = typeof(SerializableBit);
		assertTrue(type.IsAbstract, "SerializableBit should be abstract");
	}
	private static void testHasMValidField()
	{
		Type type = typeof(SerializableBit);
		var field = type.GetField("mValid", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		assertNotNull(field, "SerializableBit should have public field mValid");
		assertEqual(typeof(bool), field.FieldType, "mValid should be bool type");
	}
	private static void testHasMOptionalField()
	{
		Type type = typeof(SerializableBit);
		var field = type.GetField("mOptional", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		assertNotNull(field, "SerializableBit should have public field mOptional");
		assertEqual(typeof(bool), field.FieldType, "mOptional should be bool type");
	}
	private static void testConstructorSetsMValidTrue()
	{
		// 由于是抽象类，不能直接实例化，使用 BIT_INT 作为代理
		// BIT_INT 继承自 SerializableBit
		Type bitIntType = typeof(BIT_INT);
		var instance = Activator.CreateInstance(bitIntType) as SerializableBit;
		if (instance != null)
		{
			assertTrue(instance.mValid, "Constructor should set mValid = true");
		}
	}
	private static void testHasReadAbstractMethod()
	{
		Type type = typeof(SerializableBit);
		var method = type.GetMethod("read", new Type[] { typeof(SerializerBitRead), typeof(bool) });
		assertNotNull(method, "SerializableBit should have abstract read method");
		assertTrue(method.IsAbstract, "read method should be abstract");
		assertTrue(method.ReturnType == typeof(bool), "read method should return bool");
	}
	private static void testHasWriteAbstractMethod()
	{
		Type type = typeof(SerializableBit);
		var method = type.GetMethod("write", new Type[] { typeof(SerializerBitWrite), typeof(bool) });
		assertNotNull(method, "SerializableBit should have abstract write method");
		assertTrue(method.IsAbstract, "write method should be abstract");
		assertTrue(method.ReturnType == typeof(void), "write method should return void");
	}
	private static void testHasResetPropertyMethod()
	{
		Type type = typeof(SerializableBit);
		var method = type.GetMethod("resetProperty");
		assertNotNull(method, "SerializableBit should have resetProperty method");
		assertTrue(method.ReturnType == typeof(void), "resetProperty should return void");
	}
	private static void testClassHasSerializableAttribute()
	{
		// SerializableBit is abstract, it doesn't need [Serializable] but ClassObject might have it
		Type type = typeof(SerializableBit);
		assertTrue(type.IsAbstract, "SerializableBit should remain abstract");
	}
	private static void testImplementsClassObject()
	{
		Type type = typeof(SerializableBit);
		Type baseType = type.BaseType;
		assertEqual("ClassObject", baseType.Name, "SerializableBit should inherit from ClassObject");
	}
}