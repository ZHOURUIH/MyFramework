using static TestAssert;

// ExcelDataT<T>.setTable 单测
//
// 设计要点:
//   - setTable 是 static 且无 getter, 通过子类暴露 protected static mTable 验证赋值生效。
//   - ExcelTableT<T> 构造函数内部会调用 ExcelDataT<T>.setTable(this),
//     因此本测试同时覆盖了"构造表格即绑定数据表"的链路。
//   - mTable 是 ExcelDataT<具体类型> 的泛型静态(不同 T 各一份), 属全局静态残留,
//     用例结束须 setTable(null) 清掉, 避免污染后续 Excel 相关测试。
public static class ExcelDataTTest
{
	public static void Run()
	{
		testSetTableViaConstructor();
		testSetTableExplicitAndClear();
	}

	// 测试数据行类型(ExcelData 为具体类, mID 公有)
	private class MyData : ExcelData { }
	// 测试列表表类型
	private class MyTable : ExcelTableT<MyData> { }
	// 暴露 mTable 以便断言
	private class MyDataT : ExcelDataT<MyData>
	{
		public static ExcelTableT<MyData> peek()
		{
			return mTable;
		}
	}

	// ─── 构造表格自动 setTable ──────────────────────────────────────
	private static void testSetTableViaConstructor()
	{
		MyTable table = new MyTable();
		try
		{
			assertTrue(MyDataT.peek() == table, "ExcelTableT 构造应自动 setTable 到对应 ExcelDataT");
		}
		finally
		{
			ExcelDataT<MyData>.setTable(null);   // 清理泛型静态残留
		}
	}

	// ─── 显式 setTable 与 setTable(null) 清零 ───────────────────────
	private static void testSetTableExplicitAndClear()
	{
		MyTable t1 = new MyTable();
		MyTable t2 = new MyTable();
		try
		{
			// 构造先后触发两次 setTable, mTable 最终指向最后一次构造的 t2
			assertTrue(MyDataT.peek() == t2, "连续构造后 mTable 应指向最后一个");

			// 显式 setTable 覆盖
			ExcelDataT<MyData>.setTable(t1);
			assertTrue(MyDataT.peek() == t1, "显式 setTable 后 mTable 指向 t1");
		}
		finally
		{
			ExcelDataT<MyData>.setTable(null);   // 清空泛型静态残留
		}
	}
}
