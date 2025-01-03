
// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public class WindowObjectFixedT<T> : WindowObjectT<T> where T : myUIObject, new()
{
	public WindowObjectFixedT(LayoutScript script) : base(script)
	{
		mScript.addWindowObject(this);
	}
	// 使用itemRoot作为Root
	public virtual void assignWindow(myUIObject itemRoot)
	{
		mRoot = itemRoot as T;
	}
	// 在指定的父节点下获取一个物体,将parent下名字为name的节点作为Root
	public virtual void assignWindow(myUIObject parent, string name)
	{
		mScript.newObject(out mRoot, parent, name);
	}
}