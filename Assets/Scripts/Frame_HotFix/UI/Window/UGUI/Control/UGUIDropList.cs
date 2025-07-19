using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StringUtility;
using static WidgetUtility;
using static FrameBaseHotFix;
using static UnityUtility;

// 自定义的下拉列表
public class UGUIDropList : WindowObjectUGUI
{
	protected WindowStructPool<DropItem> mItemPool;	// 显示项的对象池
	protected Action mSelectCallback;				// 选项切换时的回调
	protected myUGUIObject mMask;                   // 点击遮罩,用于点击空白处关闭下拉列表
#if USE_TMP
	protected myUGUITextTMP mLabel;                 // 显示当前选项的文本
#else
	protected myUGUIText mLabel;					// 显示当前选项的文本
#endif
	protected myUGUIObject mOptions;				// 所有选项的下拉框
	protected myUGUIObject mViewport;				// 所有选项的父节点
	protected myUGUIDragView mContent;				// 所有选项的父节点
	protected myUGUIObject mTemplate;				// 选项的模板
	protected DropItem mCurSelect;					// 当前选中的选项
	protected int mSelectIndex;                     // 当前选中的下标
	public UGUIDropList(IWindowObjectOwner parent) : base(parent)
	{
		mItemPool = new(this);
	}
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
		newObject(out mLabel, "Label");
		newObject(out mOptions, "Options");
		newObject(out mMask, mOptions, "Mask");
		newObject(out mViewport, mOptions, "Viewport");
		newObject(out mContent, mViewport, "Content");
		newObject(out mTemplate, mContent, "Template", 0);
	}
	public override void init()
	{
		base.init();
		mItemPool.init(mContent, mTemplate);
		mLabel.registeCollider(onClick);
		mMask.registeCollider(onMaskClick);
		mContent.initDragView(mViewport, DRAG_DIRECTION.VERTICAL);
		mOptions.setActive(false);
		mMask.setActive(false);
		// 确认选项的父节点拥有Canvas组件,可以渲染在所有节点之上
		if (mOptions.tryGetUnityComponent(out Canvas optionsCanvas))
		{
			optionsCanvas.overrideSorting = true;
		}
		else
		{
			logError("下拉列表框的Options节点需要拥有Canvas组件");
		}
		if (!mOptions.tryGetUnityComponent<GraphicRaycaster>(out _))
		{
			logError("下拉列表框的Options节点需要拥有GraphicRaycaster组件");
		}
	}
	public void clearOptions() 
	{
		mSelectIndex = 0;
		mItemPool.unuseAll(); 
	}
	public void setSelectCallback(Action callback) { mSelectCallback = callback; }
	public void setOptions(List<string> options, List<int> customValue = null, bool triggerEvent = true) 
	{
		if (!mInited)
		{
			logError("还未执行初始化,不能设置选项");
			return;
		}
		if (customValue != null && options.Count != customValue.Count)
		{
			logError("附加数据的数量与选项的数量不一致");
			return;
		}
		mItemPool.unuseAll();
		int count = options.Count;
		for (int i = 0; i < count; ++i)
		{
			DropItem item = mItemPool.newItem();
			item.setText(options[i]);
			if (customValue != null)
			{
				item.setCustomValue(customValue[i]);
			}
			item.setParent(this);
		}
		autoGridVertical(mContent);
		mContent.alignParentTopCenter();
		setSelect(0, triggerEvent);
	}
	public void setSelect(int value, bool triggerEvent = true) 
	{
		var usedList = mItemPool.getUsedList();
		if (value >= 0 && value < usedList.Count)
		{
			mSelectIndex = value;
			mCurSelect = usedList[mSelectIndex];
			mLabel.setText(mCurSelect.getText());
			if (triggerEvent)
			{
				mSelectCallback?.Invoke();
			}
		}
	}
	public int getSelect() { return mSelectIndex; }
	public string getSelectedText() { return mCurSelect?.getText() ?? EMPTY; }
	public int getSelectedCustomValue() { return mCurSelect?.getCustomValue() ?? 0; }
	public void dropItemClick(DropItem item)
	{
		setSelect(mItemPool.getUsedList().IndexOf(item));
		showOptions(false);
	}
	public void showOptions(bool show)
	{
		mOptions.setActive(show);
		mMask.setActive(show);
		// 每次显示下拉列表时,都需要重新计算一下显示深度
		if (show)
		{
			mOptions.getOrAddUnityComponent<Canvas>().sortingOrder = mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO);
			mOptions.getLayout().refreshUIDepth(mRoot, true);
			mContent.alignParentTop();
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onClick()
	{
		Vector3 labelPivot = mLabel.getPositionNoPivot();
		Vector3 labelSize = mLabel.getWindowSize();
		float labelLeftInParent = labelPivot.x - labelSize.x * 0.5f;
		float labelBottomInParent = labelPivot.y - labelSize.y * 0.5f;
		Vector2 size = mOptions.getWindowSize();
		mOptions.setPosition(new(labelLeftInParent + size.x * 0.5f, labelBottomInParent - size.y * 0.5f));
		showOptions(true);
	}
	protected void onMaskClick()
	{
		showOptions(false);
	}
}