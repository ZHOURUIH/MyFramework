
public interface IRecyclableUI : IRecyclable
{
	// 在回收时调用
	public void recycle();
	public void assignWindow(myUGUIObject parent, myUGUIObject template, string name);
}