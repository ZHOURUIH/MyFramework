
// 可回收UI接口,被引用计数管理的UI对象需实现此接口,回收时调用recycle
public interface IRecyclableUI : IRecyclable
{
	// 在回收时调用
	public void recycle();
	public void assignWindow(myUGUIObject parent, myUGUIObject template, string name);
}