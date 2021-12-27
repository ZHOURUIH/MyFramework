using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;
using System.Reflection;
using UnityEditor.Callbacks;
using System;
using ILRuntime.Runtime.Enviorment;

public class SpriteRefrenceInfo
{
	public string mSpriteName;
	public string mFileName;
	public UnityEngine.Object mObject;
}

public class FileGUIDLines
{
	public string mProjectFileName;     // 相对于项目的相对路径,也就是以Assets开头
	public List<string> mContainGUIDLines;
}

public class PrefabNodeItem
{
	public string mGameObjectID;
	public string mSpriteID;
	public string mName;
}

public class EditorCommonUtility : FrameUtility
{
	protected static char[] mHexUpperChar;
	protected static char[] mHexLowerChar;
	protected static string mHexString = "ABCDEFabcdef0123456789";
	protected const int GUID_LENGTH = 32;
	protected const string KEY_FUNCTION = "resetProperty";
	protected const string CODE_LOCATE_KEYWORD = "代码检测";
	public static bool messageYesNo(string info)
	{
		return EditorUtility.DisplayDialog("提示", info, "确认", "取消");
	}
	public static new void messageBox(string info, bool errorOrInfo)
	{
		string title = errorOrInfo ? "错误" : "提示";
		EditorUtility.DisplayDialog(title, info, "确认");
		if (errorOrInfo)
		{
			Debug.LogError(info);
		}
		else
		{
			Debug.Log(info);
		}
	}
	// 将一个GameObject生成为一个prefab文件,path是以Assets开头的目录
	public static void gameObjectToPrefab(string path, GameObject go)
	{
		validPath(ref path);
		createDir(projectPathToFullPath(path));
		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path + go.name + ".prefab");
		prefab.transform.localPosition = Vector3.zero;
		prefab.transform.localEulerAngles = Vector3.zero;
		prefab.transform.localScale = Vector3.one;
	}
	// 将一个节点下的所有一级子节点生成为prefab文件,path是以Assets开头的目录
	public static void childsToPrefab(string path, GameObject go)
	{
		Transform transform = go.transform;
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			gameObjectToPrefab(path, transform.GetChild(i).gameObject);
		}
	}
	
	// 查找文件在其他地方的引用情况
	public static int searchFiles(string pattern, string guid, string fileName, bool loadFile, Dictionary<string, UnityEngine.Object> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		generateNextIndex(guid, out int[] guidNextIndex);
		rightToLeft(ref fileName);
		string metaSuffix = ".meta";
		if (allFileText == null)
		{
			string[] files = Directory.GetFiles(Application.dataPath + "/" + FrameDefine.GAME_RESOURCES, pattern, SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; ++i)
			{
				string file = files[i];
				if (KMPSearch(File.ReadAllText(file), guid, guidNextIndex) < 0)
				{
					continue;
				}
				rightToLeft(ref file);
				fullPathToProjectPath(ref file);
				removeEndString(ref file, metaSuffix);
				if (fileName == file)
				{
					continue;
				}
				if (loadFile)
				{
					refrenceList.Add(file, loadAsset(file));
				}
				else
				{
					refrenceList.Add(file, null);
				}
			}
		}
		else
		{
			if (pattern == "*.*")
			{
				pattern = EMPTY;
			}
			if (pattern[0] == '*')
			{
				pattern = pattern.Remove(0, 1);
			}
			foreach (var item in allFileText)
			{
				if (!endWith(item.Key, pattern, false))
				{
					continue;
				}
				foreach (var fileGUIDLines in item.Value)
				{
					string curFile = fileGUIDLines.mProjectFileName;
					// 简单过滤一下meta文件的判断,因为meta文件的文件名最后一个字符肯定是a
					if (curFile[curFile.Length - 1] == 'a')
					{
						removeEndString(ref curFile, metaSuffix);
					}
					foreach (var line in fileGUIDLines.mContainGUIDLines)
					{
						// 查找是否包含GUID
						if (KMPSearch(line, guid, guidNextIndex) < 0)
						{
							continue;
						}
						if (fileName != curFile && !refrenceList.ContainsKey(curFile))
						{
							if (loadFile)
							{
								refrenceList.Add(curFile, loadAsset(curFile));
							}
							else
							{
								refrenceList.Add(curFile, null);
							}
						}
						break;
					}
				}
			}
		}
		return refrenceList.Count;
	}
	// 检查shader是否有指定的属性
	public static bool checkTextureInShader(string texturePropertyName, string shaderContent)
	{
		splitLine(shaderContent, out string[] lines);
		int propertyLine = -1;
		int propertyStartLine = -1;
		int propertyEndLine = -1;
		string texturePropertyKey = texturePropertyName + "(";
		int[] texturePropertyNextIndex;
		generateNextIndex(texturePropertyKey, out texturePropertyNextIndex);
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeAllEmpty(lines[i]);
			// 找到Properties
			if (lines[i] == "Properties")
			{
				propertyLine = i;
				propertyStartLine = i + 2;
			}
			if (propertyLine >= 0 && lines[i] == "{")
			{
				propertyStartLine = i + 1;
			}
			if (propertyStartLine >= 0 && lines[i] == "}")
			{
				propertyEndLine = i - 1;
				break;
			}
		}
		if (propertyLine < 0 || propertyStartLine < 0 || propertyEndLine < 0)
		{
			return false;
		}
		for (int i = propertyStartLine; i <= propertyEndLine; ++i)
		{
			if (KMPSearch(lines[i], texturePropertyKey, texturePropertyNextIndex) >= 0)
			{
				return true;
			}
		}
		return false;
	}
	// 检查材质引用的贴图是否存在
	public static void checkMaterialTextureValid(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		openTxtFileLines(projectPathToFullPath(path), out string[] materialLines);
		for (int i = 0; i < materialLines.Length; ++i)
		{
			materialLines[i] = removeAll(materialLines[i], ' ');
		}
		bool startTexture = false;
		string textureStr = "m_Texture";
		generateNextIndex(textureStr, out int[] textureNextIndex);
		string guidKey = "guid:";
		generateNextIndex(guidKey, out int[] guidNextIndex);
		for (int i = 0; i < materialLines.Length; ++i)
		{
			string line = materialLines[i];
			if (!startTexture)
			{
				// 开始查找贴图属性
				if (line == "m_TexEnvs:")
				{
					startTexture = true;
				}
			}
			else
			{
				// 找到属性名
				if (line[0] == '-')
				{
					string textureLine = materialLines[i + 1];
					if (KMPSearch(textureLine, textureStr, textureNextIndex) < 0)
					{
						Debug.LogError("material texture property error, " + path, loadAsset(path));
						return;
					}
					if (KMPSearch(textureLine, guidKey, guidNextIndex) >= 0)
					{
						int startIndex = textureLine.IndexOf(guidKey) + guidKey.Length;
						int endIndex = textureLine.IndexOf(',', startIndex);
						string textureGUID = textureLine.Substring(startIndex, endIndex - startIndex);
						string textureFile = findAsset(textureGUID, ".png", false, allFileText);
						if (textureFile == EMPTY)
						{
							textureFile = findAsset(textureGUID, ".tga", false, allFileText);
						}
						if (textureFile == EMPTY)
						{
							textureFile = findAsset(textureGUID, ".jpg", false, allFileText);
						}
						if (textureFile == EMPTY)
						{
							textureFile = findAsset(textureGUID, ".cubemap", false, allFileText);
						}
						if (textureFile == EMPTY)
						{
							textureFile = findAsset(textureGUID, ".tif", false, allFileText);
						}
						if (textureFile == EMPTY)
						{
							Debug.LogError("在GameResource中找不到材质引用的资源 : " + path, loadAsset(path));
						}
					}
				}
				// 贴图属性查找结束
				if (line == "m_Floats:" || line == "m_Colors:")
				{
					break;
				}
			}
		}
	}
	// 检查材质是否使用了无效的贴图属性,needTextureGUID表示是否只检测引用了贴图的属性
	public static void checkMaterialTexturePropertyValid(string path, Dictionary<string, List<FileGUIDLines>> allFileText, bool needTextureGUID)
	{
		string shaderGUID = EMPTY;
		string materialContent = openTxtFile(projectPathToFullPath(path), true);
		string[] materialLines = split(materialContent, "\r\n");
		foreach (var item in materialLines)
		{
			string line = removeAll(item, ' ');
			if (line.StartsWith("m_Shader:"))
			{
				string key = "guid:";
				int startIndex = line.IndexOf(key) + key.Length;
				int endIndex = line.IndexOf(',', startIndex);
				shaderGUID = line.Substring(startIndex, endIndex - startIndex);
				break;
			}
		}
		if (shaderGUID == EMPTY)
		{
			return;
		}
		if (endWith(shaderGUID, "000000"))
		{
			Debug.LogError("材质使用了内置shader,无法找到shader文件", loadAsset(path));
			return;
		}
		string shaderFile = findAsset(shaderGUID, ".shader", true, allFileText);
		if (shaderFile == EMPTY)
		{
			Debug.LogWarning("can not find material shader:" + path, loadAsset(path));
			return;
		}
		string shaderContent = openTxtFile(shaderFile, true);
		if (shaderContent == EMPTY)
		{
			return;
		}
		List<string> texturePropertyList = new List<string>();
		bool startTexture = false;
		foreach (var item in materialLines)
		{
			string line = removeAll(item, ' ');
			if (!startTexture)
			{
				// 开始查找贴图属性
				if (line == "m_TexEnvs:")
				{
					startTexture = true;
				}
			}
			else
			{
				// 找到属性名
				string preKey = "-";
				if (line.StartsWith(preKey))
				{
					if (!needTextureGUID || hasGUID(line))
					{
						texturePropertyList.Add(line.Substring(preKey.Length, line.Length - preKey.Length - 1));
					}
				}
				// 贴图属性查找结束
				if (line == "m_Floats:" || line == "m_Colors:")
				{
					break;
				}
			}
		}
		bool materialValid = true;
		foreach (var item in texturePropertyList)
		{
			// 找到一个shader中没有的贴图属性
			if (!checkTextureInShader(item, shaderContent))
			{
				Debug.LogError("材质中使用了无效的贴图属性:" + path, loadAsset(path));
				materialValid = false;
				break;
			}
		}
		if (materialValid)
		{
			Debug.Log("材质贴图属性正常:" + path);
		}
	}
	public static Dictionary<string, string> getSpriteGUIDs(string path)
	{
		Dictionary<string, string> spriteGUIDs = new Dictionary<string, string>();
		openTxtFileLines(projectPathToFullPath(path + ".meta"), out string[] atlasLines);
		bool spriteStart = false;
		foreach (var item in atlasLines)
		{
			string line = removeAll(item, ' ');
			if (line == "fileIDToRecycleName:")
			{
				spriteStart = true;
			}
			else if (line == "externalObjects:{}")
			{
				break;
			}
			if (!spriteStart)
			{
				continue;
			}
			string[] elem = split(line, ':');
			if (elem.Length != 2)
			{
				break;
			}
			spriteGUIDs.Add(elem[0], elem[1]);
		}
		return spriteGUIDs;
	}
	public static void searchSpriteRefrence(string path, Dictionary<string, SpriteRefrenceInfo> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		refrenceList.Clear();
		string atlasGUID = AssetDatabase.AssetPathToGUID(path);
		var spriteGUID = getSpriteGUIDs(path);
		foreach (var item in spriteGUID)
		{
			searchSprite(atlasGUID, item.Key, item.Value, refrenceList, allFileText);
		}
	}
	public static void searchSprite(string atlasGUID, string spriteGUID, string spriteName, Dictionary<string, SpriteRefrenceInfo> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		string key = "m_Sprite: {fileID: " + spriteGUID + ", guid: " + atlasGUID;
		generateNextIndex(key, out int[] keyNextIndex);
		foreach (var item in allFileText)
		{
			string suffix = item.Key;
			if (suffix != ".prefab")
			{
				continue;
			}
			foreach (var fileGUIDLines in item.Value)
			{
				foreach (var line in fileGUIDLines.mContainGUIDLines)
				{
					if (KMPSearch(line, key, keyNextIndex) < 0)
					{
						continue;
					}
					string file = item.Key;
					if (!refrenceList.ContainsKey(file))
					{
						SpriteRefrenceInfo info = new SpriteRefrenceInfo();
						info.mSpriteName = spriteName;
						info.mFileName = file;
						info.mObject = loadAsset(file);
						refrenceList.Add(file, info);
					}
					break;
				}
			}
		}
	}
	public static void searchFileRefrence(string path, bool loadFile, Dictionary<string, UnityEngine.Object> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		refrenceList.Clear();
		HashSet<string> guidList = new HashSet<string>();
		guidList.Add(AssetDatabase.AssetPathToGUID(path));
		bool isTexture = endWith(path, ".png", false) ||
						endWith(path, ".tga", false) ||
						endWith(path, ".jpg", false) ||
						endWith(path, ".cubemap", false) ||
						endWith(path, ".tif", false);
		// 如果是图片文件,则需要查找其中包含的sprite
		if (isTexture)
		{
			openTxtFileLines(projectPathToFullPath(path + ".meta"), out string[] lines, false);
			string keyStr = "spriteID: ";
			int[] keyNextIndex;
			generateNextIndex(keyStr, out keyNextIndex);
			foreach (var item in lines)
			{
				if (KMPSearch(item, keyStr, keyNextIndex) < 0)
				{
					continue;
				}
				int index = item.IndexOf(keyStr) + keyStr.Length;
				if (item.Length >= index + GUID_LENGTH)
				{
					guidList.Add(item.Substring(index, GUID_LENGTH));
				}
			}
		}
		bool isShader = endWith(path, ".shader");
		bool isAnimClip = endWith(path, ".anim");
		bool isAnimator = endWith(path, ".controller") ||
						  endWith(path, ".overrideController");
		bool isModel = endWith(path, ".fbx", false);
		foreach (var item in guidList)
		{
			searchFiles("*.prefab", item, path, loadFile, refrenceList, allFileText);
			searchFiles("*.unity", item, path, loadFile, refrenceList, allFileText);
			// 只有贴图和shader才会从材质中查找引用
			if (isTexture || isShader)
			{
				searchFiles("*.mat", item, path, loadFile, refrenceList, allFileText);
			}
			searchFiles("*.asset", item, path, loadFile, refrenceList, allFileText);
			// 只有动画文件才会在状态机中查找
			if (isAnimClip || isAnimator || isModel)
			{
				searchFiles("*.controller", item, path, loadFile, refrenceList, allFileText);
				searchFiles("*.overrideController", item, path, loadFile, refrenceList, allFileText);
			}
			searchFiles("*.meta", item, path, loadFile, refrenceList, allFileText);
		}
	}
	public static Dictionary<string, List<FileGUIDLines>> getAllResourceFileText(string[] patterns = null)
	{
		return getAllFileText(FrameDefine.F_GAME_RESOURCES_PATH, patterns);
	}
	public static bool hasGUID(string line)
	{
		int count = line.Length;
		if (count < GUID_LENGTH)
		{
			return false;
		}
		int guidCharCount = 0;
		for (int i = 0; i < count; ++i)
		{
			if (!isGUIDChar(line[i]))
			{
				guidCharCount = 0;
				continue;
			}
			// 连续32个字符都是符合guid字符规则,则可能包含guid
			++guidCharCount;
			if (guidCharCount >= GUID_LENGTH)
			{
				return true;
			}
		}
		return false;
	}
	public static bool isGUIDChar(char value) { return isNumberic(value) || isLower(value); }
	public static Dictionary<string, List<FileGUIDLines>> getAllFileText(string path, string[] patterns = null)
	{
		List<string> supportPatterns = new List<string>() { ".prefab", ".unity", ".mat", ".asset", ".meta", ".controller", ".overrideController" };
		if (patterns != null)
		{
			supportPatterns.AddRange(patterns);
		}
		// key是后缀名,value是该后缀名的文件信息列表
		var allFileText = new Dictionary<string, List<FileGUIDLines>>();
		var files = new List<string>();
		findFiles(path, files, supportPatterns);
		foreach (var item in files)
		{
			string curSuffix = getFileSuffix(item);
			if (!allFileText.TryGetValue(curSuffix, out List<FileGUIDLines> guidLines))
			{
				guidLines = new List<FileGUIDLines>();
				allFileText.Add(curSuffix, guidLines);
			}
			FileGUIDLines fileGUIDLines = new FileGUIDLines();
			string fileContent = File.ReadAllText(item);
			string[] lines = split(fileContent, '\n');
			List<string> list = new List<string>();
			foreach (var lineItem in lines)
			{
				if (hasGUID(lineItem))
				{
					list.Add(lineItem);
				}
			}
			string fileName = item;
			rightToLeft(ref fileName);
			fullPathToProjectPath(ref fileName);
			fileGUIDLines.mProjectFileName = fileName;
			fileGUIDLines.mContainGUIDLines = list;
			guidLines.Add(fileGUIDLines);
		}
		return allFileText;
	}
	// 根据后缀获取指定文件路径下的指定资源的所有GUID(filePath:查找路径, assetType : 后缀类型名, tipText : 查找类型提示,默认为空)
	public static Dictionary<string, string> getAllGUIDBySuffixInFilePath(string filePath, string assetType, string tipText = "")
	{
		var files = new List<string>();
		var allGUIDDic = new Dictionary<string, string>();
		findFiles(filePath, files, assetType);
		int fileCount = files.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("正在查找所有" + tipText + "资源", "进度:", i + 1, fileCount);
			string file = files[i];
			openTxtFileLines(file, out string[] lines);
			foreach (var line in lines)
			{
				if (line.Contains("guid: "))
				{
					allGUIDDic.Add(removeStartString(line, "guid: "), file);
					break;
				}
			}
		}
		EditorUtility.ClearProgressBar();
		return allGUIDDic;
	}
	// 根据后缀获取指定文件路径下指定类型资源的所有GUID和spriteID(filePath:查找路径, assetType : 后缀类型名,tipText : 查找类型提示,默认为空)
	public static Dictionary<string, string> getAllGUIDAndSpriteIDBySuffixInFilePath(string filePath, string assetType, string tipText = "")
	{
		var files = new List<string>();
		var allGUIDDic = new Dictionary<string, string>();
		findFiles(filePath, files, assetType);
		int fileCount = files.Count;
		const string spritesArrMark = "TextureImporter:";
		for (int i = 0; i < fileCount; ++i)
		{
			EditorUtility.DisplayProgressBar("正在查找所有" + tipText + "资源", "进度:" + (i + 1) + "/" + fileCount, (float)(i + 1) / fileCount);
			string file = files[i];
			openTxtFileLines(file, out string[] lines);
			for (int j = 0; j < lines.Length - 1; ++j)
			{
				if (lines[j].Contains("guid: "))
				{
					allGUIDDic.Add(removeStartString(lines[j], "guid: "), file);
					// 如果.meat文件中guid的下一行为"TextureImporter:"说明这个.meta文件为图集类型的.meta文件
					if (lines[j + 1].Contains(spritesArrMark))
					{
						continue;
					}
					break;
				}
				if (hasGUID(lines[j]) && lines[j].Contains("spriteID: "))
				{
					int startIndex = findFirstSubstr(lines[j], "spriteID: ", 0, true);
					string spriteID = lines[j].Substring(startIndex);
					if (!allGUIDDic.ContainsKey(spriteID))
					{
						allGUIDDic.Add(spriteID, file);
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();
		return allGUIDDic;
	}
	// 获得文件中引用到了cs脚本的所在行
	public static Dictionary<string, FileGUIDLines> getScriptRefrenceFileText(string path)
	{
		// key是后缀名,value是该后缀名的文件信息列表
		var allFileText = new Dictionary<string, FileGUIDLines>();
		List<string> files = new List<string>();
		findFiles(path, files, new List<string>() { ".prefab", ".unity" });
		int filesCounts = files.Count;
		int curFileIndex = 0;
		foreach (var item in files)
		{
			EditorUtility.DisplayProgressBar("查找所有脚本文件的引用", "进度:" + (curFileIndex + 1) + "/" + filesCounts, (float)(curFileIndex + 1) / filesCounts);
			var fileGUIDLines = new FileGUIDLines();
			openTxtFileLines(item, out string[] lines);
			List<string> list = new List<string>();
			foreach (var lineItem in lines)
			{
				if (hasGUID(lineItem) && lineItem.Contains("m_Script:"))
				{
					int startIndex = findFirstSubstr(lineItem, "guid: ", 0, true);
					int endIndex = findFirstSubstr(lineItem, ", ", startIndex);
					list.Add(lineItem.Substring(startIndex, endIndex - startIndex));
				}
			}
			fileGUIDLines.mProjectFileName = fullPathToProjectPath(rightToLeft(item));
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
			++curFileIndex;
		}
		EditorUtility.ClearProgressBar();
		return allFileText;
	}
	// 获取引用的所有材质的guid拼接的字符串
	public static string getGUIDSplitStr(string[] lines, int startIndex)
	{
		// 内置材质GUID
		const string BUILD_IN_GUID = "0000000000000000f000000000000000";
		string splitStr = "";
		for (int i = startIndex + 1; i < lines.Length - 1; ++i)
		{
			if (!hasGUID(lines[i]))
			{
				break;
			}
			if (lines[i].Contains(BUILD_IN_GUID))
			{
				continue;
			}
			int subStartIndex = findFirstSubstr(lines[i], "guid: ", 0, true);
			int subEndIndex = findFirstSubstr(lines[i], ", ", subStartIndex);
			splitStr += "-" + lines[i].Substring(subStartIndex, subEndIndex - subStartIndex);
		}
		return splitStr;
	}
	// 获得文件中引用到了Material的所在行
	public static Dictionary<string, FileGUIDLines> getMaterialRefrenceFileText(string path)
	{
		// key是文件名,value是文件信息列表
		var allFileText = new Dictionary<string, FileGUIDLines>();
		List<string> fileList = new List<string>();
		findFiles(path, fileList, new List<string> { ".prefab", ".unity" });
		int filesCounts = fileList.Count;
		int curFileIndex = 0;
		foreach (var item in fileList)
		{
			EditorUtility.DisplayProgressBar("查找所有材质的引用", "进度:" + (curFileIndex + 1) + "/" + filesCounts, (float)(curFileIndex + 1) / filesCounts);
			var fileGUIDLines = new FileGUIDLines();
			openTxtFileLines(item, out string[] lines);
			List<string> list = new List<string>();
			for (int i = 0; i < lines.Length; ++i)
			{
				if (lines[i].Contains("m_Materials:") && !isEmpty(getGUIDSplitStr(lines, i)))
				{
					list.Add(getGUIDSplitStr(lines, i));
				}
			}
			fileGUIDLines.mProjectFileName = fullPathToProjectPath(rightToLeft(item));
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
			++curFileIndex;
		}
		EditorUtility.ClearProgressBar();
		return allFileText;
	}
	// 获得文件中具有引用的所在行
	public static Dictionary<string, FileGUIDLines> getAllRefrenceFileText(string path)
	{
		// key是后缀名,value是该后缀名的文件信息列表
		var allFileText = new Dictionary<string, FileGUIDLines>();
		List<string> files = new List<string>();
		findFiles(path, files, new List<string>() { ".prefab", ".unity", ".mat", ".controller" });
		int filesCounts = files.Count;
		int curFileIndex = 0;
		foreach (var item in files)
		{
			EditorUtility.DisplayProgressBar("查找所有的引用", "进度:" + (curFileIndex + 1) + "/" + filesCounts, (float)(curFileIndex + 1) / filesCounts);
			var fileGUIDLines = new FileGUIDLines();
			openTxtFileLines(item, out string[] lines);
			List<string> list = new List<string>();
			foreach (var lineItem in lines)
			{
				if (hasGUID(lineItem) && lineItem.Contains("guid: "))
				{
					int startIndex = findFirstSubstr(lineItem, "guid: ", 0, true);
					int endIndex = findFirstSubstr(lineItem, ',', startIndex);
					list.Add(lineItem.Substring(startIndex, endIndex - startIndex));
				}
			}
			fileGUIDLines.mProjectFileName = rightToLeft(item);
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
			++curFileIndex;
		}
		EditorUtility.ClearProgressBar();
		return allFileText;
	}
	public static Dictionary<string, string> checkAtlasNotExistSprite(string path)
	{
		var notExistsprites = new Dictionary<string, string>();
		var spriteGUIDs = getSpriteGUIDs(path);
		string atlasContent = openTxtFile(projectPathToFullPath(path + ".meta"), true);
		int startIndex = atlasContent.IndexOf("externalObjects: {}");
		foreach (var item in spriteGUIDs)
		{
			if (atlasContent.IndexOf(item.Value, startIndex) == -1)
			{
				notExistsprites.Add(item.Key, item.Value);
			}
		}
		return notExistsprites;
	}
	public static BuildReport buildAndroid(string apkPath, BuildOptions buildOptions)
	{
		return buildGame(apkPath, BuildTarget.Android, BuildTargetGroup.Android, buildOptions);
	}
	public static BuildReport buildWindows(string outputPath, BuildOptions buildOptions)
	{
		return buildGame(outputPath, BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, buildOptions);
	}
	public static BuildReport buildIOS(string outputPath, BuildOptions buildOptions)
	{
		return buildGame(outputPath, BuildTarget.iOS, BuildTargetGroup.iOS, buildOptions);
	}
	public static BuildReport buildMacOS(string outputPath, BuildOptions buildOptions)
	{
		return buildGame(outputPath, BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, buildOptions);
	}
	public static bool fixMeshOfMeshCollider(GameObject go)
	{
		if (go == null)
		{
			return false;
		}

		bool modified = false;
		var collider = go.GetComponent<MeshCollider>();
		var meshFiliter = go.GetComponent<MeshFilter>();
		if (collider != null && meshFiliter != null)
		{
			collider.sharedMesh = meshFiliter.sharedMesh;
			collider.convex = false;
			modified = true;
		}
		// 修复所有子节点
		Transform transform = go.transform;
		for (int i = 0; i < transform.childCount; ++i)
		{
			modified |= fixMeshOfMeshCollider(transform.GetChild(i).gameObject);
		}
		return modified;
	}
	public static void displayProgressBar(string title, string info, int curCount, int totalCount)
	{
		EditorUtility.DisplayProgressBar(title, info + (curCount + 1) + "/" + totalCount, (float)(curCount + 1) / totalCount);
	}
	public static UnityEngine.Object loadAsset(string filePath)
	{
		return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
	}
	public static GameObject loadGameObject(string filePath)
	{
		return loadAsset(filePath) as GameObject;
	}
	// 图片尺寸是否为2的n次方
	public static bool isSizePow2(Texture2D tex)
	{
		return isPow2(tex.width) && isPow2(tex.height);
	}
	// 是否忽略该文件
	public static bool isIgnoreFile(string filePath, IEnumerable<string> ignoreArr = null)
	{
		// ILRuntime自动生成的代码需要忽略
		foreach (var str in FrameDefineExtension.IGNORE_SCRIPTS_CHECK)
		{
			if (filePath.Contains(str))
			{
				return true;
			}
		}
		if (ignoreArr != null)
		{
			foreach (var element in ignoreArr)
			{
				if (filePath.Contains(element))
				{
					return true;
				}
			}
		}
		return false;
	}
	// 获取常量变量名称,如果这行不是常量返回值为空
	public static string findConstVariableName(string codeLine)
	{
		codeLine = removeComment(codeLine);
		codeLine = removeQuotedStrings(codeLine);
		// 因为常量定义行必须包含 const 关键字,所以不包含时返回空值
		if (!codeLine.Contains(" const "))
		{
			return null;
		}

		// 从等号前找到第一个字段命名字符,开始获取常量名,继续往前找,知道再找到一个非字段命名字符,截取出常量名
		int equalIndex = codeLine.IndexOf('=');
		if (equalIndex < 0)
		{
			return null;
		}
		return findLastFunctionString(codeLine, equalIndex, out _);
	}
	// 获取枚举类型名称,如果这行不是枚举类型则返回空
	public static string findEnumVariableName(string codeLine)
	{
		codeLine = removeComment(codeLine);
		codeLine = removeQuotedStrings(codeLine);
		string[] codeLis = split(codeLine, ' ', '\t', ':');
		if (codeLis == null)
		{
			return null;
		}
		for (int i = 0; i < codeLis.Length; ++i)
		{
			if (codeLis[i] == "enum")
			{
				if (i + 1 < codeLis.Length)
				{
					return codeLis[i + 1];
				}
				break;
			}
		}
		return null;
	}
	// 返回成员变量的名字 如果不是成员变量返回null
	public static string findMemberVariableName(string codeLine)
	{
		// 移除注释和字符串
		codeLine = removeComment(codeLine);
		codeLine = removeQuotedStrings(codeLine);

		// 移除;
		int semiIndex = codeLine.IndexOf(';');
		if (semiIndex >= 0)
		{
			codeLine = codeLine.Remove(semiIndex);
		}

		// 移除=以及后面所有的字符
		int euqalIndex = codeLine.IndexOf('=');
		if (euqalIndex >= 0)
		{
			codeLine = codeLine.Remove(euqalIndex);
		}

		// 从后往前把所有的空格和制表符移除
		codeLine = removeEndEmpty(codeLine);

		// 移除模板参数
		codeLine = removeFirstBetweenPairChars(codeLine, '<', '>', out _, out _);

		// 先根据空格分割字符串
		string[] elements = split(codeLine, ' ', '\t');
		if (elements == null || elements.Length < 2)
		{
			return null;
		}
		List<string> elementList = new List<string>(elements);
		// 移除开始的public,protected,private,static等修饰符
		bool hasPermission = false;
		while (elementList.Count > 0)
		{
			string firstString = elementList[0];
			if (firstString == "public" || firstString == "protected" || firstString == "private")
			{
				hasPermission = true;
			}
			if (firstString == "public" ||
				firstString == "protected" ||
				firstString == "private" ||
				firstString == "static" ||
				firstString == "const" ||
				firstString == "volatile" ||
				firstString == "readonly")
			{
				elementList.RemoveAt(0);
			}
			else
			{
				break;
			}
		}
		// 成员变量需要有访问权限设置
		if (!hasPermission)
		{
			return null;
		}
		// 剩下的字符串应该是一个类型和变量名,所以需要判断出除了最后一个元素以外,其他元素是否能组成一个类型,如果只有2个元素了,则就认为是变量定义
		string variableName = elementList[elementList.Count - 1];
		if (elementList.Count != 2)
		{
			return null;
		}
		// 如果最终获取的不符合变量名的语法,则不是变量名
		if (!isFunctionName(variableName))
		{
			return null;
		}
		return variableName;
	}
	// 获取一行中的类名,如果这一行不是类定义行返回值为空
	public static string findClassName(string codeLine)
	{
		// 移除注释和字符串
		codeLine = removeComment(codeLine);
		codeLine = removeQuotedStrings(codeLine);

		int startIndex;
		if (!isClassLine(codeLine))
		{
			startIndex = findFirstSubstr(codeLine, " struct ", 0, true);
		}
		else
		{
			startIndex = findFirstSubstr(codeLine, " class ", 0, true);
		}
		if (startIndex < 0)
		{
			return null;
		}
		int spaceIndex = codeLine.IndexOf(' ', startIndex);
		int colonIndex = codeLine.IndexOf(':', startIndex);
		// 两个符号均未找到
		if (spaceIndex < 0 && colonIndex < 0)
		{
			return null;
		}
		// 两个符号找到其中一个
		if (spaceIndex < 0 || colonIndex < 0)
		{
			return codeLine.Substring(startIndex, getMax(spaceIndex, colonIndex) - startIndex);
		}
		// 两个符号全部都找到
		return codeLine.Substring(startIndex, getMin(spaceIndex, colonIndex) - startIndex);
	}
	// 查找作用域(代码行数组, 类声明下标, 结束下标)
	public static bool findRegionBody(string[] lines, int classNameIndex, out int endIndex)
	{
		// 未配对的大括号数量
		int num = 0;
		for (int i = classNameIndex; i < lines.Length; ++i)
		{
			foreach (var item in lines[i])
			{
				if (item == '{')
				{
					++num;
				}
				if (item == '}')
				{
					if (--num == 0)
					{
						endIndex = i;
						return true;
					}
				}
			}
		}
		endIndex = -1;
		return false;
	}
	// 检测注释标准
	public static void doCheckCommentStandard(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 如果是文件流,网址行,移除干扰字符串
			int index = findFirstSubstr(lines[i], "://", 0, true);
			if (index >= 0)
			{
				lines[i] = lines[i].Substring(index);
			}
			// 注释下标
			int noteIndex = findFirstSubstr(lines[i], "//", 0, true);
			if (noteIndex < 0)
			{
				continue;
			}
			string noteStr = lines[i].Substring(noteIndex);
			// 超过10个'-'代表该行为分割行,忽略
			if (noteStr.Contains("----------"))
			{
				continue;
			}
			if (noteStr.Length > 0 && noteStr[0] != ' ')
			{
				Debug.LogError("注释双斜线后一位应该为空格" + addFileLine(filePath, i + 1));
				continue;
			}
			// 移除所有空格和制表符
			noteStr = removeAllEmpty(noteStr);
			if (noteStr.Length == 0)
			{
				Debug.LogError("注释后没有内容, 应当移除注释" + addFileLine(filePath, i + 1));
				continue;
			}
		}
	}
	// 逐行检测成员变量的赋值
	public static void doCheckScriptMemberVariableValueAssignment(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 移除注释
			string line = lines[i];
			line = removeComment(line);
			// 查找类名,查找不到类名说明不是类声明行,跳过,检索下一行
			if (findClassName(line) == null)
			{
				continue;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				continue;
			}
			// 查找基类如果继承自MonoBehaviour就忽略
			if (findBaseClassName(line) == typeof(MonoBehaviour).Name)
			{
				for (int j = i; j <= endIndex; ++j)
				{
					lines[j] = EMPTY;
				}
				i = endIndex + 1;
				continue;
			}

			// 不符合忽略条件的在类块内进行检查
			for (int j = i + 1; j < endIndex; ++j)
			{
				string codeLine = lines[j];
				if (findMemberVariableName(codeLine) == null)
				{
					continue;
				}
				// 不检测常量与全局变量的赋值
				string[] codeList = split(codeLine, ' ');
				if (arrayContains(codeList, "static") || arrayContains(codeList, "const"))
				{
					continue;
				}
				// 移除前面所有制表符
				codeLine = removeStartEmpty(codeLine);
				// 说明变量有特性修饰
				if (codeLine[0] == '[')
				{
					codeLine = codeLine.Substring(codeLine.IndexOf(']') + 1);
				}
				if (codeLine.IndexOf('=') >= 0)
				{
					Debug.LogError("有成员变量在定义时被赋值: " + addFileLine(filePath, j + 1));
				}
			}
		}
	}
	// 根据名称获取程序集
	public static Assembly getAssembly(string assemblyName)
	{
		// 获取Assembly集合
		Assembly[] assembly = System.AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assembly.Length; ++i)
		{
			// 获取工程
			if (assembly[i].GetName().Name == assemblyName)
			{
				return assembly[i];
			}
		}
		return null;
	}
	// 加载热更程序集
	public static Assembly loadHotFixAssembly()
	{
		if (!isFileExist(FrameDefine.F_ASSET_BUNDLE_PATH + FrameDefine.ILR_FILE))
		{
			return null;
		}
		return Assembly.LoadFile(FrameDefine.F_ASSET_BUNDLE_PATH + FrameDefine.ILR_FILE);
	}
	public static Type findClass(Assembly assembly, string className)
	{
		if (assembly == null)
		{
			return null;
		}
		// 获取到类型
		Type[] types = assembly.GetTypes();
		for (int i = 0; i < types.Length; ++i)
		{
			if (types[i].Name == className)
			{
				return types[i];
			}
		}
		return null;
	}
	// 检测命令命名规范
	public static void doCheckCommandName(string filePath, string[] lines, Assembly csharpAssembly, Assembly hotfixAssembly)
	{
		if (!filePath.Contains("/CommandSystem/"))
		{
			return;
		}
		string folder = getFolderName(filePath);
		//处于CommandSystem文件夹下的脚本忽略
		if (folder == "CommandSystem")
		{
			return;
		}
		if (!folder.StartsWith("Cmd"))
		{
			checkScriptTip("该命令文件夹没有以Cmd开头: " + folder, filePath, 0);
			return;
		}
		if (folder != "CmdGlobal" && folder != "CmdWindow")
		{
			string receiverClass = removeStartString(folder, "Cmd");
			if (findClass(csharpAssembly, receiverClass) == null && findClass(hotfixAssembly, receiverClass) == null)
			{
				checkScriptTip("未找到命令接收者的类: " + receiverClass, filePath, 0);
				return;
			}
		}
		if (!getFileName(filePath).StartsWith(folder))
		{
			checkScriptTip("该命令脚本没有以目录名开头", filePath, 0);
			return;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			string className = findClassName(lines[i]);
			// 该行代码不是类的命名行
			if (className == null)
			{
				continue;
			}
			int templateIndex = className.IndexOf('<');
			// 包含'<'符号说明是泛型类
			if (templateIndex > 0)
			{
				className = className.Substring(0, templateIndex);
			}
			// 判断类名是否与脚本文件名相同
			if (className != getFileNameNoSuffix(filePath, true))
			{
				checkScriptTip("该命令类名与脚本名不一致", filePath, i + 1);
			}
			break;
		}
	}
	// 获取一类声明行中的该类的父类的名字,如果这一行不是类声明行或不继承任何类返回值为空
	public static string findBaseClassName(string codeLine)
	{
		if (!isClassLine(codeLine))
		{
			return null;
		}
		int colonIndex = codeLine.IndexOf(':');
		// 该类没有继承对象
		if (colonIndex < 0)
		{
			return null;
		}
		int startIndex = -1;
		int endIndex = -1;
		for (int i = colonIndex + 1; i < codeLine.Length; ++i)
		{
			if (codeLine[i] == ' ')
			{
				continue;
			}
			if (startIndex < 0)
			{
				startIndex = i;
				continue;
			}
			if (codeLine[i] == ' ' || codeLine[i] == ',')
			{
				endIndex = i;
				break;
			}
		}
		if (startIndex > 0 && endIndex < 0)
		{
			endIndex = codeLine.Length;
		}
		if (startIndex < 0 || endIndex < 0)
		{
			return null;
		}
		return codeLine.Substring(startIndex, endIndex - startIndex);
	}
	// 逐行检查脚本中的命名规范
	public static void doCheckScriptLineByLine(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 移除注释
			string codeLine = removeComment(lines[i]);
			// 查找类名
			string className = findClassName(codeLine);
			if (className == null)
			{
				continue;
			}
			// 类名以Debug结尾且继承自MonoBehaviour的类, 需要忽略
			if (findBaseClassName(codeLine) == typeof(MonoBehaviour).Name && endWith(className, "Debug"))
			{
				continue;
			}
			if (isIgnoreFile(filePath, FrameDefineExtension.IGNORE_FILE_CHECK_FUNCTION))
			{
				return;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				continue;
			}

			// 忽略包含SCJsonHttp, SCJsonHttp, JsonUDPData的类,并将类块内容置空
			bool ignoreClass = false;
			foreach (var item in FrameDefineExtension.IGNORE_CHECK_CLASS)
			{
				// 包含忽略字符串
				if (className.Contains(item))
				{
					ignoreClass = true;
					break;
				}
			}
			// 类名内包含忽略字符串,将内容置空
			if (ignoreClass)
			{
				for (int j = i; j <= endIndex; ++j)
				{
					lines[j] = EMPTY;
				}
				i = endIndex + 1;
				continue;
			}
			// 不包含忽略字符串在类块内进行检查
			for (int j = i + 1; j < endIndex; ++j)
			{
				string line = lines[j];
				if (!line.Contains("public ") && !line.Contains("protected ") && !line.Contains("private "))
				{
					continue;
				}
				// 检查命名规范
				doCheckFunctionName(filePath, j, line, className);
				doCheckConstVariable(filePath, j, line);
				doCheckEnumVariable(filePath, j, line);
				doCheckNormalVariable(filePath, j, line, lines[getMin(j + 1, lines.Length - 1)]);
			}
		}
	}
	// 检查常量
	public static void doCheckConstVariable(string filePath, int index, string codeLine)
	{
		string constVarName = findConstVariableName(codeLine);
		if (constVarName == null)
		{
			return;
		}
		if (!isUpperString(constVarName))
		{
			Debug.LogError("常量命名不规范" + addFileLine(filePath, index + 1));
		}
	}
	// 检查枚举
	public static void doCheckEnumVariable(string filePath, int index, string codeLine)
	{
		string enumName = findEnumVariableName(codeLine);
		// 返回值为空说明这一行不是枚举声明行
		if (isEmpty(enumName))
		{
			return;
		}

		// 枚举必须全部为大写
		if (!isUpperString(enumName))
		{
			Debug.LogError("枚举必须全部是大写" + addFileLine(filePath, index + 1));
		}
	}
	// 检查函数命名规范(所属文件路径, 所处行数, 代码行内容, 所属类名)
	public static void doCheckFunctionName(string filePath, int index, string codeLine, string className)
	{
		// 返回值为空说明这一行不是函数声明行
		if (!findFunctionName(codeLine, out bool isContructor, out string functionName))
		{
			return;
		}
		// 忽略构造函数
		if (isContructor)
		{
			return;
		}
		// 忽略指定的函数
		foreach (var item in FrameDefineExtension.IGNORE_CHECK_FUNCTION)
		{
			if (codeLine.Contains(item))
			{
				return;
			}
		}
		// 全大写的函数名可忽略
		if (isUpperString(functionName))
		{
			return;
		}
		if (!isLower(functionName[0]))
		{
			Debug.LogError("函数命名不规范: 需要以小写字母开头" + addFileLine(filePath, index + 1));
		}
	}
	// 检查普通变量
	public static void doCheckNormalVariable(string filePath, int index, string codeLine, string nextLine)
	{
		string[] codeList = split(codeLine, ' ');
		if (arrayContains(codeList, "static") || arrayContains(codeList, "const"))
		{
			return;
		}
		string normalVariable = findMemberVariableName(codeLine);
		if (normalVariable == null)
		{
			return;
		}
		// 忽略属性变量
		if (nextLine.IndexOf('{') >= 0)
		{
			return;
		}
		// 变量名长度必须大于2并且首字母必须以m开头且m后的首字母要大写
		if (normalVariable.Length < 2)
		{
			Debug.LogError("变量命名不规范: 成员变量长度小于2" + addFileLine(filePath, index + 1));
			return;
		}
		if (isUpper(normalVariable[0]))
		{
			Debug.LogError("变量命名不规范: 成员变量以大写字母开头" + addFileLine(filePath, index + 1));
			return;
		}
		if (normalVariable[0] != 'm')
		{
			Debug.LogError("变量命名不规范: 成员变量没有以m开头" + addFileLine(filePath, index + 1));
			return;
		}
		if (!isUpper(normalVariable[1]))
		{
			Debug.LogError("变量命名不规范: 成员变量m前缀后字母应该大写" + addFileLine(filePath, index + 1));
			return;
		}
	}
	// 检查单行代码长度
	public static void doCheckSingleCheckCodeLineWidth(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			// 设置忽略,忽略函数命名,函数命名行,有长字符串行,成员变量定义,委托定义
			if (findFunctionName(line, out _, out _) ||
				hasLongStr(line) ||
				findMemberVariableName(line) != null ||
				line.Contains(" delegate "))
			{
				continue;
			}
			if (generateCharWidth(line) > 140)
			{
				Debug.LogError("单行代码太长,超出了140个字符宽度" + addFileLine(filePath, i + 1));
				findMemberVariableName(line);
			}
		}
	}
	// 检查分隔代码行宽度
	public static void doCheckCodeSeparateLineWidth(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			if (!line.Contains("//----------"))
			{
				continue;
			}
			if (line != "	//------------------------------------------------------------------------------------------------------------------------------")
			{
				checkScriptTip("分隔行字符数应该为130", filePath, i + 1);
				continue;
			}
		}
	}
	// 检查当前代码行是否有长字符串
	public static bool hasLongStr(string codeLine)
	{
		string tmpStr = "";
		bool startRecord = false;
		// 先根据双引号分割字符串
		foreach (var element in codeLine)
		{
			do
			{
				if (element != '\"')
				{
					continue;
				}
				if (!startRecord)
				{
					startRecord = true;
					continue;
				}
				if (generateCharWidth(tmpStr) > 10)
				{
					// 有长字符串 结束判断
					return true;
				}
				tmpStr = "";
				startRecord = false;
			} while (false);
			if (startRecord)
			{
				tmpStr += element;
			}
		}
		// 检测到最后没有长字符串
		return false;
	}
	// 设置在资源中丢失引用的对象列表
	public static void setCheckRefObjectsList(List<string> missingRefObjectsList, string fileName, IEnumerable<string> missingList)
	{
		openTxtFileLines(fileName, out string[] lines);
		for (int i = 0; i < lines.Length; ++i)
		{
			foreach (var guid in missingList)
			{
				if (lines[i].Contains(guid))
				{
					missingRefObjectsList.Add(findObjectNameWithScriptGUID(lines, i));
					break;
				}
			}
		}
	}
	// 根据对象中的GUID获取引用对应资源的对象的名字
	public static string findObjectNameWithScriptGUID(string[] lines, int scriptLineIndex)
	{
		string refID = null;
		int refIDIndex = 0;
		for (int i = scriptLineIndex; i > 0; --i)
		{
			if (refID == null && lines[i].IndexOf('&') >= 0)
			{
				refID = lines[i].Substring(lines[i].IndexOf('&') + 1);
			}
			if (!isEmpty(refID) && lines[i].Contains("fileID: " + refID))
			{
				refIDIndex = i;
				break;
			}
		}
		for (int i = refIDIndex; i < scriptLineIndex; ++i)
		{
			if (lines[i].Contains("m_Name: "))
			{
				return lines[i].Substring(lines[i].IndexOf(": ") + 1);
			}
		}
		return null;
	}
	// 输出丢失资源引用信息
	public static void debugMissingRefInformation(Dictionary<string, List<string>> missingRefAssetsList, string missingType)
	{
		// 输出定位丢失脚本引用的资源信息
		foreach (var lineData in missingRefAssetsList)
		{
			// 将丢失引用的资源中的丢失引用对象的列表存入列表中
			var missingRefObjectsList = new List<string>();
			setCheckRefObjectsList(missingRefObjectsList, lineData.Key, lineData.Value);
			string assetPath = lineData.Key;
			UnityEngine.Object obj = loadAsset(assetPath);
			Debug.LogError("有" + missingType + "的引用丢失" + obj.name + "\n" + stringsToString(missingRefObjectsList, '\n'), obj);
			if (endWith(assetPath, ".unity"))
			{
				for (int i = 0; i < missingRefObjectsList.Count; ++i)
				{
					Debug.LogError("有" + missingType + "的引用丢失,Scene:" + assetPath + "\n" + stringsToString(missingRefObjectsList, '\n'));
				}
			}
		}
	}
	// 检查是否引用了该列表中的GUID,如果引用就加入到错误引用字典中(参数: 错误引用列表, 带有引用的文件信息字典, 要进行比对的GUID列表)
	// 检查fileDic中的文件是否有引用GUIDsList中的资源,如果有,则将其放入检测结果列表中
	// errorRefAssetDic的第一个Key是文件的路径,第二个Key是非法引用的GUID,Value是此非法GUID的文件名
	// guidList的key是GUID,value是此GUID的文件名
	public static void doCheckRefGUIDInFilePath(Dictionary<string, Dictionary<string, string>> errorRefAssetDic,
												   Dictionary<string, FileGUIDLines> fileDic,
												   Dictionary<string, string> guidList)
	{
		foreach (var fileData in fileDic)
		{
			FileGUIDLines projectFile = fileData.Value;
			foreach (var guid in projectFile.mContainGUIDLines)
			{
				if (!guidList.TryGetValue(guid, out string fileName))
				{
					continue;
				}
				if (!errorRefAssetDic.TryGetValue(fileData.Value.mProjectFileName, out Dictionary<string, string> errorRefGUIDList))
				{
					errorRefGUIDList = new Dictionary<string, string>();
					errorRefAssetDic.Add(projectFile.mProjectFileName, errorRefGUIDList);
				}
				errorRefGUIDList.Add(guid, fileName);
			}
		}
	}
	// 检查Protobuf的消息字段的顺序
	public static void doCheckProtoMemberOrder(string file, string[] lines)
	{
		int realOrder = 0;
		bool findProtoContract = false;
		for (int i = 0; i < lines.Length - 1; ++i)
		{
			if (lines[i] == "[ProtoContract]")
			{
				findProtoContract = true;
				continue;
			}
			if (!findProtoContract)
			{
				continue;
			}
			if (lines[i].Contains("[ProtoMember("))
			{
				int startIndex = findFirstSubstr(lines[i], "[ProtoMember(", 0, true);
				int endIndex = findFirstSubstr(lines[i], ',', startIndex);
				if (SToI(lines[i].Substring(startIndex, endIndex - startIndex)) - 1 != realOrder++)
				{
					Debug.LogError("Protobuf的消息字段顺序检测:有不符合规定的字段顺序." + addFileLine(file, i + 1));
				}
			}
		}
	}
	// 检查空行
	public static void doCheckEmptyLine(string file, string[] lines)
	{
		// 移除开始的空格和空行
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeStartEmpty(lines[i]);
		}

		// 函数名的上一行不能为空行,需要保留空白字符进行检测
		for (int i = 1; i < lines.Length; ++i)
		{
			if (findFunctionName(lines[i], out _, out _) && lines[i - 1].Length == 0)
			{
				Debug.LogError("函数名的上一行发现空行." + addFileLine(file, i));
			}
		}

		// 先去除所有行的空白字符
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeAllEmpty(lines[i]);
		}
		// 不能有两个连续空行
		for (int i = 0; i < lines.Length - 1; ++i)
		{
			if (lines[i].Length == 0 && lines[i + 1].Length == 0)
			{
				Debug.LogError("有连续两个空行." + addFileLine(file, i + 1));
			}
		}
		// 左大括号的下一行不能为空行
		for (int i = 1; i < lines.Length - 1; ++i)
		{
			if (lines[i] == "{" && lines[i + 1].Length == 0)
			{
				Debug.LogError("左大括号的下一行发现空行." + addFileLine(file, i + 2));
			}
		}
		// 右大括号的上一行不能为空行
		for (int i = 1; i < lines.Length; ++i)
		{
			if (lines[i] == "}" && lines[i - 1].Length == 0)
			{
				Debug.LogError("右大括号的上一行发现空行." + addFileLine(file, i));
			}
		}
		// 文件第一行不能为空行
		if (lines[0].Length == 0)
		{
			Debug.LogError("文件第一行发现空行." + addFileLine(file, 1));
		}
		// 文件最后一行不能为空行
		if (lines[lines.Length - 1].Length == 0)
		{
			Debug.LogError("文件最后一行发现空行." + addFileLine(file, lines.Length));
		}
	}
	// 检查空格
	public static void doCheckSpace(string file, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			// 当前字符是否在字符串中
			bool stringing = false;
			for (int j = 0; j < line.Length; ++j)
			{
				if (line[j] == '"')
				{
					stringing = !stringing;
					continue;
				}
				if (stringing)
				{
					continue;
				}
				// 运算符两边需要添加空格,如果遇到+-*/=,则需要判断后面的紧接的字符是否为=,因为这些符号只能跟=连接
				// 先判断两个字符的
				if (j < line.Length - 1 && isDoubleOperator(line[j], line[j + 1]))
				{
					// 连续两个/是注释符号,后面也需要加空格
					if (line[j] == '/' && line[j + 1] == '/')
					{
						int commentSpacePos = j + 2;
						// 如果是分隔行,则允许不带空格
						if (commentSpacePos < line.Length && line[commentSpacePos] != ' ' && line.IndexOf("--------------", commentSpacePos) < 0)
						{
							Debug.LogError("注释的双斜杠后面需要有空格" + addFileLine(file, i + 1));
						}
						// 如果已经检测到了注释,则不需要再继续检测了
						break;
					}

					int expectedFrontSpacePos = j - 1;
					int expectedBackSpacePos = j + 2;
					// 如果是++或--,则只能在一边加空格
					if (line[j] == '+' && line[j + 1] == '+' || line[j] == '-' && line[j + 1] == '-')
					{
						// 如果++前面有变量,则前面不需要加空格
						if (j > 0 && isLetter(line[j - 1]))
						{
							expectedFrontSpacePos = -1;
							// 如果后面已经有;了,则后面也不需要再添加空格
							if (j + 2 < line.Length && line[j + 2] == ';')
							{
								expectedBackSpacePos = -1;
							}
						}
						// 如果后面有变量,且前面没有任何非空字符串,则前面也不需要添加空格
						else
						{
							expectedBackSpacePos = -1;
							if (!hasNonEmptyCharInFront(line, j))
							{
								expectedFrontSpacePos = -1;
							}
						}
					}
					if (expectedBackSpacePos >= 0 && expectedBackSpacePos < line.Length && line[expectedBackSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
					}
					// 跳过下一个字符,进入下一次循环
					++j;
					continue;
				}

				// 只是一个单独的运算符
				if (isSingleOperator(line[j]))
				{
					// 需要特殊判断<>符号
					// 遇到<符号时,如果后面能找到一个>,则认为不是一个运算符
					if (line[j] == '<' && line.IndexOf('>', j) >= 0)
					{
						continue;
					}
					// 遇到>符号时,如果前面能找到一个<,则认为不是一个运算符
					if (line[j] == '>' && line.IndexOf('<', 0) >= 0 && line.IndexOf('<', 0) < j)
					{
						continue;
					}
					int expectedFrontSpacePos = j - 1;
					int expectedBackSpacePos = j + 1;
					if (expectedBackSpacePos >= 0 && expectedBackSpacePos < line.Length && line[expectedBackSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.Substring(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
					}
				}

				// 逗号后面需要添加空格
				if (line[j] == ',' && hasNonEmptyCharInBack(line, j + 1))
				{
					if (j + 1 < line.Length && line[j + 1] != ' ')
					{
						Debug.LogError("逗号后面需要有空格" + addFileLine(file, i + 1));
					}
				}
			}
		}
	}
	//检查UI变量名与节点名不一致的代码
	public static void doCheckDifferentNodeName(string file, string fileName, Transform[] allChildTrans, string[] lines)
	{
		if (lines == null || lines.Length == 0)
		{
			Debug.LogError("fields 有问题 file：" + file);
			return;
		}
		if (allChildTrans == null || allChildTrans.Length == 0)
		{
			Debug.LogError("prefab 有问题 file：" + file);
			return;
		}

		// 用于过滤UI变量
		string myUGUI = " myUGUI";
		// 用于存储变量行
		var linesDic = new Dictionary<string, int>();
		bool finish = false;
		bool start = false;
		for (int i = 0; i < lines.Length; ++i)
		{
			// 遇到函数就可以停止遍历了
			if (lines[i].IndexOf('(') >= 0)
			{
				break;
			}
			if (lines[i].Contains(myUGUI))
			{
				if (finish)
				{
					Debug.LogError("检测UI变量名规范:  在布局:" + fileName + " 中请保持UI变量中不要混入其他变量." + addFileLine(file, i));
					return;
				}
				start = true;
				// 数组变量可以忽略
				if (lines[i].Contains("[]"))
				{
					continue;
				}
				string[] variableArr = split(lines[i], ' ');
				string variableStr = variableArr[variableArr.Length - 1];
				removeStartString(ref variableStr, "m");
				removeEndString(ref variableStr, ";");
				linesDic.Add(variableStr, i + 1);
			}
			else
			{
				if (start)
				{
					finish = true;
				}
			}
		}

		// 查看类的成员变量是与prefab节点名称匹配
		int beforeIndex = -1;
		foreach (var item in linesDic)
		{
			bool found = false;
			for (int i = 0; i < allChildTrans.Length; ++i)
			{
				if (item.Key != allChildTrans[i].name)
				{
					continue;
				}
				found = true;
				// 找到的下标比上一次找到的高
				if (i > beforeIndex)
				{
					beforeIndex = i;
					break;
				}
				if (i <= beforeIndex)
				{
					Debug.LogError("在布局:" + fileName + " 变量名:m" + item.Key + " 的顺序错了." + addFileLine(file, item.Value));
					return;
				}
			}
			if (!found)
			{
				Debug.LogError("在布局:" + fileName + " 变量名:m" + item.Key + " 没有找到." + addFileLine(file, item.Value));
				return;
			}
		}
	}
	public static bool hasNonEmptyCharInFront(string str, int endIndex)
	{
		for (int i = 0; i < endIndex; ++i)
		{
			if (str[i] != '\t' && str[i] != ' ')
			{
				return true;
			}
		}
		return false;
	}
	public static bool hasNonEmptyCharInBack(string str, int startIndex)
	{
		for (int i = startIndex; i < str.Length; ++i)
		{
			// 如果遇到注释则直接返回
			if (i + 1 < str.Length && str[i] == '/' && str[i + 1] == '/')
			{
				return false;
			}
			if (str[i] != '\t' && str[i] != ' ')
			{
				return true;
			}
		}
		return false;
	}
	// 是否为运算符
	public static bool isSingleOperator(char c)
	{
		return c == '+' || c == '-' || c == '*' || c == '/' || c == '=' || c == '<' || c == '>';
	}
	// 两个连续的字符是否为合法的运算符组合
	public static bool isDoubleOperator(char c0, char c1)
	{
		// 如果第二个字符是=,则第一个字符允许是以下字符
		if (c1 == '=')
		{
			return c0 == '+' || c0 == '-' || c0 == '*' || c0 == '/' || c0 == '=' || c0 == '<' || c0 == '>' || c0 == '|' || c0 == '&' || c0 == '!';
		}
		return c0 == '+' && c1 == '+' ||
				c0 == '-' && c1 == '-' ||
				c0 == '/' && c1 == '/' ||
				c0 == '|' && c1 == '|' ||
				c0 == '&' && c1 == '&' ||
				c0 == '=' && c1 == '>' ||
				c0 == '>' && c1 == '>' ||
				c0 == '>' && c1 == '>';
	}
	// 判断一行字符是不是类声明行
	public static bool isClassLine(string codeLine)
	{
		codeLine = removeComment(codeLine);
		codeLine = removeQuotedStrings(codeLine);
		return findFirstSubstr(codeLine, " class ") >= 0;
	}
	// 判断一行字符串是不是函数名声明行,如果是,则返回是否为构造函数,函数名
	public static bool findFunctionName(string line, out bool isConstructor, out string functionName)
	{
		functionName = null;
		isConstructor = false;
		// 移除注释和字符串
		line = removeComment(line);
		line = removeQuotedStrings(line);
		// 消除所有的尖括号内的字符
		line = removeFirstBetweenPairChars(line, '<', '>', out _, out _);

		// 先根据空格分割字符串
		string[] elements = split(line, ' ', '\t');
		if (elements == null || elements.Length < 2)
		{
			return false;
		}

		// 移除可能存在的where约束
		List<string> elementList = new List<string>(elements);
		int whereIndex = elementList.IndexOf("where");
		if (whereIndex >= 0)
		{
			elementList.RemoveRange(whereIndex, elementList.Count - whereIndex);
		}

		// 移除开始的public,protected,private,static,virtual,override修饰符
		bool isAbstract = false;
		while (elementList.Count > 0)
		{
			string firstString = elementList[0];
			// 包含委托关键字则直接返回false
			if (firstString == "delegate")
			{
				return false;
			}
			if (firstString == "public" ||
				firstString == "protected" ||
				firstString == "private" ||
				firstString == "static" ||
				firstString == "virtual" ||
				firstString == "abstract" ||
				firstString == "new" ||
				firstString == "override")
			{
				if (firstString == "abstract")
				{
					isAbstract = true;
				}
				elementList.RemoveAt(0);
			}
			else
			{
				break;
			}
		}
		string str0 = elementList[0];
		if (str0 == "if" ||
			str0 == "while" ||
			str0 == "else" ||
			str0 == "foreach" ||
			str0 == "for" ||
			startWith(str0, "if(") ||
			startWith(str0, "for(") ||
			startWith(str0, "while(") ||
			startWith(str0, "foreach("))
		{
			return false;
		}

		// 然后将剩下的元素通过空格拼接在一起,还原出原始的字符串
		string newLine = stringsToString(elementList, ' ');
		// 函数名肯定会包含括号
		int leftBracketIndex = newLine.IndexOf('(');
		if (leftBracketIndex < 0)
		{
			return false;
		}
		// '('前最多有两个元素（返回值，函数名）
		List<string> functionElements = new List<string>();
		int lastElementIndex = -1;
		for (int i = leftBracketIndex; i >= 0; --i)
		{
			if (lastElementIndex < 0 && isFunctionNameChar(newLine[i]))
			{
				lastElementIndex = i;
				continue;
			}
			if (lastElementIndex >= 0)
			{
				if (!isFunctionNameChar(newLine[i]))
				{
					functionElements.Add(newLine.Substring(i + 1, lastElementIndex - i));
					lastElementIndex = -1;
				}
				else if (i == 0)
				{
					functionElements.Add(newLine.Substring(i, lastElementIndex + 1));
					lastElementIndex = -1;
				}
				if (functionElements.Count > 2)
				{
					return false;
				}
			}
		}
		isConstructor = functionElements.Count == 1;
		if (functionElements.Count > 0)
		{
			functionName = functionElements[0];
		}

		// 抽象函数没有定义,包含abstract关键字,括号
		if (isAbstract)
		{
			return newLine[newLine.Length - 1] == ';';
		}

		// 函数名与函数定义不在同一行的
		if (newLine.IndexOf('{') < 0 && newLine.IndexOf('}') < 0)
		{
			// 函数名肯定是以括号结尾
			return newLine[newLine.Length - 1] == ')';
		}
		// 函数名与函数定义在同一行
		else
		{
			// 函数定义需要包含大括号
			return newLine.IndexOf('{') >= 0;
		}
	}
	public static void doCheckAtlasNotExistSprite(string path)
	{
		var notExistSprites = checkAtlasNotExistSprite(path);
		foreach (var item in notExistSprites)
		{
			Debug.Log("图集:" + path + "中的图片:" + item.Value + "不存在", loadAsset(path));
		}
	}
	public static void doCheckAtlasRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceList = new Dictionary<string, SpriteRefrenceInfo>();
		searchSpriteRefrence(path, refrenceList, allFileText);
		foreach (var item in refrenceList)
		{
			Debug.Log("图集:" + path + "被布局:" + item.Key + "所引用, sprite:" + item.Value.mSpriteName, item.Value.mObject);
		}
		Debug.Log("图集" + path + "被" + refrenceList.Count + "个布局引用");
	}
	public static void doCheckUnusedTexture(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceMaterialList = new Dictionary<string, UnityEngine.Object>();
		// 先查找引用该贴图的材质
		searchFileRefrence(path, false, refrenceMaterialList, allFileText);
		if (refrenceMaterialList.Count == 0)
		{
			Debug.Log("资源未引用:" + path, loadAsset(path));
		}
	}
	public static void doSearchRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		var refrenceList = new Dictionary<string, UnityEngine.Object>();
		string fileName = getFileName(path);
		Debug.Log("<<<<<<<开始查找" + fileName + "的引用.......");
		searchFileRefrence(path, false, refrenceList, allFileText);
		foreach (var item in refrenceList)
		{
			Debug.Log(item.Key, loadAsset(item.Key));
		}
		Debug.Log(">>>>>>>完成查找" + fileName + "的引用, 共有" + refrenceList.Count + "处引用");
	}
	// 确保路径为相对于Project的路径
	public static string ensureProjectPath(string filePath)
	{
		if (!startWith(filePath, FrameDefine.P_ASSETS_PATH) || !startWith(filePath, FrameDefine.P_HOT_FIX_PATH))
		{
			// Assets资源路径
			if (filePath.Contains("/" + FrameDefine.P_ASSETS_PATH))
			{
				filePath = fullPathToProjectPath(filePath);
			}
			if (filePath.Contains("/" + FrameDefine.P_HOT_FIX_PATH))
			{
				// 转换HotFix中文件的绝对路径到相对路径
				filePath = FrameDefine.P_HOT_FIX_PATH + filePath.Substring(FrameDefine.F_HOT_FIX_PATH.Length);
			}
		}
		return filePath;
	}
	// 检查代码提示
	public static void checkScriptTip(string tipInfo, string filePath, int lineNumber)
	{
		filePath = ensureProjectPath(filePath);
		// 非热更文件
		if (filePath.StartsWith(FrameDefine.P_ASSETS_PATH))
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber), loadAsset(filePath));
		}
		else
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber));
		}
	}
	public static string addFileLine(string file, int line)
	{
		return "\nFile:" + ensureProjectPath(file) + "\nLine:" + line + "\n" + CODE_LOCATE_KEYWORD;
	}
	public static void removeMetaFile(List<string> fileList)
	{
		for (int i = 0; i < fileList.Count; ++i)
		{
			if (endWith(fileList[i], ".meta"))
			{
				fileList.RemoveAt(i);
				--i;
			}
		}
	}
	public static void roundRectTransformToInt(RectTransform rectTransform)
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
	public static void doCheckResetProperty(Assembly assemly, string path)
	{
		// 遍历目录,存储所有文件名和对应文本内容
		var classInfoList = new Dictionary<string, ClassInfo>();
		getCSharpFile(path, classInfoList);
		// 不需要检测的基类
		List<Type> ignoreBaseClass = new List<Type>();
		ignoreBaseClass.Add(typeof(myUIObject));
		ignoreBaseClass.Add(typeof(FrameSystem));
		ignoreBaseClass.Add(typeof(LayoutScript));
		ignoreBaseClass.Add(typeof(WindowShader));
		ignoreBaseClass.Add(typeof(PooledWindow));
		ignoreBaseClass.Add(typeof(WindowObject));
		ignoreBaseClass.Add(typeof(OBJECT));
		ignoreBaseClass.Add(typeof(ExcelData));
		ignoreBaseClass.Add(typeof(SQLiteData));
		ignoreBaseClass.Add(typeof(SceneProcedure));
		ignoreBaseClass.Add(typeof(NetPacketTCPFrame));
		ignoreBaseClass.Add(typeof(SQLiteTable));
#if USE_ILRUNTIME
		ignoreBaseClass.Add(typeof(CrossBindingAdaptorType));
#endif
		ignoreBaseClass.AddRange(FrameDefineExtension.IGNORE_RESETPROPERTY_CLASS);
		// 获取到类型
		Type[] types = assemly.GetTypes();
		for (int i = 0; i < types.Length; ++i)
		{
			Type type = types[i];
			// 是否继承自需要忽略的基类
			bool isIgnoreClass = false;
			for (int j = 0; j < ignoreBaseClass.Count; ++j)
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
			// `表示是模板类
			int templateIndex = className.IndexOf('`');
			if (templateIndex >= 0)
			{
				className = className.Substring(0, templateIndex);
			}
			if (!classInfoList.TryGetValue(className, out ClassInfo info))
			{
				Debug.LogError("class:" + className + " 程序集中有此类,但是代码文件中找不到此类");
				continue;
			}
			// LayoutSystem中的文件不需要检测
			if (info.mFilePath.Contains("/" + FrameDefine.LAYOUT_SYSTEM + "/"))
			{
				continue;
			}
			// 判断类是否包含函数resetProperty()
			// BindingFlags.DeclaredOnly 仅考虑在提供的类型的层次结构级别上声明的成员。不考虑继承的成员
			MethodInfo methodInfo = type.GetMethod(KEY_FUNCTION, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
			if (methodInfo == null)
			{
				Debug.LogError("class:" + className + " 没有包含: " + KEY_FUNCTION + "()" + addFileLine(info.mFilePath, info.mFunctionLine));
				continue;
			}

			detectResetAll(className, fieldMembers, info);
		}
	}
	public static bool detectResetAll(string className, List<MemberInfo> fieldMembers, ClassInfo classInfo)
	{
		int index = 0;
		bool find = false;
		int startIndex = -1;
		int endIndex = -1;
		bool hasOverride = false;

		for (int i = 0; i < classInfo.mLines.Count; ++i)
		{
			string line = classInfo.mLines[i];
			if (line.Contains("void " + KEY_FUNCTION + "()"))
			{
				hasOverride = line.Contains("override");
				find = true;
				classInfo.mFunctionLine += i;
			}
			if (!find)
			{
				continue;
			}
			if (line.IndexOf('{') >= 0)
			{
				if (index == 0)
				{
					startIndex = i;
				}
				++index;
			}
			if (line.IndexOf('}') >= 0)
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

		if (!find)
		{
			Debug.LogError("class:" + className + " 没有包含: " + KEY_FUNCTION + "()" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}
		if (!hasOverride && className != "ClassObject")
		{
			Debug.LogError("class:" + className + " 没有重写: " + KEY_FUNCTION + "()" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}

		// 所有待重置的成员变量列表
		List<string> notResetMemberList = new List<string>();
		for (int i = 0; i < fieldMembers.Count; ++i)
		{
			notResetMemberList.Add(fieldMembers[i].Name);
		}

		List<string> resetFunctionLines = new List<string>();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			resetFunctionLines.Add(classInfo.mLines[startIndex + i]);
		}
		List<char> letters = new List<char>();
		for (int i = 0; i < 'z' - 'a' + 1; ++i)
		{
			letters.Add((char)('A' + i));
			letters.Add((char)('a' + i));
		}
		for (int i = 0; i < 10; ++i)
		{
			letters.Add((char)('0' + i));
		}
		letters.Add('_');
		char[] seperates = generateOtherASCII(letters.ToArray());
		for (int i = 0; i < resetFunctionLines.Count; ++i)
		{
			// 文本用分隔符拆分,判断其中是否有变量名,一行最多只允许出现一个成员变量
			string[] strList = split(resetFunctionLines[i], seperates);
			for (int j = 0; j < notResetMemberList.Count; ++j)
			{
				// 如果检测到已经重置了,则将其从待重置列表中移除
				if (arrayContains(strList, notResetMemberList[j]))
				{
					notResetMemberList.RemoveAt(j);
					break;
				}
			}
		}

		// 是否调用了基类的resetProperty
		bool callBaseReset = false;
		for (int i = 0; i < resetFunctionLines.Count; ++i)
		{
			if (resetFunctionLines[i].Contains("base." + KEY_FUNCTION + "();"))
			{
				callBaseReset = true;
				break;
			}
		}
		if (!callBaseReset && className != "ClassObject")
		{
			Debug.LogError("class:" + className + " 没有调用基类resetProperty" + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}

		if (notResetMemberList.Count > 0)
		{
			string memberLines = "有如下成员未重置:\n";
			for (int i = 0; i < notResetMemberList.Count; ++i)
			{
				memberLines += notResetMemberList[i] + "\n";
			}
			Debug.LogError("class:" + className + " 成员变量未能全部重置\n" + memberLines + addFileLine(classInfo.mFilePath, classInfo.mFunctionLine));
			return false;
		}
		return true;
	}
	public static void getCSharpFile(string path, Dictionary<string, ClassInfo> fileInfos)
	{
		List<string> fileList = new List<string>();
		findFiles(path, fileList, ".cs");
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
					if (line.IndexOf('{') >= 0)
					{
						nameSpace = line.Substring(startIndex, line.IndexOf('{') - startIndex) + ".";
					}
					else
					{
						nameSpace = line.Substring(startIndex) + ".";
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
	public static void parseClass(Dictionary<string, ClassInfo> fileInfos, string nameSpace, string[] fileLines, int startIndex, int endIndex, string path)
	{
		List<string> classLines = new List<string>();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			classLines.Add(removeAll(fileLines[i + startIndex], '\t'));
		}
		string headLine = fileLines[startIndex];
		int nameStartIndex = findFirstSubstr(headLine, "class ", 0, true);
		string className;
		int colonIndex = headLine.IndexOf(':');
		if (colonIndex >= 0)
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex, colonIndex - nameStartIndex), ' ');
		}
		else
		{
			className = removeAll(nameSpace + headLine.Substring(nameStartIndex), ' ');
		}
		// 模板类,则去除模板属性,只保留类名
		int templateIndex = className.IndexOf('<');
		if (templateIndex >= 0)
		{
			className = className.Remove(templateIndex, className.IndexOf('>') - templateIndex + 1);
		}
		// 因为有些类是内部类,所以仍然存在重名情况,需要排除
		if (!fileInfos.ContainsKey(className))
		{
			ClassInfo info = new ClassInfo();
			info.mLines.AddRange(classLines);
			info.mFilePath = path;
			info.mFunctionLine = startIndex + 1;
			fileInfos.Add(className, info);
		}
	}
	public static string findStackTrace()
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
	public static void doCheckFunctionOrder(string filePath, string[] lines)
	{
		bool checkingPublic = false;
		bool checkingProtected = false;
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			if (!findFunctionName(line, out _, out _))
			{
				continue;
			}

			string[] elements = split(line, true, ' ', '\t');
			string firstString = elements[0];
			if (firstString == "public")
			{
				if (checkingPublic)
				{
					continue;
				}
				if (!checkingPublic && !checkingProtected)
				{
					checkingPublic = true;
				}
				if (checkingProtected)
				{
					Debug.LogError("顺序不符" + addFileLine(filePath, i + 1));
					return;
				}
			}
			else if (firstString == "protected")
			{
				if (checkingPublic)
				{
					int checkLineNum = i - 1;
					if (lines[checkLineNum].Contains("//") && !lines[checkLineNum].Contains("----"))
					{
						checkLineNum -= 1;
					}
					if (!lines[checkLineNum].Contains("//----"))
					{
						Debug.LogError("请添加//----" + addFileLine(filePath, i));
						return;
					}
				}
				checkingProtected = true;
				checkingPublic = false;
			}
		}
	}
	public static void doCheckComment(string filePath, string[] lines)
	{
		for (int i = 0; i < lines.Length; ++i)
		{
			// 查找类名
			string className = findClassName(lines[i]);
			if (!isEmpty(className))
			{
				doCheckClassComment(filePath, lines, i, className);
				continue;
			}
			// 类名未找到的情况下查找枚举名
			string enumName = findEnumVariableName(lines[i]);
			if (!isEmpty(enumName))
			{
				doCheckEnumComment(filePath, lines, i, enumName);
				continue;
			}
		}
	}
	// 检查枚举的注释
	public static void doCheckEnumComment(string filePath, string[] lines, int index, string enumName)
	{
		int lastIndex = index - 1;
		// 类名的上一行需要写对于该类的注释
		while (true)
		{
			string lastLine = removeAllEmpty(lines[lastIndex]);
			if (lastLine.Contains("//"))
			{
				break;
			}

			if (lastLine.IndexOf('}') >= 0 || lastIndex == 0 || lastLine.Length == 0)
			{
				Debug.LogError("枚举的上一行需要添加注释" + addFileLine(filePath, lastIndex + 1));
				break;
			}
			--lastIndex;
		}
		// 查找类块
		if (!findRegionBody(lines, index, out int endIndex))
		{
			Debug.LogError("查找枚举定义失败!!!" + addFileLine(filePath, index + 1));
			return;
		}
		// 检测枚举值的注释
		for (int j = index + 1; j < endIndex; ++j)
		{
			if (lines[j].IndexOf('{') < 0 && !lines[j].Contains("//"))
			{
				Debug.LogError("添加" + enumName + "." + lines[j] + "的注释!!!" + addFileLine(filePath, j + 1));
			}
			continue;
		}
	}
	public static void doCheckClassComment(string filePath, string[] lines, int index, string className)
	{
		int lastIndex = index - 1;
		// 类名的上一行需要写对于该类的注释
		while (true)
		{
			string lastLine = removeAllEmpty(lines[lastIndex]);
			if (lastLine.Contains("//"))
			{
				break;
			}
			if (lastLine.IndexOf('}') >= 0 || lastIndex == 0 || lastLine.Length == 0)
			{
				Debug.LogError("类的上一行需要添加注释" + addFileLine(filePath, lastIndex + 1));
				break;
			}
			--lastIndex;
		}

		// 类成员变量的后面需要写该成员变量的注释,myUGUI的变量除外.GameBase,FrameBase的除外
		if (className == typeof(FrameBase).Name)
		{
			return;
		}
		// 查找类块
		if (!findRegionBody(lines, index, out int endIndex))
		{
			Debug.LogError("查找类体失败!!!" + addFileLine(filePath, index + 1));
			return;
		}
		for (int j = index + 1; j < endIndex; ++j)
		{
			// 成员变量
			string normalVarlable = findMemberVariableName(lines[j]);
			if (!isEmpty(normalVarlable) && !lines[j].Contains("myUGUI") && !lines[j].Contains("//"))
			{
				Debug.LogError("需要在成员变量的后面增加注释!!!" + addFileLine(filePath, j + 1));
			}
		}
	}
	public static void doCheckSystemFunction(string filePath, string[] lines)
	{
		string[] vector3IgnoreList = new string[]
		{
			"right",
			"left",
			"up",
			"back",
			"forward",
			"one",
			"zero",
			"down"
		};
		for (int i = 0; i < lines.Length; ++i)
		{
			// 过滤特殊的类
			string className = findClassName(lines[i]);
			if (isEmpty(className))
			{
				continue;
			}
			// 查找类块
			if (!findRegionBody(lines, i, out int endIndex))
			{
				Debug.LogError("查找类体失败!!!" + addFileLine(filePath, i + 1));
				return;
			}
			// 查找UnityEngine.Debug
			if (findBaseClassName(lines[i]) == typeof(MonoBehaviour).Name ||
				className == typeof(BinaryUtility).Name ||
				className == typeof(CSharpUtility).Name ||
				className == typeof(FileUtility).Name ||
				className == typeof(FrameUtility).Name ||
				className == typeof(MathUtility).Name ||
				className == typeof(StringUtility).Name ||
				className == typeof(TimeUtility).Name ||
				className == typeof(UnityUtility).Name ||
#if USE_ILRUNTIME
				className == typeof(ILRSystem).Name ||
#endif
				className == typeof(WidgetUtility).Name)
			{
				i = endIndex + 1;
				continue;
			}
			for (int j = i + 1; j < endIndex + 1; ++j)
			{
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Debug), "DrawLine");
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Mathf));
				doCheckFunctionCall(lines[j], filePath, j + 1, typeof(Vector3), vector3IgnoreList);
			}
		}
	}
	public static void doCheckFunctionCall(string lineString, string filePath, int index, Type classType, params string[] ignoreFuncionList)
	{
		// 移除注释
		string codeLine = removeComment(lineString);
		// 移除字符串
		codeLine = removeQuotedStrings(codeLine);
		// 函数名中不包含非常量的使用 即使是常量也可以忽略
		if (findFunctionName(lineString, out _, out _))
		{
			return;
		}

		string className = classType.ToString();
		// 去除命名空间
		int namespaceIndex = className.IndexOf('.');
		if (namespaceIndex >= 0)
		{
			className = className.Substring(namespaceIndex + 1);
		}
		// 一行字符串中会出现多个className
		while (true)
		{
			// 移除className之前的字符串。
			int firstIndex = findFirstSubstr(codeLine, className, 0, false);
			if (firstIndex < 0)
			{
				break;
			}

			// 过滤以className结尾的类 例如MyDebug
			if (firstIndex > 0 && isFunctionNameChar(codeLine[firstIndex - 1]))
			{
				// 移除这部分字符串
				codeLine = codeLine.Substring(firstIndex + className.Length);
				continue;
			}

			// 找到一个有效的方法
			string functionString = findFirstFunctionString(codeLine, firstIndex + className.Length, out int resultIndex);
			if (isEmpty(functionString))
			{
				break;
			}

			// 在方法名之前查找. 如果没找到，或者找到的下标在resultIndex之后，就返回
			int dotIndex = codeLine.IndexOf('.', firstIndex + className.Length);
			if (dotIndex == -1 || dotIndex > resultIndex)
			{
				break;
			}

			// 如果在忽略函数名集合中，就移除掉再继续找
			if (arrayContains(ignoreFuncionList, functionString))
			{
				codeLine = codeLine.Substring(resultIndex + functionString.Length);
				continue;
			}

			Debug.LogError("不允许使用" + className + "!!!" + addFileLine(filePath, index));
			break;
		}
	}
	// 在一个字符串中从前往后找,找到第一个符合函数名(或者类名,变量名,判断方式都是一样的)标准的字符串
	public static string findFirstFunctionString(string code, int startIndex, out int resultIndex)
	{
		resultIndex = -1;
		for (int i = startIndex; i < code.Length; ++i)
		{
			if (resultIndex < 0)
			{
				if (isFunctionNameChar(code[i]))
				{
					resultIndex = i;
				}
			}
			else
			{
				if (!isFunctionNameChar(code[i]))
				{
					return code.Substring(resultIndex, i - resultIndex);
				}
				if (i == code.Length - 1)
				{
					return code.Substring(resultIndex);
				}
			}
		}
		return null;
	}
	// 在一个字符串中从后往前找,找到第一个符合函数名(或者类名,变量名,判断方式都是一样的)标准的字符串,endIndex表示从后往前找时开始的下标
	public static string findLastFunctionString(string code, int endIndex, out int resultIndex)
	{
		int lastIndex = -1;
		resultIndex = -1;
		for (int i = endIndex; i >= 0; --i)
		{
			if (lastIndex < 0)
			{
				if (isFunctionNameChar(code[i]))
				{
					lastIndex = i;
				}
			}
			else
			{
				if (!isFunctionNameChar(code[i]))
				{
					resultIndex = i + 1;
					return code.Substring(resultIndex, lastIndex - resultIndex + 1);
				}
				if (i == 0)
				{
					resultIndex = 0;
					return code.Substring(resultIndex, lastIndex - resultIndex);
				}
			}
		}
		return null;
	}
	// 判断字符串是否是类名,函数名,变量名也都可以使用这个来判断
	public static bool isFunctionName(string str)
	{
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (!isFunctionNameChar(str[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 查询c在类名,函数名,变量名命名中是否允许使用
	public static bool isFunctionNameChar(char c)
	{
		return c == '_' || isNumberic(c) || isLetter(c);
	}
	// 移除注释,只考虑最后出现的双斜杠,如果注释的双斜杠后面还有双斜杠,则不能准确判断
	public static string removeComment(string codeLine)
	{
		int index = codeLine.LastIndexOf("//");
		if (index >= 0)
		{
			// 判断双斜杠之前有多少个双引号,奇数个则表示双斜杠在字符串内
			string preString = codeLine.Remove(index);
			int quotCount = getCharCount(preString, '"');
			if ((quotCount & 1) == 0)
			{
				return preString;
			}
		}
		return codeLine;
	}
	// 移除字符串 一行代码可有多个字符串拼接组成
	public static string removeQuotedStrings(string codeLine)
	{
		int lastIndex = -1;
		for (int i = codeLine.Length - 1; i >= 0; --i)
		{
			if (codeLine[i] != '"')
			{
				continue;
			}

			if (lastIndex == -1)
			{
				lastIndex = i;
			}
			else
			{
				codeLine = codeLine.Remove(i, lastIndex - i + 1);
				lastIndex = -1;
			}
		}
		return codeLine;
	}
	// 查询.prefab文件中m_Sprite是否引用了unity内置资源
	public static void doCheckBuiltinUI(string filePath, string[] lines)
	{
		filePath = fullPathToProjectPath(filePath);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string code = lines[i];
			if (!code.Contains("m_Sprite:") && !code.Contains("m_Texture:"))
			{
				continue;
			}

			int index = findFirstSubstr(code, " guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点图片为空，filePath:" + filePath + ", 节点名:" + findGameObejctNameInPrefab(lines, i), loadAsset(filePath));
				continue;
			}
			code = code.Substring(index);

			string guid = code.Substring(0, findFirstSubstr(code, ",", 0, false));
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			// Resources下还有内置的资源，所有需要指定文件夹
			if (!assetPath.Contains(FrameDefine.P_GAME_RESOURCES_PATH) &&
				!assetPath.Contains(FrameDefine.P_RESOURCES_ATLAS_PATH) &&
				!assetPath.Contains(FrameDefine.P_RESOURCES_TEXTURE_PATH))
			{
				Debug.LogError("UI节点图片引用了内置资源，filePath:" + filePath + ", 节点名:" + findGameObejctNameInPrefab(lines, i), loadAsset(filePath));
			}
		}
	}
	// 查询.prefab文件中m_Font是否引用了unity内置资源
	public static void doCheckBuiltinFont(string filePath, string[] lines)
	{
		filePath = fullPathToProjectPath(filePath);
		int lineCount = lines.Length;
		for (int i = 0; i < lineCount; ++i)
		{
			string code = lines[i];
			if (!code.Contains("m_Font:"))
			{
				continue;
			}

			int index = findFirstSubstr(code, " guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点字体为空，filePath:" + filePath + ", 节点名称:" + findGameObejctNameInPrefab(lines, i), loadAsset(filePath));
			}
			else
			{
				string guid = code.Substring(index, code.IndexOf(',', index) - index);
				Debug.Log("Font assetPath:" + AssetDatabase.GUIDToAssetPath(guid) + ", file:" + filePath, loadAsset(filePath));
			}
		}
	}
	// 在prefab文件中寻找指定行所属的节点名
	public static string findGameObejctNameInPrefab(string[] lines, int lineIndex)
	{
		// 一直往上找,直到找到一个GameObject:
		for (int i = lineIndex; i >= 0; --i)
		{
			if (lines[i] != "GameObject:")
			{
				continue;
			}
			// 然后从这一行开始往下找,找到名字
			for (int j = i + 1; j < lines.Length; ++j)
			{
				int nameIndex = findFirstSubstr(lines[j], "m_Name: ", 0, true);
				if (nameIndex >= 0)
				{
					return lines[j].Substring(nameIndex);
				}
			}
		}
		return null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 获取图片的引用信息 filePath 文件路径 assetType类型 tipText提示信息
	protected static Dictionary<string, Dictionary<string, PrefabNodeItem>> getImageReferenceInfo(string filePath, string assetType, string tipText = "")
	{
		// 初始化和查找相应文件
		var files = new List<string>();
		var allPrefabMap = new Dictionary<string, Dictionary<string, PrefabNodeItem>>();
		findFiles(filePath, files, assetType);
		int fileCount = files.Count;
		string currentID = EMPTY;
		for (int i = 0; i < fileCount; ++i)
		{
			EditorUtility.DisplayProgressBar("正在查找所有" + tipText + "资源", "进度:" + (i + 1) + "/" + fileCount, (float)(i + 1) / fileCount);
			string file = files[i];
			allPrefabMap[file] = new Dictionary<string, PrefabNodeItem>();
			var currentItem = allPrefabMap[file];
			openTxtFileLines(file, out string[] lines);
			for (int j = 0; j < lines.Length - 1; ++j)
			{
				// 处理Prefab的节点ID
				if (lines[j].StartsWith("---"))
				{
					string componentID = lines[j].Substring(findFirstSubstr(lines[j], " &", 0, true));
					currentItem.Add(componentID, new PrefabNodeItem());
					currentID = componentID;
				}
				// 所属GameObject的ID
				if (lines[j].StartsWith("  m_GameObject:"))
				{
					currentItem[currentID].mGameObjectID = lines[j].Substring(findFirstSubstr(lines[j], "fileID: ", 0, true));
				}
				// 节点名称(只有Prefab文本文件的GameObject才有)
				if (lines[j].StartsWith("  m_Name:"))
				{
					currentItem[currentID].mName = lines[j].Substring(findFirstSubstr(lines[j], ": ", 0, true));
				}
				// 对应Sprite的ID
				if (lines[j].StartsWith("  m_Sprite:"))
				{
					// 部分预制体会存在两行显示的情况,导致索引位置不一致
					int startIndex = findFirstSubstr(lines[j], "guid: ", 0, true);
					if (startIndex >= 0)
					{
						int endIndex = lines[j].IndexOf(", type:");
						if (endIndex < 0)
						{
							endIndex = lines[j].Length;
						}
						currentItem[currentID].mSpriteID = lines[j].Substring(startIndex, endIndex - startIndex);
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();
		return allPrefabMap;
	}
	// 根据GUID查找资源文件
	protected static string findAsset(string guid, string suffix, bool showAsset, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		int[] guidNextIndex;
		generateNextIndex(guid, out guidNextIndex);
		string metaSuffix = ".meta";
		if (allFileText == null)
		{
			string[] files = Directory.GetFiles(Application.dataPath + "/" + FrameDefine.GAME_RESOURCES, "*.*", SearchOption.AllDirectories);
			foreach (var item in files)
			{
				string file = item;
				if (!endWith(file, suffix + metaSuffix, false))
				{
					continue;
				}
				if (KMPSearch(File.ReadAllText(file), guid, guidNextIndex) < 0)
				{
					continue;
				}
				removeEndString(ref file, metaSuffix);
				if (showAsset)
				{
					fullPathToProjectPath(ref file);
					rightToLeft(ref file);
					Debug.Log(file, loadAsset(file));
				}
				return file;
			}
			return EMPTY;
		}
		foreach (var item in allFileText)
		{
			string curSuffix = item.Key;
			// 只判断meta文件
			if (curSuffix != metaSuffix)
			{
				continue;
			}
			foreach (var fileGUIDLines in item.Value)
			{
				string curFile = fileGUIDLines.mProjectFileName;
				if (!endWith(curFile, suffix + metaSuffix, false))
				{
					continue;
				}
				foreach (var line in fileGUIDLines.mContainGUIDLines)
				{
					if (KMPSearch(line, guid, guidNextIndex) < 0)
					{
						continue;
					}
					removeEndString(ref curFile, metaSuffix);
					if (showAsset)
					{
						Debug.Log(curFile, loadAsset(curFile));
					}
					return curFile;
				}
			}
		}
		return EMPTY;
	}
	protected static BuildReport buildGame(string outputPath, BuildTarget target, BuildTargetGroup targetGroup, BuildOptions buildOptions)
	{
		BuildPlayerOptions options = new BuildPlayerOptions();
		options.scenes = new string[] { FrameDefine.START_SCENE };
		options.locationPathName = outputPath;
		options.targetGroup = targetGroup;
		options.target = target;
		options.options = buildOptions;
		return BuildPipeline.BuildPlayer(options);
	}
	[OnOpenAsset(1)]
	protected static bool OnOpenAsset(int instanceID, int line)
	{
		// 自定义函数，用来获取log中的stacktrace，定义在后面。
		string stack_trace = findStackTrace();
		// 通过stacktrace来定位是否是我们自定义的log，我的log中有特殊文字 "检测resetProperty()"
		if (isEmpty(stack_trace))
		{
			return false;
		}

		// 只有包含特定的关键字才会跳转到代码
		if (!stack_trace.Contains(CODE_LOCATE_KEYWORD))
		{
			return false;
		}

		// 打开代码文件的指定行
		string filePath = null;
		int fileLine = 0;
		string[] debugInfoLines = split(stack_trace, false, '\n');
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

		if (filePath == null)
		{
			return false;
		}
		// 如果以Asset开头打开Asset文件并定位到该行
		if (filePath.StartsWith(FrameDefine.P_ASSETS_PATH))
		{
			UnityEngine.Object codeObject = loadAsset(filePath);
			if (codeObject == null || codeObject.GetInstanceID() == instanceID && fileLine == line)
			{
				return false;
			}
			AssetDatabase.OpenAsset(codeObject, fileLine);
		}
		else
		{
			// 如果以HotFix开头只打开文件不定位到行
			EditorUtility.OpenWithDefaultApp(FrameDefine.F_PROJECT_PATH + filePath);
		}
		return true;
	}
}