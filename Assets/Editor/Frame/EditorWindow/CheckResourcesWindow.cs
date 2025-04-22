using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static StringUtility;
using static EditorCommonUtility;
using static FrameDefine;
using static FileUtility;
using static MathUtility;

public class RefInfo
{
	public UObject mObject;
	public string mFileName;				// 文件名,以Assets开头
	public List<string> mRefInGameRes;		// GameResources中引用了此文件的文件列表
	public bool mOuterRefCount;				// 其他地方的引用次数,比如代码直接引用,表格引用等
}

public class CheckResourcesWindow : GameEditorWindow
{
	protected Dictionary<string, RefInfo> mFileReferenceList = new();
	protected ScrollArea mScrollArea = new();	// 滚动区域
	protected string mInputGUID;				// 要查询的guid
	protected static int mPageSize = 100;       // 每页显示的数量
	protected bool mShowOnlyMultiRef;			// 是否仅显示被多个目录所引用文件
	protected int mCurPage;						// 当前页下标
	public void start()
	{
		Show();
		minSize = new(1200, 800);
		mScrollArea.init(1500, 600);
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected override void onGUI()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject).rightToLeft();
		if (path.isEmpty())
		{
			path = P_GAME_RESOURCES_PATH;
			removeEndSlash(ref path);
		}
		label("当前选中的文件或文件夹:" + path);
		using (new GUILayout.VerticalScope())
		{
			using (new GUILayout.HorizontalScope())
			{
				if (button("检查资源引用", 150))
				{
					AssetDatabase.Refresh();
					// 选择的是文件,则只查找文件的引用
					if (isFileExist(path))
					{
						if (EditorUtility.DisplayDialog("查找资源引用", "确认查找文件夹中所有文件的引用? " + path, "确认", "取消"))
						{
							mFileReferenceList.Clear();
							Dictionary<string, List<string>> tempList = new();
							doCheck(path, tempList, getAllResourceFileText());
							// 这里的GameMenu,collectUsedFile需要在Game层自己实现,
							HashSet<string> outerRefList = GameMenu.collectUsedFile();
							foreach (var item in tempList)
							{
								mFileReferenceList.Add(item.Key, new()
								{
									mFileName = item.Key,
									mRefInGameRes = item.Value,
									mOuterRefCount = outerRefList.Contains(item.Key),
									mObject = AssetDatabase.LoadAssetAtPath<UObject>(item.Key)
								});
							}
						}
					}
					// 选择的是目录,则查找目录中所有文件的引用
					else if (isDirExist(path))
					{
						if (EditorUtility.DisplayDialog("查找资源引用", "确认查找文件夹中所有文件的引用? " + path, "确认", "取消"))
						{
							var allFileText = getAllResourceFileText();
							// 不查找meta文件的引用
							List<string> validFiles = new();
							foreach (string item in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
							{
								if (!item.EndsWith(".meta"))
								{
									validFiles.Add(item.rightToLeft());
								}
							}
							mFileReferenceList.Clear();
							Dictionary<string, List<string>> tempList = new();
							// 开始查找所有文件的引用
							int count = validFiles.Count;
							for (int i = 0; i < count; ++i)
							{
								displayProgressBar("查找资源引用", "进度: ", i + 1, count);
								doCheck(validFiles[i], tempList, allFileText);
							}
							// 这里的GameMenu,collectUsedFile需要在Game层自己实现,
							HashSet<string> outerRefList = GameMenu.collectUsedFile();
							foreach (var item in tempList)
							{
								mFileReferenceList.Add(item.Key, new()
								{
									mFileName = item.Key,
									mRefInGameRes = item.Value,
									mOuterRefCount = outerRefList.Contains(item.Key),
									mObject = AssetDatabase.LoadAssetAtPath<UObject>(item.Key)
								});
							}
							clearProgressBar();
						}
					}
					mCurPage = 0;
				}
				if (toggle(ref mShowOnlyMultiRef, "仅显示被多个文件夹引用的文件"))
				{
					mCurPage = 0;
				}
			}
			using (new GUILayout.HorizontalScope())
			{
				textField(ref mInputGUID, 300);
				if (button("根据GUID查找文件", 150))
				{
					var allMeta = getAllResourceMeta();
					allMeta.TryGetValue(mInputGUID, out string filePath);
					filePath = fullPathToProjectPath(filePath).removeEndString(".meta");
					Debug.Log("查找到的文件:" + filePath + ", guid:" + mInputGUID, loadAsset(filePath));
				}
			}
		}
		// 每一行显示文件名,被引用次数,如果只被一个文件引用,则显示文件名,如果都是被同一个文件夹中的文件引用,则显示文件名
		using (new GUILayout.HorizontalScope())
		{
			labelWidth("文件名", 250);
			labelWidth("是否已移动", 100);
			labelWidth("引用次数", 100);
			labelWidth("是否被外部引用", 100);
			labelWidth("所有引用目录", 250);
			labelWidth("引用文件名", 350);
		}
		using (new GUILayout.VerticalScope())
		{
			Dictionary<string, RefInfo> tempFileRefList = new();
			// key是sourceFile,Value是destFile
			Dictionary<string, string> needMoveFileList = new();
			List<string> needDeleteFileList = new();
			// 添加滚动区域,设置滚动区域的高度
			using (new ScrollAreaScope(mScrollArea))
			{
				if (mShowOnlyMultiRef)
				{
					foreach (var item in mFileReferenceList)
					{
						List<string> refList = item.Value.mRefInGameRes;
						if (refList.Count > 1)
						{
							bool usedInSingleFolder = true;
							string refFilePath = null;
							foreach (string refFile in refList)
							{
								if (refFilePath == null)
								{
									refFilePath = getFilePath(refFile);
								}
								else if (refFilePath != getFilePath(refFile))
								{
									usedInSingleFolder = false;
									break;
								}
							}
							if (!usedInSingleFolder)
							{
								tempFileRefList.add(item);
							}
						}
					}
				}
				else
				{
					tempFileRefList = new(mFileReferenceList);
				}

				int index = 0;
				int startIndex = mCurPage * mPageSize;
				int endIndex = (mCurPage + 1) * mPageSize;
				foreach (var item in tempFileRefList)
				{
					if (index < startIndex || index >= endIndex)
					{
						++index;
						continue;
					}
					++index;
					using (new GUILayout.HorizontalScope())
					{
						// 文件名
						if (button(getFolderName(item.Key) + "/" + getFileNameWithSuffix(item.Key), item.Key, 250))
						{
							if (item.Value.mObject != null)
							{
								EditorGUIUtility.PingObject(item.Value.mObject);
								Selection.activeObject = item.Value.mObject;
							}
						}
						// 文件是否已经移动
						if (AssetDatabase.GetAssetPath(item.Value.mObject) != item.Key)
						{
							labelWidth("已经移动", 100);
						}
						else
						{
							labelWidth("未移动", 100);
						}
						List<string> refList = item.Value.mRefInGameRes;
						// 引用次数
						labelWidth(IToS(refList.Count), 100);
						// 是否被外部引用
						labelWidth(item.Value.mOuterRefCount ? "是" : "/", 100);

						// 过滤一下重复的文件夹
						Dictionary<string, string> refFolderList = new();
						foreach (string refFile in refList)
						{
							refFolderList.TryAdd(getFolderName(refFile), getFilePath(refFile));
						}

						string allRefFolder = EMPTY;
						string allRefFolderTip = EMPTY;
						foreach (var refPair in refFolderList)
						{
							allRefFolder += refPair.Key + ",";
							allRefFolderTip += refPair.Value + "\n";
						}

						// 所有引用目录
						labelWidth(allRefFolder, 250, allRefFolderTip);

						// 引用文件名
						string destPath = null;
						if (refList.Count == 1)
						{
							destPath = getFilePath(refList[0]);
						}
						string refString = EMPTY;
						string refTipString = EMPTY;
						foreach (string refFile in refList)
						{
							refString += refFile.removeStartString(P_GAME_RESOURCES_PATH) + ",";
							refTipString += refFile + "\n";
						}
						if (!refString.isEmpty())
						{
							labelWidth(refString, 350, refTipString);
						}
						else
						{
							labelWidth("/", 350);
						}

						// 判断是否只被一个文件夹中的文件所引用
						bool isSingleFolderUsed = false;
						if (refList.Count > 1)
						{
							bool usedInSingleFolder = true;
							string refFilePath = null;
							foreach (string refFile in refList)
							{
								if (refFilePath == null)
								{
									refFilePath = getFilePath(refFile);
								}
								else if (refFilePath != getFilePath(refFile))
								{
									usedInSingleFolder = false;
									break;
								}
							}
							if (usedInSingleFolder)
							{
								isSingleFolderUsed = true;
								destPath = refFilePath;
							}
						}

						// 如果文件只在一个地方被引用,则可以移动到被引用的文件夹中,如果此文件已经在指定的文件夹了,则不用显示按钮
						string fullPath = projectPathToFullPath(item.Key);
						if ((refList.Count == 1 || isSingleFolderUsed) &&
							getFilePath(item.Key) != destPath &&
							isFileExist(fullPath))
						{
							string destFile = projectPathToFullPath(destPath + "/") + getFileNameWithSuffix(item.Key);
							needMoveFileList.add(fullPath, destFile);
							if (button("将文件移动到引用处", 150))
							{
								moveFile(fullPath, destFile);
								moveFile(fullPath + ".meta", destFile + ".meta");
								AssetDatabase.Refresh();
							}
						}
						// 如果文件没有被任何资源引用,也没有外部引用,则可以删除
						else if (refList.Count == 0 &&
								!item.Value.mOuterRefCount &&
								isFileExist(fullPath))
						{
							needDeleteFileList.Add(fullPath);
							if (button("删除文件", 150))
							{
								deleteFile(fullPath);
								deleteFile(fullPath + ".meta");
								AssetDatabase.Refresh();
							}
						}
						else
						{
							labelWidth("无需操作", 250);
						}
					}
				}
			}

			using (new GUILayout.HorizontalScope(GUILayout.Width(170)))
			{
				if (button("上一页"))
				{
					mCurPage = clampMin(mCurPage - 1);
				}
				int pageCount = ceil(tempFileRefList.Count / (float)mPageSize);
				label("第" + IToS(clampMax(mCurPage + 1, pageCount)) + "/" + IToS(pageCount) + "页");
				if (button("下一页"))
				{
					mCurPage = clampMax(mCurPage + 1, pageCount - 1);
				}
				if (needMoveFileList.Count > 0)
				{
					if (button("将本页全部需要移动的文件移动到引用处", 350))
					{
						foreach (var item in needMoveFileList)
						{
							moveFile(item.Key, item.Value);
							moveFile(item.Key + ".meta", item.Value + ".meta");
						}
						AssetDatabase.Refresh();
					}
				}
				if (needDeleteFileList.Count > 0)
				{
					if (button("删除本页所有未使用的文件", 250))
					{
						foreach (string item in needDeleteFileList)
						{
							deleteFile(item);
							deleteFile(item + ".meta");
						}
						AssetDatabase.Refresh();
					}
				}
			}
		}
	}
	protected void doCheck(string path, Dictionary<string, List<string>> refList, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		DateTime start = DateTime.Now;
		Dictionary<string, UObject> refrenceList = new();
		searchFileRefrence(path, false, refrenceList, allFileText, false);
		refList.add(path.rightToLeft(), new(refrenceList.Keys));
		Debug.Log("查找" + path + "的引用,引用数量:" + refrenceList.Count+ "耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒");
	}
}
