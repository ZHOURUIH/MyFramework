using System;

public class GameEventInfo : IClassObject
{
	public IEventListener mLisntener;
	public EventCallback mCallback;
	public int mType;
	public void resetProperty()
	{
		mLisntener = null;
		mCallback = null;
		mType = 0;
	}
}