using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MemberData
{
	public GameObject mObject;
	public string mType;
	public int mDefaultActive;	// 默认是否激活,0标识保持prefab中的状态,1标识设置为激活,2标识设置为不激活
	public bool mIsArray;		// 是否为数组
	public int mArrayLength;	// 如果是数组,数组的长度
}

public class UGUIGenerator : MonoBehaviour
{
	public List<MemberData> mMemberList = new();	// 需要访问的节点列表
	public bool mIsPersistent;						// 是否为常驻界面
	public void addNewItem()
	{
		MemberData data = new();
		data.mType = typeof(myUGUIObject).ToString();
		mMemberList.Add(data);
	}
}