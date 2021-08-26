using System;
using System.Collections.Generic;

// Dll导入系统
// Dll导入可以使用两种方式
// 1.直接使用DllImport标签对函数进行导入声明
// 2.通过自定义的Dll类,注册要导入的库文件名,然后内部通过Kernel32中的函数去获取想要调用的函数(仅windows可用)
public class DllImportSystem : FrameSystem
{
	protected Dictionary<string, Dll> mDllLibraryList;		// 通过Kernel32导入的动态库列表
	public DllImportSystem()
	{
		mDllLibraryList = new Dictionary<string, Dll>();
	}
	// 将要执行的函数转换为委托
	public T invoke<T>(string library, string funcName) where T : Delegate
	{
		if (!mDllLibraryList.TryGetValue(library, out Dll dll))
		{
			logError("找不到动态库:" + library + ", 请确认该库是否已注册");
			return default(T);
		}
		return dll.getFunction<T>(funcName, typeof(T));
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void registerDLL(string name)
	{
		Dll dll = new Dll();
		dll.init(name);
		mDllLibraryList.Add(dll.getName(), dll);
	}
}