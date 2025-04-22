#if USE_TMP
using TMPro;
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static FileUtility;
using static MathUtility;
using static FrameBaseDefine;
using static FrameDefine;

public class FindTextWindow : GameEditorWindow
{
	protected Dictionary<MaskableGraphic, GameObject> mTextPrefabList = new();
	protected Dictionary<string, List<MaskableGraphic>> mTextSortList = new();
	protected Dictionary<MaskableGraphic, string> mTextEditList = new();
	protected ScrollArea mScrollArea = new();
	protected int mPageIndex;
	protected const int mPageSize = 100;
	protected int mAllCount;
	protected int mMaxPageIndex;
	protected string mSearchPath = P_ASSETS_PATH;   // 路径以Assets开头
	protected string mExcludePathString;			// 每个路径以Assets开头,多个路径之间用;隔开
	public void start()
	{
		Show();
		minSize = new(1550, 1050);
		mScrollArea.init(1500, 1000);
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected override void onGUI()
	{
		if (button("开始提取所有文本组件", 200, 30))
		{
			findTextComponent();
		}
		textField(ref mSearchPath, "查找的路径", 1000);
		textField(ref mExcludePathString, "排除的路径:", 1000);
		label("总共" + mAllCount + "个文本");
		using (new ScrollAreaScope(mScrollArea))
		{
			using (new GUILayout.VerticalScope())
			{
				int index = 0;
				foreach (var prefabPair in mTextSortList)
				{
					if (prefabPair.Value.Count == 0)
					{
						continue;
					}
					foreach (MaskableGraphic text in prefabPair.Value)
					{
						if (index / mPageSize == mPageIndex)
						{
							using var a = new GUILayout.HorizontalScope();
							labelWidth("", 100);
							labelWidth(text.name, 200);
							labelWidth("文本:" + getText(text), 500);
							labelWidth("输入新文本:", 100);
							string newStr = mTextEditList[text];
							textField(ref newStr, 300);
							mTextEditList[text] = newStr;
							GameObject prefabInstance = mTextPrefabList[text];
							if (button("更新文本", 80, 30))
							{
								setText(text, newStr);
								EditorUtility.SetDirty(prefabInstance);
								if (PrefabUtility.IsPartOfPrefabInstance(text.gameObject))
								{
									PrefabUtility.ApplyObjectOverride(text.gameObject, AssetDatabase.GetAssetPath(prefabInstance), InteractionMode.UserAction);
								}
								AssetDatabase.SaveAssetIfDirty(prefabInstance);
								AssetDatabase.Refresh();
							}
							if (button("定位文件", 80, 30))
							{
								EditorGUIUtility.PingObject(prefabInstance);
							}
							if (button("定位节点", 80, 30))
							{
								PrefabNodeLocator.FocusNodeInPrefabMode(text.name);
							}
						}
						++index;
					}
				}
			}
		}
		using (new GUILayout.HorizontalScope())
		{
			if (button("首页"))
			{
				mPageIndex = 0;
			}
			if (button("上一页"))
			{
				mPageIndex = clampMin(mPageIndex - 1);
			}
			label(mPageIndex + 1 + "/" + mMaxPageIndex + 1);
			if (button("下一页"))
			{
				mPageIndex = clampMax(mPageIndex + 1, mMaxPageIndex);
			}
			if (button("末页"))
			{
				mPageIndex = mMaxPageIndex;
			}
		}
	}
	protected void findTextComponent()
	{
		List<string> excludeList = new();
		excludeList.addRange(mExcludePathString?.Split(';'));
		mTextPrefabList.Clear();
		mTextSortList.Clear();
		mTextEditList.Clear();
		Dictionary<GameObject, List<MaskableGraphic>> textList = new();
		foreach (string file in findFilesNonAlloc(F_PROJECT_PATH + mSearchPath, ".prefab"))
		{
			string newFile = file.Replace('\\', '/');
			bool isIgnore = false;
			foreach (string exclude in excludeList)
			{
				if (newFile.StartsWith(F_PROJECT_PATH + exclude))
				{
					isIgnore = true;
					break;
				}
			}
			if (isIgnore)
			{
				continue;
			}
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(newFile.removeStartString(F_PROJECT_PATH));
			if (prefab == null)
			{
				continue;
			}
			var list = textList.add(prefab, new());
			list.AddRange(prefab.GetComponentsInChildren<Text>(true));
#if USE_TMP
			list.AddRange(prefab.GetComponentsInChildren<TextMeshProUGUI>(true));
#endif
		}
		mAllCount = 0;
		foreach (var item in textList)
		{
			foreach (MaskableGraphic text in item.Value)
			{
				mTextSortList.getOrAddNew(getText(text)).add(text);
				mTextPrefabList.add(text, item.Key);
				mTextEditList.Add(text, getText(text));
			}
			mAllCount += item.Value.Count;
		}
		mMaxPageIndex = generateBatchCount(mAllCount, mPageSize) - 1;
	}
	protected string getText(MaskableGraphic graphic)
	{
		if (graphic is Text textCom)
		{
			return textCom.text;
		}
#if USE_TMP
		else if (graphic is TextMeshProUGUI tmpCom)
		{
			return tmpCom.text;
		}
#endif
		return null;
	}
	protected void setText(MaskableGraphic graphic, string newStr)
	{
		if (graphic is Text textCom)
		{
			textCom.text = newStr;
		}
#if USE_TMP
		else if (graphic is TextMeshProUGUI tmpCom)
		{
			tmpCom.text = newStr;
		}
#endif
	}
}