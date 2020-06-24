using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 状态组,状态组可以指定哪些状态是互斥的,如果是互斥的,则添加属于该组的状态时,会移除已有的该组中的所有状态
public class StateGroup : GameBase
{
	public GROUP_MUTEX_OPERATION mCoexist;		// 该组中的状态是否可以共存
	public List<Type> mStateList;
	public Type mMainState;
	public StateGroup()
	{	
		mStateList = new List<Type>();
	}
	public void setCoexist(GROUP_MUTEX_OPERATION coexist) { mCoexist = coexist; }
	public void setMainState<T>() where T : PlayerState
	{
		if(mMainState != null)
		{
			logError("state group's main state is not empty!");
			return;
		}
		mMainState = typeof(T);
	}
	public void addState<T>() where T : PlayerState
	{
		mStateList.Add(typeof(T));
	}
	public bool hasState<T>() where T : PlayerState
	{
		return mStateList.Contains(typeof(T));
	}
}