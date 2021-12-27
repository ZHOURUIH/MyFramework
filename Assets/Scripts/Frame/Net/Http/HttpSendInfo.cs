using System;
using System.Collections.Generic;

// Http消息的发送信息
public class HttpSendInfo : FrameBase
{
	public string mMessage;		// 发送的消息内容
	public string mUrl;			// 发送的地址
	public ushort mType;		// 发送的消息类型
	public override void resetProperty()
	{
		base.resetProperty();
		mMessage = null;
		mType = 0;
		mUrl = null;
	}
}