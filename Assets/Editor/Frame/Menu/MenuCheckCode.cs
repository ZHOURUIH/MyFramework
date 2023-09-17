using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using static StringUtility;
using static FileUtility;
using static EditorCommonUtility;
using static FrameDefine;

public class MenuCheckCode
{
	[MenuItem("检查代码/一键检查代码规范", false, 200)]
	public static void doAllScriptCheck()
	{
		checkResetProperty();
		checkHotFixResetProperty();
		checkCodeEmptyLine();
		checkProtobufMsgOrder();
		checkDifferentNodeName();
		checkSingleCodeLineLength();
		checkPropertyName();
		checkFunctionOrder();
		checkComment();
		checkMemberVariableAssignmentValue();
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
		Assembly assemly = null;
		Assembly[] assembly = System.AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assembly.Length; ++i)
		{
			// 获取工程
			if (assembly[i].GetName().Name == "Assembly-CSharp")
			{
				assemly = assembly[i];
				break;
			}
		}
		if (assemly == null)
		{
			Debug.LogError("找不到指定的程序集");
			return;
		}

		doCheckResetProperty(assemly, F_SCRIPTS_PATH);
		Debug.Log("检查完毕");
	}
	[MenuItem("检查代码/检查热更代码" + KEY_FUNCTION, false, 200)]
	public static void checkHotFixResetProperty()
	{
		Debug.Log("开始检查热更代码" + KEY_FUNCTION);
		doCheckResetProperty(Assembly.Load(openFile(F_ASSET_BUNDLE_PATH + ILR_FILE, true)), F_HOT_FIX_PATH);
		Debug.Log("检查完毕");
	}
	[MenuItem("检查代码/检查代码空行", false, 201)]
	public static void checkCodeEmptyLine()
	{
		Debug.Log("开始检查代码空行...");
		var fileList = new List<string>();
		findFiles(F_SCRIPTS_PATH, fileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, fileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileList[i];
			displayProgressBar("检查代码空行", "进度: ", i + 1, fileCount);
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckEmptyLine(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空行");
	}
	[MenuItem("检查代码/检查代码空格", false, 202)]
	public static void checkCodeSpace()
	{
		Debug.Log("开始检查代码空格...");
		var fileList = new List<string>();
		findFiles(F_SCRIPTS_PATH, fileList, ".cs");
		findFiles(F_HOT_FIX_GAME_PATH, fileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("检查代码空格", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSpace(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查代码空格");
	}
	[MenuItem("检查代码/检查代码Protobuf消息字段顺序", false, 203)]
	public static void checkProtobufMsgOrder()
	{
		Debug.Log("开始检查Protobuf的消息字段顺序...");
		var fileList = new List<string>();
		findFiles(F_ASSETS_PATH, fileList, ".cs");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("消息字段顺序", "进度: ", i + 1, fileCount);
			string file = fileList[i];
			if (isIgnoreFile(file))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckProtoMemberOrder(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查Protobuf的消息字段顺序");
	}
	[MenuItem("检查代码/检查UI变量名", false, 204)]
	public static void checkDifferentNodeName()
	{
		Debug.Log("开始检查UI变量名与节点名不一致的代码...");
		// 缓存所有预制体的Transform
		var fileListPrefab = new List<string>();
		findFiles(F_LAYOUT_PATH, fileListPrefab, ".prefab");
		findFiles(F_RESOURCES_LAYOUT_PREFAB_PATH, fileListPrefab, ".prefab");
		var prefabChildTransform = new Dictionary<string, Transform[]>();
		int processCount = fileListPrefab.Count;
		for (int i = 0; i < processCount; ++i)
		{
			string filePath = fullPathToProjectPath(fileListPrefab[i]);
			GameObject targetPrefab = loadGameObject(filePath);
			var childTrans = targetPrefab.transform.GetComponentsInChildren<Transform>(true);
			string prefabName = removeStartString(removeSuffix(getFileName(filePath)), "UI");
			if (!prefabChildTransform.ContainsKey(prefabName))
			{
				prefabChildTransform.Add(prefabName, childTrans);
			}
		}

		// 读取cs文件
		var fileListCS = new List<string>();
		findFiles(F_SCRIPTS_UI_SCRIPT_PATH, fileListCS, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, fileListCS, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, fileListCS, ".cs");
		int fileCount = fileListCS.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string file = fileListCS[i];
			string fileNameNoSuffix = removeSuffix(getFileName(fullPathToProjectPath(file)));
			if (!startWith(fileNameNoSuffix, "Script") || isIgnoreFile(file, IGNORE_LAYOUT_SCRIPT))
			{
				continue;
			}
			string layoutName = removeStartString(fileNameNoSuffix, "Script");
			if (!prefabChildTransform.TryGetValue(layoutName, out Transform[] transforms))
			{
				Debug.LogError("脚本名为:" + layoutName + "的cs文件没找到相对应的预制体");
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			displayProgressBar("UI变量名匹配", "进度: ", i + 1, fileCount);
			doCheckDifferentNodeName(file, layoutName, transforms, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("完成检查UI变量名与节点名不一致的代码");
	}
	[MenuItem("检查代码/检查单行代码长度", false, 205)]
	public static void checkSingleCodeLineLength()
	{
		Debug.Log("开始逐行测代码长度");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查代码行长度", "进度: ", +1, fileCount);
			string file = scriptFileList[i];
			// ILRuntime自动生成的代码，文件忽略列表和MyStringBuilder都不需要检测代码长度
			if (isIgnoreFile(file, IGNORE_CODE_WIDTH))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSingleCheckCodeLineWidth(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查代码/检查命名规范", false, 206)]
	public static void checkPropertyName()
	{
		Debug.Log("开始检查命名规范");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查命名规范", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, IGNORE_VARIABLE_CHECK))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckScriptLineByLine(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查函数排版", false, 207)]
	public static void checkFunctionOrder()
	{
		Debug.Log("开始检查函数排版");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
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
			openTxtFileLines(file, out string[] lines);
			doCheckFunctionOrder(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查注释", false, 208)]
	public static void checkComment()
	{
		Debug.Log("开始检查注释");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查注释", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, IGNORE_COMMENT))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckComment(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem("检查代码/检查成员变量赋值", false, 209)]
	public static void checkMemberVariableAssignmentValue()
	{
		Debug.Log("开始检查成员变量赋值");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查成员变量赋值", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, IGNORE_CONSTRUCT_VALUE))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckScriptMemberVariableValueAssignment(file, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem("检查代码/检查注释后是否添加空格", false, 210)]
	public static void checkCommentStandard()
	{
		Debug.Log("开始检查注释后是否添加空格");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
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
			openTxtFileLines(filePath, out string[] lines);
			doCheckCommentStandard(filePath, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查代码/检查内置函数的调用", false, 211)]
	public static void checkSystemFunction()
	{
		Debug.Log("开始检查内置函数调用");
		var scriptFileList = new List<string>();
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");

		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查UnityEngine", "进度: ", i, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, IGNORE_SYSTEM_FUNCTION_CHECK))
			{
				continue;
			}
			openTxtFileLines(file, out string[] lines);
			doCheckSystemFunction(file, lines);
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
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
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
			openTxtFileLines(filePath, out string[] lines);
			doCheckCommandName(filePath, lines, assembly, hotFixAssembly);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem("检查代码/代码分隔行长度", false, 213)]
	public static void checkCodeSeparateLineWidth()
	{
		Debug.Log("开始检查分隔行的宽度");
		// 所有代码文件的列表
		var scriptFileList = new List<string>();
		// 寻找指定文件格式的格式
		findFiles(F_SCRIPTS_PATH, scriptFileList, ".cs");
		// 在热更文件夹中寻找指定文件格式的格式
		findFiles(F_HOT_FIX_GAME_PATH, scriptFileList, ".cs");
		findFiles(F_HOT_FIX_FRAME_PATH, scriptFileList, ".cs");
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
			openTxtFileLines(filePath, out string[] lines);
			doCheckCodeSeparateLineWidth(filePath, lines);
		}
		EditorUtility.ClearProgressBar();
		Debug.Log("结束检测");
	}
}