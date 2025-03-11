
public interface IRecycleable
{
	// 在回收时调用
	public void recycle();
	public void setAssignID(long assignID);
	public long getAssignID();
}