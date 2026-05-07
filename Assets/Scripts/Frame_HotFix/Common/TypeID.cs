using System.Threading;

static public class TypeID
{
	static public int mGlobalCounter = 0;	// 全局唯一计数器
}

// 用来获取Type的对应ID,比GetHashCode要快,线程安全
static public class TypeID<T>
{
	public static readonly int ID = Interlocked.Increment(ref TypeID.mGlobalCounter);
}