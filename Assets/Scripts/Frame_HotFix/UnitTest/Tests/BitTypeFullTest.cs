#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// BIT_* 类型序列化器测试：通过 SerializerBitWrite/Read 做 roundtrip
public static class BitTypeFullTest
{
	public static void Run()
	{
		testBIT_BOOL();
		testBIT_INT();
		testBIT_UINT();
		testBIT_LONG();
		testBIT_FLOAT();
		testBIT_STRING();
	}

	private static void testBIT_BOOL()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_BOOL();
		v.set(true); v.write(w, false);
		v.set(false); v.write(w, false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_BOOL();
		vr.read(r, false);
		AssertEqual(true, (bool)vr);
		vr.read(r, false);
		AssertEqual(false, (bool)vr);
	}

	private static void testBIT_INT()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_INT();
		v.set(42); v.write(w, true);
		v.set(-1); v.write(w, true);
		v.set(0); v.write(w, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_INT();
		vr.read(r, true);
		AssertEqual(42, (int)vr);
		vr.read(r, true);
		AssertEqual(-1, (int)vr);
		vr.read(r, true);
		AssertEqual(0, (int)vr);
	}

	private static void testBIT_UINT()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_UINT();
		v.set(0u); v.write(w, false);
		v.set(12345u); v.write(w, false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_UINT();
		vr.read(r, false);
		AssertEqual(0u, (uint)vr);
		vr.read(r, false);
		AssertEqual(12345u, (uint)vr);
	}

	private static void testBIT_LONG()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_LONG();
		v.set(long.MaxValue); v.write(w, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_LONG();
		vr.read(r, true);
		AssertEqual(long.MaxValue, (long)vr);
	}

	private static void testBIT_FLOAT()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_FLOAT();
		v.set(3.14f); v.write(w, false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_FLOAT();
		vr.read(r, false);
		AssertEqual(3.14f, (float)vr);
	}

	private static void testBIT_STRING()
	{
		var w = new SerializerBitWrite();
		var v = new BIT_STRING();
		v.set("hello"); v.write(w, false);
		v.set(""); v.write(w, false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var vr = new BIT_STRING();
		vr.read(r, false);
		AssertEqual("hello", (string)vr);
		vr.read(r, false);
		AssertEqual("", (string)vr);
	}
	private static void AssertEqual(bool e, bool a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(uint e, uint a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(long e, long a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
