using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 添加abstract的作用是不允许直接挂这个脚本到GameObject上,需要挂子类
public abstract class UGUIGeneratorBase : MonoBehaviour
{
	public List<MemberData> mMemberList = new();    // 需要访问的节点列表
	public void addNewItem()
	{
		mMemberList.Add(new());
	}
	public void addNewPool()
	{
		MemberData data = new();
		data.mWindowType = WINDOW_TYPE.POOL;
		mMemberList.Add(data);
	}
	public void addScrollList()
	{
		MemberData data = new();
		data.mWindowType = WINDOW_TYPE.SCROLL_LIST;
		mMemberList.Add(data);
	}
	// 检查所有的节点是否合法,也就是确认都是当前节点的子节点
	public bool checkMembers()
	{
		bool isValid = true;
		foreach (MemberData data in mMemberList)
		{
			if (data.mObject != null && !isTransformChild(transform, data.mObject.transform))
			{
				logError("设置的节点错误,不属于当前的子节点,name:" + data.mObject.name);
				isValid = false;
			}
		}
		return isValid;
	}
}