using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using static StringUtility;
using static WidgetUtility;
using static FrameBase;
using static UnityUtility;

// 自定义的下拉列表
public class UGUIDropList : WindowObjectUGUI
{
	protected WindowObjectPool<DropItem> mItemPool;
	protected UnityAction mSelectCallback;		// 选项切换时的回调
	protected myUGUIObject mMask;               // 点击遮罩,用于点击空白处关闭下拉列表
	protected myUGUIText mLabel;				// 显示当前选项的文本
	protected myUGUIObject mOptions;            // 所有选项的父节点
	protected myUGUIObject mTemplate;			// 选项的模板
	protected DropItem mCurSelect;              // 当前选中的选项
	protected int mSelectIndex;                 // 当前选中的下标
	public override void setScript(LayoutScript script)
	{
		base.setScript(script);
		mItemPool = new WindowObjectPool<DropItem>(script);
	}
	public override void assignWindow(myUIObject parent, string name)
	{
		base.assignWindow(parent, name);
		mScript.newObject(out mMask, mRoot, "Mask");
		mScript.newObject(out mLabel, mRoot, "Label");
		mScript.newObject(out mOptions, mRoot, "Options");
		mScript.newObject(out mTemplate, mOptions, "Template", 0);
	}
	public override void init()
	{
		base.init();
		mItemPool.init(mOptions, mTemplate, typeof(DropItem));
		mScript.registeCollider(mLabel, onClick);
		mScript.registeCollider(mMask, onMaskClick);
		mScript.registeCollider(mOptions);
		mOptions.setActive(false);
		mMask.setActive(false);
		// 确认选项的父节点拥有Canvas组件,可以渲染在所有节点之上
		Canvas optionsCanvas = mOptions.getUnityComponent<Canvas>();
		if (!optionsCanvas.overrideSorting)
		{
			logError("下拉列表框的选项父节点需要拥有Canvas组件,且overrideSorting为true");
		}
	}
	public void clearOptions() 
	{
		mSelectIndex = 0;
		mItemPool.unuseAll(); 
	}
	public void setSelectCallback(UnityAction callback) { mSelectCallback = callback; }
	public void setOptions(List<string> options, bool triggerEvent = true) 
	{
		mItemPool.unuseAll();
		for (int i = 0; i < options.Count; ++i)
		{
			DropItem item = mItemPool.newItem();
			item.setText(options[i]);
			item.setParent(this);
		}
		autoGridVertical(mOptions, true);
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
	public string getText() { return mCurSelect != null ? mCurSelect.getText() : EMPTY; }
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
			mOptions.getUnityComponent<Canvas>().sortingOrder = mLayoutManager.generateRenderOrder(null, 0, LAYOUT_ORDER.ALWAYS_TOP_AUTO);
			mOptions.getLayout().refreshUIDepth(mRoot, true);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onClick(IMouseEventCollect obj, Vector3 mousePos)
	{
		float labelLeftInParent = mLabel.getPositionNoPivot().x - mLabel.getWindowSize().x * 0.5f;
		float labelBottomInParent = mLabel.getPositionNoPivot().y - mLabel.getWindowSize().y * 0.5f;
		mOptions.setPosition(new Vector3(labelLeftInParent + mOptions.getWindowSize().x * 0.5f, labelBottomInParent - mOptions.getWindowSize().y * 0.5f));
		showOptions(true);
	}
	protected void onMaskClick(IMouseEventCollect obj, Vector3 mousePos)
	{
		showOptions(false);
	}
}