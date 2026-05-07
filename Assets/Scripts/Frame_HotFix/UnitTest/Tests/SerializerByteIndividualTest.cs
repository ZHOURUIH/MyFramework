#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// SerializerWrite/Read 逐类型字节读写测试
public static class SerializerByteIndividualTest
{
	public static void Run()
	{
		testBool();
		testByte();
		testShort();
		testInt();
		testLong();
		testFloat();
		testString();
	}

	private static void testBool()
	{
		var w = new SerializerWrite();
		w.write(true);
		w.write(false);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out bool v1);
		r.read(out bool v2);
		Assert(v1);
		Assert(!v2);
	}

	private static void testByte()
	{
		var w = new SerializerWrite();
		w.write((byte)0);
		w.write((byte)255);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out byte v1);
		r.read(out byte v2);
		AssertEqual((byte)0, v1);
		AssertEqual((byte)255, v2);
	}

	private static void testShort()
	{
		var w = new SerializerWrite();
		w.write((short)100);
		w.write((short)-200);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out short v1);
		r.read(out short v2);
		AssertEqual((short)100, v1);
		AssertEqual((short)-200, v2);
	}

	private static void testInt()
	{
		var w = new SerializerWrite();
		w.write(42);
		w.write(-1);
		w.write(0);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out int v1);
		r.read(out int v2);
		r.read(out int v3);
		AssertEqual(42, v1);
		AssertEqual(-1, v2);
		AssertEqual(0, v3);
	}

	private static void testLong()
	{
		var w = new SerializerWrite();
		w.write(long.MaxValue);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out long v1);
		AssertEqual(long.MaxValue, v1);
	}

	private static void testFloat()
	{
		var w = new SerializerWrite();
		w.write(3.14f);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out float v1);
		AssertEqual(3.14f, v1);
	}

	private static void testString()
	{
		var w = new SerializerWrite();
		w.writeString("hello");
		w.writeString("");
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.readString(out string v1);
		r.readString(out string v2);
		AssertEqual("hello", v1);
		AssertEqual("", v2);
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(byte e, byte a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(short e, short a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(long e, long a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
