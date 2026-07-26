using Newtonsoft.Json;

// 带模板类型的http消息类
// 使用泛型参数CSBody(请求体)和SCBody(响应体),自动完成JSON序列化与反序列化
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