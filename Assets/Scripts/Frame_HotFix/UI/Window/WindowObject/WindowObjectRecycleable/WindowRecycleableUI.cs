
// 可用于WindowStructPool和WindowStructPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
// 根节点是myUIObject类型
public abstract class WindowRecycleableUI : WindowObjectRecycleableT<myUIObject>
{
	public WindowRecycleableUI(LayoutScript script) : base(script) { }
}