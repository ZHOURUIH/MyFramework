using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using static StringUtility;
using static FileUtility;
using static EditorCommonUtility;
using static FrameDefine;
using static EditorDefine;
using static EditorFileUtility;

public class MenuCheckCode
{
	[MenuItem("检查代码/一键检查代码规范", false, 200)]
	public static void doAllScriptCheck()
	{
		checkResetProperty();
		checkCodeEmptyLine();
		checkProtobufMsgOrder();
		checkDifferentNodeName();
		checkSingleCodeLineLength();
		checkPropertyName();
		checkFunctionOrder();
		checkComment();
		checkCommentStandard();
		checkSystemFunction();
		checkCommandName();
		checkCodeSeparateLineWidth();
	}
	[MenuItem("检查代码/检查代码" + KEY_FUNCTION, false, 200)]
	public static void checkResetProperty()
	{
		Debug.Log("开始检查代码" + KEY_FUNCTION);
		// 获取Assembly集合
		foreach (Assembly assemly in AppDomain.CurrentDomain.GetAssemblies())
		{
			// 获取工程
			if (assemly.GetName().Name == "Game")
			{
				doCheckResetProperty(assemly, F_SCRIPTS_PATH);
				Debug.Log("Game检查完毕");
			}
			if (assemly.GetName().Name == "Frame")
			{
				doCheckResetProperty(assemly, F_SCRIPTS_PATH);
				Debug.Log("Frame检查完毕");
			}
#if USE_HYBRID_CLR
			if (assemly.GetName().Name == "HotFix")
			{
				doCheckResetProperty(assemly, F_SCRIPTS_PATH);
				Debug.Log("HotFix检查完毕");
			}
#endif
		}
	}
	[MenuItem("检查代码/检查代码空行", false, 201)]
	public static void checkCodeEmptyLine()
	{
		Debug.Log("开始检查代码空行...");
		List<string> fileList = new();
		findFiles(F_SCRIPTS_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileList[i];
			displayProgressBar("检查代码空行", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(file))
			{
				continue;
			}
			doCheckEmptyLine(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空行");
	}
	[MenuItem("检查代码/检查代码空格", false, 202)]
	public static void checkCodeSpace()
	{
		Debug.Log("开始检查代码空格...");
		List<string> fileList = new();
		findFiles(F_SCRIPTS_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查代码空格", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			doCheckSpace(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空格");
	}
	[MenuItem("检查代码/检查代码Protobuf消息字段顺序", false, 203)]
	public static void checkProtobufMsgOrder()
	{
		Debug.Log("开始检查Protobuf的消息字段顺序...");
		List<string> fileList = findFilesNonAlloc(F_ASSETS_PATH, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("消息字段顺序", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			doCheckProtoMemberOrder(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查Protobuf的消息字段顺序");
	}
	[MenuItem("检查代码/检查UI变量名", false, 204)]
	public static void checkDifferentNodeName()
	{
		Debug.Log("开始检查UI变量名与节点名不一致的代码...");
		// 缓存所有预制体的Transform
		Dictionary<string, Transform[]> prefabChildTransform = new();
		foreach (string file in findFilesNonAlloc(F_UI_PREFAB_PATH, ".prefab"))
		{
			string filePath = fullPathToProjectPath(file);
			var childTrans = loadGameObject(filePath).transform.GetComponentsInChildren<Transform>(true);
			string prefabName = removeSuffix(getFileNameWithSuffix(filePath)).removeStartString("UI");
			prefabChildTransform.TryAdd(prefabName, childTrans);
		}

		// 读取cs文件
		List<string> fileListCS = new();
		findFiles(F_SCRIPTS_UI_PATH, fileListCS, ".cs");
		int fileCount = fileListCS.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileListCS[i];
			string fileNameNoSuffix = removeSuffix(getFileNameWithSuffix(fullPathToProjectPath(file)));
			if (!fileNameNoSuffix.startWith("Script") || isIgnoreFile(file, getIgnoreLayoutScript()))
			{
				continue;
			}
			string layoutName = fileNameNoSuffix.removeStartString("Script");
			if (!prefabChildTransform.TryGetValue(layoutName, out Transform[] transforms))
			{
				Debug.LogError("脚本名为:" + layoutName + "的cs文件没找到相对应的预制体");
				continue;
			}
			displayProgressBar("UI变量名匹配", "进度: ", i + 1, fileCount);
			doCheckDifferentNodeName(file, layoutName, transforms, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查UI变量名与节点名不一致的代码");
	}
	[MenuItem("检查代码/检查单行代码长度", false, 205)]
	public static void checkSingleCodeLineLength()
	{
		Debug.Log("开始逐行测代码长度");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查代码行长度", "进度: ", +1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, getIgnoreCodeWidth()))
			{
				continue;
			}
			doCheckSingleCheckCodeLineWidth(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查代码/检查命名规范", false, 206)]
	public static void checkPropertyName()
	{
		Debug.Log("开始检查命名规范");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查命名规范", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, getIgnoreVariableCheck()))
			{
				continue;
			}
			doCheckScriptLineByLine(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查函数排版", false, 207)]
	public static void checkFunctionOrder()
	{
		Debug.Log("开始检查函数排版");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查函数排版", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			doCheckFunctionOrder(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查注释", false, 208)]
	public static void checkComment()
	{
		Debug.Log("开始检查注释");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查注释", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, getIgnoreComment()))
			{
				continue;
			}
			doCheckComment(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查代码/检查注释后是否添加空格", false, 210)]
	public static void checkCommentStandard()
	{
		Debug.Log("开始检查注释后是否添加空格");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查注释后是否添加空格", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(filePath))
			{
				continue;
			}
			doCheckCommentStandard(filePath, openTxtFileLines(filePath));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查代码/检查内置函数的调用", false, 211)]
	public static void checkSystemFunction()
	{
		Debug.Log("开始检查内置函数调用");
		List<string> scriptFileList = new();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");

		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查UnityEngine", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, getIgnoreSystemFunctionCheck()))
			{
				continue;
			}
			doCheckSystemFunction(file, openTxtFileLines(file));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查命令命名规范", false, 212)]
	// 根据命名规范中的规则去检测命令的目录和文件名,类名是否正确
	public static void checkCommandName()
	{
		Debug.Log("开始检查检查命令的命名规范");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 加载程序集
		var assembly = getAssembly("Assembly-CSharp");
		var hotFixAssembly = loadHotFixAssembly();

		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查命令的命名规范", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(scriptFileList[i]))
			{
				continue;
			}
			doCheckCommandName(filePath, openTxtFileLines(filePath), assembly, hotFixAssembly);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查代码/代码分隔行长度", false, 213)]
	public static void checkCodeSeparateLineWidth()
	{
		Debug.Log("开始检查分隔行的宽度");
		// 所有代码文件的列表
		List<string> scriptFileList = new();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			string filePath = scriptFileList[i];
			displayProgressBar("检查分隔行的宽度", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(filePath))
			{
				continue;
			}
			doCheckCodeSeparateLineWidth(filePath, openTxtFileLines(filePath));
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查代码/检查热更引用了被裁剪的代码", false, 214)]
	public static void checkAccessMissingMetadata()
	{
		PlatformBase.checkAccessMissingMetadata();
	}
}