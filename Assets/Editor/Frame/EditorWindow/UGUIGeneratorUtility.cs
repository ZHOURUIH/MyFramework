using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
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
using static GameInspector;

public class UGUIGeneratorUtility
{
	protected static List<string> mCommonSubUITypeList = new();
	protected static List<string> mSubUIWithGenericTypeList = new();
	protected static List<string> mPoolTypeList = new();
	protected static List<string> mSubUIParentList = new();
	protected static List<string> mUIParentList = new();
	protected static List<string> mNormalWindowList = new();
	protected static List<string> mTempAvailableTypeList = new();
	public static void drawMemberInspector(UGUIGeneratorBase generator)
	{
		using (new GUILayout.HorizontalScope())
		{
			if (button("添加节点", 200, 25))
			{
				generator.addNewItem();
			}
			if (button("添加对象池", 200, 25))
			{
				generator.addNewPool();
			}
			if (button("添加滚动列表", 200, 25))
			{
				generator.addScrollList();
			}
		}
		List<MemberData> tempNeedRemoveData = null;
		for (int i = 0; i < generator.mMemberList.Count; ++i)
		{
			MemberData item = generator.mMemberList[i];
			using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
			{
				if (button("X", 25))
				{
					tempNeedRemoveData ??= new();
					tempNeedRemoveData.addUnique(item);
				}
				if (item.mWindowType != WINDOW_TYPE.POOL && item.mWindowType != WINDOW_TYPE.SCROLL_LIST)
				{
					GameObject newObj = objectField(item.mObject, 160);
					if (newObj != item.mObject)
					{
						if (newObj == generator.gameObject)
						{
							log("不能添加根节点");
							newObj = null;
						}
						if (generator.mMemberList.Exists((obj) => { return obj.mObject == newObj && newObj != null; }))
						{
							log("节点" + newObj.name + "已经在列表中了,不能重复添加");
							item.mObject = null;
						}
						else
						{
							item.mObject = newObj;
							// 如果是以0结尾的,就自动设置为静态数组类型的,且自动查找数组长度
							if (item.mObject != null)
							{
								string name = item.mObject.name;
								if (getLastNotNumberPos(name) == name.Length - 2 && name.endWith("0"))
								{
									item.mArrayType = ARRAY_TYPE.STATIC_ARRAY;
									item.autoSetArrayLength();
								}
							}
						}
					}
				}
				else
				{
					space(70);
					toggle(ref item.mUseCustomName, "自定义变量名");
					if (item.mUseCustomName)
					{
						textField(ref item.mCustomName, 100);
					}
				}
				int curWindowIndex = (int)item.mWindowType;
				if (displayDropDown("", "", MemberData.mWindowTypeDropList, ref curWindowIndex, 70))
				{
					item.setWindowType((WINDOW_TYPE)curWindowIndex);
				}

				List<string> typeList = null;
				switch (item.mWindowType)
				{
					case WINDOW_TYPE.NORMAL_WINDOW:
						{
							typeList = generateAvailableTypeList(item.mObject);
						}
						break;
					case WINDOW_TYPE.COMMON_SUB_UI: typeList = getCommonSubUITypeList(); break;
					case WINDOW_TYPE.SUB_PANEL: break;
					case WINDOW_TYPE.SCROLL_LIST: typeList = getSubUIWithGenericTypeList(); break;
					case WINDOW_TYPE.POOL: typeList = getPoolTypeList(); break;
				}
				if (typeList == null && item.mWindowType != WINDOW_TYPE.SUB_PANEL)
				{
					Debug.LogError("未知的WindowType:" + item.mWindowType);
					return;
				}

				// 子页面特殊判断,类型名要跟节点名字匹配
				if (item.mWindowType == WINDOW_TYPE.SUB_PANEL)
				{
					item.mType = getClassNameFromGameObject(item.mObject);
					labelWidth(item.mType, 148, ClassTypeCaches.hasClass(item.mType) ? Color.green : Color.red);
				}
				else
				{
					if ((item.mType.isEmpty() || !typeList.Contains(item.mType)) && typeList.Count > 0)
					{
						item.mType = typeList.get(0) ?? typeof(myUGUIObject).ToString();
					}
					displayDropDown("", "", typeList, ref item.mType);
				}

				int curArrayTypeIndex = (int)item.mArrayType;
				if (displayDropDown("", "", MemberData.mArrayTypeDropList, ref curArrayTypeIndex, 70))
				{
					item.mArrayType = (ARRAY_TYPE)curArrayTypeIndex;
				}
				if (item.mArrayType != ARRAY_TYPE.NONE)
				{
					if (item.mArrayType == ARRAY_TYPE.STATIC_ARRAY)
					{
						item.autoSetArrayLength();
					}
					string lenStr = item.mArrayLength.ToString();
					if (textField(ref lenStr, 30))
					{
						int.TryParse(lenStr, out item.mArrayLength);
					}
				}
				toggle(ref item.mHideError, "不显示错误");
			}

			// 有模板参数的类型
			if (item.mType == "UGUIDragViewLoop")
			{
				drawTemplateParamUGUIDragViewLoop(item);
			}
			else if (item.mType == "WindowStructPool")
			{
				drawTemplateParamWindowStructPool(item);
			}
			else if (item.mType == "WindowStructPoolMap")
			{
				drawTemplateParamWindowStructPoolMap(item);
			}
			else if (item.mType == "WindowStructPoolUnOrder")
			{
				// WindowStructPoolUnOrder跟WindowStructPool一样的参数结构
				drawTemplateParamWindowStructPool(item);
			}
			else if (item.mType == "WindowPool")
			{
				drawTemplateParamWindowPool(item);
			}
		}
		if (tempNeedRemoveData.count() > 0)
		{
			foreach (MemberData data in tempNeedRemoveData)
			{
				generator.mMemberList.Remove(data);
			}
		}
	}
	protected static void drawTemplateParamUGUIDragViewLoop(MemberData data)
	{
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			label("Viewport节点:");
			data.mViewportObject = objectField(data.mViewportObject, 160);
		}
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			label("模板节点:");
			data.mPoolTemplate = objectField(data.mPoolTemplate, 160);
			data.mParam0 = getClassNameFromGameObject(data.mPoolTemplate);
			label("模板参数0:" + data.mParam0, ClassTypeCaches.hasClass(data.mParam0) ? Color.green : Color.red);
		}
	}
	protected static void drawTemplateParamWindowStructPool(MemberData data)
	{
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			label("模板节点:");
			data.mPoolTemplate = objectField(data.mPoolTemplate, 160);
			data.mParam0 = getClassNameFromGameObject(data.mPoolTemplate);
			label("模板参数0:" + data.mParam0, ClassTypeCaches.hasClass(data.mParam0) ? Color.green : Color.red);
			label("模板根窗口类型:");
			if (data.mTemplateWindowType.isEmpty())
			{
				data.mTemplateWindowType = typeof(myUGUIObject).ToString();
			}
			displayDropDown("", "", generateAvailableTypeList(data.mPoolTemplate), ref data.mTemplateWindowType);
		}
	}
	protected static void drawTemplateParamWindowStructPoolMap(MemberData data)
	{
		// 第一个模板参数,一般都是基础数据类型,或者其他非UI的类型,需要自己输入
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			label("模板参数0:");
			textField(ref data.mParam0, 120);
		}
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			label("模板节点:");
			data.mPoolTemplate = objectField(data.mPoolTemplate, 160);
			data.mParam1 = getClassNameFromGameObject(data.mPoolTemplate);
			label("模板参数1:" + data.mParam1, ClassTypeCaches.hasClass(data.mParam1) ? Color.green : Color.red);
		}
	}
	protected static void drawTemplateParamWindowPool(MemberData data)
	{
		// 第一个模板参数
		using (new GUILayout.HorizontalScope(GUILayout.Width(200)))
		{
			space(100);
			data.mPoolTemplate = objectField(data.mPoolTemplate, 160);
			if (data.mParam0.isEmpty())
			{
				data.mParam0 = typeof(myUGUIObject).ToString();
			}
			displayDropDown("", "", generateAvailableTypeList(data.mPoolTemplate), ref data.mParam0);
		}
	}
	public static string getClassNameFromGameObject(GameObject go)
	{
		if (go == null)
		{
			return "";
		}
		if (go.TryGetComponent(out UGUISubGenerator com))
		{
			return com.mAutoType ? go.name.removeEndNumber() : com.mCustomClassName;
		}
		return go.name.removeEndNumber();
	}
	public static List<string> getSubUIParentList(bool refresh = false)
	{
		if (refresh)
		{
			mSubUIParentList.Clear();
		}
		if (mSubUIParentList.Count == 0)
		{
			mSubUIParentList.Add(typeof(WindowObjectUGUI).ToString());
			mSubUIParentList.Add(typeof(WindowRecycleableUGUI).ToString());
			mSubUIParentList.Add("DragViewItem");
		}
		return mSubUIParentList;
	}
	public static List<string> getUIParentList(bool refresh = false)
	{
		if (refresh)
		{
			mUIParentList.Clear();
		}
		if (mUIParentList.Count == 0)
		{
			mUIParentList.Add(typeof(LayoutScript).ToString());
			mUIParentList.AddRange(EditorDefine.getLayoutScriptBaseClass());
		}
		return mUIParentList;
	}
	public static List<string> getSubUIWithGenericTypeList(bool refresh = false)
	{
		if (refresh)
		{
			mSubUIWithGenericTypeList.Clear();
		}
		if (mSubUIWithGenericTypeList.Count == 0)
		{
			mSubUIWithGenericTypeList.Add("UGUIDragViewLoop");
		}
		return mSubUIWithGenericTypeList;
	}
	public static List<string> getPoolTypeList(bool refresh = false)
	{
		if (refresh)
		{
			mPoolTypeList.Clear();
		}
		if (mPoolTypeList.Count == 0)
		{
			mPoolTypeList.Add("WindowStructPool");
			mPoolTypeList.Add("WindowStructPoolMap");
			mPoolTypeList.Add("WindowStructPoolUnOrder");
			mPoolTypeList.Add("WindowPool");
		}
		return mPoolTypeList;
	}
	public static List<string> getCommonSubUITypeList(bool refresh = false)
	{
		if (refresh)
		{
			mCommonSubUITypeList.Clear();
		}
		if (mCommonSubUITypeList.Count == 0)
		{
			mCommonSubUITypeList.AddRange(getTypesInFrameHotFixDll<ICommonUI>());
			mCommonSubUITypeList.AddRange(getTypesInHotFixDll<ICommonUI>());
		}
		return mCommonSubUITypeList;
	}
	public static void generateNodeTree(Dictionary<GameObject, int> goList, GameObject root)
	{
		goList.Add(root, goList.Count);
		for (int i = 0; i < root.transform.childCount; ++i)
		{
			generateNodeTree(goList, root.transform.GetChild(i).gameObject);
		}
	}
	public static void sortMemberList(UGUIGeneratorBase generator)
	{
		// 删掉空节点再进行排序
		for (int i = 0; i < generator.mMemberList.Count; ++i)
		{
			if (generator.mMemberList[i].mObject == null && 
				generator.mMemberList[i].mWindowType != WINDOW_TYPE.POOL &&
				generator.mMemberList[i].mWindowType != WINDOW_TYPE.SCROLL_LIST)
			{
				generator.mMemberList.removeAt(i--);
			}
		}
		Dictionary<GameObject, int> goList = new();
		generateNodeTree(goList, generator.gameObject);
		generator.mMemberList.Sort((a, b) =>
		{
			// 带模板参数的始终要排在后面,为了保证都在一起,而且不会因为前置节点未创建而导致错误
			if (a.mWindowType == WINDOW_TYPE.POOL || a.mWindowType == WINDOW_TYPE.SCROLL_LIST)
			{
				return 1;
			}
			if (b.mWindowType == WINDOW_TYPE.POOL || b.mWindowType == WINDOW_TYPE.SCROLL_LIST)
			{
				return -1;
			}
			if (a.mObject == null)
			{
				return -1;
			}
			if (b.mObject == null)
			{
				return 1;
			}
			if (!goList.ContainsKey(a.mObject) || !goList.ContainsKey(a.mObject))
			{
				Debug.LogError("排序时找不到节点,请确保选中的是编辑状态下的prefab根节点");
				return 0;
			}
			return sign(goList.get(a.mObject) - goList.get(b.mObject));
		});
	}
	public static int getStringWidth(string str)
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
	public static void appendWithAlign(ref string oriStr, string appendStr, int alignWidth)
	{
		int tabCount = ceil(clampMin(alignWidth - getStringWidth(oriStr)) / 4.0f);
		for (int i = 0; i < tabCount; ++i)
		{
			oriStr += '\t';
		}
		oriStr += appendStr;
	}
	public static string findScript(string fileNameNoDirNoSuffix)
	{
		List<string> fileList = new();
		findFiles(F_SCRIPTS_PATH, fileList, ".cs");
		foreach (string file in fileList)
		{
			if (getFileNameNoSuffixNoDir(file) == fileNameNoDirNoSuffix)
			{
				return file;
			}
		}
		return null;
	}
	public static void generateNewObject(List<string> generatedLines, List<MemberData> list, List<MemberData> fixedList, List<string> createdObject, MemberData curData, GameObject root)
	{
		string curObjName = curData.getMemberName();
		GameObject parent = curData.getParentObject();
		if (parent == null)
		{
			list.Remove(curData);
			return;
		}
		// 父节点是界面的根节点,则不需要传父节点就可以直接创建
		if (parent == root)
		{
			int curDataIndex = fixedList.IndexOf(curData);
			// 创建的是成员变量
			if (curDataIndex >= 0)
			{
				generateAssignWindowLine("\t\t", generatedLines, curObjName, null, false, curData);
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				generateAssignWindowLineTemp("\t\t", generatedLines, curObjName, null, false);
			}
			createdObject.add(curObjName);
			// 从列表中移除,避免再次被遍历到,如果是临时构造的数据,自己就会移除失败,也就无需关心
			list.Remove(curData);
			return;
		}

		// 如果父节点已经创建了,则可以创建
		if (createdObject.Contains(parent.name))
		{
			int curDataIndex = fixedList.IndexOf(curData);
			string parentName;
			bool parentIsSubUI = false;
			// 父节点是成员变量
			MemberData parentData = fixedList.Find((data) => { return data.mObject != null && data.mObject.name == parent.name; });
			if (parentData != null)
			{
				parentName = "m" + parent.name;
				parentIsSubUI = parentData.mWindowType != WINDOW_TYPE.NORMAL_WINDOW;
			}
			// 父节点是临时变量
			else
			{
				parentName = parent.name.substr(0, 1).ToLower() + parent.name.removeStartCount(1);
			}
			// 创建的是成员变量
			if (curDataIndex >= 0)
			{
				generateAssignWindowLine("\t\t", generatedLines, curObjName, parentName, parentIsSubUI, curData);
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				generateAssignWindowLineTemp("\t\t", generatedLines, curObjName, parentName, parentIsSubUI);
			}
			createdObject.add(curObjName);
			list.Remove(curData);
			return;
		}

		// 父节点还没有创建,则需要判断父节点是否在成员列表中,如果不在,就需要创建临时的变量
		int parentIndex = list.FindIndex((data) => { return data.mObject != null && data.mObject.name == parent.name; });
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
	// 会将创建的临时变量的名字返回出去
	public static string generateAssignWindowLineTemp(string prefix, List<string> lines, string curName, string parentName, bool parentIsSubUI)
	{
		string varName = curName.substr(0, 1).ToLower() + curName.removeStartCount(1);
		// 如果已经添加过了getRoot,就不用再重复添加了
		if (parentIsSubUI && parentName != null && !parentName.endWith(".getRoot()"))
		{
			parentName += ".getRoot()";
		}
		string parentParam = parentName != null ? parentName + ", " : "";
		// 所有的临时变量都需要不显示错误,因为可能会跟之前重复了,但是无法通过名字来判断是否重复,比如之前创建的是数组元素
		lines.Add(prefix + "newObject(out " + typeof(myUGUIObject).ToString() + " " + varName + ", " + parentParam + "\"" + curName + "\", false);");
		return varName;
	}
	public static void generateAssignWindowLine(string prefix, List<string> lines, string curName, string parentName, bool parentIsSubUI, MemberData data)
	{
		string newName = data.mArrayType == ARRAY_TYPE.STATIC_ARRAY ? curName.removeEndNumber() : curName;
		if (parentIsSubUI && parentName != null)
		{
			parentName += ".getRoot()";
		}
		if (data.mArrayType != ARRAY_TYPE.NONE)
		{
			// 动态列表只支持控件或者子页面类型的
			if (data.mArrayType == ARRAY_TYPE.DYNAMIC_ARRAY)
			{
				if (data.mWindowType == WINDOW_TYPE.COMMON_SUB_UI || data.mWindowType == WINDOW_TYPE.SUB_PANEL)
				{
					string varName = generateAssignWindowLineTemp(prefix, lines, curName, parentName, false);
					lines.Add(prefix + "for (int i = 0; i < m" + newName + ".Length; ++i)");
					lines.Add(prefix + "{");
					string parentParam = parentName ?? "mRoot";
					lines.Add(prefix + "\tm" + newName + "[i].assignWindow(" + parentParam + ", " + varName + ", \"" + newName + "\" + IToS(i));");
					lines.Add(prefix + "}");
					// 动态生成的数组都需要把模板节点隐藏起来
					lines.Add(prefix + varName + ".setActive(false);");
				}
			}
			else
			{
				lines.Add(prefix + "for (int i = 0; i < m" + newName + ".Length; ++i)");
				lines.Add(prefix + "{");
				if (data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
				{
					string showErrorParam = data.mHideError ? ", false" : "";
					string parentParam = parentName != null ? parentName + ", " : "";
					lines.Add(prefix + "\tnewObject(out m" + newName + "[i], " + parentParam + "\"" + newName + "\" + IToS(i)" + showErrorParam + ");");
				}
				else
				{
					string parentParam = parentName ?? "mRoot";
					lines.Add(prefix + "\tm" + newName + "[i].assignWindow(" + parentParam + ", \"" + newName + "\" + IToS(i));");
				}
				lines.Add(prefix + "}");
			}
		}
		else
		{
			if (data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
			{
				string showErrorParam = data.mHideError ? ", false" : "";
				string parentParam = parentName != null ? parentName + ", " : "";
				lines.Add(prefix + "newObject(out m" + curName + ", " + parentParam + "\"" + curName + "\"" + showErrorParam + ");");
			}
			else if (data.mWindowType == WINDOW_TYPE.SUB_PANEL || data.mWindowType == WINDOW_TYPE.COMMON_SUB_UI)
			{
				string parentParam = parentName ?? "mRoot";
				lines.Add(prefix + "m" + curName + ".assignWindow(" + parentParam + ", \"" + curName + "\");");
			}
			else if (data.mWindowType == WINDOW_TYPE.SCROLL_LIST)
			{
				string parentParam = parentName ?? "mRoot";
				lines.Add(prefix + "m" + curName + ".assignWindow(" + parentParam + ", \"" + data.mViewportObject.name + "\");");
				lines.Add(prefix + "m" + curName + ".assignTemplate(\"" + data.mPoolTemplate.name + "\");");
			}
			else if (data.mWindowType == WINDOW_TYPE.POOL)
			{
				string templateTypeStr = data.mTemplateWindowType.isEmpty() || data.mTemplateWindowType == typeof(myUGUIObject).ToString() ? "" : "<" + data.mTemplateWindowType + ">";
				string parentParam = parentName != null ? parentName + ", " : "mRoot, ";
				if (data.mType == "WindowStructPoolMap")
				{
					lines.Add(prefix + "m" + curName + ".assignTemplate" + templateTypeStr + "(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
				}
				else if (data.mType == "WindowPool")
				{
					lines.Add(prefix + "m" + curName + ".assignTemplate" + templateTypeStr + "(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
				}
				else
				{
					lines.Add(prefix + "m" + curName + ".assignTemplate" + templateTypeStr + "(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
				}
			}
		}
	}
	public static bool findCustomCode(string fullPath, ref List<string> codeList, out int lineStart,
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
	// 指定类型是否需要生成assignWindow代码
	public static List<string> generateAvailableTypeList(GameObject go)
	{
		mTempAvailableTypeList.Clear();
		if (go == null)
		{
			return mTempAvailableTypeList;
		}
		// 根据优先级放入类型列表,一般有这些组件就是要用特定的功能,所以会优先加进去
		// 其他的组件可能不会在代码中进行访问,所以优先级较低
		if (go.TryGetComponent<Canvas>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUICanvas).ToString());
		}
		if (go.TryGetComponent<Button>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIButton).ToString());
		}
		if (go.TryGetComponent<CustomLine>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUICustomLine).ToString());
		}
		if (go.TryGetComponent<Dropdown>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIDropdown).ToString());
		}
		if (go.TryGetComponent<ImageNumber>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIImageNumber).ToString());
		}
		if (go.TryGetComponent<Scrollbar>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIScrollBar).ToString());
		}
		if (go.TryGetComponent<ScrollRect>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIScrollRect).ToString());
		}
		if (go.TryGetComponent<Slider>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUISlider).ToString());
		}
		if (go.TryGetComponent<TextImage>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUITextImage).ToString());
		}
