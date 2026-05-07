#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// Byte/* 类型序列化器测试：通过 SerializerWrite/Read 做 roundtrip
public static class ByteTypeFullTest
{
	public static void Run()
	{
		testBOOL();
		testBYTE();
		testSHORT();
		testINT();
		testLONG();
		testFLOAT();
		testSTRING();
	}

	private static void testBOOL()
	{
		var w = new SerializerWrite();
		var v = new BOOL();
		v.set(true); v.write(w);
		v.set(false); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new BOOL();
		vr.read(r);
		AssertEqual(true, (bool)vr);
		vr.read(r);
		AssertEqual(false, (bool)vr);
	}

	private static void testBYTE()
	{
		var w = new SerializerWrite();
		var v = new BYTE();
		v.set(0); v.write(w);
		v.set(255); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new BYTE();
		vr.read(r);
		AssertEqual((byte)0, (byte)vr);
		vr.read(r);
		AssertEqual((byte)255, (byte)vr);
	}

	private static void testSHORT()
	{
		var w = new SerializerWrite();
		var v = new SHORT();
		v.set(32767); v.write(w);
		v.set(-32768); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new SHORT();
		vr.read(r);
		AssertEqual((short)32767, (short)vr);
		vr.read(r);
		AssertEqual((short)-32768, (short)vr);
	}

	private static void testINT()
	{
		var w = new SerializerWrite();
		var v = new INT();
		v.set(42); v.write(w);
		v.set(-1); v.write(w);
		v.set(0); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new INT();
		vr.read(r);
		AssertEqual(42, (int)vr);
		vr.read(r);
		AssertEqual(-1, (int)vr);
		vr.read(r);
		AssertEqual(0, (int)vr);
	}

	private static void testLONG()
	{
		var w = new SerializerWrite();
		var v = new LONG();
		v.set(long.MaxValue); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new LONG();
		vr.read(r);
		AssertEqual(long.MaxValue, (long)vr);
	}

	private static void testFLOAT()
	{
		var w = new SerializerWrite();
		var v = new FLOAT();
		v.set(3.14f); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new FLOAT();
		vr.read(r);
		AssertEqual(3.14f, (float)vr);
	}

	private static void testSTRING()
	{
		var w = new SerializerWrite();
		var v = new STRING();
		v.set("hello"); v.write(w);
		v.set(""); v.write(w);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var vr = new STRING();
		vr.read(r);
		AssertEqual("hello", (string)vr);
		vr.read(r);
		AssertEqual("", (string)vr);
	}
	private static void AssertEqual(bool e, bool a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(byte e, byte a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(short e, short a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(long e, long a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
