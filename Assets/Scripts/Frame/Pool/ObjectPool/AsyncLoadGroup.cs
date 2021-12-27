using UnityEngine;
using System.Collections.Generic;
using System;

// 异步加载物体的组,用于同时异步加载多个对象,等待多个对象都加载完毕再通知回调
public class AsyncLoadGroup : FrameBase
{
	public Dictionary<string, GameObject> mNameList;	// 要加载的物体列表
	public CreateObjectGroupCallback mCallback;			// 创建完毕的回调
	public object mUserData;							// 回调函数的自定义参数
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