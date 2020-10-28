using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class myUGUIDropdown : myUGUIObject
{
	protected Dropdown mDropdown;
	public override void init()
	{
		base.init();
		mDropdown = mObject.GetComponent<Dropdown>();
		if (mDropdown == null)
		{
			mDropdown = mObject.AddComponent<Dropdown>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
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