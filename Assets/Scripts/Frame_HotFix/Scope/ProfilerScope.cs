using System;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
using static StringUtility;
using static FrameEditorUtility;

// 用于开始一段性能检测,不再使用时会自动释放,需要搭配using来使用,比如using var a = new ProfilerScope("test")
// 不能直接调用默认构造
public struct ProfilerScope : IDisposable
{
	private bool mBeginSample;
	public ProfilerScope(string name)
	{
		if (isEditor() || isDevelopment())
		{
			mBeginSample = true;
			Profiler.BeginSample(name);
		}
		else
		{
			mBeginSample = false;
		}
	}
	// id固定填0即可,用于避免直接调用默认构造
	public ProfilerScope(int id, [CallerMemberName] string callerName = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
	{
		if (isEditor() || isDevelopment())
		{
			mBeginSample = true;
			Profiler.BeginSample(callerName + "," + getFileNameNoSuffixNoDir(file) + ":" + IToS(line));
		}
		else
		{
			mBeginSample = false;
		}
	}
	public void Dispose()
	{
		if (mBeginSample)
		{
			Profiler.EndSample();
		}
	}
}