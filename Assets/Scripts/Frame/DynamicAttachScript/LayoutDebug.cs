using System;
using System.Collections.Generic;
using UnityEngine;

public class LayoutDebug : MonoBehaviour
{
	protected GameLayout mLayout;
	public LAYOUT Type;                 // 布局类型
	public string Name;                 // 布局名称
	public bool ScriptControlHide;      // 是否由脚本来控制隐藏
	public bool IsNGUI;                 // 是否为NGUI布局,true为NGUI,false为UGUI
	public bool IsScene;                // 是否为场景,如果是场景,就不将布局挂在NGUIRoot或者UGUIRoot下
	public bool CheckBoxAnchor;         // 是否检查布局中所有带碰撞盒的窗口是否自适应分辨率
	public bool IgnoreTimeScale;        // 更新布局时是否忽略时间缩放
	public bool BlurBack;               // 布局显示时是否需要使布局背后(比当前布局层级低)的所有布局模糊显示
	public bool AnchorApplied;          // 是否已经完成了自适应的调整
	public int RenderOrder;             // 渲染顺序,越大则渲染优先级越高
	public int DefaultLayer;            // 布局加载时所处的层
	public void setLayout(GameLayout layout) { mLayout = layout; }
	public void Update()
	{
		Type = mLayout.getType();
		Name = mLayout.getName();
		ScriptControlHide = mLayout.isScriptControlHide();
		IsNGUI = mLayout.isNGUI();
		IsScene = mLayout.isScene();
		CheckBoxAnchor = mLayout.isCheckBoxAnchor();
		IgnoreTimeScale = mLayout.isIgnoreTimeScale();
		BlurBack = mLayout.isBlurBack();
		AnchorApplied = mLayout.isAnchorApplied();
		RenderOrder = mLayout.getRenderOrder();
		DefaultLayer = mLayout.getDefaultLayer();
	}
}