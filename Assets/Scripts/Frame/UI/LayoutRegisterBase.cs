using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 布局注册类的基类,用于提供一些基础的工具函数
public class LayoutRegisterBase : FrameBase
{
	protected static void registeLayout<T>(int layout, string name) where T : LayoutScript
	{
		registeLayout<T>(layout, name, EMPTY, false, LAYOUT_LIFE_CYCLE.PART_USE);
	}
	protected static void registeLayout<T>(int layout, string name, LAYOUT_LIFE_CYCLE lifeCycle) where T : LayoutScript
	{
		registeLayout<T>(layout, name, EMPTY, false, lifeCycle);
	}
	protected static void registeLayout<T>(int layout, string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle) where T : LayoutScript
	{
		registeLayout<T>(layout, name, EMPTY, inResource, lifeCycle);
	}
	protected static void registeLayout<T>(int layout, string name, string prePath, bool inResource) where T : LayoutScript
	{
		registeLayout<T>(layout, name, prePath, inResource, LAYOUT_LIFE_CYCLE.PART_USE);
	}
	protected static void registeLayout<T>(int layout, string name, bool inResource) where T : LayoutScript
	{
		registeLayout<T>(layout, name, EMPTY, inResource, LAYOUT_LIFE_CYCLE.PART_USE);
	}
	protected static void registeLayout<T>(int layout, string name, string prePath, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle) where T : LayoutScript
	{
		mLayoutManager.registeLayout(typeof(T), layout, prePath + name + "/" + name, inResource, lifeCycle);
	}
	protected static bool assign<T>(ref T thisScript, LayoutScript value, bool created) where T : LayoutScript
	{
		if (typeof(T) == value.GetType())
		{
			thisScript = created ? value as T : null;
			return true;
		}
		return false;
	}
}