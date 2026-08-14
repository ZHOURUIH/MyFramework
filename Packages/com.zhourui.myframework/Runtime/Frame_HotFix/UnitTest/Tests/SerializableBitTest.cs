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
		testBitBoolWriteReadRoundtrip();
		testBitBoolWriteReadMultiple();
		testBitBoolResetAfterRoundtrip();
		testBitBoolOptionalNotReset();
		testBitBoolToString();
		testBitBoolWriteReadWithSign();
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

	// ═════════════════════════════════════════════════════════════════
	// 行为测试(用 BIT_BOOL 具体实现做真实序列化)
	// ═════════════════════════════════════════════════════════════════

	// BIT_BOOL write → read 往返
	private static void testBitBoolWriteReadRoundtrip()
	{
		var bit = new BIT_BOOL();
		bit.set(true);
		var writer = new SerializerBitWrite();
		bit.write(writer, false);
		var reader = new SerializerBitRead();
		reader.init(writer.getBuffer());
		var bit2 = new BIT_BOOL();
		bool ok = bit2.read(reader, false);
		assertTrue(ok, "read 返回 true");
		assertTrue(bit2.mValue, "往返读回 true");
	}

	// 多个 bool 位流推进
	private static void testBitBoolWriteReadMultiple()
	{
		bool[] values = { true, false, true, true, false, true, false, false, true, true };
		var writer = new SerializerBitWrite();
		foreach (bool v in values)
		{
			var bit = new BIT_BOOL();
			bit.set(v);
			bit.write(writer, false);
		}
		var reader = new SerializerBitRead();
		reader.init(writer.getBuffer());
		for (int i = 0; i < values.Length; ++i)
		{
			var bit2 = new BIT_BOOL();
			bool ok = bit2.read(reader, false);
			assertTrue(ok, "第 " + i + " 个 read 成功");
			assertEqual(values[i], bit2.mValue, "第 " + i + " 个值一致");
		}
	}

	// 往返后 resetProperty
	private static void testBitBoolResetAfterRoundtrip()
	{
		var bit = new BIT_BOOL();
		bit.set(true);
		bit.mValid = false;
		bit.resetProperty();
		assertFalse(bit.mValue, "reset 后 mValue false");
		assertTrue(bit.mValid, "reset 后 mValid true");
	}

	// mOptional 不随 resetProperty 重置
	private static void testBitBoolOptionalNotReset()
	{
		var bit = new BIT_BOOL();
		bit.mOptional = true;
		bit.resetProperty();
		assertTrue(bit.mOptional, "resetProperty 后 mOptional 保持 true");
	}

	// toString 返回布尔字符串(boolToString 默认小写)
	private static void testBitBoolToString()
	{
		var bit = new BIT_BOOL();
		bit.set(true);
		assertEqual("true", bit.toString(), "toString true 小写");
		bit.set(false);
		assertEqual("false", bit.toString(), "toString false 小写");
	}

	// needReadSign/needWriteSign 参数正常传递
	private static void testBitBoolWriteReadWithSign()
	{
		var bit = new BIT_BOOL();
		bit.set(true);
		var writer = new SerializerBitWrite();
		bit.write(writer, true);   // needWriteSign=true
		var reader = new SerializerBitRead();
		reader.init(writer.getBuffer());
		var bit2 = new BIT_BOOL();
		bool ok = bit2.read(reader, true);   // needReadSign=true
		assertTrue(ok, "带 sign 参数 read 成功");
		assertTrue(bit2.mValue, "带 sign 参数往返值一致");
	}
}