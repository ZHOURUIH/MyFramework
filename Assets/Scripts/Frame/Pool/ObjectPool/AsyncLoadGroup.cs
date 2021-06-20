using UnityEngine;
using System.Collections.Generic;
using System;

public class AsyncLoadGroup : FrameBase
{
	public Dictionary<string, GameObject> mNameList;
	public CreateObjectGroupCallback mCallback;
	public object mUserData;
	public AsyncLoadGroup()
	{
		mNameList = new Dictionary<string, GameObject>();
	}
	public bool isAllLoaded()
	{
		foreach (var item in mNameList)
		{
			if (item.Value == null)
			{
				return false;
			}
		}
		return true;
	}
	public void activeAll()
	{
		foreach (var item in mNameList)
		{
			item.Value.SetActive(true);
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mNameList.Clear();
		mCallback = null;
		mUserData = null;
	}
}