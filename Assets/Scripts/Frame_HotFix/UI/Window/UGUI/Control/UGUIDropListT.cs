using System.Collections.Generic;

// 自定义类型T的下拉列表
public class UGUIDropListT<T> : UGUIDropListBase where T : WindowObjectBase, IDropItem, IRecyclable
{
	protected WindowStructPool<T> mItemPool; // 显示项的对象池
	public UGUIDropListT(IWindowObjectOwner parent) : base(parent)
	{
		mItemPool = new(this);
	}
	protected override void assignWindowInternal()
	{
		base.assignWindowInternal();
		mItemPool.assignTemplate(mContent, "Template");
	}
	public override void clearOptions()
	{
		base.clearOptions();
		mItemPool.unuseAll();
	}
	protected override IDropItem getSelectByIndex(int index) { return mItemPool.getUsedList().get(index); }
	protected override int getIndexOfItem(IDropItem item) { return mItemPool.getUsedList().IndexOf(item as T); }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void createAllItem(List<string> options, List<int> customValue)
	{
		mItemPool.unuseAll();
		int count = options.Count;
		for (int i = 0; i < count; ++i)
		{
			T item = mItemPool.newItem();
			item.setText(options[i]);
			if (customValue != null)
			{
				item.setCustomValue(customValue[i]);
			}
			item.setParent(this);
		}
	}
}