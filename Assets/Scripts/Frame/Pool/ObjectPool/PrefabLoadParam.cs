using System;

public class PrefabLoadParam : GameBasePooledObject
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