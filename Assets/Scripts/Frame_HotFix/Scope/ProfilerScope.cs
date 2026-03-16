using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
using static StringUtility;
using static FrameBaseUtility;

// 用于开始一段性能检测,不再使用时会自动释放,需要搭配using来使用
// 比如using var a = new ProfilerScope("test")
// 或者using var a = new ProfilerScope(0)
// 不能直接调用默认构造
public struct ProfilerScope : IDisposable
{
	private bool mBeginSample;
	public ProfilerScope(string name)
	{
		if (isDevOrEditor())
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
		if (isDevOrEditor())
		{
			mBeginSample = true;
			// 如果想要更详细的信息,则可以使用下面被注释的哪一行
			Profiler.BeginSample(IToS(line));
			// 这里使用Path.GetFileName是为了能够在多线程调用
			//Profiler.BeginSample(callerName + "," + Path.GetFileName(file) + ":" + IToS(line));
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