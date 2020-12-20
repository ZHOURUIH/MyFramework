using UnityEngine;
using System.Collections.Generic;

public class PrefabLoadParam : IClassObject
{
	public CreateObjectCallback mCallback;
	public object mUserData;
	public int mTag;
	public void resetProperty()
	{
		mCallback = null;
		mUserData = null;
		mTag = 0;
	}
}