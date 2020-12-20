using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

public class Dll : FrameBase
{
	protected Dictionary<string, Delegate> mFunctionList;
	protected IntPtr mHandle;
	protected string mLibraryName;
	public void init(string name)
	{
		mLibraryName = name;
		mFunctionList = new Dictionary<string, Delegate>();
#if UNITY_STANDALONE_WIN
		mHandle = Kernel32.LoadLibrary(FrameDefine.F_PLUGINS_PATH + mLibraryName);
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
		if (!mFunctionList.TryGetValue(funcName, out Delegate value))
		{
			IntPtr api = Kernel32.GetProcAddress(mHandle, funcName);
			if(api == IntPtr.Zero)
			{
				logError("can not find function, name : " + funcName);
				return null;
			}
			value = Marshal.GetDelegateForFunctionPointer(api, t);
			mFunctionList.Add(funcName, value);
		}
		return value;
#else
		return null;
#endif
	}
}