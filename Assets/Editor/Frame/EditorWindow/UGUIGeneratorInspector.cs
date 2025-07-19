using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
#if USE_TMP
using TMPro;
#endif
#if USE_AVPRO_VIDEO
using RenderHeads.Media.AVProVideo;
#endif
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static EditorCommonUtility;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(UGUIGenerator))]
public class UGUIGeneratorInspector : GameInspector
{
	protected List<MemberData> mTempNeedRemoveData;
	protected List<string> mTempTypeList = new();
	protected List<string> mTempActiveList = new() { "默认", "显示", "隐藏" };
	protected override void onGUI()
	{
		base.onGUI();
		EditorGUILayout.Space();
		var generator = target as UGUIGenerator;
		if (generator == null)
		{
			return;
		}
		toggle(ref generator.mIsPersistent, "常驻界面");
		if (button("添加", 300, 25))
		{
			generator.addNewItem();
		}
		for (int i = 0; i < generator.mMemberList.Count; ++i)
		{
			MemberData item = generator.mMemberList[i];
			using (new GUILayout.HorizontalScope())
			{
				GameObject newObj = objectField(item.mObject, 160);
				if (newObj != item.mObject)
				{
					if (!generator.mMemberList.Exists((obj) => { return obj.mObject == newObj; }))
					{
						item.mObject = newObj;
					}
					else
					{
						log("节点" + newObj.name + "已经在列表中了,不能重复添加");
						item.mObject = null;
					}
				}
				generateAvailableTypeList(mTempTypeList, item.mObject);
				int curIndex = mTempTypeList.IndexOf(item.mType);
				if (displayDropDown("", "", mTempTypeList, ref curIndex))
				{
					item.mType = mTempTypeList[curIndex];
				}
				toggle(ref item.mIsArray, "数组");
				if (item.mIsArray)
				{
					string lenStr = item.mArrayLength.ToString();
					if (textField(ref lenStr, 30))
					{
						int.TryParse(lenStr, out item.mArrayLength);
					}
				}
				displayDropDown("", "", mTempActiveList, ref item.mDefaultActive, 50);
				if (button("上移", 40))
				{
					if (i > 0)
					{
						generator.mMemberList.swap(i, i - 1);
					}
					break;
				}
				if (button("下移", 40))
				{
					if (i < generator.mMemberList.Count - 1)
					{
						generator.mMemberList.swap(i, i + 1);
					}
					break;
				}
				if (button("删除", 40))
				{
					mTempNeedRemoveData ??= new();
					mTempNeedRemoveData.Add(item);
				}
			}
		}
		if (mTempNeedRemoveData.count() > 0)
		{
			foreach (MemberData data in mTempNeedRemoveData)
			{
				generator.mMemberList.Remove(data);
			}
			mTempNeedRemoveData.Clear();
		}
		EditorGUILayout.Space(10);
		using (new GUILayout.VerticalScope())
		{
			if (button("生成代码", 300, 25))
			{
				PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
				if (prefabStage == null)
				{
					logError("需要再prefab编辑模式下执行");
					return;
				}
				// 需要先保存一下prefab,否则在第一次添加UGUIGenerator组件后点击生成代码会由于没有写入文件而没有生成界面的注册信息
				GameObject prefabAsset = prefabStage.prefabContentsRoot;
				EditorUtility.SetDirty(prefabAsset);
				PrefabUtility.SaveAsPrefabAsset(prefabAsset, prefabStage.assetPath);
				AssetDatabase.Refresh();
				// 生成代码之前需要确保变量的排序
				sortMemberList(generator);
				generateUICode(generator);
			}
			if (button("打开代码", 300, 25))
			{
				AssetDatabase.OpenAsset(loadAsset(findScript(generator.gameObject.name)));
			}
		}
	}
	protected static void generateNodeTree(Dictionary<GameObject, int> goList, GameObject root)
	{
		goList.Add(root, goList.Count);
		for (int i = 0; i < root.transform.childCount; ++i)
		{
			generateNodeTree(goList, root.transform.GetChild(i).gameObject);
		}
	}
	protected static void sortMemberList(UGUIGenerator generator)
	{
		Dictionary<GameObject, int> goList = new();
		generateNodeTree(goList, generator.gameObject);
		generator.mMemberList.Sort((a, b) => 
		{
			if (a.mObject == null)
			{
				return -1;
			}
			if (b.mObject == null)
			{
				return 1;
			}
			return sign(goList[a.mObject] - goList[b.mObject]); 
		});
	}
	protected static void generateUICode(UGUIGenerator generator)
	{
		foreach (MemberData data in generator.mMemberList)
		{
			if (data.mObject == null)
			{
				log("列表中包含空物体,无法生成代码");
				return;
			}
		}
		generateRegister();
		generateLayoutScript(generator);
	}
	protected static int getStringWidth(string str)
	{
		int width = 0;
		for (int i = 0; i < str.Length; ++i)
		{
			if (str[i] == '\t')
			{
				width += 4;
			}
			else
			{
				++width;
			}
		}
		return width;
	}
	protected static void appendWithAlign(ref string oriStr, string appendStr, int alignWidth)
	{
		int tabCount = ceil(clampMin(alignWidth - getStringWidth(oriStr)) / 4.0f);
		for (int i = 0; i < tabCount; ++i)
		{
			oriStr += '\t';
		}
		oriStr += appendStr;
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
	protected static string removeEndNumber(string name)
	{
		if (isNumeric(name))
		{
			return "";
		}
		for (int i = name.Length - 1; i >= 0; --i)
		{
			if (!isNumeric(name[i]))
			{
				return name.startString(i + 1);
			}
		}
		return name;
	}
	protected static string findScript(string fileNameNoDirNoSuffix)
	{
		List<string> fileList = new();
		findFiles(F_SCRIPTS_PATH, fileList);
		foreach (string file in fileList)
		{
			if (getFileNameNoSuffixNoDir(file) == fileNameNoDirNoSuffix)
			{
				return file;
			}
		}
		return null;
	}
	// 生成UI对应的脚本
	protected static void generateLayoutScript(UGUIGenerator generator)
	{
		string layoutName = generator.gameObject.name;
		// 先找一下有没有已经存在的UI脚本
		string fileFullPath = findScript(layoutName);
		List<string> tempCreatedList = new();
		List<string> generatedAssignLines = new();
		List<MemberData> tempDataList = new(generator.mMemberList);
		while (tempDataList.Count > 0)
		{
			MemberData data = tempDataList.get(0);
			generateNewObject(generatedAssignLines, tempDataList, generator.mMemberList, tempCreatedList, data, generator.gameObject);
		}

		List<string> memberDefineList = new();
		foreach (MemberData data in generator.mMemberList)
		{
			if (data.mIsArray)
			{
				// 移除名字中末尾的数字
				string newName = removeEndNumber(data.mObject.name);
				if (newName.isEmpty())
				{
					logError("标记为数组的节点名为纯数字,无法生成数组代码");
				}
				memberDefineList.Add("\tprotected " + data.mType + "[] m" + newName + " = new " + data.mType + "[" + data.mArrayLength + "];");
			}
			else
			{
				memberDefineList.Add("\tprotected " + data.mType + " m" + data.mObject.name + ";");
			}
		}

		if (fileFullPath.isEmpty())
		{
			fileFullPath = F_SCRIPTS_HOTFIX_UI_PATH + layoutName + ".cs";
			string fileContent = "";
			line(ref fileContent, "using Obfuz;");
			line(ref fileContent, "");
			line(ref fileContent, "[ObfuzIgnore(ObfuzScope.TypeName)]");
			line(ref fileContent, "public class " + layoutName + " : LayoutScript");
			line(ref fileContent, "{");
			line(ref fileContent, "\t// auto generate member start");
			foreach (string str in memberDefineList)
			{
				line(ref fileContent, str);
			}
			line(ref fileContent, "\t// auto generate member end");
			line(ref fileContent, "\tpublic " + layoutName + "(){}");
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
			// 找不到就在类的第一行插入
			else
			{
				for (int i = 0; i < codeList.Count; ++i)
				{
					if (codeList[i].Contains(" class " + generator.gameObject.name + " "))
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
			if (findCustomCode(fileFullPath, ref codeList, out int lineStart1,
				(string line) => { return line.endWith("// auto generate assignWindow start"); },
				(string line) => { return line.endWith("// auto generate assignWindow end"); }, false))
			{
				foreach (string str in generatedAssignLines)
				{
					codeList.Insert(++lineStart1, str);
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
	protected static void generateNewObject(List<string> generatedLines, List<MemberData> list, List<MemberData> fixedList, List<string> createdObject, MemberData curData, GameObject root)
	{
		string curObjName = curData.mObject.name;
		GameObject parent = curData.mObject.transform.parent.gameObject;
		// 父节点是界面的根节点,则不需要传父节点就可以直接创建
		if (parent == root)
		{
			int curDataIndex = fixedList.FindIndex((data) => { return data.mObject.name == curObjName; });
			// 创建的是成员变量
			if (curDataIndex >= 0)
			{
				if (curData.mIsArray)
				{
					string newName = removeEndNumber(curObjName);
					generatedLines.Add("\t\tfor (int i = 0; i < " + curData.mArrayLength + "; ++i)");
					generatedLines.Add("\t\t{");
					if (curData.mDefaultActive == 0)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], \"" + newName + "\" + IToS(i));");
					}
					else if (curData.mDefaultActive == 1)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], \"" + newName + "\" + IToS(i), 1);");
					}
					else if (curData.mDefaultActive == 2)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], \"" + newName + "\" + IToS(i), 0);");
					}
					generatedLines.Add("\t\t}");
				}
				else
				{
					if (curData.mDefaultActive == 0)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", \"" + curObjName + "\");");
					}
					else if (curData.mDefaultActive == 1)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", \"" + curObjName + "\", 1);");
					}
					else if (curData.mDefaultActive == 2)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", \"" + curObjName + "\", 0);");
					}
				}
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				string varName = curObjName.substr(0, 1).ToLower() + curObjName.removeStartCount(1);
				generatedLines.Add("\t\tnewObject(out " + typeof(myUGUIObject).ToString() + " " + varName + ", \"" + curObjName + "\");");
			}
			createdObject.add(curObjName);
			// 从列表中移除,避免再次被遍历到,如果是临时构造的数据,自己就会移除失败,也就无需关心
			list.Remove(curData);
			return;
		}

		// 如果父节点已经创建了,则可以创建
		if (createdObject.Contains(parent.name))
		{
			int curDataIndex = fixedList.FindIndex((data) => { return data.mObject.name == curObjName; });
			string parentName;
			// 父节点是成员变量
			if (fixedList.FindIndex((data) => { return data.mObject.name == parent.name; }) >= 0)
			{
				parentName = "m" + parent.name;
			}
			// 父节点是临时变量
			else
			{
				parentName = parent.name.substr(0, 1).ToLower() + parent.name.removeStartCount(1);
			}
			// 创建的是成员变量
			if (curDataIndex >= 0)
			{
				if (curData.mIsArray)
				{
					string newName = removeEndNumber(curObjName);
					generatedLines.Add("\t\tfor (int i = 0; i < " + curData.mArrayLength + "; ++i)");
					generatedLines.Add("\t\t{");
					if (curData.mDefaultActive == 0)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], " + parentName + ", \"" + newName + "\" + IToS(i));");
					}
					else if (curData.mDefaultActive == 1)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], " + parentName + ", \"" + newName + "\" + IToS(i), 1);");
					}
					else if (curData.mDefaultActive == 2)
					{
						generatedLines.Add("\t\t\tnewObject(out m" + newName + "[i], " + parentName + ", \"" + newName + "\" + IToS(i), 0);");
					}
					generatedLines.Add("\t\t}");
				}
				else
				{
					if (curData.mDefaultActive == 0)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", " + parentName + ", \"" + curObjName + "\");");
					}
					else if (curData.mDefaultActive == 1)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", " + parentName + ", \"" + curObjName + "\", 1);");
					}
					else if (curData.mDefaultActive == 2)
					{
						generatedLines.Add("\t\tnewObject(out m" + curObjName + ", " + parentName + ", \"" + curObjName + "\", 0);");
					}
				}
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				string varName = curObjName.substr(0, 1).ToLower() + curObjName.removeStartCount(1);
				generatedLines.Add("\t\tnewObject(out " + typeof(myUGUIObject).ToString() + " " + varName + ", " + parentName + ", \"" + curObjName + "\");");
			}
			createdObject.add(curObjName);
			list.Remove(curData);
			return;
		}

		// 父节点还没有创建,则需要判断父节点是否在成员列表中,如果不在,就需要创建临时的变量
		int parentIndex = list.FindIndex((data) => { return data.mObject.name == parent.name; });
		if (parentIndex >= 0)
		{
			// 递归创建父节点
			generateNewObject(generatedLines, list, fixedList, createdObject, list[parentIndex], root);
			// 创建自己
			generateNewObject(generatedLines, list, fixedList, createdObject, curData, root);
		}
		else
		{
			// 父节点只是一个临时节点,则需要先创建父节点
			MemberData parentData = new();
			parentData.mObject = parent;
			parentData.mType = typeof(myUGUIObject).ToString();
			generateNewObject(generatedLines, list, fixedList, createdObject, parentData, root);
			// 创建自己
			generateNewObject(generatedLines, list, fixedList, createdObject, curData, root);
		}
	}
	protected static void line(ref string fileContent, string str, bool addReturnLine = true)
	{
		fileContent += str;
		if (addReturnLine)
		{
			fileContent += "\r\n";
		}
	}
	protected static bool findCustomCode(string fullPath, ref List<string> codeList, out int lineStart,
								Func<string, bool> startLineMatch, Func<string, bool> endLineMatch, bool showError = true)
	{
		if (codeList == null)
		{
			codeList = new();
			codeList.setRange(openTxtFileLinesSync(fullPath));
		}
		lineStart = -1;
		int endCode = -1;
		for (int i = 0; i < codeList.count(); ++i)
		{
			if (lineStart < 0 && startLineMatch(codeList[i]))
			{
				lineStart = i;
				continue;
			}
			if (lineStart >= 0)
			{
				if (endLineMatch(codeList[i]))
				{
					endCode = i;
					break;
				}
			}
		}
		if (lineStart < 0)
		{
			if (showError)
			{
				logError("找不到代码特定起始段,文件名:" + fullPath);
			}
			return false;
		}
		if (endCode < 0)
		{
			if (showError)
			{
				logError("找不到代码特定结束段,文件名:" + fullPath);
			}
			return false;
		}
		int removeLineCount = endCode - lineStart - 1;
		for (int i = 0; i < removeLineCount; ++i)
		{
			codeList.RemoveAt(lineStart + 1);
		}
		return true;
	}
	protected static void generateAvailableTypeList(List<string> list, GameObject go)
	{
		list.Clear();
		if (go == null)
		{
			return;
		}
		list.Add(typeof(myUGUIObject).ToString());
		if (go.TryGetComponent<Image>(out _))
		{
			list.Add(typeof(myUGUIImageSimple).ToString());
			list.Add(typeof(myUGUIImage).ToString());
			list.Add(typeof(myUGUIImagePro).ToString());
			list.Add(typeof(myUGUIImageAnim).ToString());
			list.Add(typeof(myUGUIImageAnimPro).ToString());
			list.Add(typeof(myUGUINumber).ToString());
		}
		if (go.TryGetComponent<RawImage>(out _))
		{
			list.Add(typeof(myUGUIRawImage).ToString());
			list.Add(typeof(myUGUIRawImageAnim).ToString());
		}
		if (go.TryGetComponent<Text>(out _))
		{
			list.Add(typeof(myUGUIText).ToString());
		}
		if (go.TryGetComponent<InputField>(out _))
		{
			list.Add(typeof(myUGUIInputField).ToString());
		}
#if USE_TMP
		if (go.TryGetComponent<TextMeshProUGUI>(out _))
		{
			list.Add(typeof(myUGUITextTMP).ToString());
		}
		if (go.TryGetComponent<TMP_InputField>(out _))
		{
			list.Add(typeof(myUGUIInputFieldTMP).ToString());
		}
#endif
		if (go.TryGetComponent<Canvas>(out _))
		{
			list.Add(typeof(myUGUICanvas).ToString());
		}
		if (go.TryGetComponent<Button>(out _))
		{
			list.Add(typeof(myUGUIButton).ToString());
		}
		if (go.TryGetComponent<CustomLine>(out _))
		{
			list.Add(typeof(myUGUICustomLine).ToString());
		}
		if (go.TryGetComponent<Dropdown>(out _))
		{
			list.Add(typeof(myUGUIDropdown).ToString());
		}
		if (go.TryGetComponent<ImageNumber>(out _))
		{
			list.Add(typeof(myUGUIImageNumber).ToString());
		}
		if (go.TryGetComponent<Scrollbar>(out _))
		{
			list.Add(typeof(myUGUIScrollBar).ToString());
		}
		if (go.TryGetComponent<ScrollRect>(out _))
		{
			list.Add(typeof(myUGUIScrollRect).ToString());
		}
		if (go.TryGetComponent<Slider>(out _))
		{
			list.Add(typeof(myUGUISlider).ToString());
		}
		if (go.TryGetComponent<TextImage>(out _))
		{
			list.Add(typeof(myUGUITextImage).ToString());
		}
#if USE_AVPRO_VIDEO
		if (go.TryGetComponent<MediaPlayer>(out _))
		{
			list.Add(typeof(myUGUIVideo).ToString());
		}
#endif
		list.Add(typeof(myUGUIDragView).ToString());
	}
}