#if USE_AVPRO_VIDEO
		if (go.TryGetComponent<MediaPlayer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIVideo).ToString());
		}
#endif
		if (go.TryGetComponent<InputField>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIInputField).ToString());
		}
#if USE_TMP
		if (go.TryGetComponent<TMP_InputField>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIInputFieldTMP).ToString());
		}
#endif

		mTempAvailableTypeList.Add(typeof(myUGUIObject).ToString());
		if (go.TryGetComponent<Image>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIImageSimple).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImage).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImagePro).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImageAnim).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImageAnimPro).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUINumber).ToString());
		}
		if (go.TryGetComponent<RawImage>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIRawImage).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIRawImageAnim).ToString());
		}
		if (go.TryGetComponent<Text>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIText).ToString());
#if USE_TMP
			mTempAvailableTypeList.Add(typeof(myUGUITextAuto).ToString());
#endif
		}
#if USE_TMP
		if (go.TryGetComponent<TextMeshProUGUI>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUITextTMP).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUITextAuto).ToString());
		}
#endif
		if (go.TryGetComponent<MeshRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUILineMesh).ToString());
		}
		if (go.TryGetComponent<LineRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUILineRenderer).ToString());
		}
		mTempAvailableTypeList.Add(typeof(myUGUIDragView).ToString());
		return mTempAvailableTypeList;
	}
	public static List<string> getNormalWindowTypeList()
	{
		if (mNormalWindowList.Count == 0)
		{
			mNormalWindowList.Add(typeof(myUGUIObject).ToString());
			mNormalWindowList.Add(typeof(myUGUIText).ToString());
			mNormalWindowList.Add(typeof(myUGUITextTMP).ToString());
			mNormalWindowList.Add(typeof(myUGUITextAuto).ToString());
			mNormalWindowList.Add(typeof(myUGUIImageSimple).ToString());
			mNormalWindowList.Add(typeof(myUGUIImage).ToString());
			mNormalWindowList.Add(typeof(myUGUIImagePro).ToString());
			mNormalWindowList.Add(typeof(myUGUIImageAnim).ToString());
			mNormalWindowList.Add(typeof(myUGUIImageAnimPro).ToString());
			mNormalWindowList.Add(typeof(myUGUIInputField).ToString());
			mNormalWindowList.Add(typeof(myUGUIInputFieldTMP).ToString());
			mNormalWindowList.Add(typeof(myUGUIImageButton).ToString());
			mNormalWindowList.Add(typeof(myUGUIImageNumber).ToString());
			mNormalWindowList.Add(typeof(myUGUINumber).ToString());
			mNormalWindowList.Add(typeof(myUGUIRawImage).ToString());
			mNormalWindowList.Add(typeof(myUGUIRawImageAnim).ToString());
			mNormalWindowList.Add(typeof(myUGUIScrollBar).ToString());
			mNormalWindowList.Add(typeof(myUGUIScrollRect).ToString());
			mNormalWindowList.Add(typeof(myUGUISlider).ToString());
			mNormalWindowList.Add(typeof(myUGUIButton).ToString());
			mNormalWindowList.Add(typeof(myUGUICanvas).ToString());
			mNormalWindowList.Add(typeof(myUGUICustomLine).ToString());
			mNormalWindowList.Add(typeof(myUGUIDragView).ToString());
			mNormalWindowList.Add(typeof(myUGUIDropdown).ToString());
			mNormalWindowList.Add(typeof(myUGUITextImage).ToString());
			mNormalWindowList.Add(typeof(myUGUILineMesh).ToString());
			mNormalWindowList.Add(typeof(myUGUILineRenderer).ToString());
			mNormalWindowList.Add(typeof(myUGUITextImage).ToString());
#if USE_AVPRO_VIDEO
			mNormalWindowList.Add(typeof(myUGUIVideo).ToString());
#endif
		}
		return mNormalWindowList;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static bool hasTypeInHotFixDll(string targetType)
	{
		foreach (Type type in Assembly.LoadFrom(F_PROJECT_PATH + "Library/ScriptAssemblies/HotFix.dll").GetTypes())
		{
			// 跳过无法加载的类型、接口和抽象类
			if (type == null || type.IsInterface || type.IsAbstract)
			{
				continue;
			}
			if (type.ToString() == targetType)
			{
				return true;
			}
		}
		return false;
	}
	// 从 DLL 文件中筛选实现指定接口的非抽象类
	protected static List<string> getTypesInHotFixDll<T>()
	{
		return getTypesInDll(F_PROJECT_PATH + "Library/ScriptAssemblies/HotFix.dll", typeof(T));
	}
	// 从 DLL 文件中筛选实现指定接口的非抽象类
	protected static List<string> getTypesInFrameHotFixDll<T>()
	{
		return getTypesInDll(F_PROJECT_PATH + "Library/ScriptAssemblies/Frame_HotFix.dll", typeof(T));
	}
	// 从 DLL 文件中筛选实现指定接口的非抽象类,keepGenericMark是否保留模板参数类型显示,默认不保留,只获取类名本身
	protected static List<string> getTypesInDll(string dllFullPath, Type targetType, bool keepGenericMark = false)
	{
		// 获取所有类型（捕获加载异常）
		List<string> typeList = new();
		foreach (Type type in Assembly.LoadFrom(dllFullPath).GetTypes())
		{
			// 跳过无法加载的类型、接口和抽象类
			if (type == null || type.IsInterface || type.IsAbstract)
			{
				continue;
			}
			// 检查是否实现了目标接口
			if (isImplementsType(type, targetType))
			{
				if (keepGenericMark)
				{
					typeList.Add(type.ToString());
				}
				else
				{
					typeList.Add(type.ToString().rangeToFirst('`'));
				}
			}
		}
		return typeList;
	}
	// 判断类型是否直接或间接实现目标接口
	protected static bool isImplementsType(Type type, Type targetType)
	{
		if (targetType == null)
		{
			return false;
		}
		// 接口类型判断
		if (targetType.IsInterface)
		{
			foreach (Type iface in type.GetInterfaces())
			{
				if (IsMatchingGenericType(iface, targetType))
				{
					return true;
				}
			}
			return false;
		}
		// 基类判断
		else
		{
			// 遍历继承链
			Type currentType = type;
			while (currentType != null && currentType != typeof(object))
			{
				if (IsMatchingGenericType(currentType, targetType))
				{
					return true;
				}
				currentType = currentType.BaseType;
			}
			return false;
		}
	}
	// 匹配泛型类型定义（支持开放/封闭泛型）
	protected static bool IsMatchingGenericType(Type typeToCheck, Type targetType)
	{
		// 泛型定义匹配（例如：IEnumerable<>）
		if (targetType.IsGenericTypeDefinition)
		{
			return typeToCheck.IsGenericType &&
				   typeToCheck.GetGenericTypeDefinition() == targetType;
		}
		// 具体类型匹配（包括封闭泛型如 IEnumerable<int>）
		else
		{
			return typeToCheck == targetType;
		}
	}
}