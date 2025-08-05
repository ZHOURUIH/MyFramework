using System.Collections.Generic;
using UnityEngine;

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
}