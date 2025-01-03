
// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public class WindowObjectUGUI : WindowObjectFixedT<myUGUIObject>
{
	public WindowObjectUGUI(LayoutScript script) : base(script) { }
}