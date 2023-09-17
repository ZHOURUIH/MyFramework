using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static StringUtility;
using static FileUtility;
using static FrameDefine;

public class NewLayoutWindow : GameEditorWindow
{
	protected string mLayoutName;
	protected bool mGenerateScript = true;
	protected bool mHotFixLayout = true;
	public void OnEnable()
	{
		minSize = new Vector2(300, 150);
		mLayoutName = "NewLayout";
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected override void onGUI()
	{
		beginVertical();
		textField(ref mLayoutName, "布局名:", 200);
		toggle(ref mGenerateScript, "是否生成代码");
		toggle(ref mHotFixLayout, "是否为热更布局");
		space(50);
		if (button("确定", 100, 40))
		{
			createLayout();
			Close();
		}
		endVertical();
	}
	protected void createLayout()
	{
		GameObject parent = GameObject.Find(UGUI_ROOT);
		GameObject createObj = new GameObject();
		createObj.transform.SetParent(parent.transform);
		createObj.layer = parent.layer;
		createObj.name = "UI" + mLayoutName;

		// 开始创建新布局
		RectTransform transform = createObj.AddComponent<RectTransform>();
		WidgetUtility.setRectSize(transform, new Vector2(STANDARD_WIDTH, STANDARD_HEIGHT));
		// 添加Canvas,并且勾选override Sorting
		createObj.AddComponent<Canvas>().overrideSorting = true;
		// 添加不保持宽高比的ScaleAnchor
		MenuAnchor.addScaleAnchor(createObj, false);
		Selection.activeGameObject = createObj;

		if (!mGenerateScript)
		{
			return;
		}
		// 新建脚本
		using (new ClassScope<MyStringBuilder>(out var layoutScript))
		{
			line(layoutScript, "using UnityEngine;");
			line(layoutScript, "using System;");
			line(layoutScript, "using System.Collections;");
			line(layoutScript, "using System.Collections.Generic;");
			line(layoutScript, "");
			line(layoutScript, "public class UI" + mLayoutName + " : LayoutScript");
			line(layoutScript, "{");
			line(layoutScript, "\tpublic override void assignWindow()");
			line(layoutScript, "\t{");
			line(layoutScript, "");
			line(layoutScript, "\t}");
			line(layoutScript, "\tpublic override void init()");
			line(layoutScript, "\t{");
			line(layoutScript, "\t\tbase.init();");
			line(layoutScript, "\t}");
			line(layoutScript, "}", false);
			if (mHotFixLayout)
			{
				writeTxtFile(F_HOT_FIX_UI_PATH + "Script/UI" + mLayoutName + ".cs", layoutScript.ToString());
			}
			else
			{
				writeTxtFile(F_SCRIPTS_UI_SCRIPT_PATH + "UI" + mLayoutName + ".cs", layoutScript.ToString());
			}
		}

		// 布局ID
		string layoutDefineName = nameToUpper(mLayoutName, false);
		string frameEnumFileName;
		if (mHotFixLayout)
		{
			frameEnumFileName = F_HOT_FIX_GAME_PATH + "Common/FrameEnumExtension.cs";
		}
		else
		{
			frameEnumFileName = F_SCRIPTS_GAME_PATH + "Common/FrameEnumExtension.cs";
		}
		processFileLine(frameEnumFileName, (List<string> fileLines) =>
		{
			bool layoutDefineStart = false;
			for (int i = 0; i < fileLines.Count; ++i)
			{
				if (mHotFixLayout && fileLines[i].Contains("public class LAYOUT") || 
					!mHotFixLayout && fileLines[i].Contains("public class LAYOUT_ILR"))
				{
					layoutDefineStart = true;
					continue;
				}
				// 找到定义的结束
				if (layoutDefineStart && fileLines[i].Contains("}"))
				{
					string lastDefine = fileLines[i - 1];
					int equalPos = lastDefine.IndexOf('=');
					string defineValue = lastDefine.Substring(equalPos + 2, lastDefine.Length - 1 - equalPos - 2);
					int lastLayoutID = SToI(defineValue);
					fileLines.Insert(i, "\tpublic const int " + layoutDefineName + " = " + IToS(lastLayoutID + 1) + ";");
					break;
				}
			}
		});

		// 脚本注册
		string layoutRegisterFileName;
		if (mHotFixLayout)
		{
			layoutRegisterFileName = F_HOT_FIX_UI_PATH + "LayoutRegisterILR.cs";
		}
		else
		{
			layoutRegisterFileName = F_SCRIPTS_UI_PATH + "LayoutRegister.cs";
		}
		processFileLine(layoutRegisterFileName, (List<string> fileLines)=>
		{
			for (int i = 0; i < fileLines.Count; ++i)
			{
				if (fileLines[i].Contains("mLayoutManager.addScriptCallback(onScriptChanged);"))
				{
					if (mHotFixLayout)
					{
						fileLines.Insert(i - 1, "\t\tregisteLayout<UI" + mLayoutName + ">(LAYOUT_ILR." + layoutDefineName + ", \"UI" + mLayoutName + "\");");
					}
					else
					{
						fileLines.Insert(i - 1, "\t\tregisteLayout<UI" + mLayoutName + ">(LAYOUT." + layoutDefineName + ", \"UI" + mLayoutName + "\");");
					}
					break;
				}
			}

			bool scriptCallbackStart = false;
			for (int i = 0; i < fileLines.Count; ++i)
			{
				if (fileLines[i].Contains("protected static void onScriptChanged(LayoutScript script, bool created = true)"))
				{
					i += 6;
					scriptCallbackStart = true;
					continue;
				}
				if (scriptCallbackStart && fileLines[i].Contains("}"))
				{
					fileLines.Insert(i, "\t\tif (assign(ref mUI" + mLayoutName + ", script, created)) return;");
					break;
				}
			}
		});

		// 布局脚本引用
		string gameBaseFileName;
		if (mHotFixLayout)
		{
			gameBaseFileName = F_HOT_FIX_GAME_PATH + "Common/GameBaseILR.cs";
		}
		else
		{
			gameBaseFileName = F_SCRIPTS_GAME_PATH + "Common/GameBase.cs";
		}
		processFileLine(gameBaseFileName, (List<string> fileLines)=>
		{
			bool layoutScriptStart = false;
			for (int i = 0; i < fileLines.Count; ++i)
			{
				if (fileLines[i].Contains("// LayoutScript"))
				{
					layoutScriptStart = true;
					continue;
				}
				if (layoutScriptStart && !fileLines[i].Contains("public static Script"))
				{
					fileLines.Insert(i, "\tpublic static UI" + mLayoutName + " mUI" + mLayoutName + ";");
					break;
				}
			}
		});
	}
}