#if USE_ILRUNTIME
using System.IO;
using ILRuntime.Mono.Cecil.Pdb;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using UnityEngine;
using System.Threading;
using System.Collections;
using static UnityUtility;
using static FrameBase;
using static FrameUtility;
using static FrameDefine;
using System.Runtime.InteropServices;

public delegate void InitILRCallback(ILRAppDomain appdomain);

// ILRuntime系统,用于实现ILRuntime热更
public class ILRSystem : FrameSystem
{
	protected ILRAppDomain mAppDomain;		// ILRuntime的程序域
	protected MemoryStream mDllFile;		// 加载的Dll文件
	protected MemoryStream mPDBFile;        // 加载的PDB文件
	protected InitILRCallback mInitFunc;	// 外部设置的初始化ILR的函数
	public void launchILR()
	{
		destroyILR();
		mAppDomain = new ILRAppDomain();
		mGameFramework.StartCoroutine(loadILRuntime());
	}
	public override void destroy()
	{
		destroyILR();
	}
	public ILRAppDomain getAppDomain() { return mAppDomain; }
	public void destroyILR()
	{
		mAppDomain?.Dispose();
		mDllFile?.Dispose();
		mPDBFile?.Dispose();
		mAppDomain = null;
		mDllFile = null;
		mPDBFile = null;
	}
	public void setInitILRFunc(InitILRCallback func) { mInitFunc = func; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected IEnumerator loadILRuntime()
	{
		// 下载dll文件
		string dllDownloadPath = availableReadPath(ILR_FILE);
		checkDownloadPath(ref dllDownloadPath);
		WWW wwwDll = new WWW(dllDownloadPath);
		while (!wwwDll.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(wwwDll.error))
		{
			logError(wwwDll.error + ": " + dllDownloadPath);
		}
		mDllFile = new MemoryStream(wwwDll.bytes);
		wwwDll.Dispose();
		// 下载pdb文件
#if UNITY_EDITOR
		string pdbDownloadPath = availableReadPath(ILR_PDB_FILE);
		checkDownloadPath(ref pdbDownloadPath);
		WWW wwwPDB = new WWW(pdbDownloadPath);
		while (!wwwPDB.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(wwwPDB.error))
		{
			logError(wwwPDB.error + ": " + pdbDownloadPath);
		}
		mPDBFile = new MemoryStream(wwwPDB.bytes);
		wwwPDB.Dispose();
#endif
		// 加载dll
		mAppDomain.LoadAssembly(mDllFile, mPDBFile, new PdbReaderProvider());
#if UNITY_EDITOR
		// 固定绑定56000端口,用于ILRuntime调试
		string result = mAppDomain.DebugService.StartDebugService(56000, false);
		if (result == null)
		{
			logForce("启动IL调试成功,端口:" + mAppDomain.DebugService.getDebugPort());
		}
		else
		{
			logError("启动IL调试失败:" + result);
		}
#endif
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS)
		// 由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
		mAppDomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
		mInitFunc(mAppDomain);

		// 初始化完毕后开始执行热更工程中的逻辑
		ILRFrameUtility.start();
		mGameFramework.hotFixInited();
	}
}
#endif