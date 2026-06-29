using Newtonsoft.Json;

// 带模板类型的http消息类
public class NetPacketHttpT<CSBody, SCBody> : NetPacketHttp where CSBody : IResetProperty, new()
{
	public SCBody mBody;
	public CSBody mSendBody = new();
	public override void read(string message) { mBody = JsonConvert.DeserializeObject<SCBody>(message); }
	public override string write() { return JsonConvert.SerializeObject(mSendBody); }
	public override void resetProperty()
	{
		base.resetProperty();
		mSendBody.resetProperty();
		mBody = default;
	}
}