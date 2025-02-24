using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static FrameDefine;
using static FrameEditorUtility;
using static UnityUtility;
using static FileUtility;

// 表示一个通过Kernel32加载的动态库对象
public class Dll : ClassObject
{
	protected Dictionary<string, Delegate> mFunctionList = new();	// 已经获取的动态库中的函数列表
	protected IntPtr mHandle;										// 动态库句柄
	protected string mLibraryName;									// 动态库名字
	public void init(string name)
	{
		mLibraryName = name;
		if (isWindows() && isFileExist(F_PLUGINS_PATH + mLibraryName))
		{
			mHandle = Kernel32.LoadLibrary(F_PLUGINS_PATH + mLibraryName);
		}
	}
	public override void destroy()
	{
		base.destroy();
		if (isWindows() && mHandle != null && mHandle != IntPtr.Zero)
		{
			Kernel32.FreeLibrary(mHandle);
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mFunctionList.Clear();
		mHandle = IntPtr.Zero;
		mLibraryName = null;
	}
	public string getName() { return mLibraryName; }
	public T getFunction<T>(string funcName, Type t) where T : Delegate
	{
		if (isWindows())
		{
			if (!mFunctionList.TryGetValue(funcName, out Delegate value))
			{
				IntPtr api = Kernel32.GetProcAddress(mHandle, funcName);
				if (api == IntPtr.Zero)
				{
					logError("can not find function, name : " + funcName);
					return default;
				}
				value = Marshal.GetDelegateForFunctionPointer(api, t);
				mFunctionList.Add(funcName, value);
			}
			return value as T;
		}
		else
		{
			return default;
		}
	}
}