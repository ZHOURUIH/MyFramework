using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
#if USE_AVPRO_VIDEO
using RenderHeads.Media.AVProVideo;
#endif
using static UnityUtility;
using static FileUtility;
using static StringUtility;
using static MathUtility;
using static FrameDefine;
using static GameInspector;
using static FrameUtility;
using static FrameBaseDefine;

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
		label("可以选中节点再按Ctrl+W将节点添加到下面");
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
			if (button("添加无限滚动列表", 200, 25))
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
				drawMemberLine(generator, item, ref tempNeedRemoveData);
			}

			// 有模板参数的类型
			if (item.mWindowType == WINDOW_TYPE.POOL)
			{
				if (item.mType == "WindowStructPool")
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
			else if (item.mWindowType == WINDOW_TYPE.SCROLL_LIST)
			{
				if (item.mType == "UGUIDragViewLoop")
				{
					drawTemplateParamUGUIDragViewLoop(item);
				}
			}
		}
		generator.mMemberList.remove(tempNeedRemoveData);
		if (button("添加节点", 200, 25))
		{
			generator.addNewItem();
		}
	}
	protected static void drawMemberLine(UGUIGeneratorBase generator, MemberData item, ref List<MemberData> tempNeedRemoveData)
	{
		if (button("X", 25))
		{
			tempNeedRemoveData ??= new();
			tempNeedRemoveData.addUnique(item);
		}
		if (item.mWindowType == WINDOW_TYPE.POOL || item.mWindowType == WINDOW_TYPE.SCROLL_LIST)
		{
			space(70);
			toggle(ref item.mUseCustomName, "自定义变量名");
			if (item.mUseCustomName)
			{
				textField(ref item.mCustomName, 100);
			}
		}
		else
		{
			item.setObject(objectField(item.mObject, 160), generator);
		}
		int curWindowIndex = (int)item.mWindowType;
		if (displayDropDown("", "", MemberData.mWindowTypeDropList, ref curWindowIndex, 70))
		{
			item.setWindowType((WINDOW_TYPE)curWindowIndex);
		}

		List<string> typeList = null;
		switch (item.mWindowType)
		{
			case WINDOW_TYPE.NORMAL_WINDOW: typeList = generateAvailableTypeList(item.mObject); break;
			case WINDOW_TYPE.COMMON_CONTROL: typeList = getCommonSubUITypeList(); break;
			case WINDOW_TYPE.SUB_UI: break;
			case WINDOW_TYPE.SCROLL_LIST: typeList = getSubUIWithGenericTypeList(); break;
			case WINDOW_TYPE.POOL: typeList = getPoolTypeList(); break;
		}
		if (typeList == null && item.mWindowType != WINDOW_TYPE.SUB_UI)
		{
			Debug.LogError("未知的WindowType:" + item.mWindowType);
			return;
		}

		// 子页面特殊判断,类型名要跟节点名字匹配
		if (item.mWindowType == WINDOW_TYPE.SUB_UI)
		{
			item.setType(getClassNameFromGameObject(item.mObject));
			toggle(ref item.mUseCustomName, "自定义变量名");
			if (item.mUseCustomName)
			{
				if (item.mCustomName.isEmpty())
				{
					item.mCustomName = item.mType;
				}
				textField(ref item.mCustomName, 120);
			}
			else
			{
				labelWidth(item.mType, 148, ClassTypeCaches.hasClass(item.mType) ? Color.green : Color.red);
			}
		}
		else
		{
			if ((item.mType.isEmpty() || !typeList.Contains(item.mType)) && typeList.Count > 0)
			{
				item.setType(typeList.get(0) ?? typeof(myUGUIObject).ToString());
			}
			if (displayDropDown("", "", typeList, ref item.mType))
			{
				item.setType(item.mType);
			}
		}
		bool hasRegisterTypes = item.mType == typeof(LegendButton).ToString() ||
								item.mType == typeof(UGUICheckbox).ToString() ||
								item.mType == typeof(UGUITab).ToString();
		if ((item.mWindowType == WINDOW_TYPE.NORMAL_WINDOW ||
			(item.mWindowType == WINDOW_TYPE.COMMON_CONTROL && hasRegisterTypes)) && item.mArrayType == ARRAY_TYPE.NONE)
		{
			if (toggle(ref item.mRegisterCollider, "注册点击") && item.mRegisterCollider)
			{
				item.mHasClickEvent = true;
			}
			if (item.mRegisterCollider)
			{
				toggle(ref item.mHasClickEvent, "点击事件");
			}
			else
			{
				item.mHasClickEvent = false;
			}
		}

		int curArrayTypeIndex = (int)item.mArrayType;
		if (displayDropDown("", "", MemberData.mArrayTypeDropList, ref curArrayTypeIndex, 70))
		{
			item.setArrayType((ARRAY_TYPE)curArrayTypeIndex);
		}
		if (item.mArrayType != ARRAY_TYPE.NONE)
		{
			string lenStr = item.mArrayLength.ToString();
			if (textField(ref lenStr, 30))
			{
				int.TryParse(lenStr, out item.mArrayLength);
			}
		}
		if (item.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
		{
			toggle(ref item.mHideError, "不显示错误");
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
	public static List<string> getSubUIParentList(bool refresh = false)
	{
		if (refresh)
		{
			mSubUIParentList.Clear();
		}
		if (mSubUIParentList.Count == 0)
		{
			mSubUIParentList.AddRange(getTypesWithAttributeInFrameHotFixDll<CommonWindowObjectAttribute>());
			mSubUIParentList.AddRange(getTypesWithAttributeInHotFixDll<CommonWindowObjectAttribute>());
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
			mUIParentList.AddRange(getTypesWithAttributeInFrameHotFixDll<LayoutScriptBaseAttribute>());
			mUIParentList.AddRange(getTypesWithAttributeInHotFixDll<LayoutScriptBaseAttribute>());
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
			mPoolTypeList.AddRange(getTypesWithAttributeInFrameHotFixDll<CommonWindowPoolAttribute>());
			mPoolTypeList.AddRange(getTypesWithAttributeInHotFixDll<CommonWindowPoolAttribute>());
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
			mCommonSubUITypeList.AddRange(getTypesWithAttributeInFrameHotFixDll<CommonControlAttribute>());
			mCommonSubUITypeList.AddRange(getTypesWithAttributeInHotFixDll<CommonControlAttribute>());
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
		return fileList.find(file => getFileNameNoSuffixNoDir(file) == fileNameNoDirNoSuffix);
	}
	public static void generateNewObject(List<string> generatedLines, List<MemberData> list, List<MemberData> fixedList, List<GameObject> createdVariableObject, MemberData curData, GameObject root)
	{
		string curObjName = curData.getMemberName();
		GameObject parent = curData.getParentObject();
		if (list.removeIf(curData, parent == null))
		{
			return;
		}
		// 父节点是界面的根节点,则不需要传父节点就可以直接创建
		if (parent == root)
		{
			int curDataIndex = fixedList.IndexOf(curData);
			// 创建的是成员变量
			if (curDataIndex >= 0)
			{
				generateAssignWindowLine("\t\t", generatedLines, null, false, curData);
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				generateAssignWindowLineTemp("\t\t", generatedLines, curObjName, null, false);
			}
			createdVariableObject.add(curData.mObject);
			// 从列表中移除,避免再次被遍历到,如果是临时构造的数据,自己就会移除失败,也就无需关心
			list.Remove(curData);
			return;
		}

		// 如果父节点已经创建了,则可以创建
		string parentName;
		bool parentIsSubUI = false;
		// 父节点是成员变量
		MemberData parentData = fixedList.Find(data => data.mObject != null && data.mObject == parent && data.mArrayType == ARRAY_TYPE.NONE);
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
		if (createdVariableObject.Contains(parent))
		{
			// 创建的是成员变量
			if (fixedList.IndexOf(curData) >= 0)
			{
				generateAssignWindowLine("\t\t", generatedLines, parentName, parentIsSubUI, curData);
			}
			// 创建的是临时变量,临时变量不考虑数组类型
			else
			{
				generateAssignWindowLineTemp("\t\t", generatedLines, curObjName, parentName, parentIsSubUI);
			}
			createdVariableObject.add(curData.mObject);
			list.Remove(curData);
		}
		else
		{
			// 父节点还没有创建,则需要判断父节点是否在成员列表中,如果不在,就需要创建临时的变量
			int parentIndex = list.FindIndex((data) => { return data.mObject != null && data.mObject.name == parent.name && data.mArrayType == ARRAY_TYPE.NONE; });
			if (parentIndex >= 0)
			{
				// 递归创建父节点
				generateNewObject(generatedLines, list, fixedList, createdVariableObject, list[parentIndex], root);
				// 创建自己
				generateNewObject(generatedLines, list, fixedList, createdVariableObject, curData, root);
			}
			else
			{
				// 父节点只是一个临时节点,则需要先创建父节点
				MemberData newParentData = new();
				newParentData.mObject = parent;
				newParentData.setType<myUGUIObject>();
				generateNewObject(generatedLines, list, fixedList, createdVariableObject, newParentData, root);
				// 创建自己
				generateNewObject(generatedLines, list, fixedList, createdVariableObject, curData, root);
			}
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
	// prefix用于控制缩进
	// memberName是当前成员的名字,可能是自定义的
	// curObjectName就是节点自身的名字
	// parentName是父节点的变量名字,如果父节点是子页面类型的,则需要在父节点变量名字后面加上.getRoot()
	// parentIsSubUI是父节点是否是子页面类型的,因为子页面类型的父节点需要特殊处理一下
	// MemberData包含了当前成员的所有信息,可能会用到
	public static void generateAssignWindowLine(string prefix, List<string> lines, string parentName, bool parentIsSubUI, MemberData data)
	{
		if (parentIsSubUI && parentName != null)
		{
			parentName += ".getRoot()";
		}
		string memberName = data.getMemberName();
		string gameObjectName = data.getGameObjectName();
		if (data.mArrayType != ARRAY_TYPE.NONE)
		{
			string newMemberName = data.mArrayType == ARRAY_TYPE.STATIC_ARRAY ? memberName.removeEndNumber() : memberName;
			string newGameObjectName = data.mArrayType == ARRAY_TYPE.STATIC_ARRAY ? gameObjectName.removeEndNumber() : gameObjectName;
			// 动态列表只支持控件或者子页面类型的
			if (data.mArrayType == ARRAY_TYPE.DYNAMIC_ARRAY)
			{
				if (data.mWindowType == WINDOW_TYPE.COMMON_CONTROL || data.mWindowType == WINDOW_TYPE.SUB_UI)
				{
					string varName = generateAssignWindowLineTemp(prefix, lines, gameObjectName, parentName, false);
					lines.Add(prefix + "for (int i = 0; i < m" + newMemberName + ".Length; ++i)");
					lines.Add(prefix + "{");
					string parentParam = parentName ?? "mRoot";
					lines.Add(prefix + "\tm" + newMemberName + "[i].assignWindow(" + parentParam + ", " + varName + ", \"" + newGameObjectName + "\" + IToS(i));");
					lines.Add(prefix + "}");
					// 动态生成的数组都需要把模板节点隐藏起来
					lines.Add(prefix + varName + ".setActive(false);");
				}
			}
			else
			{
				lines.Add(prefix + "for (int i = 0; i < m" + newMemberName + ".Length; ++i)");
				lines.Add(prefix + "{");
				if (data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
				{
					string showErrorParam = data.mHideError ? ", false" : "";
					string parentParam = parentName != null ? parentName + ", " : "";
					lines.Add(prefix + "\tnewObject(out m" + newMemberName + "[i], " + parentParam + "\"" + newGameObjectName + "\" + IToS(i)" + showErrorParam + ");");
				}
				else
				{
					string parentParam = parentName ?? "mRoot";
					lines.Add(prefix + "\tm" + newMemberName + "[i].assignWindow(" + parentParam + ", \"" + newGameObjectName + "\" + IToS(i));");
				}
				lines.Add(prefix + "}");
			}
		}
		else
		{
			string createVarName = "m" + memberName;
			if (data.mWindowType == WINDOW_TYPE.NORMAL_WINDOW)
			{
				string showErrorParam = data.mHideError ? ", false" : "";
				string parentParam = parentName != null ? parentName + ", " : "";
				lines.Add(prefix + "newObject(out " + createVarName + ", " + parentParam + "\"" + gameObjectName + "\"" + showErrorParam + ");");
			}
			else if (data.mWindowType == WINDOW_TYPE.SUB_UI || data.mWindowType == WINDOW_TYPE.COMMON_CONTROL)
			{
				string parentParam = parentName ?? "mRoot";
				lines.Add(prefix + createVarName + ".assignWindow(" + parentParam + ", \"" + gameObjectName + "\");");
			}
			else if (data.mWindowType == WINDOW_TYPE.SCROLL_LIST)
			{
				string parentParam = parentName ?? "mRoot";
				lines.Add(prefix + createVarName + ".assignWindow(" + parentParam + ", \"" + data.mViewportObject.name + "\");");
				lines.Add(prefix + createVarName + ".assignTemplate(\"" + data.mPoolTemplate.name + "\");");
			}
			else if (data.mWindowType == WINDOW_TYPE.POOL)
			{
				string templateTypeStr = data.mTemplateWindowType.isEmpty() || data.mTemplateWindowType == typeof(myUGUIObject).ToString() ? "" : "<" + data.mTemplateWindowType + ">";
				string parentParam = parentName != null ? parentName + ", " : "mRoot, ";
				if (data.mType == "WindowStructPoolMap")
				{
					lines.Add(prefix + createVarName + ".assignTemplate" + templateTypeStr + "(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
				}
				else if (data.mType == "WindowPool")
				{
					lines.Add(prefix + createVarName + ".assignTemplate(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
				}
				else
				{
					lines.Add(prefix + createVarName + ".assignTemplate" + templateTypeStr + "(" + parentParam + "\"" + data.mPoolTemplate.name + "\");");
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
		if (go.TryGetComponent<TMP_InputField>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIInputFieldTMP).ToString());
		}
		if (go.TryGetComponent<TileImageRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUITileImage).ToString());
		}

		mTempAvailableTypeList.Add(typeof(myUGUIObject).ToString());
		if (go.TryGetComponent<Image>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUIImageSimple).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImage).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImagePro).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImageAnim).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImageAnimPro).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUIImageButton).ToString());
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
			mTempAvailableTypeList.Add(typeof(myUGUITextAuto).ToString());
		}
		if (go.TryGetComponent<TextMeshProUGUI>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUITextTMP).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUITextAuto).ToString());
		}
		if (go.TryGetComponent<MeshRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUILineMesh).ToString());
		}
		if (go.TryGetComponent<LineRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUILineRenderer).ToString());
		}
		if (go.TryGetComponent<SpriteRenderer>(out _))
		{
			mTempAvailableTypeList.Add(typeof(myUGUISprite).ToString());
			mTempAvailableTypeList.Add(typeof(myUGUISpriteAnim).ToString());
		}
		mTempAvailableTypeList.Add(typeof(myUGUIDragView).ToString());
		return mTempAvailableTypeList;
	}
	public static void setAllInspectorsLocked(bool locked)
	{
		Type inspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		if (inspectorType == null)
		{
			Debug.LogError("InspectorWindow type not found");
			return;
		}

		FieldInfo lockTrackerField = inspectorType.GetField("m_LockTracker", BindingFlags.Instance | BindingFlags.NonPublic);
		if (lockTrackerField == null)
		{
			Debug.LogError("m_LockTracker not found");
			return;
		}

		var inspectorWindow = EditorWindow.GetWindow(inspectorType);
		if (inspectorWindow == null || !inspectorType.IsAssignableFrom(inspectorWindow.GetType()))
		{
			return;
		}

		try
		{
			object lockTracker = lockTrackerField.GetValue(inspectorWindow);
			if (lockTracker == null)
			{
				return;
			}

			Type lockTrackerType = lockTracker.GetType();
			PropertyInfo isLockedProperty = lockTrackerType.GetProperty("isLocked",
					BindingFlags.Instance |
					BindingFlags.Public |
					BindingFlags.NonPublic);

			MethodInfo flipLockedMethod = null;
			while (lockTrackerType != null)
			{
				flipLockedMethod = lockTrackerType.GetMethod("FlipLocked", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				if (flipLockedMethod != null)
				{
					break;
				}
				lockTrackerType = lockTrackerType.BaseType;
			}

			if (isLockedProperty == null || flipLockedMethod == null)
			{
				return;
			}

			// 只有状态不一致时才调用FlipLocked
			if ((bool)isLockedProperty.GetValue(lockTracker) != locked)
			{
				flipLockedMethod.Invoke(lockTracker, null);
				inspectorWindow.Repaint();
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 从 DLL 文件中筛选实现指定接口的非抽象类
	protected static List<string> getTypesWithAttributeInHotFixDll<T>() where T : Attribute
	{
		return getTypesWithAttributeInDll(F_PROJECT_PATH + "Library/ScriptAssemblies/" + HOTFIX_FILE, typeof(T));
	}
	// 从 DLL 文件中筛选实现指定接口的非抽象类
	protected static List<string> getTypesWithAttributeInFrameHotFixDll<T>() where T : Attribute
	{
		return getTypesWithAttributeInDll(F_PROJECT_PATH + "Library/ScriptAssemblies/" + HOTFIX_FRAME_FILE, typeof(T));
	}
	// 从 DLL 文件中筛选实现指定接口的非抽象类,keepGenericMark是否保留模板参数类型显示,默认不保留,只获取类名本身
	protected static List<string> getTypesWithAttributeInDll(string dllFullPath, Type attribute, bool keepGenericMark = false)
	{
		// 获取所有类型（捕获加载异常）
		List<string> typeList = new();
		foreach (Type type in Assembly.LoadFrom(dllFullPath).GetTypes())
		{
			// 跳过无法加载的类型、接口和抽象类
			if (type == null || type.IsInterface)
			{
				continue;
			}
			foreach (CustomAttributeData item in type.CustomAttributes)
			{
				if (item.AttributeType == attribute)
				{
					typeList.Add(keepGenericMark ? type.ToString() : type.ToString().rangeToFirst('`'));
					break;
				}
			}
		}
		return typeList;
	}
}