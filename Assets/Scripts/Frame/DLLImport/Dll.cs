using System;
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
	public T getFunction<T>(string funcName, Type t) where T : Delegate
	{
#if UNITY_STANDALONE_WIN
		if (!mFunctionList.TryGetValue(funcName, out Delegate value))
		{
			IntPtr api = Kernel32.GetProcAddress(mHandle, funcName);
			if(api == IntPtr.Zero)
			{
				logError("can not find function, name : " + funcName);
				return default(T);
			}
			value = Marshal.GetDelegateForFunctionPointer(api, t);
			mFunctionList.Add(funcName, value);
		}
		return value as T;
#else
		return default(T);
#endif
	}
}