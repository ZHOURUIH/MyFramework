
// 表示一个与Http服务器交互的消息基类
public class NetPacketHttp : NetPacket
{
	protected string mURL;				// 接口地址
	protected HTTP_METHOD mMethod;		// 请求方式,默认post,在子类构造中赋值
	public NetPacketHttp()
	{
		mMethod = HTTP_METHOD.POST;
	}
	public virtual string write() { return null; }
	public virtual void read(string message) { }
	public string getUrl() { return mURL; }
	public HTTP_METHOD getMethod() { return mMethod; }
	public virtual int timeout() { return 10000; }
	public override void resetProperty()
	{
		base.resetProperty();
		// mURL,mMethod不需要重置，在子类的构造中赋值
		// mURL = null;
		// mMethod = HTTP_METHOD;
	}
	// http消息的子类不需要重写execute,而是直接传入lambda表达式
	public sealed override void execute() { }
}