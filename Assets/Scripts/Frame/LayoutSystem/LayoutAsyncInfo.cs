using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct LayoutAsyncInfo
{
	public LayoutAsyncDone mCallback;
	public GameLayout mLayout;
	public LAYOUT_ORDER mOrderType;
	public GUI_TYPE mGUIType;
	public string mName;
	public bool mIsScene;
	public int mRenderOrder;
	public int mID;
}