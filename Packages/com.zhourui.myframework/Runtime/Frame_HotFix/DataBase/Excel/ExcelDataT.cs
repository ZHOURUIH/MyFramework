
// 带模板类型的基类,目的只是为了能够让子类能够直接访问到表格对象
public class ExcelDataT<T> : ExcelData where T : ExcelData
{
	protected static ExcelTableT<T> mTable;
	public static void setTable(ExcelTableT<T> table) { mTable = table; }
}