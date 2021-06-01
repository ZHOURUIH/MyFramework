#if USE_ILRUNTIME
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using ILRuntime.Runtime.CLRBinding;
using ILRuntime.Runtime.Enviorment;
using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using System.Reflection;

[Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding : FrameUtility
{
	[MenuItem("ILRuntime/生成跨域继承适配器")]
	public static void GenerateCrossbindAdapter()
	{
		if (EditorApplication.isCompiling)
		{
			Debug.LogError("正在编译,请等待编译完成再重试");
			return;
		}
		// 由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
		// 大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题
		HashSet<Type> classTypeList = new HashSet<Type>();
		ILRLaunchFrame.collectCrossInheritClass(classTypeList);
		// 先删除所有自动生成的文件
		List<string> oldFiles = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_ILRUNTIME_PATH + "GeneratedCrossBinding/", oldFiles, ".cs");
		foreach(var item in oldFiles)
		{
			deleteFile(item);
		}
		foreach(var item in classTypeList)
		{
			generateAdapter(item);
		}
		generateAdaterRegister(classTypeList, FrameDefine.F_SCRIPTS_ILRUNTIME_PATH);
		AssetDatabase.Refresh();
	}
	[MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
	public static void GenerateCLRBindingByAnalysis()
	{
		if (EditorApplication.isCompiling)
		{
			Debug.LogError("正在编译,请等待编译完成再重试");
			return;
		}
		// 用新的分析热更dll调用引用来生成绑定代码
		Debug.Log("如果自动分析有报错,先尝试重新编译热更工程后再分析");
		ILRAppDomain domain = new ILRAppDomain();
		string dllPath = FrameDefine.P_STREAMING_ASSETS_PATH + FrameDefine.ILR_FILE_NAME;
		using (FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
		{
			domain.LoadAssembly(fs);
			// 这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
			CrossAdapterRegister.registeCrossAdaptor(domain);
			// 值类型绑定
			ILRLaunchFrame.registeValueTypeBinder(domain);
			BindingCodeGenerator.GenerateBindingCode(domain, FrameDefine.P_SCRIPTS_ILRUNTIME_PATH + "GeneratedCLRBinding");
		}
		AssetDatabase.Refresh();
	}
	//------------------------------------------------------------------------------------------------------------
	protected static void generateAdapter(Type type, string nameSpace = "HotFix")
	{
		string prePath = FrameDefine.P_SCRIPTS_ILRUNTIME_PATH + "GeneratedCrossBinding/";
		createDir(prePath);
		string fullFileName = prePath + type.Name + "Adapter.cs";
		using (StreamWriter sw = new StreamWriter(fullFileName))
		{
			sw.WriteLine(CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(type, nameSpace));
		}
	}
	protected static void generateAdaterRegister(HashSet<Type> typeList, string path)
	{
		string file = "";
		line(ref file, "#if USE_ILRUNTIME");
		line(ref file, "using UnityEngine;");
		line(ref file, "using System;");
		line(ref file, "using ILRAppDomain = ILRuntime.Runtime.Enviorment.AppDomain;");
		line(ref file, "using HotFix;");
		line(ref file, "");
		line(ref file, "// 用于绑定所有跨域继承的适配器");
		line(ref file, "public static class CrossAdapterRegister");
		line(ref file, "{");
		line(ref file, "\tpublic static void registeCrossAdaptor(ILRAppDomain appDomain)");
		line(ref file, "\t{");
		foreach(var item in typeList)
		{
			line(ref file, "\t\tappDomain.RegisterCrossBindingAdaptor(new " + item.Name + "Adapter());");
		}
		line(ref file, "\t}");
		line(ref file, "}");
		line(ref file, "#endif");
		writeTxtFile(path + "CrossAdapterRegister.cs", file);
	}
	protected static void line(ref string str, string line)
	{
		str += line + "\r\n";
	}
}
#endif