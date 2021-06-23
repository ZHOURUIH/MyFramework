using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Callbacks;
#if USE_ILRUNTIME
using ILRuntime.Runtime.Enviorment;
#endif

public class ClassInfo
{
	public List<string> mLines;
	public string mFilePath;
	public int mFunctionLine;
	public ClassInfo()
	{
		mLines = new List<string>();
	}
}

public class EditorMenu : EditorCommonUtility
{
	protected const string KEY_FUNCTION = "resetProperty";
	[MenuItem("快捷操作/启动游戏 _F5", false, 0)]
	public static void StartGame()
	{
		if(!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.P_RESOURCES_SCENE_PATH + "start.unity", OpenSceneMode.Single);
			EditorApplication.isPlaying = true;
		}
		else
		{
			EditorApplication.isPlaying = false;
		}
	}
	[MenuItem("快捷操作/暂停游戏 _F6", false, 0)]
	public static void PauseGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.isPaused = !EditorApplication.isPaused;
		}
	}
	[MenuItem("快捷操作/单帧执行 _F7", false, 0)]
	public static void StepGame()
	{
		if (EditorApplication.isPlaying)
		{
			EditorApplication.Step();
		}
	}
	[MenuItem("快捷操作/打开初始场景 _F9")]
	public static void JumpGameSceme()
	{
		if (!EditorApplication.isPlaying)
		{
			EditorSceneManager.OpenScene(FrameDefine.P_RESOURCES_SCENE_PATH + "start.unity");
		}
	}
	[MenuItem("快捷操作/调整物体坐标为整数 _F1")]
	public static void adjustUIRectToInt()
	{
		GameObject go = Selection.activeGameObject;
		if (go == null)
		{
			return;
		}
		EditorUtility.SetDirty(go);
		roundRectTransformToInt(go.GetComponent<RectTransform>());
	}
	[MenuItem("快捷操作/检测" + KEY_FUNCTION)]
	public static void detectResetProperty()
	{
		// 遍历目录,存储所有文件名和对应文本内容
		var classInfoList = new Dictionary<string, ClassInfo>();
		saveFileInfo(classInfoList);
		// 获取Assembly集合
		Assembly assemly = null;
		Assembly[] assembly = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assembly.Length; ++i)
		{
			// 获取工程
			if (assembly[i].GetName().Name == "Assembly-CSharp")
			{
				assemly = assembly[i];
				break;
			}
		}
		if(assemly == null)
		{
			logError("找不到指定的程序集");
			return;
		}

		List<Type> ignoreBaseClass = new List<Type>();
		ignoreBaseClass.Add(typeof(myUIObject));
		ignoreBaseClass.Add(typeof(FrameSystem));
		ignoreBaseClass.Add(typeof(SocketPacket));
#if USE_ILRUNTIME
		ignoreBaseClass.Add(typeof(CrossBindingAdaptorType));
