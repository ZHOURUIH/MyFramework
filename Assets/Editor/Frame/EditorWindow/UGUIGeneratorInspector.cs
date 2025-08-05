using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static FrameDefine;
using static EditorCommonUtility;
using static UGUIGeneratorUtility;

[CustomEditor(typeof(UGUIGenerator))]
public class UGUIGeneratorInspector : GameInspector
{
	protected List<MemberData> mTempNeedRemoveData;
	protected List<string> mTempSingleWindowTypeList = new();
	protected override void onGUI()
	{
		base.onGUI();
		EditorGUILayout.Space();
		var generator = target as UGUIGenerator;
		if (generator == null)
		{
			return;
		}
		using (new EditorModifyScope(this))
		{
			toggle(ref generator.mIsPersistent, "常驻界面");
			// 基类类型
			using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
			{
				label("基类:");
				List<string> parentList = getUIParentList();
				if (generator.mParentType.isEmpty() || !parentList.Contains(generator.mParentType))
				{
					generator.mParentType = parentList.get(0) ?? typeof(LayoutScript).ToString();
				}
				displayDropDown("", "", parentList, ref generator.mParentType);
			}
			drawMemberInspector(generator);
		}
		EditorGUILayout.Space(10);
		using (new GUILayout.VerticalScope())
		{
			if (button("生成代码", 300, 25))
			{
				// 生成代码之前需要确保变量的排序
				sortMemberList(generator);
				PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (prefabStage == null)
				{
					logError("需要再prefab编辑模式下执行");
					return;
				}
				// 需要先保存一下prefab,否则在第一次添加UGUIGenerator组件后点击生成代码会由于没有写入文件而没有生成界面的注册信息
				// 而且上面排序以后也是需要保存的
				GameObject prefabAsset = prefabStage.prefabContentsRoot;
				PrefabUtility.SaveAsPrefabAsset(prefabAsset, prefabStage.assetPath);
				EditorUtility.ClearDirty(prefabAsset);
				PrefabStageUtility.GetCurrentPrefabStage().ClearDirtiness();
				AssetDatabase.Refresh();

				generateRegister();
				generateLayoutScript(generator);
			}
			if (button("打开代码", 300, 25))
			{
				AssetDatabase.OpenAsset(loadAsset(findScript(getClassNameFromGameObject(generator.gameObject))));
			}
		}
	}
	// 生成界面注册和全局可访问的脚本静态变量
	protected static void generateRegister()
	{
		List<string> uiList = new();
		List<string> insertList = new();
		List<string> fileList = findFilesNonAlloc(F_UI_PREFAB_PATH, ".prefab");
		int fileCount = fileList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			// 如果将移动端和PC端都放到同一个目录中,则需要排除一下移动端的(这里只是一种自定义的规则,不同项目可能不一样)
			if (fileList[i].Contains("_Mobile"))
			{
				continue;
			}
			GameObject prefabObj = loadGameObject(fullPathToProjectPath(fileList[i]));
			if (prefabObj == null)
			{
				continue;
			}
			if (!prefabObj.TryGetComponent<UGUIGenerator>(out var comGenerator))
			{
				continue;
			}
			string fileName = getFileNameNoSuffixNoDir(fileList[i]);
			uiList.Add(fileName);
			string lineString;
			if (comGenerator.mIsPersistent)
			{
				lineString = "\t\tregisteLayoutPersist<" + fileName + ">((script) =>";
			}
			else
			{
				lineString = "\t\tregisteLayout<" + fileName + ">((script) =>";
			}
			string endString;
			string subPath = fileList[i].removeStartString(F_UI_PREFAB_PATH).removeEndString(getFileNameWithSuffix(fileList[i]));
			if (subPath.isEmpty())
			{
				endString = "{ m" + fileName + " = script; });";
			}
			else
			{
				endString = "{ m" + fileName + " = script; }, \"" + subPath + "\");";
			}
			appendWithAlign(ref lineString, endString, 68);
			insertList.Add(lineString);
		}
		// LayoutRegisterHotFix
		string registerFileFullPath = F_SCRIPTS_HOTFIX_UI_PATH + "LayoutRegisterHotFix.cs";
		if (isFileExist(registerFileFullPath))
		{
			List<string> codeList = null;
			if (findCustomCode(registerFileFullPath, ref codeList, out int lineStart0,
				(string line) => { return line.endWith("// auto generate start"); },
				(string line) => { return line.endWith("// auto generate end"); }))
			{
				foreach (string str in insertList)
				{
					codeList.Insert(++lineStart0, str);
				}
			}
			writeTxtFile(registerFileFullPath, stringsToString(codeList, "\r\n"));
		}

