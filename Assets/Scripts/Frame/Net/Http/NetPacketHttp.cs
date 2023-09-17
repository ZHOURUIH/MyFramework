using System;

// 表示一个与Http服务器交互的消息基类
public class NetPacketHttp : NetPacket
{
	protected string mURL;		// 接口地址
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