#endif
		// 获取到类型
		Type[] types = assemly.GetTypes();
		for (int i = 0; i < types.Length; ++i)
		{
			Type type = types[i];
			// 是否继承自需要忽略的基类
			bool isIgnoreClass = false;
			for(int j = 0; j < ignoreBaseClass.Count; ++j)
			{
				if (ignoreBaseClass[j].IsAssignableFrom(type))
				{
					isIgnoreClass = true;
					break;
				}
			}
			// 判断类是否继承自 IClassObject  
			if (isIgnoreClass || !typeof(ClassObject).IsAssignableFrom(type))
			{
				continue;
			}
			// 获取类成员变量
			MemberInfo[] memberInfo = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			var fieldMembers = new List<MemberInfo>();
			for (int k = 0; k < memberInfo.Length; ++k)
			{
				// 成员变量 筛选出类型为字段
				if (memberInfo[k].MemberType == MemberTypes.Field)
				{
					fieldMembers.Add(memberInfo[k]);
				}
			}
			if (fieldMembers.Count == 0)
			{
				continue;
			}
			string className = type.Name;
			if (!classInfoList.TryGetValue(className, out ClassInfo info))
			{
				logError("检测" + KEY_FUNCTION + "()--" + className + "--程序集中有此类,但是代码文件中找不到此类");
				continue;
			}
			// 判断类是否包含函数ResetProperty()
			// BindingFlags.DeclaredOnly 仅考虑在提供的类型的层次结构级别上声明的成员。不考虑继承的成员
			MethodInfo methodInfo = type.GetMethod(KEY_FUNCTION, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			if (methodInfo == null)
			{
				logError("检测" + KEY_FUNCTION + "()--" + className + "--没有包含: " + KEY_FUNCTION + "()\n" +
						 "File:" + info.mFilePath + "\n" +
						 "Line:" + info.mFunctionLine + "\n");
				continue;
			}
			detectResetAll(className, fieldMembers, info);
		}
		log("检查完毕");
	}
	[OnOpenAsset(1)]
	public static bool OnOpenAsset(int instanceID, int line)
	{
		// 自定义函数，用来获取log中的stacktrace，定义在后面。
		string stack_trace = findStackTrace();
		// 通过stacktrace来定位是否是我们自定义的log，我的log中有特殊文字 "检测resetProperty()"
		if (isEmpty(stack_trace))
		{
			return false;
		}
		if (stack_trace.Contains("检测" + KEY_FUNCTION + "()"))
		{
			string filePath = null;
			int fileLine = 0;
			string[] debugInfoLines = split(stack_trace, false, "\n");
			for (int i = 0; i < debugInfoLines.Length; ++i)
			{
				if (startWith(debugInfoLines[i], "File:"))
				{
					filePath = removeStartString(debugInfoLines[i], "File:");
				}
				else if (startWith(debugInfoLines[i], "Line:"))
				{
					fileLine = SToI(removeStartString(debugInfoLines[i], "Line:"));
				}
			}
			UnityEngine.Object codeObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(UnityEngine.Object));
			if (codeObject == null)
			{
				return false;
			}
			if (codeObject.GetInstanceID() == instanceID && fileLine == line)
			{
				return false;
			}
			AssetDatabase.OpenAsset(codeObject, fileLine);
			return true;
		}
		return false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void roundRectTransformToInt(RectTransform rectTransform)
	{
		if (rectTransform == null)
		{
			return;
		}
		rectTransform.localPosition = round(rectTransform.localPosition);
		setRectSize(rectTransform, round(getRectSize(rectTransform)));
		int childCount = rectTransform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			roundRectTransformToInt(rectTransform.GetChild(i) as RectTransform);
		}
	}
	protected static bool detectResetAll(string className, List<MemberInfo> fieldMembers, ClassInfo classInfo)
	{
		int index = 0;
		bool find = false;
		int startIndex = -1;
		int endIndex = -1;
		bool hasOverride = false;

		for (int i = 0; i < classInfo.mLines.Count; ++i)
		{
			if (classInfo.mLines[i].Contains("void " + KEY_FUNCTION + "()"))
			{
				hasOverride = classInfo.mLines[i].Contains("override");
				find = true;
				classInfo.mFunctionLine += i;
			}
			if (!find)
			{
				continue;
			}
			if (classInfo.mLines[i].Contains("{"))
			{
				if (index == 0)
				{
					startIndex = i;
				}
				++index;
			}
			if (classInfo.mLines[i].Contains("}"))
			{
				--index;
				if (index == 0)
				{
					endIndex = i;
				}
			}
			if (startIndex >= 0 && endIndex >= 0)
			{
				break;
			}
		}
		int count = 0;
		List<string> contents = new List<string>();
		if (find)
		{
			for (int i = 0; i < endIndex - startIndex + 1; ++i)
			{
				contents.Add(Regex.Replace(classInfo.mLines[startIndex + i], "\\s", ""));
			}
			for (int i = 0; i < contents.Count; ++i)
			{
				// 文本用分隔符拆分,判断其中是否有变量名,一行最多只允许出现一个成员变量
				string[] strList = split(contents[i], true, ".", ",", "=", "[", "]", "/", "(", ")", ";", "+", "-", "*", "%");
				for (int j = 0; j < fieldMembers.Count; ++j)
				{
					if (arrayContains(strList, fieldMembers[j].Name))
					{
						++count;
						break;
					}
				}
			}
		}

		if (hasOverride && !contents.Contains("base." + KEY_FUNCTION + "();") || count < fieldMembers.Count)
		{
			logError("检测" + KEY_FUNCTION + "()--" + className + "--成员变量未能全部重置\n" +
							 "File:" + classInfo.mFilePath + "\n" +
							 "Line:" + classInfo.mFunctionLine + "\n");
			return false;
		}
		return true;
	}
	protected static void saveFileInfo(Dictionary<string, ClassInfo> fileInfos)
	{
		List<string> fileList = new List<string>();
		findFiles(FrameDefine.F_SCRIPTS_PATH, fileList, ".cs");
		foreach (var item in fileList)
		{
			string[] fileLines = File.ReadAllLines(item);
			int classBeginIndex = -1;
			string nameSpace = EMPTY;
			for (int i = 0; i < fileLines.Length; ++i)
			{
				string line = fileLines[i];
				if (line.Contains("namespace "))
				{
					int startIndex = findFirstSubstr(line, "namespace ", 0, true);
					if (line.Contains("{"))
					{
						nameSpace = line.Substring(startIndex, line.IndexOf("{") - startIndex) + ".";
					}
					else
					{
						nameSpace = line.Substring(startIndex, line.Length - startIndex) + ".";
					}
				}
				if (line.Contains("public class") || line.Contains("public abstract class") || line.Contains("public partial class"))
				{
					if (classBeginIndex >= 0)
					{
						parseClass(fileInfos, nameSpace, fileLines, classBeginIndex, i - 1, item);
					}
					classBeginIndex = i;
				}
			}
			if (classBeginIndex >= 0)
			{
				parseClass(fileInfos, nameSpace, fileLines, classBeginIndex, fileLines.Length - 1, item);
			}
		}
	}
	protected static void parseClass(Dictionary<string, ClassInfo> fileInfos, string nameSpace, string[] fileLines, int startIndex, int endIndex, string path)
	{
		List<string> classLines = new List<string>();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			classLines.Add(removeAll(fileLines[i + startIndex], "\t"));
		}
		string headLine = fileLines[startIndex];
		int nameStartIndex = findFirstSubstr(headLine, "class ", 0, true);
		string className;
		int colonIndex = headLine.IndexOf(":");
		if (colonIndex >= 0)
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex, colonIndex - nameStartIndex), " ");
		}
		else
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex, headLine.Length - nameStartIndex), " ");
		}
		// 因为有些类是内部类,所以仍然存在重名情况,需要排除
		if (!fileInfos.ContainsKey(className))
		{
			ClassInfo info = new ClassInfo();
			info.mLines.AddRange(classLines);
			info.mFilePath = fullPathToProjectPath(path);
			info.mFunctionLine = startIndex + 1;
			fileInfos.Add(className, info);
		}
	}
	protected static string findStackTrace()
	{
		// 找到UnityEditor.EditorWindow的assembly
		var assembly_unity_editor = Assembly.GetAssembly(typeof(EditorWindow));
		if (assembly_unity_editor == null)
		{
			return null;
		}

		// 找到类UnityEditor.ConsoleWindow
		var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
		if (type_console_window == null)
		{
			return null;
		}
		// 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
		var field_console_window = type_console_window.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		if (field_console_window == null)
		{
			return null;
		}
		// 获取ms_ConsoleWindow的值
		var instance_console_window = field_console_window.GetValue(null);
		if (instance_console_window == null)
		{
			return null;
		}

		// 如果console窗口是焦点窗口的话，获取stacktrace信息
		if ((object)EditorWindow.focusedWindow == instance_console_window)
		{
			// 通过assembly获取类ListViewState
			var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
			if (type_list_view_state == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ListView
			var field_list_view = type_console_window.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field_list_view == null)
			{
				return null;
			}

			// 获取m_ListView的值
			var value_list_view = field_list_view.GetValue(instance_console_window);
			if (value_list_view == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
			var field_active_text = type_console_window.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field_active_text == null)
			{
				return null;
			}

			// 获得m_ActiveText的值，就是我们需要的stacktrace
			return field_active_text.GetValue(instance_console_window).ToString();
		}
		return null;
	}
}