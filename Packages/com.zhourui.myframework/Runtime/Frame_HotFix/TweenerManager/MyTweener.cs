
// 自定义的渐变类
public class MyTweener : ComponentOwner
{
	public void init()
	{
		initComponents();
	}
	public virtual bool isDoing() { return false; }
}