
// 表示一个与服务器的连接
public class NetConnect : CommandReceiver
{
	protected string mLabel;		// 服务器名字
	public void setLabel(string label) { mLabel = label; }
	public string getLabel() { return mLabel; }
	public override void resetProperty()
	{
		base.resetProperty();
		mLabel = null;
	}
}