		// GameBaseILR
		string gameBaseFileFullPath = F_SCRIPTS_HOTFIX_PATH + "Common/GameBaseHotFix.cs";
		if (isFileExist(gameBaseFileFullPath))
		{
			List<string> codeList = null;
			if (findCustomCode(gameBaseFileFullPath, ref codeList, out int lineStart0,
				(string line) => { return line.endWith("// auto generate LayoutScript start"); },
				(string line) => { return line.endWith("// auto generate LayoutScript end"); }))
			{
				foreach (string str in uiList)
				{
					codeList.Insert(++lineStart0, "\tpublic static " + str + " m" + str + ";");
				}
			}
			writeTxtFile(gameBaseFileFullPath, stringsToString(codeList, "\r\n"));
		}
	}
	// 生成UI对应的脚本
	protected static void generateLayoutScript(UGUIGenerator generator)
	{
		string layoutName = getClassNameFromGameObject(generator.gameObject);
		// 先找一下有没有已经存在的UI脚本
		string fileFullPath = findScript(layoutName);

		// 成员变量定义的代码
		List<string> memberDefineList = new();
		memberDefineList.add("[ObfuzIgnore(ObfuzScope.TypeName)]");
		memberDefineList.add("public class " + layoutName + " : " + generator.mParentType);
		memberDefineList.add("{");
		foreach (MemberData data in generator.mMemberList)
		{
			string type = data.getTypeName();
			string memberName = data.getMemberName();
			if (memberName.isEmpty())
			{
				Debug.LogError("找不到变量名,节点为空,且未输入变量名");
				return;
			}
			if (data.mArrayType != ARRAY_TYPE.NONE)
			{
				// 移除名字中末尾的数字
				string newName = memberName.removeEndNumber();
				if (newName.isEmpty())
				{
					logError("标记为数组的节点名为纯数字,无法生成数组代码");
				}
				memberDefineList.Add("\tprotected " + type + "[] m" + newName + " = new " + type + "[" + data.mArrayLength + "];");
			}
			else
			{
				memberDefineList.Add("\tprotected " + type + " m" + memberName + ";");
			}
		}

		// assignWindow中的代码
		List<string> tempCreatedList = new();
		List<string> generatedAssignLines = new();
		List<MemberData> tempDataList = new(generator.mMemberList);
		while (tempDataList.Count > 0)
		{
			MemberData data = tempDataList.get(0);
			generateNewObject(generatedAssignLines, tempDataList, generator.mMemberList, tempCreatedList, data, generator.gameObject);
		}

		// 构造函数的代码
		List<string> constructorLines = new();
		foreach (MemberData data in generator.mMemberList)
		{
			if (data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
			{
				continue;
			}
			string memberName = data.getMemberName();
			if (data.mArrayType != ARRAY_TYPE.NONE)
			{
				string newName = memberName.removeEndNumber();
				constructorLines.add("\t\tfor (int i = 0; i < m" + newName + ".Length; ++i)");
				constructorLines.add("\t\t{");
				constructorLines.add("\t\t\tm" + newName + "[i] = new(this);");
				constructorLines.add("\t\t}");
			}
			else
			{
				constructorLines.add("\t\tm" + memberName + " = new(this);");
			}
		}

		if (fileFullPath.isEmpty())
		{
			fileFullPath = F_SCRIPTS_HOTFIX_UI_PATH + layoutName + ".cs";
			string fileContent = "";
			line(ref fileContent, "using Obfuz;");
			line(ref fileContent, "");
			line(ref fileContent, "// auto generate member start");
			foreach (string str in memberDefineList)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t// auto generate member end");
			line(ref fileContent, "\tpublic " + layoutName + "()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\t// auto generate constructor start");
			foreach (string str in constructorLines)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t\t// auto generate constructor end");
			line(ref fileContent, "\t}");
			line(ref fileContent, "\tpublic override void assignWindow()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\t// auto generate assignWindow start");
			foreach (string str in generatedAssignLines)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t\t// auto generate assignWindow end");
			line(ref fileContent, "\t}");
			line(ref fileContent, "\tpublic override void init()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\tbase.init();");
			line(ref fileContent, "\t}");
			line(ref fileContent, "\tpublic override void onGameState()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\tbase.onGameState();");
			line(ref fileContent, "\t}");
			line(ref fileContent, "}");
			writeTxtFile(fileFullPath, fileContent);
			// 新生成文件后需要刷新一下资源
			AssetDatabase.Refresh();
		}
		else
		{
			// 成员变量定义
			List<string> codeList = null;
			if (findCustomCode(fileFullPath, ref codeList, out int lineStart0,
				(string line) => { return line.endWith("// auto generate member start"); },
				(string line) => { return line.endWith("// auto generate member end"); }, false))
			{
				foreach (string str in memberDefineList)
				{
					codeList.Insert(++lineStart0, str);
				}
			}
			else
			{
				// 找不到就在类的第一行插入
				for (int i = 0; i < codeList.Count; ++i)
				{
					if (codeList[i].Contains(" class " + layoutName + " "))
					{
						int lineStart = i - 2;
						codeList.Insert(++lineStart, "\t// auto generate member start");
						foreach (string str in memberDefineList)
						{
							codeList.Insert(++lineStart, str);
						}
						codeList.Insert(++lineStart, "\t// auto generate member end");
						break;
					}
				}
			}

			// 构造函数
			if (findCustomCode(fileFullPath, ref codeList, out int lineStart1,
				(string line) => { return line.endWith("// auto generate constructor start"); },
				(string line) => { return line.endWith("// auto generate constructor end"); }, false))
			{
				foreach (string str in constructorLines)
				{
					codeList.Insert(++lineStart1, str);
				}
			}
			// 找不到就在构造的第一行插入
			else
			{
				for (int i = 0; i < codeList.Count; ++i)
				{
					if (codeList[i].Contains("public " + layoutName + "()"))
					{
						int lineStart = i + 1;
						codeList.Insert(++lineStart, "\t\t// auto generate constructor start");
						foreach (string str in constructorLines)
						{
							codeList.Insert(++lineStart, str);
						}
						codeList.Insert(++lineStart, "\t\t// auto generate constructor end");
						break;
					}
				}
			}

			// assignWindow
			if (findCustomCode(fileFullPath, ref codeList, out int lineStart2,
				(string line) => { return line.endWith("// auto generate assignWindow start"); },
				(string line) => { return line.endWith("// auto generate assignWindow end"); }, false))
			{
				foreach (string str in generatedAssignLines)
				{
					codeList.Insert(++lineStart2, str);
				}
			}
			// 找不到就在assignWindow的第一行插入
			else
			{
				for (int i = 0; i < codeList.Count; ++i)
				{
					if (codeList[i].Contains("public override void assignWindow()"))
					{
						int lineStart = i + 1;
						codeList.Insert(++lineStart, "\t\t// auto generate assignWindow start");
						foreach (string str in generatedAssignLines)
						{
							codeList.Insert(++lineStart, str);
						}
						codeList.Insert(++lineStart, "\t\t// auto generate assignWindow end");
						break;
					}
				}
			}
			writeTxtFile(fileFullPath, stringsToString(codeList, "\r\n"));
		}
	}
}