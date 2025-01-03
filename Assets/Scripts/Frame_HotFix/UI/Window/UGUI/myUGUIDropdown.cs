using UnityEngine.UI;
using System.Collections.Generic;

// 封装的UGUI的Dropdown下拉列表
public class myUGUIDropdown : myUGUIObject
{
	protected Dropdown mDropdown;	// UGUI的Dropdown组件
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mDropdown))
		{
			mDropdown = mObject.AddComponent<Dropdown>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public Dropdown getDropdown() { return mDropdown; }
	public void clearOptions() { mDropdown.ClearOptions(); }
	public void addOptions(List<string> opstions) { mDropdown.AddOptions(opstions); }
	public void setSelect(int value) { mDropdown.value = value; }
	public int getSelect() { return mDropdown.value; }
	public string getText() { return mDropdown.options[mDropdown.value].text; }
}