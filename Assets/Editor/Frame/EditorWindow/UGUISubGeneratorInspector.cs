﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using static UnityUtility;
using static StringUtility;
using static FileUtility;
using static FrameDefine;
using static EditorCommonUtility;
using static UGUIGeneratorUtility;

[CustomEditor(typeof(UGUISubGenerator))]
public class UGUISubGeneratorInspector : GameInspector
{
	protected override void onGUI()
	{
		base.onGUI();
		EditorGUILayout.Space();
		var generator = target as UGUISubGenerator;
		if (generator == null)
		{
			return;
		}
		using (new EditorModifyScope(this))
		{
			// 基类类型
			using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
			{
				if (!generator.mOnlyForMarkType)
				{
					label("基类:");
					List<string> parentList = getSubUIParentList();
					if (generator.mParentType.isEmpty() || !parentList.Contains(generator.mParentType))
					{
						generator.mParentType = parentList.get(0) ?? typeof(LayoutScript).ToString();
					}
					displayDropDown("", "", parentList, ref generator.mParentType, 250);
				}
				toggle(ref generator.mAutoType, "自动设置类名");
				if (!generator.mAutoType)
				{
					textField(ref generator.mCustomClassName, 150);
				}
				else
				{
					generator.mCustomClassName = generator.gameObject.name.removeEndNumber();
				}
				toggle(ref generator.mOnlyForMarkType, "仅用于标记类型", "是否仅用于标记类型,而不是用于生成代码");
			}
			if (!generator.mOnlyForMarkType)
			{
				drawMemberInspector(generator);
			}
		}
		EditorGUILayout.Space(10);
		using (new GUILayout.VerticalScope())
		{
			if (!generator.mOnlyForMarkType)
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

					generateScript(generator);
				}
			}
			if (button("打开代码", 300, 25))
			{
				AssetDatabase.OpenAsset(loadAsset(findScript(getClassNameFromGameObject(generator.gameObject))));
			}
		}
	}
	protected static void generateScript(UGUISubGenerator generator)
	{
		string subUIName = getClassNameFromGameObject(generator.gameObject);
		// 先找一下有没有已经存在的子UI脚本
		string fileFullPath = findScript(subUIName);

		// 成员变量定义的代码
		List<string> memberDefineList = new();
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

		// assignWindowInternal中的代码
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
			fileFullPath = F_SCRIPTS_HOTFIX_UI_PATH + "InnerClass/" + subUIName + ".cs";
			string fileContent = "";
			// 对象池节点的代码会特殊判断
			if (generator.mParentType == "DragViewItem")
			{
				line(ref fileContent, "public class " + subUIName + " : " + generator.mParentType + "<" + subUIName + ".Data>");
				line(ref fileContent, "{");
				line(ref fileContent, "\tpublic class Data : ClassObject");
				line(ref fileContent, "\t{");
				line(ref fileContent, "\t\tpublic override void resetProperty()");
				line(ref fileContent, "\t\t{}");
				line(ref fileContent, "\t}");
			}
			else
			{
				line(ref fileContent, "public class " + subUIName + " : " + generator.mParentType);
				line(ref fileContent, "{");
			}

			// 成员变量定义
			if (generator.mParentType == "DragViewItem")
			{
				line(ref fileContent, "\t// auto generate member start");
			}
			else
			{
				line(ref fileContent, "// auto generate member start");
			}
			foreach (string str in memberDefineList)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t// auto generate member end");

			// 构造函数
			if (constructorLines.isEmpty())
			{
				line(ref fileContent, "\tpublic " + subUIName + "(IWindowObjectOwner parent) : base(parent){}");
			}
			else
			{
				line(ref fileContent, "\tpublic " + subUIName + "(IWindowObjectOwner parent) : base(parent)");
				line(ref fileContent, "\t{");
				line(ref fileContent, "\t\t// auto generate constructor start");
				foreach (string str in constructorLines)
				{
					line(ref fileContent, str);
				}
				line(ref fileContent, "\t\t// auto generate constructor end");
				line(ref fileContent, "\t}");
			}

			// assignWindowInternal
			line(ref fileContent, "\tprotected override void assignWindowInternal()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\tbase.assignWindowInternal();");
			line(ref fileContent, "\t\t// auto generate assignWindowInternal start");
			foreach (string str in generatedAssignLines)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t\t// auto generate assignWindowInternal end");
			line(ref fileContent, "\t}");

			// 初始化
			line(ref fileContent, "\tpublic override void init()");
			line(ref fileContent, "\t{");
			line(ref fileContent, "\t\tbase.init();");
			line(ref fileContent, "\t}");

			// 对象池节点对象需要SetData
			if (generator.mParentType == "DragViewItem")
			{
				line(ref fileContent, "\tpublic override void setData(Data data)");
				line(ref fileContent, "\t{");
				line(ref fileContent, "\t}");
			}
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
					if (codeList[i].Contains(" class " + subUIName + " "))
					{
						int lineStart = i + 1;
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

			// 构造函数,即使为空的,也要查找,可以去除旧的不用的行
			// 
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
				if (!constructorLines.isEmpty())
				{
					for (int i = 0; i < codeList.Count; ++i)
					{
						if (codeList[i].Contains("public " + subUIName + "(IWindowObjectOwner "))
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
			}

			// assignWindowInternal
			if (findCustomCode(fileFullPath, ref codeList, out int lineStart2,
				(string line) => { return line.endWith("// auto generate assignWindowInternal start"); },
				(string line) => { return line.endWith("// auto generate assignWindowInternal end"); }, false))
			{
				foreach (string str in generatedAssignLines)
				{
					codeList.Insert(++lineStart2, str);
				}
			}
			// 找不到就在assignWindowInternal的第一行插入
			else
			{
				for (int i = 0; i < codeList.Count; ++i)
				{
					if (codeList[i].Contains("protected override void assignWindowInternal()"))
					{
						int lineStart = i + 2;
						codeList.Insert(++lineStart, "\t\t// auto generate assignWindowInternal start");
						foreach (string str in generatedAssignLines)
						{
							codeList.Insert(++lineStart, str);
						}
						codeList.Insert(++lineStart, "\t\t// auto generate assignWindowInternal end");
						break;
					}
				}
			}
			writeTxtFile(fileFullPath, stringsToString(codeList, "\r\n"));
		}
	}
}