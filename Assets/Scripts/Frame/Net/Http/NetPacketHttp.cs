using System;

public class NetPacketHttp : NetPacket
{
	protected string mURL;
	public virtual string write() { return null; }
	public virtual void read(ref string message) { }
	public string getUrl() { return mURL; }
	public override void resetProperty()
	{
		base.resetProperty();
		// mURL不需要重置，在子类的构造中赋值
		// mURL = null;
	}
}