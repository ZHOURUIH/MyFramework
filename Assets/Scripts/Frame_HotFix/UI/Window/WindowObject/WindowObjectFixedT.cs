
// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public abstract class WindowObjectFixedT<T> : WindowObjectT<T> where T : myUGUIObject, new()
{
	public WindowObjectFixedT(IWindowObjectOwner parent) : base(parent)
	{
		// 只有Fixed窗口需要在这里添加,可回收的窗口会在对象池中进行管理
		mScript.addWindowObject(this);
	}
}

// 根节点是myUGUIObject类型
public abstract class WindowObjectUGUI : WindowObjectFixedT<myUGUIObject>
{
	public WindowObjectUGUI(IWindowObjectOwner parent) : base(parent) { }
}