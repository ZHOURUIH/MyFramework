using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Profiling;
using static FrameBaseUtility;

// 用于开始一段性能检测,不再使用时会自动释放,需要搭配using来使用
// 比如using var a = new ProfilerScope("test")
// 或者using var a = new ProfilerScope(0)
// 不能直接调用默认构造
public struct ProfilerScope : IDisposable
{
	private static readonly bool mValid = isDevOrEditor();
	private ProfilerMarker.AutoScope mScope;
	private static class LineMarkerCache
	{
		private const int MARKER_COUNT = 30000;
		private static readonly ProfilerMarker[] mProfilerMarkers = new ProfilerMarker[MARKER_COUNT];
		private static readonly bool[] mMarkerCreated = new bool[MARKER_COUNT];
		private static readonly object mLock = new();
		public static ProfilerMarker getMarker(int line)
		{
			if (Volatile.Read(ref mMarkerCreated[line]))
			{
				return mProfilerMarkers[line];
			}
			lock (mLock)
			{
				if (!mMarkerCreated[line])
				{
					mProfilerMarkers[line] = new ProfilerMarker(line.IToS());
					Volatile.Write(ref mMarkerCreated[line], true);
				}
				return mProfilerMarkers[line];
			}
		}
	}
	public ProfilerScope(string name)
	{
		mScope = mValid ? new ProfilerMarker(name).Auto() : default;
	}
	// id固定填0即可,用于避免直接调用默认构造
	public ProfilerScope(int id, [CallerMemberName] string callerName = null, [CallerLineNumber] int line = 0, [CallerFilePath] string file = null)
	{
		// 如果想要更详细的信息,则可以使用下面被注释的那一行
		mScope = mValid ? LineMarkerCache.getMarker(line).Auto() : default;
		// 更加准确的信息显示,但是会有额外的GC和性能消耗,这里使用Path.GetFileName是为了能够在多线程调用
		//mScope = mValid ? new ProfilerMarker(callerName + "," + Path.GetFileName(file) + ":" + IToS(line)).Auto() : default;
	}
	public void Dispose()
	{
		if (mValid)
		{
			mScope.Dispose();
		}
	}
}