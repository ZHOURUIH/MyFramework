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
		testDisposeExplicit();
		testDisposeExplicit2();
		testNestedDifferentScopeTypes();
		testNestedSameTypeScope();
		testSequentialScopes();
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
	// Dispose 显式调用 — 释放后对象回到池, 再次获取不崩
	// ═════════════════════════════════════════════════════════════════
	private static void testDisposeExplicit()
	{
		// ListScope.Dispose
		ListScope<int> listScope = new ListScope<int>(out var list);
		assertNotNull(list, "ListScope 获取 List 非空");
		list.Add(7);
		listScope.Dispose();
		// ListScope2.Dispose(双 List)
		ListScope2<int> listScope2 = new ListScope2<int>(out var l0, out var l1);
		l0.Add(1);
		l1.Add(2);
		assertEqual(1, l0.Count, "l0 可写");
		assertEqual(1, l1.Count, "l1 可写");
		listScope2.Dispose();
		// ArrayScope.Dispose(数组)
		ArrayScope<int> arrayScope = new ArrayScope<int>(out var arr, 4);
		assertEqual(4, arr.Length, "ArrayScope 数组长度 4");
		arr[0] = 9;
		arrayScope.Dispose();
		// Dispose 后再次使用不崩(对象池可继续分配)
		using (new ListScope<int>(out var list2))
		{
			assertNotNull(list2, "Dispose 后重新获取 List 正常");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// Dispose 显式调用 2 — ClassScope/ClassScope2/DicScope/HashSetScope2
	// ═════════════════════════════════════════════════════════════════
	private static void testDisposeExplicit2()
	{
		// ClassScope.Dispose
		ClassScope<TestScopeObject> classScope = new ClassScope<TestScopeObject>(out var c0);
		assertNotNull(c0, "ClassScope 获取对象非空");
		classScope.Dispose();
		// ClassScope2.Dispose(双对象)
		ClassScope2<TestScopeObject> classScope2 = new ClassScope2<TestScopeObject>(out var c1, out var c2);
		assertNotNull(c1, "ClassScope2 第一个对象非空");
		assertNotNull(c2, "ClassScope2 第二个对象非空");
		classScope2.Dispose();
		// DicScope.Dispose
		DicScope<string, int> dicScope = new DicScope<string, int>(out var dict);
		assertNotNull(dict, "DicScope 获取字典非空");
		dict["k"] = 1;
		dicScope.Dispose();
		// HashSetScope2.Dispose(双 HashSet)
		HashSetScope2<int> hashScope2 = new HashSetScope2<int>(out var h0, out var h1);
		assertNotNull(h0, "HashSetScope2 第一个集合非空");
		assertNotNull(h1, "HashSetScope2 第二个集合非空");
		h0.Add(3);
		hashScope2.Dispose();
		// Dispose 后重新使用不崩
		using (new DicScope<string, int>(out var dict2))
		{
			assertNotNull(dict2, "Dispose 后重新获取字典正常");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 嵌套组合: 不同类型 scope 嵌套使用
	// ═════════════════════════════════════════════════════════════════
	private static void testNestedDifferentScopeTypes()
	{
		using (new ListScope<int>(out var list))
		{
			list.Add(1);
			list.Add(2);
			// 内嵌 DicScope
			using (new DicScope<string, int>(out var dict))
			{
				dict["a"] = list.Count;
				assertEqual(2, dict["a"], "内嵌字典读取外层列表 count");
				// 再内嵌 HashSetScope
				using (new HashSetScope<int>(out var set))
				{
					set.Add(10);
					assertTrue(set.Contains(10), "最内层集合可写");
				}
			}
			// 外层列表不受内层释放影响
			assertEqual(2, list.Count, "内层 scope 释放后外层列表完好");
			assertEqual(1, list[0], "外层列表元素 1 完好");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 嵌套组合: 同类型 scope 嵌套
	// ═════════════════════════════════════════════════════════════════
	private static void testNestedSameTypeScope()
	{
		using (new ListScope<int>(out var outer))
		{
			outer.Add(1);
			using (new ListScope<int>(out var inner))
			{
				inner.Add(2);
				assertEqual(2, inner[0], "内层列表可写");
			}
			assertEqual(1, outer.Count, "外层列表不受内层影响");
			assertEqual(1, outer[0], "外层元素完好");
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 顺序组合: 多个 scope 交替获取与释放
	// ═════════════════════════════════════════════════════════════════
	private static void testSequentialScopes()
	{
		// 顺序使用 3 个 scope(各自独立作用域)
		int sum = 0;
		using (new ListScope<int>(out var list1))
		{
			list1.Add(10);
			sum += list1[0];
		}
		using (new ListScope<int>(out var list2))
		{
			list2.Add(20);
			list2.Add(30);
			sum += list2[0] + list2[1];
		}
		using (new ListScope<int>(out var list3))
		{
			list3.Add(40);
			sum += list3[0];
		}
		assertEqual(100, sum, "顺序 scope 求和 10+20+30+40=100");
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
