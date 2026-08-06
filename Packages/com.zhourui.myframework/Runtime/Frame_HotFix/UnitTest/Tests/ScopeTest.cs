using static TestAssert;

// Scope 作用域类单元测试 — 覆盖未纳入测试的 Scope 分配/释放结构
//   ByteArrayScope / ClassScope / ClassScope2 / ListScope / ListScope3 / ListScope4
//   DicScope / HashSetScope / HashSetScope2 / MyStringBuilderScope / MyStringBuilderScope2
// 均通过 using(new XxxScope(...)) 自动从对象池分配并释放, 验证不抛异常且数据可用
public static class ScopeTest
{
	// 测试用 ClassObject 子类
	private class TestScopeObject : ClassObject
	{
		public int mValue;
		public override void resetProperty()
		{
			base.resetProperty();
			mValue = 0;
		}
	}

	public static void Run()
	{
		testByteArrayScope();
		testClassScope();
		testClassScope2();
		testListScope();
		testListScope3();
		testListScope4();
		testDicScope();
		testHashSetScope();
		testHashSetScope2();
		testMyStringBuilderScope();
		testMyStringBuilderScope2();
	}

	// ═════════════════════════════════════════════════════════════════
	// ByteArrayScope
	// ═════════════════════════════════════════════════════════════════
	private static void testByteArrayScope()
	{
		using (new ByteArrayScope(out byte[] arr, 16))
		{
			assertNotNull(arr, "byte[] 不应为空");
			assert(arr.Length >= 16, "byte[] 长度应 >= 16");
			arr[0] = 0xAB;
			arr[15] = 0xCD;
			assertEqual((byte)0xAB, arr[0], "写入首字节应保留");
			assertEqual((byte)0xCD, arr[15], "写入末字节应保留");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// ClassScope / ClassScope2
	// ═════════════════════════════════════════════════════════════════
	private static void testClassScope()
	{
		using (new ClassScope<TestScopeObject>(out var obj))
		{
			assertNotNull(obj, "ClassObject 不应为空");
			obj.mValue = 7;
			assertEqual(7, obj.mValue, "字段可写读");
		}
	}
	private static void testClassScope2()
	{
		using (new ClassScope2<TestScopeObject>(out var a, out var b))
		{
			assertNotNull(a, "第一个对象不应为空");
			assertNotNull(b, "第二个对象不应为空");
			a.mValue = 1;
			b.mValue = 2;
			assertEqual(1, a.mValue, "a 字段写读");
			assertEqual(2, b.mValue, "b 字段写读");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// ListScope / ListScope3 / ListScope4
	// ═════════════════════════════════════════════════════════════════
	private static void testListScope()
	{
		using (new ListScope<int>(out var list))
		{
			assertNotNull(list, "List 不应为空");
			list.Add(10);
			list.Add(20);
			assertEqual(2, list.Count, "添加两个元素后 Count=2");
			assertEqual(20, list[1], "第二个元素正确");
		}
	}
	private static void testListScope3()
	{
		using (new ListScope3<string>(out var l0, out var l1, out var l2))
		{
			assertNotNull(l0, "l0 不应为空");
			assertNotNull(l1, "l1 不应为空");
			assertNotNull(l2, "l2 不应为空");
			l1.Add("x");
			assertEqual(1, l1.Count, "l1 可写读");
			assertEqual("x", l1[0], "l1 元素正确");
		}
	}
	private static void testListScope4()
	{
		using (new ListScope4<float>(out var l0, out var l1, out var l2, out var l3))
		{
			assertNotNull(l0, "l0 不应为空");
			assertNotNull(l1, "l1 不应为空");
			assertNotNull(l2, "l2 不应为空");
			assertNotNull(l3, "l3 不应为空");
			l2.Add(3.14f);
			assertEqual(1, l2.Count, "l2 可写读");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// DicScope
	// ═════════════════════════════════════════════════════════════════
	private static void testDicScope()
	{
		using (new DicScope<string, int>(out var dict))
		{
			assertNotNull(dict, "Dictionary 不应为空");
			dict["k"] = 5;
			assertEqual(5, dict["k"], "键值写入可读");
			assertEqual(1, dict.Count, "Count=1");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// HashSetScope / HashSetScope2
	// ═════════════════════════════════════════════════════════════════
	private static void testHashSetScope()
	{
		using (new HashSetScope<int>(out var set))
		{
			assertNotNull(set, "HashSet 不应为空");
			set.Add(1);
			set.Add(2);
			set.Add(1);
			assertEqual(2, set.Count, "去重后 Count=2");
			assertTrue(set.Contains(1), "包含 1");
		}
	}
	private static void testHashSetScope2()
	{
		using (new HashSetScope2<int>(out var s0, out var s1))
		{
			assertNotNull(s0, "s0 不应为空");
			assertNotNull(s1, "s1 不应为空");
			s0.Add(9);
			s1.Add(8);
			assertEqual(1, s0.Count, "s0 可写读");
			assertEqual(1, s1.Count, "s1 可写读");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// MyStringBuilderScope / MyStringBuilderScope2
	// ═════════════════════════════════════════════════════════════════
	private static void testMyStringBuilderScope()
	{
		using (new MyStringBuilderScope(out var sb))
		{
			assertNotNull(sb, "MyStringBuilder 不应为空");
			sb.add("hello");
			sb.add(" world");
			assertEqual("hello world", sb.ToString(), "add 拼接正确");
		}
	}
	private static void testMyStringBuilderScope2()
	{
		using (new MyStringBuilderScope2(out var a, out var b))
		{
			assertNotNull(a, "a 不应为空");
			assertNotNull(b, "b 不应为空");
			a.add("a");
			b.add("b");
			assertEqual("a", a.ToString(), "a 内容正确");
			assertEqual("b", b.ToString(), "b 内容正确");
		}
	}
}
