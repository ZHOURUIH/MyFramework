#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// 深度序列化测试：混合类型 + 大量数据回环
public static class SerializerDeepTypeTest
{
	public static void Run()
	{
		testMixedTypes();
		testLargeBuffer();
	}

	private static void testMixedTypes()
	{
		var w = new SerializerWrite();
		w.write(42);
		w.write(3.14f);
		w.writeString("mixed");
		w.write((short)123);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.read(out int vi);
		r.read(out float vf);
		r.readString(out string vs);
		r.read(out short vs2);
		AssertEqual(42, vi);
		AssertEqual(3.14f, vf);
		AssertEqual("mixed", vs);
		AssertEqual((short)123, vs2);
	}

	private static void testLargeBuffer()
	{
		var w = new SerializerWrite();
		for (int i = 0; i < 100; i++)
		{
			w.write(i);
		}
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		for (int i = 0; i < 100; i++)
		{
			r.read(out int v);
			AssertEqual(i, v);
		}
	}

	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(float e, float a) { float d = e - a; if (d < 0) d = -d; if (d > 0.0001f) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(short e, short a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
	private static void AssertEqual(string e, string a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
