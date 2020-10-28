using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DllImportExtern : FrameSystem
{
	protected static Dictionary<string, Dll> mDllLibraryList;
	public DllImportExtern()
	{
		mDllLibraryList = new Dictionary<string, Dll>();
	}
	//将要执行的函数转换为委托
	public static Delegate Invoke(string library, string funcName, Type t)
	{
		if (mDllLibraryList.ContainsKey(library))
		{
			return mDllLibraryList[library].getFunction(funcName, t);
		}
		return null;
	}
	public override void init()
	{
		base.init();
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
		registerDLL(Winmm.WINMM_DLL);
#endif
	}
	public override void destroy()
	{
		foreach (var library in mDllLibraryList)
		{
			library.Value.destroy();
		}
		mDllLibraryList.Clear();
		base.destroy();
	}
	protected void registerDLL(string name)
	{
		Dll dll = new Dll();
		dll.init(name);
		mDllLibraryList.Add(dll.getName(), dll);
	}
}