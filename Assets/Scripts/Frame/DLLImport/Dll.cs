using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

public class Dll : GameBase
{
	protected Dictionary<string, Delegate> mFunctionList;
	protected IntPtr mHandle;
	protected string mLibraryName;
	public void init(string name)
	{
		mLibraryName = name;
		mFunctionList = new Dictionary<string, Delegate>();
#if UNITY_STANDALONE_WIN
		mHandle = Kernel32.LoadLibrary(CommonDefine.F_PLUGINS_PATH + mLibraryName);
#endif
	}
	public void destroy()
	{
#if UNITY_STANDALONE_WIN
		Kernel32.FreeLibrary(mHandle);
#endif
		mFunctionList = null;
	}
	public string getName() { return mLibraryName; }
	public Delegate getFunction(string funcName, Type t)
	{
#if UNITY_STANDALONE_WIN
		if (!mFunctionList.ContainsKey(funcName))
		{
			IntPtr api = Kernel32.GetProcAddress(mHandle, funcName);
			if(api == IntPtr.Zero)
			{
				logError("can not find function, name : " + funcName);
				return null;
			}
			Delegate dele = Marshal.GetDelegateForFunctionPointer(api, t);
			mFunctionList.Add(funcName, dele);
		}
		return mFunctionList[funcName];
#else
		return null;
#endif
	}
}