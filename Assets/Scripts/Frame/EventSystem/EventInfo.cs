using System;

public class GameEventInfo : FrameBase
{
	public object mLisntener;
	public EventCallback mCallback;
	public int mType;
	public override void resetProperty()
	{
		base.resetProperty();
		mLisntener = null;
		mCallback = null;
		mType = 0;
	}
}