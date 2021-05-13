using UnityEngine;
using System;
using System.Collections.Generic;

public class AtlasLoadParam : FrameBase
{
	public AtlasLoadDone mCallback;
	public object mUserData;
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mUserData = null;
	}
}