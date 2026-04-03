
// GameBaseHotFix的部分类,用于定义SQLite表格的对象,需要自己在SQLiteRegister中赋值
public partial class GBH
{
#if USE_SQLITE
	public static SQLiteDemo mSQLiteDemo;
#endif
}
