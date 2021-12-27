using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
		GameObject parent = GameObject.Find(FrameDefine.UGUI_ROOT);
		GameObject createObj = new GameObject();
		createObj.transform.SetParent(parent.transform);
		createObj.layer = parent.layer;
		createObj.name = "UI" + mLayoutName;

		// 开始创建新布局
		RectTransform transform = createObj.AddComponent<RectTransform>();
		WidgetUtility.setRectSize(transform, new Vector2(FrameDefineExtension.STANDARD_WIDTH, FrameDefineExtension.STANDARD_HEIGHT));
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
		MyStringBuilder layoutScript = FrameUtility.STRING();
		StringUtility.line(layoutScript, "using UnityEngine;");
		StringUtility.line(layoutScript, "using System;");
		StringUtility.line(layoutScript, "using System.Collections;");
		StringUtility.line(layoutScript, "using System.Collections.Generic;");
		StringUtility.line(layoutScript, "");
		StringUtility.line(layoutScript, "public class UI" + mLayoutName + " : LayoutScript");
		StringUtility.line(layoutScript, "{");
		StringUtility.line(layoutScript, "\tpublic override void assignWindow()");
		StringUtility.line(layoutScript, "\t{");
		StringUtility.line(layoutScript, "");
		StringUtility.line(layoutScript, "\t}");
		StringUtility.line(layoutScript, "\tpublic override void init()");
		StringUtility.line(layoutScript, "\t{");
		StringUtility.line(layoutScript, "\t\tbase.init();");
		StringUtility.line(layoutScript, "\t}");
		StringUtility.line(layoutScript, "}", false);
		if (mHotFixLayout)
		{
			FileUtility.writeTxtFile(FrameDefine.F_HOT_FIX_LAYOUT_PATH + "Script/UI" + mLayoutName + ".cs", FrameUtility.END_STRING(layoutScript));
		}
		else
		{
			FileUtility.writeTxtFile(FrameDefine.F_SCRIPTS_LAYOUT_SCRIPT_PATH + "UI" + mLayoutName + ".cs", FrameUtility.END_STRING(layoutScript));
		}

		// 布局ID
		string layoutDefineName = StringUtility.nameToUpper(mLayoutName, false);
		string frameEnumFileName;
		if (mHotFixLayout)
		{
			frameEnumFileName = FrameDefine.F_HOT_FIX_GAME_PATH + "Common/FrameEnumExtension.cs";
		}
		else
		{
			frameEnumFileName = FrameDefine.F_SCRIPTS_GAME_PATH + "Common/FrameEnumExtension.cs";
		}
		FileUtility.processFileLine(frameEnumFileName, (List<string> fileLines) =>
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
					int lastLayoutID = StringUtility.SToI(defineValue);
					fileLines.Insert(i, "\tpublic const int " + layoutDefineName + " = " + StringUtility.IToS(lastLayoutID + 1) + ";");
					break;
				}
			}
		});

		// 脚本注册
		string layoutRegisterFileName;
		if (mHotFixLayout)
		{
			layoutRegisterFileName = FrameDefine.F_HOT_FIX_LAYOUT_PATH + "LayoutRegisterILR.cs";
		}
		else
		{
			layoutRegisterFileName = FrameDefine.F_SCRIPTS_LAYOUT_PATH + "LayoutRegister.cs";
		}
		FileUtility.processFileLine(layoutRegisterFileName, (List<string> fileLines)=>
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
			gameBaseFileName = FrameDefine.F_HOT_FIX_GAME_PATH + "Common/GameBaseILR.cs";
		}
		else
		{
			gameBaseFileName = FrameDefine.F_SCRIPTS_GAME_PATH + "Common/GameBase.cs";
		}
		FileUtility.processFileLine(gameBaseFileName, (List<string> fileLines)=>
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