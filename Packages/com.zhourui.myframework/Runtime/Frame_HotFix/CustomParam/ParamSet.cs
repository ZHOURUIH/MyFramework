using System;
using System.Collections.Generic;

// 存储参数的解析委托
public class ParamSet
{
	protected List<Delegate> mSetParamCallbackList; // 参数注册的函数列表,用于解析表格的参数
	public void resetProperty()
	{
		mSetParamCallbackList?.Clear();
	}
	public bool setParam(int index, string stringParam) 
	{
		if (mSetParamCallbackList.get(index) is FloatCallback floatCallback)
		{
			float.TryParse(stringParam, out float value);
			floatCallback.Invoke(value);
		}
		else if (mSetParamCallbackList.get(index) is StringCallback strCallback)
		{
			strCallback.Invoke(stringParam);
		}
		else
		{
			return false;
		}
		return true;
	}
	public bool setParam(int index, float floatParam, string stringParam)
	{
		if (mSetParamCallbackList.get(index) is FloatCallback floatCallback)
		{
			floatCallback.Invoke(floatParam);
		}
		else if (mSetParamCallbackList.get(index) is StringCallback strCallback)
		{
			strCallback.Invoke(stringParam);
		}
		else
		{
			return false;
		}
		return true;
	}
	public int getParamCount() { return mSetParamCallbackList.count(); }
	public void registeParam(StringCallback callback)
	{
		mSetParamCallbackList ??= new();
		mSetParamCallbackList.Add(callback);
	}
	public void registeParam(FloatCallback callback)
	{
		mSetParamCallbackList ??= new();
		mSetParamCallbackList.Add(callback);
	}
	public void initFromParam(List<string> paramList)
	{
		int index = 0;
		foreach (string param in paramList)
		{
			setParam(index++, param);
		}
	}
}