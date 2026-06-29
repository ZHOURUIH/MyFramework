
// 可使用对象池进行创建和销毁的对象
public static class ClassObjectExtension
{
	public static bool isValid(this ClassObject obj)
	{
		return obj != null && !obj.isDestroy();
	}
}