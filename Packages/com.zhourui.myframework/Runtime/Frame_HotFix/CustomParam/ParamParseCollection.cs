using System;
using System.Collections.Generic;
using static FrameUtility;
using static UnityUtility;

// 用于存储预解析的参数对象,在所有需要解析参数数据的地方都可以使用这个来存储解析后的数据,在需要时直接拷贝解析后的参数对象来使用
// 初始化时需要注册参数类型和参数的值,也就是调用registe和registeParamTemplate
public class ParamParseCollection
{
	protected Dictionary<int, int> mTemplateTypeList = new();				// key是模板ID,value是对应的类型ID
	protected Dictionary<int, List<string>> mTemplateRawParamList = new();  // 所有模板的原始参数数据列表,key是模板ID
	protected Dictionary<int, ParamBase> mParamTemplateList = new();		// 参数的预解析列表,key是模板ID,一个类型可以对应多个模板,每个模板的参数值可以不一样,只是格式一样
	protected Dictionary<int, Type> mParamTypeList = new();					// 参数的类型列表,key是类型ID
	public void registe<T>(int typeID) where T : ParamBase
	{
		mParamTypeList.Add(typeID, typeof(T));
	}
	public void registe(int typeID, Type type)
	{
		mParamTypeList.Add(typeID, type);
	}
	public void registeParamTemplate(int templateID, int typeID, params string[] param)
	{
		mTemplateTypeList.add(templateID, typeID);
		mTemplateRawParamList.getOrAddNew(templateID).setRange(param);
	}
	public ParamBase getParamTemplate(int templateID)
	{
		int typeID = mTemplateTypeList.get(templateID);
		if (typeID == 0)
		{
			logError("找不到参数模板对应的类型,TemplateID:" + templateID + ", 需要调用registeParamTemplate对其进行初始化");
			return null;
		}
		if (getParamInternal(typeID, templateID, out ParamBase param))
		{
			param?.mParamSet?.initFromParam(mTemplateRawParamList.get(templateID));
		}
		return param;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 返回值表示获取到的param是否需要对参数进行初始化
	protected bool getParamInternal(int typeID, int templateID, out ParamBase param)
	{
		bool success = mParamTemplateList.TryGetValue(templateID, out param);
		if (!success && mParamTypeList.TryGetValue(typeID, out Type paramType))
		{
			param = mParamTemplateList.add(templateID, CLASS<ParamBase>(paramType));
			param.registeAllParam();
		}
		return !success;
	}
}