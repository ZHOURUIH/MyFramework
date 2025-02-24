using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using static UnityUtility;
using static StringUtility;

// 与C#有关的工具函数
public class CSharpUtility
{
	protected static int mIDMaker;			// 用于生成客户端唯一ID的种子
	protected static int mMainThreadID;		// 主线程ID
	public static void setMainThreadID(int mainThreadID) { mMainThreadID = mainThreadID; }
	public static bool isMainThread() { return Thread.CurrentThread.ManagedThreadId == mMainThreadID; }
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		try
		{
			return Activator.CreateInstance(classType, param) as T;
		}
		catch (Exception e)
		{
			logException(e, "create instance error! type:" + classType);
			return null;
		}
	}
	public static T intToEnum<T, IntT>(IntT value) where T : Enum
	{
		return (T)Enum.ToObject(typeof(T), value);
	}
	public static void parseFileList(string content, Dictionary<string, GameFileInfo> list)
	{
		if (content.isEmpty())
		{
			return;
		}
		foreach (string line in splitLine(content))
		{
			var info = GameFileInfo.createInfo(line);
			list.addNotNullKey(info.mFileName, info);
		}
	}
	public static bool arrayContains<T>(T[] array, T value, int arrayLen = -1)
	{
		if (array.isEmpty())
		{
			return false;
		}
		if (arrayLen == -1)
		{
			arrayLen = array.Length;
		}
		for (int i = 0; i < arrayLen; ++i)
		{
			if (EqualityComparer<T>.Default.Equals(array[i], value))
			{
				return true;
			}
		}
		return false;
	}
	// 对比两个版本号,返回值表示整个版本号的大小比较结果,lowerVersion表示小版本号的比较结果,higherVersion表示大版本号比较的结果
	// 此函数只判断3位的版本号,也就是版本号0.版本号1.版本号2的格式,不支持2位的版本号
	public static VERSION_COMPARE compareVersion3(string remote, string local, out VERSION_COMPARE lowerVersion, out VERSION_COMPARE higherVersion)
	{
		if (remote.isEmpty())
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		if (local.isEmpty())
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		List<long> sourceFormat = SToLs(remote, '.');
		List<long> targetFormat = SToLs(local, '.');
		if (sourceFormat.Count != 3)
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		if (targetFormat.Count != 3)
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		lowerVersion = VERSION_COMPARE.EQUAL;
		higherVersion = VERSION_COMPARE.EQUAL;
		if (remote == local)
		{
			return VERSION_COMPARE.EQUAL;
		}
		const long MaxMiddleVersion = 100000000000;
		long sourceFullVersion = sourceFormat[0] * MaxMiddleVersion * MaxMiddleVersion + sourceFormat[1] * MaxMiddleVersion + sourceFormat[2];
		long targetFullVersion = targetFormat[0] * MaxMiddleVersion * MaxMiddleVersion + targetFormat[1] * MaxMiddleVersion + targetFormat[2];
		long sourceBigVersion = sourceFormat[0] * MaxMiddleVersion + sourceFormat[1];
		long targetBigVersion = targetFormat[0] * MaxMiddleVersion + targetFormat[1];
		if (sourceBigVersion > targetBigVersion)
		{
			higherVersion = VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (sourceBigVersion < targetBigVersion)
		{
			higherVersion = VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			higherVersion = VERSION_COMPARE.EQUAL;
		}
		if (sourceFormat[2] > targetFormat[2])
		{
			lowerVersion = VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (sourceFormat[2] < targetFormat[2])
		{
			lowerVersion = VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			lowerVersion = VERSION_COMPARE.EQUAL;
		}
		if (sourceFullVersion > targetFullVersion)
		{
			return VERSION_COMPARE.LOCAL_LOWER;
		}
		else if (sourceFullVersion < targetFullVersion)
		{
			return VERSION_COMPARE.REMOTE_LOWER;
		}
		else
		{
			return VERSION_COMPARE.EQUAL;
		}
	}
	// ensureInterval为true表示保证每次间隔一定不小于interval,false表示保证一定时间内的触发次数,而不保证每次间隔一定小于interval
	public static bool tickTimerLoop(ref float timer, float elapsedTime, float interval, bool ensureInterval = false)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			if (ensureInterval)
			{
				timer = interval;
			}
			else
			{
				timer += interval;
				// 如果加上间隔以后还是小于0,则可能间隔太小了,需要将计时重置到间隔时间,避免计时停止
				if (timer <= 0.0f)
				{
					timer = interval;
				}
			}
			return true;
		}
		return false;
	}
	public static bool tickTimerOnce(ref float timer, float elapsedTime)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			timer = -1.0f;
			return true;
		}
		return false;
	}
	// preFrameCount为1表示返回调用getLineNum的行号
	public static int getLineNum(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileLineNumber();
	}
	// preFrameCount为1表示返回调用getCurSourceFileName的文件名
	public static string getCurSourceFileName(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileName();
	}
	// 此处不使用MyStringBuilder,因为打印堆栈时一般都是产生了某些错误,再使用MyStringBuilder可能会引起无限递归
	public static string getStackTrace(int depth = 20)
	{
		++depth;
		StringBuilder fullTrace = new();
		StackTrace trace = new(true);
		for (int i = 0; i < trace.FrameCount; ++i)
		{
			if (i == 0)
			{
				continue;
			}
			if (i >= depth)
			{
				break;
			}
			StackFrame frame = trace.GetFrame(i);
			if (frame.GetFileName().isEmpty())
			{
				break;
			}
			fullTrace.Append("at ");
			fullTrace.Append(frame.GetFileName());
			fullTrace.Append(":");
			fullTrace.AppendLine(IToS(frame.GetFileLineNumber()));
		}
		return fullTrace.ToString();
	}
	public static int makeID()
	{
		if (mIDMaker >= 0x7FFFFFFF)
		{
			logError("ID已超过最大值");
		}
		return ++mIDMaker;
	}
}