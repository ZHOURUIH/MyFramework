#if USE_ILRUNTIME
using ILRuntime.Reflection;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

// 与C#有关的工具函数
public class CSharpUtility : TimeUtility
{
	protected static uint mIDMaker;
	protected static int mMainThreadID;
	public static void setMainThreadID(int mainThreadID) { mMainThreadID = mainThreadID; }
	public static bool isMainThread() { return Thread.CurrentThread.ManagedThreadId == mMainThreadID; }
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		T obj;
		try
		{
#if USE_ILRUNTIME
			if (classType is ILRuntimeWrapperType)
			{
				obj = Activator.CreateInstance((classType as ILRuntimeWrapperType).CLRType.TypeForCLR, param) as T;
			}
			else if (classType is ILRuntimeType)
			{
				obj = (classType as ILRuntimeType).ILType.Instantiate(param).CLRInstance as T;
			}
			else
			{
				obj = Activator.CreateInstance(classType, param) as T;
			}
#else
			obj = Activator.CreateInstance(classType, param) as T;
#endif
		}
		catch (Exception e)
		{
			UnityUtility.logError("create instance error! " + e.Message + ", inner error:" + e.InnerException?.Message);
			obj = null;
		}
		return obj;
	}
	public static T deepCopy<T>(T obj) where T : class
	{
		// 如果是字符串或值类型则直接返回
		if (obj == null || obj is string || Typeof(obj).IsValueType)
		{
			return obj;
		}
		object retval = createInstance<object>(Typeof(obj));
		FieldInfo[] fields = Typeof(obj).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		int count = fields.Length;
		for (int i = 0; i < count; ++i)
		{
			FieldInfo field = fields[i];
			field.SetValue(retval, deepCopy(field.GetValue(obj)));
		}
		return (T)retval;
	}
	// preFrameCount为1表示返回调用getLineNum的行号
	public static int getLineNum(int preFrameCount = 1)
	{
		var st = new StackTrace(preFrameCount, true);
		return st.GetFrame(0).GetFileLineNumber();
	}
	// preFrameCount为1表示返回调用getCurSourceFileName的文件名
	public static string getCurSourceFileName(int preFrameCount = 1)
	{
		var st = new StackTrace(preFrameCount, true);
		return st.GetFrame(0).GetFileName();
	}
	// 此处不使用MyStringBuilder,因为打印堆栈时一般都是产生了某些错误,再使用MyStringBuilder可能会引起无限递归
	public static string getStackTrace()
	{
		string fullTrace = "";
		var trace = new StackTrace(true);
		for (int i = 0; i < trace.FrameCount; ++i)
		{
			if (i == 0)
			{
				continue;
			}
			StackFrame frame = trace.GetFrame(i);
			if (isEmpty(frame.GetFileName()))
			{
				break;
			}
			fullTrace += "at " + frame.GetFileName() + ":" + frame.GetFileLineNumber() + "\n";
		}
		return fullTrace;
	}
	// 此处只是定义一个空函数,为了能够进行重定向,因为只有在重定向中才能获取真正的堆栈信息
	public static string getILRStackTrace() { return ""; }
	public static uint makeID()
	{
		if (mIDMaker >= 0xFFFFFFFF)
		{
			UnityUtility.logError("ID已超过最大值");
		}
		return ++mIDMaker;
	}
	public static void notifyIDUsed(uint id)
	{
		mIDMaker = getMax(mIDMaker, id);
	}
	// 移除数组中的第index个元素,validElementCount是数组中有效的元素个数
	public static void removeElement<T>(T[] array, int validElementCount, int index)
	{
		if (index < 0 || index >= validElementCount)
		{
			return;
		}
		int moveCount = validElementCount - index - 1;
		for (int i = 0; i < moveCount; ++i)
		{
			array[index + i] = array[index + i + 1];
		}
	}
	// 移除数组中的所有value,T为引用类型
	public static int removeClassElement<T>(T[] array, int validElementCount, T value) where T : class
	{
		// 从后往前遍历删除
		for (int i = validElementCount - 1; i >= 0; --i)
		{
			if (array[i] == value)
			{
				removeElement(array, validElementCount, i);
				--validElementCount;
			}
		}
		return validElementCount;
	}
	// 移除数组中的所有value,T为继承自IEquatable的值类型
	public static int removeValueElement<T>(T[] array, int validElementCount, T value) where T : IEquatable<T>
	{
		// 从后往前遍历删除
		for (int i = validElementCount - 1; i >= 0; --i)
		{
			if (array[i].Equals(value))
			{
				removeElement(array, validElementCount, i);
				--validElementCount;
			}
		}
		return validElementCount;
	}
	public static bool arrayContainsValue<T>(T[] array, T value, int arrayLen = -1) where T : IEquatable<T>
	{
		if (arrayLen == -1)
		{
			arrayLen = array.Length;
		}
		for (int i = 0; i < arrayLen; ++i)
		{
			if (array[i].Equals(value))
			{
				return true;
			}
		}
		return false;
	}
	public static bool arrayContains<T>(T[] array, T value, int arrayLen = -1) where T : class
	{
		if (arrayLen == -1)
		{
			arrayLen = array.Length;
		}
		for (int i = 0; i < arrayLen; ++i)
		{
			if (array[i].Equals(value))
			{
				return true;
			}
		}
		return false;
	}
	public static IPAddress hostNameToIPAddress(string hostName)
	{
		IPAddress[] ipList = Dns.GetHostAddresses(hostName);
		if (ipList != null && ipList.Length > 0)
		{
			return ipList[0];
		}
		return null;
	}
	public static T popFirstElement<T>(HashSet<T> list)
	{
		T elem = default;
		foreach(var item in list)
		{
			elem = item;
			break;
		}
		list.Remove(elem);
		return elem;
	}
	// 获取类型,因为ILR的原因,如果是热更工程中的类型,直接使用typeof获取的是错误的类型
	// 所以需要使用此函数获取真实的类型,要获取真实类型必须要有一个实例
	// 为了方便调用,所以写在CSharpUtility中
	public static Type Typeof<T>()
	{
		Type type = typeof(T);
#if USE_ILRUNTIME
		if (typeof(CrossBindingAdaptorType).IsAssignableFrom(type) ||
			typeof(ILTypeInstance).IsAssignableFrom(type) ||
			typeof(ILRuntimeWrapperType).IsAssignableFrom(type) ||
			typeof(ILRuntimeType).IsAssignableFrom(type))
		{
			UnityUtility.logError("无法获取热更工程中的类型,请确保没有在热更工程中调用Typeof<>(), 在热更工程中获取类型请使用typeof()," +
					"或者没有调用CMD_MAIN,PACKET_MAIN,LIST_MAIN这类的只能在主工程中调用的函数");
			return null;
		}
#endif
		return type;
	}
	public static Type Typeof(object obj)
	{
#if USE_ILRUNTIME
		return obj?.GetActualType();
#else
		return obj?.GetType();
#endif
	}
}