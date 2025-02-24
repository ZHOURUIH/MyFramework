using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using static StringUtility;
using static WidgetUtility;
using static FrameBaseHotFix;
using static UnityUtility;
using static MathUtility;

// 自定义的下拉列表
public class UGUIDropList : WindowObjectUGUI
{
	protected WindowStructPool<DropItem> mItemPool;	// 显示项的对象池
	protected UnityAction mSelectCallback;			// 选项切换时的回调
	protected myUGUIObject mMask;					// 点击遮罩,用于点击空白处关闭下拉列表
	protected myUGUIText mLabel;					// 显示当前选项的文本
	protected myUGUIScrollRect mOptions;			// 所有选项的下拉框
	protected myUGUIObject mViewport;				// 所有选项的父节点
	protected myUGUIObject mContent;				// 所有选项的父节点
	protected myUGUIObject mTemplate;				// 选项的模板
	protected DropItem mCurSelect;					// 当前选中的选项
	protected int mMaxViewportLength;				// 下拉列表显示的最大长度,超过此长度就需要滑动才能看到,小于此长度就全部显示
	protected int mSelectIndex;                     // 当前选中的下标
	public UGUIDropList(LayoutScript script)
		: base(script) 
	{
		mItemPool = new(script);
	}
	public override void assignWindow(myUIObject parent, string name)
	{
		base.assignWindow(parent, name);
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
		mScript.registeScrollRect(mOptions, mViewport, mContent);
		mOptions.setActive(false);
		mMask.setActive(false);
		// 确认选项的父节点拥有Canvas组件,可以渲染在所有节点之上
		mOptions.tryGetUnityComponent(out Canvas optionsCanvas);
		if (optionsCanvas == null)
		{
			logError("下拉列表框的Options节点需要拥有Canvas组件");
		}
		else
		{
			optionsCanvas.overrideSorting = true;
		}
		
		mOptions.tryGetUnityComponent(out GraphicRaycaster raycaster);
		if (raycaster == null)
		{
			logError("下拉列表框的Options节点需要拥有GraphicRaycaster组件");
		}
		mMaxViewportLength = (int)mOptions.getWindowSize().y;
	}
	public void clearOptions() 
	{
		mSelectIndex = 0;
		mItemPool.unuseAll(); 
	}
	public void setSelectCallback(UnityAction callback) { mSelectCallback = callback; }
	public void setOptions(List<string> options, List<int> customValue = null, bool triggerEvent = true) 
	{
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
		setSelect(0, triggerEvent);
		int viewportLength = clampMax((int)mContent.getWindowSize().y, mMaxViewportLength);
		mViewport.setWindowHeight(viewportLength);
		mOptions.setWindowHeight(viewportLength);
		mViewport.setPosition(Vector3.zero);
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
	public void setMaxViewportLegth(int length) { mMaxViewportLength = length; }
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