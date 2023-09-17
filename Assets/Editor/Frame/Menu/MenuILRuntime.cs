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
using static FileUtility;
using static StringUtility;
using static FrameDefine;

[Obfuscation(Exclude = true)]
public class MenuILRuntimeCLR
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
		var classTypeList = new HashSet<Type>();
		ILRLaunchExtension ilr = new ILRLaunchExtension();
		ilr.collectCrossInheritClass(classTypeList);
		// 先删除所有自动生成的文件
		var oldFiles = new List<string>();
		findFiles(F_SCRIPTS_ILRUNTIME_PATH + "GeneratedCrossBinding/", oldFiles, new List<string>() { ".cs" });
		foreach(var item in oldFiles)
		{
			deleteFile(item);
		}
		foreach(var item in classTypeList)
		{
			generateAdapter(item);
		}
		generateAdaterRegister(classTypeList, F_SCRIPTS_ILRUNTIME_PATH);
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
		var domain = new ILRAppDomain();
		string dllPath = P_ASSET_BUNDLE_PATH + ILR_FILE;
		using (var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read))
		{
			domain.LoadAssembly(fs);
			// 这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
			CrossAdapterRegister.registeCrossAdaptor(domain);
			// 值类型绑定
			ILRLaunchExtension ilr = new ILRLaunchExtension();
			ilr.valueTypeBind(domain);
			BindingCodeGenerator.GenerateBindingCode(domain, P_SCRIPTS_ILRUNTIME_PATH + "GeneratedCLRBinding");
			// 部分脚本的额外修改
			modifyCSharpUtility();
			modifyUnityUtility();
		}
		AssetDatabase.Refresh();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void generateAdapter(Type type, string nameSpace = "HotFix")
	{
		string prePath = P_SCRIPTS_ILRUNTIME_PATH + "GeneratedCrossBinding/";
		createDir(prePath);
		string fullFileName = prePath + type.Name + "Adapter.cs";
		using (var sw = new StreamWriter(fullFileName))
		{
			string str = CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(type, nameSpace);
			// 需要将Command的适配器重新修改一下
			if (type == typeof(Command))
			{
				str = adjustStaticAdapterFunctionDeclare(str, "execute");
			}
			else if(type == typeof(WindowObject) || type == typeof(WindowObjectUI) || type == typeof(WindowObjectUGUI))
			{
				// WindowObjectUGUI有三个assignWindow,其余两个类型有两个assignWindow
				str = adjustStaticAdapterFunctionDeclare(str, "assignWindow");
				str = adjustStaticAdapterFunctionDeclare(str, "assignWindow");
				str = adjustStaticAdapterFunctionDeclare(str, "assignWindow");
				str = adjustStaticAdapterFunctionDeclare(str, "init");
				str = adjustStaticAdapterFunctionDeclare(str, "reset");
				str = adjustStaticAdapterFunctionDeclare(str, "setScript");
				str = adjustStaticAdapterFunctionDeclare(str, "recycle");
				str = adjustStaticAdapterFunctionDeclare(str, "destroy");
			}
			else if (type == typeof(LayoutScript))
			{
				str = adjustStaticAdapterFunctionDeclare(str, "onGameState");
			}
			sw.WriteLine(str);
		}
	}
	protected static string adjustStaticAdapterFunctionDeclare(string fileContent, string functionName)
	{
		splitLine(fileContent, out string[] lines);
		List<string> lineList = new List<string>(lines);
		string newExecuteDeclare = null;
		for(int i = 0; i < lineList.Count; ++i)
		{
			// 将execute的函数声明备份,并去除static关键字
			if (newExecuteDeclare == null && lineList[i].Contains("(\"" + functionName + "\")"))
			{
				newExecuteDeclare = replaceAll(lineList[i], "static ", "    protected ");
				lineList.RemoveAt(i);
				--i;
			}
			// 如果到了Adapter的定义还没有找到指定的函数的变量,则不再继续执行
			if (lineList[i].Contains("public class Adapter "))
			{
				if (newExecuteDeclare != null)
				{
					lineList.Insert(i + 2, newExecuteDeclare);
				}
				break;
			}
		}
		return stringsToString(lineList, "\r\n");
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
		line(ref file, "#endif", false);
		writeTxtFile(path + "CrossAdapterRegister.cs", file);
	}
	protected static void modifyCSharpUtility()
	{
		string fileName = F_SCRIPTS_ILRUNTIME_PATH + "GeneratedCLRBinding/CSharpUtility_Binding.cs";
		int lineCount = openTxtFileLines(fileName, out string[] fileLines, false);
		if (fileLines != null)
		{
			List<string> lines = new List<string>(fileLines);
			for (int i = 0; i < lineCount; ++i)
			{
				if (lines[i].Contains("CSharpUtility.getILRStackTrace("))
				{
					lines[i + 2] = "            // 只能在此处才能获取DLL内的堆栈";
					lines.Insert(i + 3, "            string stacktrace = __domain.DebugService.GetStackTrace(__intp);");
					lines.Insert(i + 4, "            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method + stacktrace);");
					break;
				}
			}
			writeTxtFile(fileName, stringsToString(lines, "\r\n"));
		}
	}
	protected static void modifyUnityUtility()
	{
		string fileName = F_SCRIPTS_ILRUNTIME_PATH + "GeneratedCLRBinding/UnityUtility_Binding.cs";
		openTxtFileLines(fileName, out string[] fileLines, false);
		if (fileLines != null)
		{
			List<string> lines = new List<string>(fileLines);
			// log
			int lineCount = lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				if (lines[i].Contains("global::UnityUtility.log("))
				{
					lines[i] = "            // 只能在此处才能获取DLL内的堆栈";
					lines.Insert(i + 1, "            string stacktrace = __domain.DebugService.GetStackTrace(__intp);");
					lines.Insert(i + 2, "            global::UnityUtility.log(@info + \"\\n\" + stacktrace, @level);");
					break;
				}
			}
			
			// logException
			lineCount = lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				if (lines[i].Contains("global::UnityUtility.logException("))
				{
					lines[i] = "            // 只能在此处才能获取DLL内的堆栈";
					lines.Insert(i + 1, "            string stacktrace = __domain.DebugService.GetStackTrace(__intp);");
					lines.Insert(i + 2, "            global::UnityUtility.logException(@e, @info + \"\\n\" + stacktrace);");
					break;
				}
			}

			// logError
			lineCount = lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				if (lines[i].Contains("global::UnityUtility.logError("))
				{
					lines[i] = "            // 只能在此处才能获取DLL内的堆栈";
					lines.Insert(i + 1, "            string stacktrace = __domain.DebugService.GetStackTrace(__intp);");
					lines.Insert(i + 2, "            global::UnityUtility.logError(@info + \"\\n\" + stacktrace);");
					break;
				}
			}

			// logForce
			lineCount = lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				if (lines[i].Contains("global::UnityUtility.logForce("))
				{
					lines[i] = "            // 只能在此处才能获取DLL内的堆栈";
					lines.Insert(i + 1, "            string stacktrace = __domain.DebugService.GetStackTrace(__intp);");
					lines.Insert(i + 2, "            global::UnityUtility.logForce(@info + \"\\n\" + stacktrace);");
					break;
				}
			}

			writeTxtFile(fileName, stringsToString(lines, "\r\n"));
		}
	}
}
#endif