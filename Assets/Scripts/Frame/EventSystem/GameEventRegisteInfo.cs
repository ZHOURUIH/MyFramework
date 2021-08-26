using System;

// 游戏事件的注册信息
public class GameEventRegisteInfo : FrameBase
{
	public EventCallback mCallback;		// 监听事件的回调函数
	public object mLisntener;			// 监听者
	public int mEventType;				// 事件ID
	public override void resetProperty()
	{
		base.resetProperty();
		mLisntener = null;
		mCallback = null;
		mEventType = 0;
	}
}