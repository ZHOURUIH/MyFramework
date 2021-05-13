using System;

public class PrefabLoadParam : FrameBase
{
	public CreateObjectCallback mCallback;
	public object mUserData;
	public int mTag;
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mUserData = null;
		mTag = 0;
	}
}