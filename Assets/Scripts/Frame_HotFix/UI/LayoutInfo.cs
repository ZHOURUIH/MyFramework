using System;
using System.Collections.Generic;

// 加载布局时传递的参数信息
public class LayoutInfo : ClassObject
{
	protected List<GameLayoutCallback> mCallbackList;	// 回调列表
	public LAYOUT_ORDER mOrderType;		// 显示顺序类型
	public Type mType;					// 布局脚本类型
	public string mName;				// 布局名字
	public bool mIsScene;				// 是否为场景布局,场景布局不会挂在UGUIRoot下面
	public int mRenderOrder;            // 显示顺序
	public override void resetProperty()
	{
		base.resetProperty();
		mCallbackList?.Clear();
		mOrderType = LAYOUT_ORDER.AUTO;
		mType = null;
		mName = null;
		mIsScene = false;
		mRenderOrder = 0;
	}
	public void addCallback(GameLayoutCallback callback)
	{
		mCallbackList ??= new();
		mCallbackList.Add(callback);
	}
	public void callAll(GameLayout layout)
	{
		if (mCallbackList.isEmpty())
		{
			return;
		}
		// 需要先拷贝一份,因为在回调过程中可能会修改当前对象
		using var a = new ListScope<GameLayoutCallback>(out var tempList, mCallbackList);
		mCallbackList.Clear();
		foreach (GameLayoutCallback callback in tempList)
		{
			callback?.Invoke(layout);
		}
	}
}