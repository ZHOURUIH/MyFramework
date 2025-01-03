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
		if (mSetParamCallbackList.getSafe(index) is FloatCallback floatCallback)
		{
			float.TryParse(stringParam, out float value);
			floatCallback.Invoke(value);
		}
		else if (mSetParamCallbackList.getSafe(index) is StringCallback strCallback)
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
		if (mSetParamCallbackList.getSafe(index) is FloatCallback floatCallback)
		{
			floatCallback.Invoke(floatParam);
		}
		else if (mSetParamCallbackList.getSafe(index) is StringCallback strCallback)
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
	public void initFromParam(IList<string> paramList)
	{
		int index = 0;
		foreach (string param in paramList)
		{
			setParam(index++, param);
		}
	}
	public void initFromParam(string param0)
	{
		setParam(0, param0);
	}
	public void initFromParam(string param0, string param1)
	{
		setParam(0, param0);
		setParam(1, param1);
	}
	public void initFromParam(string param0, string param1, string param2)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
	}
	public void initFromParam(string param0, string param1, string param2, string param3)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
		setParam(8, param8);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
		setParam(8, param8);
		setParam(9, param9);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
		setParam(8, param8);
		setParam(9, param9);
		setParam(10, param10);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string param11)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
		setParam(8, param8);
		setParam(9, param9);
		setParam(10, param10);
		setParam(11, param11);
	}
	public void initFromParam(string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string param11, string param12)
	{
		setParam(0, param0);
		setParam(1, param1);
		setParam(2, param2);
		setParam(3, param3);
		setParam(4, param4);
		setParam(5, param5);
		setParam(6, param6);
		setParam(7, param7);
		setParam(8, param8);
		setParam(9, param9);
		setParam(10, param10);
		setParam(11, param11);
		setParam(12, param12);
	}
}