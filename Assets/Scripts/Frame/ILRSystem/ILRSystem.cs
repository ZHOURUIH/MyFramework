#if USE_ILRUNTIME
using System.IO;
using ILRuntime.Mono.Cecil.Pdb;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using UnityEngine;
using System.Threading;
using System.Collections;

public delegate void OnHotFixLoaded(ILRAppDomain appDomain);

public class ILRSystem : FrameSystem
{
	protected ILRAppDomain mAppDomain;
	protected MemoryStream mDllFile;
	protected MemoryStream mPDBFile;
	protected OnHotFixLoaded mOnHotFixLoaded;
	public void launchILR(OnHotFixLoaded callback)
	{
		mOnHotFixLoaded = callback;
		destroyILR();
		mAppDomain = new ILRAppDomain();
		mGame.StartCoroutine(loadILRuntime());
	}
	public override void destroy()
	{
		destroyILR();
	}
	public override void update(float elapsedTime) { }
	public ILRAppDomain getAppDomain() { return mAppDomain; }
	public void destroyILR()
	{
		mAppDomain = null;
		mDllFile?.Dispose();
		mPDBFile?.Dispose();
		mDllFile = null;
		mPDBFile = null;
	}
	//------------------------------------------------------------------------------------------------
	protected IEnumerator loadILRuntime()
	{
		// 下载dll文件
		string dllDownloadPath = FrameDefine.F_STREAMING_ASSETS_PATH + FrameDefine.ILR_FILE_NAME;
		checkDownloadPath(ref dllDownloadPath, true);
		WWW wwwDll = new WWW(dllDownloadPath);
		while (!wwwDll.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(wwwDll.error))
		{
			Debug.LogError(wwwDll.error + ": " + dllDownloadPath);
		}
		mDllFile = new MemoryStream(wwwDll.bytes);
		wwwDll.Dispose();
		// 下载pdb文件
#if UNITY_EDITOR
		string pdbDownloadPath = FrameDefine.F_STREAMING_ASSETS_PATH + FrameDefine.ILR_PDB_FILE_NAME;
		checkDownloadPath(ref pdbDownloadPath, true);
		WWW wwwPDB = new WWW(pdbDownloadPath);
		while (!wwwPDB.isDone)
		{
			yield return null;
		}
		if (!string.IsNullOrEmpty(wwwPDB.error))
		{
			Debug.LogError(wwwPDB.error + ": " + pdbDownloadPath);
		}
		mPDBFile = new MemoryStream(wwwPDB.bytes);
		wwwPDB.Dispose();
#endif
		// 加载dll
		mAppDomain.LoadAssembly(mDllFile, mPDBFile, new PdbReaderProvider());
#if UNITY_EDITOR
		// 固定绑定56000端口,用于ILRuntime调试
		mAppDomain.DebugService.StartDebugService(56000);
#endif
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
		//由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
		mAppDomain.UnityMainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
		mOnHotFixLoaded?.Invoke(mAppDomain);
	}
}
#endif