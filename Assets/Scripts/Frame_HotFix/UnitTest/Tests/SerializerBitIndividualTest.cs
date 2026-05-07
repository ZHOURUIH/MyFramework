#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// SerializerBitWrite/Read 逐类型读写测试
public static class SerializerBitIndividualTest
{
	public static void Run()
	{
		testBool();
		testByte();
		testSByte();
		testShort();
		testUShort();
		testInt();
		testUInt();
		testLong();
		testFloat();
		testString();
	}

	private static void testBool()
	{
		var w = new SerializerBitWrite();
		w.write(true);
		w.write(false);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out bool v1);
		r.read(out bool v2);
		Assert(v1);
		Assert(!v2);
	}

	private static void testByte()
	{
		var w = new SerializerBitWrite();
		w.write((byte)0);
		w.write((byte)255);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out byte v1);
		r.read(out byte v2);
		AssertEqual((byte)0, v1);
		AssertEqual((byte)255, v2);
	}

	private static void testSByte()
	{
		var w = new SerializerBitWrite();
		w.write((sbyte)-127, true);
		w.write((sbyte)127, true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out sbyte v1, true);
		r.read(out sbyte v2, true);
		AssertEqual((sbyte)-127, v1);
		AssertEqual((sbyte)127, v2);
	}

	private static void testShort()
	{
		var w = new SerializerBitWrite();
		w.write((short)32767, true);
		w.write((short)-32767, true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out short v1, true);
		r.read(out short v2, true);
		AssertEqual((short)32767, v1);
		AssertEqual((short)-32767, v2);
	}

	private static void testUShort()
	{
		var w = new SerializerBitWrite();
		w.write((ushort)0);
		w.write((ushort)65535);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out ushort v1);
		r.read(out ushort v2);
		AssertEqual((ushort)0, v1);
		AssertEqual((ushort)65535, v2);
	}

	private static void testInt()
	{
		var w = new SerializerBitWrite();
		w.write(42, true);
		w.write(-1, true);
		w.write(0, true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out int v1, true);
		r.read(out int v2, true);
		r.read(out int v3, true);
		AssertEqual(42, v1);
		AssertEqual(-1, v2);
		AssertEqual(0, v3);
	}

	private static void testUInt()
	{
		var w = new SerializerBitWrite();
		w.write(0u);
		w.write(100u);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out uint v1);
		r.read(out uint v2);
		AssertEqual(0u, v1);
		AssertEqual(100u, v2);
	}

	private static void testLong()
	{
		var w = new SerializerBitWrite();
		w.write(long.MaxValue, true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out long v1, true);
		AssertEqual(long.MaxValue, v1);
	}

	private static void testFloat()
	{
		var w = new SerializerBitWrite();
		w.write(3.14f, false);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out float v1, false);
		AssertEqual(3.14f, v1);
	}

	private static void testString()
	{
		var w = new SerializerBitWrite();
		w.writeString("test");
		w.writeString("");
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.readString(out string v1);
		r.readString(out string v2);
		AssertEqual("test", v1);
		AssertEqual("", v2);
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(byte e, byte a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(sbyte e, sbyte a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(short e, short a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(ushort e, ushort a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(uint e, uint a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(long e, long a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
