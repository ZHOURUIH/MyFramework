using System;
using System.Collections.Generic;
using static FrameUtility;

// 用于存储预解析的参数对象
public class CustomParamParseCollection
{
	protected Dictionary<int, CustomParam> mParamTemplateList = new();  // 参数的预解析列表
	protected Dictionary<int, Type> mParamTypeList = new();				// 参数的类型列表
	public void registe<T>(int typeID) where T : CustomParam
	{
		mParamTypeList.Add(typeID, typeof(T));
	}
	public void registe(int typeID, Type type)
	{
		mParamTypeList.Add(typeID, type);
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7, param8);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7, param8, param9);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string param11)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, string param0, string param1, string param2, string param3, string param4, string param5, string param6, string param7, string param8, string param9, string param10, string param11, string param12)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(param0, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12);
		}
		return param;
	}
	public CustomParam getParamTemplate(int typeID, int templateID, List<string> paramList)
	{
		if (getParamInternal(typeID, templateID, out CustomParam param))
		{
			param?.mParamSet?.initFromParam(paramList);
		}
		return param;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 返回值表示获取到的param是否需要对参数进行初始化
	protected bool getParamInternal(int typeID, int templateID, out CustomParam param)
	{
		bool success = mParamTemplateList.TryGetValue(templateID, out param);
		if (!success && mParamTypeList.TryGetValue(typeID, out Type paramType))
		{
			param = mParamTemplateList.add(templateID, CLASS<CustomParam>(paramType));
			param.registeParams();
		}
		return !success;
	}
}