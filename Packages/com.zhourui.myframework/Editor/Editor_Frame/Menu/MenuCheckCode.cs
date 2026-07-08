using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static FileUtility;
using static EditorCommonUtility;
using static FrameDefine;
using static EditorFileUtility;
using static FrameBaseDefine;
using static EditorDefine;

public class MenuCheckCode
{
    public const string MENU_NAME = MENU_ROOT_NAME + "检查代码/";
    [MenuItem(MENU_NAME + "一键检查代码规范", false, 200)]
	public static void doAllScriptCheck()
	{
		checkCodeEmptyLine();
		checkProtobufMsgOrder();
		checkSingleCodeLineLength();
		checkPropertyName();
		checkFunctionOrder();
		checkComment();
		checkCommentStandard();
		checkSystemFunction();
		checkCommandName();
		checkCodeSeparateLineWidth();
	}
	[MenuItem(MENU_NAME + "检查代码空行", false, 201)]
	public static void checkCodeEmptyLine()
	{
		Debug.Log("开始检查代码空行...");
		List<string> fileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
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
		clearProgressBar();
		Debug.Log("完成检查代码空行");
	}
	[MenuItem(MENU_NAME + "检查代码空格", false, 202)]
	public static void checkCodeSpace()
	{
		Debug.Log("开始检查代码空格...");
		List<string> fileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
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
		clearProgressBar();
		Debug.Log("完成检查代码空格");
	}
	[MenuItem(MENU_NAME + "检查代码Protobuf消息字段顺序", false, 203)]
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
		clearProgressBar();
		Debug.Log("完成检查Protobuf的消息字段顺序");
	}
	[MenuItem(MENU_NAME + "检查单行代码长度", false, 205)]
	public static void checkSingleCodeLineLength()
	{
		Debug.Log("开始逐行测代码长度");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查代码行长度", "进度: ", +1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameEditorSettings.getInstance().IgnoreCodeWidth))
			{
				continue;
			}
			doCheckSingleCheckCodeLineWidth(file, openTxtFileLines(file));
		}
		clearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem(MENU_NAME + "检查命名规范", false, 206)]
	public static void checkPropertyName()
	{
		Debug.Log("开始检查命名规范");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
		// 开始进行检查操作
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查命名规范", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameEditorSettings.getInstance().IgnoreVariableCheck))
			{
				continue;
			}
			doCheckScriptLineByLine(file, openTxtFileLines(file));
		}
		clearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem(MENU_NAME + "检查函数排版", false, 207)]
	public static void checkFunctionOrder()
	{
		Debug.Log("开始检查函数排版");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
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
		clearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem(MENU_NAME + "检查注释", false, 208)]
	public static void checkComment()
	{
		Debug.Log("开始检查注释");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查注释", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameEditorSettings.getInstance().IgnoreComment))
			{
				continue;
			}
			doCheckComment(file, openTxtFileLines(file));
		}
		clearProgressBar();
		Debug.Log("检测结束");
	}
	[MenuItem(MENU_NAME + "检查注释后是否添加空格", false, 210)]
	public static void checkCommentStandard()
	{
		Debug.Log("开始检查注释后是否添加空格");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
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
		clearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem(MENU_NAME + "检查内置函数的调用", false, 211)]
	public static void checkSystemFunction()
	{
		Debug.Log("开始检查内置函数调用");
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
		int fileCount = scriptFileList.Count;
		for (int i = 0; i < fileCount; i++)
		{
			displayProgressBar("检查UnityEngine", "进度: ", i + 1, fileCount);
			string file = scriptFileList[i];
			if (isIgnoreFile(file, FrameEditorSettings.getInstance().IgnoreSystemFunctionCheck))
			{
				continue;
			}
			doCheckSystemFunction(file, openTxtFileLines(file));
		}
		clearProgressBar();
		Debug.Log("结束检查");
	}
	[MenuItem(MENU_NAME + "检查命令命名规范", false, 212)]
	// 根据命名规范中的规则去检测命令的目录和文件名,类名是否正确
	public static void checkCommandName()
	{
		Debug.Log("开始检查检查命令的命名规范");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");

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
			doCheckCommandName(filePath, openTxtFileLines(filePath));
		}
		clearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem(MENU_NAME + "代码分隔行长度", false, 213)]
	public static void checkCodeSeparateLineWidth()
	{
		Debug.Log("开始检查分隔行的宽度");
		// 寻找指定文件格式的格式
		List<string> scriptFileList = findFilesNonAlloc(F_SCRIPTS_PATH, ".cs");
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
		clearProgressBar();
		Debug.Log("结束检测");
	}
	[MenuItem(MENU_NAME + "检查热更引用了被裁剪的代码", false, 214)]
	public static void checkAccessMissingMetadata()
	{
		PlatformUtility.checkAccessMissingMetadata();
	}
}