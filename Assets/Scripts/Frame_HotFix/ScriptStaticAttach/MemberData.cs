﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseUtility;

public enum WINDOW_TYPE : byte
{
	NORMAL_WINDOW,			// 单独的窗口
	COMMON_SUB_UI,			// 通用的控件
	SUB_PANEL,				// 子页面
	SCROLL_LIST,			// 滚动列表
	POOL,					// 对象池
}

public enum ARRAY_TYPE : byte
{
	NONE,					// 不是数组
	STATIC_ARRAY,			// 静态数组,就是直接获取界面上已经存在的节点存放到数组中
	DYNAMIC_ARRAY,			// 动态数组,就是根据一个模板创建多个节点放到数组中,类似对象池,但是创建的是非对象池类型的节点,动态列表不支持单独的窗口类型
}

[Serializable]
public class MemberData
{
	public static List<string> mWindowTypeDropList = new() 
	{
		"窗口",
		"通用控件",
		"子页面",
		"滚动列表",
		"对象池",
	};
	public static List<string> mArrayTypeDropList = new()
	{
		"非数组",
		"静态数组",
		"动态数组",
	};
	public GameObject mObject;          // 对应的GameObject,并不会真正使用,只是用于获取名字
	public string mType;				// 变量类型
	public ARRAY_TYPE mArrayType;		// 数组类型
	public int mArrayLength;			// 如果是数组,数组的长度
	public WINDOW_TYPE mWindowType;     // 窗口类型
	public GameObject mViewportObject;	// 滚动列表的Viewport节点,也是滚动列表自身所在的节点
	public GameObject mPoolTemplate;    // 对象池中所有节点的模板节点
	public string mTemplateWindowType;	// 模板节点的窗口自身类型,也就是模板对象的根节点类型,一般是myUGUIObject,有时候也可能是别的类型
	public string mParam0;				// 参数名0,也是一个类型名
	public string mParam1;              // 参数名1,也是一个类型名
	public string mCustomName;			// 使用自定义变量名时输入的名字
	public bool mHideError;             // 是否会隐藏运行时的创建错误,比如重复创建,或者找不到GameObject等错误
	public bool mUseCustomName;			// 是否使用自定义名字,而不是自动获取节点名作为变量名
	public GameObject getParentObject()
	{
		Transform parent = null;
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			mWindowType == WINDOW_TYPE.COMMON_SUB_UI ||
			mWindowType == WINDOW_TYPE.SUB_PANEL)
		{
			parent = mObject.transform.parent;
		}
		else if (mWindowType == WINDOW_TYPE.SCROLL_LIST)
		{
			parent = mViewportObject.transform.parent;
		}
		else if (mWindowType == WINDOW_TYPE.POOL)
		{
			parent = mPoolTemplate.transform.parent;
		}
		return parent != null ? parent.gameObject : null;
	}
	public string getMemberName()
	{
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
		{
			return mObject.name;
		}
		else if (mWindowType == WINDOW_TYPE.COMMON_SUB_UI)
		{
			return mObject.name;
		}
		else if (mWindowType == WINDOW_TYPE.SUB_PANEL)
		{
			return mObject.name;
		}
		else if (mWindowType == WINDOW_TYPE.SCROLL_LIST)
		{
			if (mUseCustomName)
			{
				return mCustomName;
			}
			if (mPoolTemplate != null)
			{
				return mPoolTemplate.name + "List";
			}
		}
		else if (mWindowType == WINDOW_TYPE.POOL)
		{
			if (mUseCustomName)
			{
				return mCustomName;
			}
			if (mPoolTemplate != null)
			{
				return mPoolTemplate.name + "Pool";
			}
		}
		return "";
	}
	public string getTypeName()
	{
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
		{
			return mType;
		}
		else if (mWindowType == WINDOW_TYPE.COMMON_SUB_UI)
		{
			return mType;
		}
		else if (mWindowType == WINDOW_TYPE.SUB_PANEL)
		{
			return mType;
		}
		else if (mWindowType == WINDOW_TYPE.SCROLL_LIST)
		{
			return mType + "<" + mParam0 + ", " + mParam0 + ".Data>";
		}
		else if (mWindowType == WINDOW_TYPE.POOL)
		{
			if (mType == "WindowStructPoolMap")
			{
				return mType + "<" + mParam0 + ", " + mParam1 + ">";
			}
			else
			{
				return mType + "<" + mParam0 + ">";
			}
		}
		return "";
	}
	public void setWindowType(WINDOW_TYPE type)
	{
		if (mWindowType == type)
		{
			return;
		}
		mWindowType = type;
		// 不同的类型只需要不同的参数,不匹配类型的参数需要清掉
		if (mWindowType == WINDOW_TYPE.SCROLL_LIST || mWindowType == WINDOW_TYPE.POOL)
		{
			mObject = null;
			mTemplateWindowType = typeof(myUGUIObject).ToString();
		}
		else
		{
			mPoolTemplate = null;
			mParam0 = null;
			mParam1 = null;
		}
		if (mWindowType != WINDOW_TYPE.SCROLL_LIST)
		{
			mViewportObject = null;
		}
	}
	public void autoSetArrayLength()
	{
		GameObject parent = mObject.transform.parent.gameObject;
		string preName = mObject.name.removeEndString("0");
		for (int j = 0; j < 1000; ++j)
		{
			if (getGameObject(preName + j, parent) == null)
			{
				mArrayLength = j;
				break;
			}
		}
	}
}