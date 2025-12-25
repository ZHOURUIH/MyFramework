
public interface IRecyclable
{
	// 在回收时调用
	public void recycle();
	public void setAssignID(long assignID);
	public long getAssignID();
	public void assignWindow(myUGUIObject parent, myUGUIObject template, string name);
}