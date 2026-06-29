using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using static StringUtility;
using static FrameBaseUtility;

// 用于开始一段性能检测,不再使用时会自动释放,需要搭配using来使用
// 比如using var a = new ProfilerScope("test")
// 或者using var a = new ProfilerScope(0)
// 不能直接调用默认构造
public struct ProfilerScope : IDisposable
{
	private static readonly bool mValid = isDevOrEditor();
	private static readonly ProfilerMarker[] mProfilerMarkers = CreateMarkers();
	private ProfilerMarker.AutoScope mScope;
	private static ProfilerMarker[] CreateMarkers()
	{
		var arr = new ProfilerMarker[30000];
		for (int i = 0; i < arr.Length; ++i)
		{
			arr[i] = new ProfilerMarker(IToS(i));
		}
		return arr;
	}
	public ProfilerScope(string name)
	{
		mScope = mValid ? new ProfilerMarker(name).Auto() : default;
	}
	// id固定填0即可,用于避免直接调用默认构造
	public ProfilerScope(int id, [CallerMemberName] string callerName = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
	{
		if (mValid)
		{
			// 如果想要更详细的信息,则可以使用下面被注释的哪一行
			mScope = mProfilerMarkers[line].Auto();
			// 更加准确的信息显示,但是会有额外的GC和性能消耗,这里使用Path.GetFileName是为了能够在多线程调用
			//mScope = new ProfilerMarker(callerName + "," + Path.GetFileName(file) + ":" + IToS(line)).Auto();
		}
		else
		{
			mScope = default;
		}
	}
	public void Dispose()
	{
		if (mValid)
		{
			mScope.Dispose();
		}
	}
}