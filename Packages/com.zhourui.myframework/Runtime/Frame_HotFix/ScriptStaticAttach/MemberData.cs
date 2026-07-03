using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseUtility;
using static StringUtility;
using static FrameUtility;

[Serializable]
public class MemberData
{
	public static List<string> mWindowTypeDropList = new()
	{
		"窗口",
		"通用控件",
		"子页面",
		"无限滚动列表",
		"对象池",
	};
	public static List<string> mArrayTypeDropList = new()
	{
		"非数组",
		"静态数组",
		"动态数组",
	};
	public GameObject mObject;			// 对应的GameObject,并不会真正使用,只是用于获取名字
	public string mType;				// 变量类型
	public ARRAY_TYPE mArrayType;		// 数组类型
	public int mArrayLength;			// 如果是数组,数组的长度
	public WINDOW_TYPE mWindowType;		// 窗口类型
	public GameObject mViewportObject;	// 滚动列表的Viewport节点,也是滚动列表自身所在的节点
	public GameObject mPoolTemplate;	// 对象池中所有节点的模板节点
	public string mTemplateWindowType;	// 模板节点的窗口自身类型,也就是模板对象的根节点类型,一般是myUGUIObject,有时候也可能是别的类型
	public string mParam0;				// 参数名0,也是一个类型名
	public string mParam1;				// 参数名1,也是一个类型名
	public string mCustomName;			// 使用自定义变量名时输入的名字
	public bool mHideError;				// 是否会隐藏运行时的创建错误,比如重复创建,或者找不到GameObject等错误
	public bool mUseCustomName;			// 是否使用自定义名字,而不是自动获取节点名作为变量名
	public bool mRegisterCollider;		// 是否需要注册碰撞事件,如果是true,会在生成的代码中调用registeCollider方法注册碰撞事件
	public bool mHasClickEvent;			// 是否只注册碰撞事件,不注册点击事件
	public GameObject getParentObject()
	{
		Transform parent = null;
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			mWindowType == WINDOW_TYPE.COMMON_CONTROL ||
			mWindowType == WINDOW_TYPE.SUB_UI)
		{
			parent = mObject != null ? mObject.transform.parent : null;
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
	public void setType<T>()
	{
		setType(typeof(T).ToString());
	}
	public void setType(string type)
	{
		mType = type;
		checkRegisterCollider();
	}
	public string getGameObjectName() 
	{
		return mObject != null ? mObject.name : "";
	}
	public bool isValid()
	{
		// 只有普通窗口、通用控件和子页面类型的成员需要关联GameObject,其他类型的成员不需要关联GameObject
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			mWindowType == WINDOW_TYPE.COMMON_CONTROL ||
			mWindowType == WINDOW_TYPE.SUB_UI)
		{
			return mObject != null;
		}
		return true;
	}
	public string getMemberName()
	{
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			mWindowType == WINDOW_TYPE.COMMON_CONTROL)
		{
			return mObject != null ? mObject.name : "";
		}
		if (mWindowType == WINDOW_TYPE.SUB_UI)
		{
			if (mUseCustomName)
			{
				return mCustomName;
			}
			return mObject != null ? mObject.name : "";
		}
		if (mWindowType == WINDOW_TYPE.SCROLL_LIST)
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
		if (mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			mWindowType == WINDOW_TYPE.COMMON_CONTROL ||
			mWindowType == WINDOW_TYPE.SUB_UI)
		{
			return mType;
		}
		if (mWindowType == WINDOW_TYPE.SCROLL_LIST)
		{
			return mType + "<" + mParam0 + ", " + mParam0 + ".Data>";
		}
		if (mWindowType == WINDOW_TYPE.POOL)
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
		checkRegisterCollider();
	}
	public void autoSetArrayLength()
	{
		if (mObject == null)
		{
			return;
		}
		GameObject parent = mObject.transform.parent.gameObject;
		string preName = mObject.name.removeEndString("0");
		for (int j = 0; j < 1000; ++j)
		{
			if (findGameObject(preName + j, parent) == null)
			{
				mArrayLength = j;
				break;
			}
		}
	}
	public void setArrayType(ARRAY_TYPE type)
	{
		mArrayType = type;
		// 数组类型的成员不适合自动注册事件,因为情况太复杂了
		if (mArrayType != ARRAY_TYPE.NONE)
		{
			mRegisterCollider = false;
			mHasClickEvent = false;
		}
		if (mArrayType == ARRAY_TYPE.STATIC_ARRAY)
		{
			autoSetArrayLength();
		}
		checkRegisterCollider();
	}
	public bool setObject(GameObject go, UGUIGeneratorBase generator)
	{
		if (go == mObject)
		{
			return false;
		}
		if (go == generator.gameObject)
		{
			Debug.LogError("不能添加根节点");
			go = null;
			return false;
		}
		if (generator.mMemberList.Exists((obj) => { return obj.mObject == go && go != null; }))
		{
			Debug.LogError("节点" + go.name + "已经在列表中了,不能重复添加");
			mObject = null;
			return false;
		}
		mObject = go;
		if (mObject == null)
		{
			return true;
		}
		// 如果是以0结尾的,就自动设置为静态数组类型的,且自动查找数组长度
		string name = mObject.name;
		if (getLastNotNumberPos(name) == name.Length - 2 && name.endWith("0"))
		{
			setArrayType(ARRAY_TYPE.STATIC_ARRAY);
		}
		if (mObject.TryGetComponent<UGUISubGenerator>(out _))
		{
			setWindowType(WINDOW_TYPE.SUB_UI);
			setType(getClassNameFromGameObject(mObject));
		}
		// 简单判断一下有可能设置的类型,比如如果名字带Checkbox,则可能是UGUICheckbox
		if (name.Contains("Checkbox"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<UGUICheckbox>();
		}
		else if (name.Contains("Tab"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<UGUITab>();
		}
		else if (name.Contains("Progress"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<UGUIProgress>();
		}
		// 如果拖拽的节点下只有一个Text节点,则可默认设置为LegendButton
		else if (name.Contains("Button") ||
			(mObject.transform.childCount == 1 && mObject.transform.GetChild(0).name == "Text"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<LegendButton>();
		}
		else if (name.Contains("Slider"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<UGUISlider>();
		}
		else if (name.Contains("DropList") || name.Contains("Dropdown"))
		{
			setWindowType(WINDOW_TYPE.COMMON_CONTROL);
			setType<UGUIDropList>();
		}
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkRegisterCollider()
	{
		if (mWindowType == WINDOW_TYPE.COMMON_CONTROL)
		{
			// 只有非数组的LegendButton,TabItem,UGUICheckbox类型的成员才适合自动注册事件,因为其他类型的成员情况太复杂了,不适合自动注册事件
			if ((mType == typeof(LegendButton).ToString() || 
				mType == typeof(UGUITab).ToString() || 
				mType == typeof(UGUICheckbox).ToString())
				&& mArrayType == ARRAY_TYPE.NONE)
			{
				mRegisterCollider = true;
				mHasClickEvent = true;
			}
			else
			{
				mRegisterCollider = false;
				mHasClickEvent = false;
			}
		}
		// 除了上面的情况,其他除了普通窗口外,均不支持自动注册点击事件
		else if (mWindowType != WINDOW_TYPE.NORMAL_WINDOW)
		{
			mRegisterCollider = false;
			mHasClickEvent = false;
		}
	}
}