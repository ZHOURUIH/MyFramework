using System;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
#if USE_GOOGLE_PLAY_ASSET_DELIVERY
using Google.Android.AppBundle.Editor;
#endif
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UObject = UnityEngine.Object;
using static MathUtility;
using static StringUtility;
using static FileUtility;
using static CSharpUtility;
using static WidgetUtility;
using static FrameDefine;
using static EditorDefine;
using static UnityUtility;
using static EditorFileUtility;
using static FrameBaseDefine;

public class SpriteRefrenceInfo
{
	public string mSpriteName;
	public string mFileName;
	public UObject mObject;
}

public class FileGUIDLines
{
	public HashSet<string> mContainGUIDLines;
	public string mProjectFileName;     // 相对于项目的相对路径,也就是以Assets开头
	public UObject mObject;             // 有些文件不是文本格式的,所以加载成资源对象进行访问
}

public class PrefabNodeItem
{
	public string mResourceID;
	public string mGameObjectName;
}

public class EditorCommonUtility
{
	protected static char[] mHexUpperChar;
	protected static char[] mHexLowerChar;
	protected static string mHexString = "ABCDEFabcdef0123456789";
	protected const int GUID_LENGTH = 32;
	public const string KEY_FUNCTION = "resetProperty";
	protected const string CODE_LOCATE_KEYWORD = "代码检测";
	public static bool messageYesNo(string info)
	{
		return EditorUtility.DisplayDialog("提示", info, "确认", "取消");
	}
	public static void messageOK(string info)
	{
		EditorUtility.DisplayDialog("提示", info, "确认");
		Debug.Log(info);
	}
	public static void messageError(string info)
	{
		EditorUtility.DisplayDialog("错误", info, "确认");
		Debug.LogError(info);
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
	// 查找文件在其他地方的引用情况,查找fileName在allFileText中指定后缀的文件中的引用情况
	public static int searchFiles(string pattern, string guid, string fileName, bool loadFile, Dictionary<string, UObject> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText, bool checkUnuseOnly)
	{
		int[] guidNextIndex = null;
		fileName = fileName.rightToLeft();
		string metaSuffix = ".meta";
		if (pattern == "*.*")
		{
			pattern = EMPTY;
		}
		else if (pattern[0] == '*')
		{
			pattern = pattern.Remove(0, 1);
		}
		foreach (var item in allFileText)
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			// 一些文件的meta中肯定不会引用任何文件
			if (pattern == metaSuffix)
			{
				if (item.Key.endWith("bytes.meta") ||
					item.Key.endWith("tpsheet.meta") ||
					item.Key.endWith("mp3.meta") ||
					item.Key.endWith("wave.meta") ||
					item.Key.endWith("prefab.meta"))
				{
					continue;
				}
			}
			if (!item.Key.endWith(pattern, false))
			{
				continue;
			}
			foreach (FileGUIDLines fileGUIDLines in item.Value)
			{
				string curFile = fileGUIDLines.mProjectFileName;
				// 简单过滤一下meta文件的判断,因为meta文件的文件名最后一个字符肯定是a
				if (curFile[^1] == 'a')
				{
					curFile = curFile.removeEndString(metaSuffix);
				}
				foreach (string line in fileGUIDLines.mContainGUIDLines)
				{
					// 查找是否包含GUID
					if (KMPSearch(line, guid, ref guidNextIndex) < 0)
					{
						continue;
					}
					if (fileName != curFile && !refrenceList.ContainsKey(curFile))
					{
						refrenceList.Add(curFile, loadFile ? loadAsset(curFile) : null);
						if (checkUnuseOnly)
						{
							return refrenceList.Count;
						}
					}
					break;
				}
			}

			// 在非文本文件中查找是否有引用
			if (pattern.endWith(".asset"))
			{
				foreach (FileGUIDLines fileGUIDLines in item.Value)
				{
					// 地形数据
					List<UObject> assetList = new();
					var terrainData = fileGUIDLines.mObject as TerrainData;
					if (terrainData != null)
					{
						// holesTexture
						assetList.addNotNull(terrainData.holesTexture);
						// alphamapTextures
						foreach (Texture2D tex in terrainData.alphamapTextures)
						{
							assetList.addNotNull(tex);
						}
						// terrainLayers
						foreach (TerrainLayer layer in terrainData.terrainLayers)
						{
							assetList.Add(layer);
							assetList.addNotNull(layer.diffuseTexture);
							assetList.addNotNull(layer.normalMapTexture);
							assetList.addNotNull(layer.maskMapTexture);
						}
						// detailPrototypes
						foreach (DetailPrototype detail in terrainData.detailPrototypes)
						{
							assetList.addNotNull(detail.prototypeTexture);
						}
						// treePrototypes
						foreach (TreePrototype tree in terrainData.treePrototypes)
						{
							assetList.addNotNull(tree.prefab);
						}

						foreach (UObject asset in assetList)
						{
							string curGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
							if (guid == curGuid)
							{
								refrenceList.Add(fileGUIDLines.mProjectFileName, fileGUIDLines.mObject);
								if (checkUnuseOnly)
								{
									return refrenceList.Count;
								}
								break;
							}
						}
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
		int[] texturePropertyNextIndex = null;
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = lines[i].removeAllEmpty();
			string line = lines[i];
			// 找到Properties
			if (line == "Properties")
			{
				propertyLine = i;
				propertyStartLine = i + 2;
			}
			if (propertyLine >= 0 && line == "{")
			{
				propertyStartLine = i + 1;
			}
			if (propertyStartLine >= 0 && line == "}")
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
			if (KMPSearch(lines[i], texturePropertyKey, ref texturePropertyNextIndex) >= 0)
			{
				return true;
			}
		}
		return false;
	}
	// 检查材质引用的贴图是否存在
	public static void checkMaterialTextureValid(string path, Dictionary<string, string> allFileMeta)
	{
		openTxtFileLines(projectPathToFullPath(path), out string[] materialLines);
		for (int i = 0; i < materialLines.Length; ++i)
		{
			materialLines[i] = materialLines[i].removeAll(' ');
		}
		bool startTexture = false;
		string textureStr = "m_Texture";
		generateNextIndex(textureStr, out int[] textureNextIndex);
		string guidKey = "guid:";
		int[] guidNextIndex = null;
		string shaderContent = EMPTY;
		for (int i = 0; i < materialLines.Length; ++i)
		{
			string line = materialLines[i];
			if (!startTexture)
			{
				if (line.StartsWith("m_Shader:"))
				{
					string key = "guid:";
					int startIndex = line.IndexOf(key) + key.Length;
					string shaderGUID = line.rangeToFirst(startIndex, ',');
					if (shaderGUID.isEmpty() ||
						shaderGUID.endWith("000000") ||
						!allFileMeta.TryGetValue(shaderGUID, out string shaderFile))
					{
						return;
					}
					shaderContent = openTxtFile(shaderFile.removeEndString(".meta"), true);
					if (shaderContent.isEmpty())
					{
						return;
					}
				}
				// 开始查找贴图属性
				if (line == "m_TexEnvs:")
				{
					startTexture = true;
				}
				continue;
			}
			// 找到属性名
			if (line[0] == '-')
			{
				if (!checkTextureInShader(line.removeAll(' ').removeStartCount(1), shaderContent))
				{
					// 过滤材质没使用的贴图属性
					continue;
				}
				string textureLine = materialLines[i + 1];
				if (KMPSearch(textureLine, textureStr, ref textureNextIndex) < 0)
				{
					Debug.LogError("material texture property error, " + path, loadAsset(path));
					return;
				}
				if (KMPSearch(textureLine, guidKey, ref guidNextIndex) >= 0)
				{
					int startIndex = textureLine.IndexOf(guidKey) + guidKey.Length;
					string textureGUID = textureLine.rangeToFirst(startIndex, ',');
					if (!allFileMeta.ContainsKey(textureGUID))
					{
						Debug.LogError("在GameResource中找不到材质引用的资源 : " + path + ",引用的Guid:" + textureGUID, loadAsset(path));
					}
				}
			}
			// 贴图属性查找结束
			if (line.startWith("m_Floats:") || line.startWith("m_Colors:") || line.startWith("m_Ints:"))
			{
				break;
			}
		}
	}
	// 检查材质是否使用了shader未引用的贴图属性
	public static void checkMaterialTexturePropertyValid(string path, Dictionary<string, string> allFileMeta)
	{
		string shaderGUID = null;
		string[] materialLines = split(openTxtFile(projectPathToFullPath(path), true), "\r\n");
		// 找到shader的guid
		foreach (string item in materialLines)
		{
			string line = item.removeAll(' ');
			if (line.StartsWith("m_Shader:"))
			{
				string key = "guid:";
				int startIndex = line.IndexOf(key) + key.Length;
				shaderGUID = line.rangeToFirst(startIndex, ',');
				break;
			}
		}
		if (shaderGUID.isEmpty())
		{
			return;
		}
		if (shaderGUID.endWith("000000"))
		{
			Debug.LogError("材质使用了内置shader,无法找到shader文件", loadAsset(path));
			return;
		}
		allFileMeta.TryGetValue(shaderGUID, out string shaderFile);
		if (shaderFile.isEmpty())
		{
			Debug.LogWarning("can not find material shader:" + path, loadAsset(path));
			return;
		}
		string shaderContent = openTxtFile(shaderFile.removeEndString(".meta"), true);
		if (shaderContent.isEmpty())
		{
			return;
		}

		// 找到所有引用到的贴图guid
		List<string> texturePropertyList = new();
		bool startTexture = false;
		foreach (string item in materialLines)
		{
			string line = item.removeAll(' ');
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
				string endStr = ":";
				if (line.StartsWith(preKey))
				{
					if (hasGUID(line))
					{
						texturePropertyList.Add(line.range(preKey.Length, line.Length - endStr.Length));
					}
				}
				// 贴图属性查找结束
				if (line == "m_Floats:" || line == "m_Colors:")
				{
					break;
				}
			}
		}
		// 检查贴图是否在shader中用到了
		bool materialValid = true;
		foreach (string item in texturePropertyList)
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
		Dictionary<string, string> spriteGUIDs = new();
		bool spriteStart = false;
		foreach (string item in openTxtFileLines(projectPathToFullPath(path + ".meta")))
		{
			string line = item.removeAll(' ');
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
		int[] keyNextIndex = null;
		foreach (var item in allFileText)
		{
			string suffix = item.Key;
			if (suffix != ".prefab")
			{
				continue;
			}
			foreach (FileGUIDLines fileGUIDLines in item.Value)
			{
				foreach (string line in fileGUIDLines.mContainGUIDLines)
				{
					if (KMPSearch(line, key, ref keyNextIndex) < 0)
					{
						continue;
					}
					if (!refrenceList.ContainsKey(suffix))
					{
						SpriteRefrenceInfo info = new();
						info.mSpriteName = spriteName;
						info.mFileName = suffix;
						info.mObject = loadAsset(suffix);
						refrenceList.Add(suffix, info);
					}
					break;
				}
			}
		}
	}
	public static void searchFileRefrence(string path, bool loadFile, Dictionary<string, UObject> refrenceList, Dictionary<string, List<FileGUIDLines>> allFileText, bool checkUnuseOnly)
	{
		refrenceList.Clear();
		HashSet<string> guidList = new() { AssetDatabase.AssetPathToGUID(path) };
		bool isTexture = isTextureSuffix(getFileSuffix(path));
		// 如果是图片文件,则需要查找其中包含的sprite
		if (isTexture)
		{
			string keyStr = "spriteID: ";
			int[] keyNextIndex = null;
			foreach (string item in openTxtFileLines(projectPathToFullPath(path + ".meta"), false))
			{
				if (KMPSearch(item, keyStr, ref keyNextIndex) < 0)
				{
					continue;
				}
				int index = item.IndexOf(keyStr) + keyStr.Length;
				if (item.Length >= index + GUID_LENGTH)
				{
					guidList.Add(item.substr(index, GUID_LENGTH));
				}
			}
		}
		bool isShader = path.endWith(".shader", false) || path.endWith(".shadergraph", false);
		bool isSubShader = path.endWith(".shadersubgraph", false);
		bool isAnimClip = path.endWith(".anim", false);
		bool isAnimator = path.endWith(".controller", false) || path.endWith(".overrideController", false);
		bool isModel = path.endWith(".fbx", false);
		foreach (string item in guidList)
		{
			// 只有贴图和shader才会从材质中查找引用
			if (isTexture || isShader)
			{
				if (searchFiles("*.mat", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
			}
			// subshader需要在shadergraph和shadersubgraph中查找
			if (isSubShader)
			{
				if (searchFiles("*.shadergraph", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
				if (searchFiles("*.shadersubgraph", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
			}
			if (isTexture)
			{
				if (searchFiles("*.terrainlayer", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
				if (searchFiles("*.shadergraph", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
				if (searchFiles("*.shadersubgraph", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
			}
			// 只有动画文件才会在状态机中查找
			if (isAnimClip || isAnimator || isModel)
			{
				if (searchFiles("*.controller", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
				if (searchFiles("*.overrideController", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
				{
					return;
				}
			}
			if (searchFiles("*.asset", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
			{
				return;
			}
			if (searchFiles("*.prefab", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
			{
				return;
			}
			if (searchFiles("*.unity", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
			{
				return;
			}
			if (searchFiles("*.meta", item, path, loadFile, refrenceList, allFileText, checkUnuseOnly) > 0 && checkUnuseOnly)
			{
				return;
			}
		}
	}
	// 查找资源引用了哪些文件
	public static void searchFileRefrenceOther(string path, bool loadFile, Dictionary<string, UObject> refrenceList, Dictionary<string, string> allMetaList)
	{
		refrenceList.Clear();
		FileGUIDLines guids = getGUIDsInFile(path);
		foreach (string guid in (guids?.mContainGUIDLines).safe())
		{
			if (allMetaList.TryGetValue(guid, out string otherPath))
			{
				otherPath = otherPath.removeEndString(".meta");
				refrenceList.Add(otherPath, loadFile ? loadAsset(otherPath) : null);
			}
		}
	}
	public static Dictionary<string, List<FileGUIDLines>> getAllResourceFileText(string[] patterns = null)
	{
		return getAllFileText(F_GAME_RESOURCES_PATH, patterns);
	}
	public static Dictionary<string, string> getAllResourceMeta()
	{
		return getAllResourceMeta(F_GAME_RESOURCES_PATH);
	}
	public static string getGUIDString(string line)
	{
		int count = line.Length;
		if (count < GUID_LENGTH)
		{
			return null;
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
			if (++guidCharCount >= GUID_LENGTH)
			{
				return line.substr(i + 1 - guidCharCount, guidCharCount);
			}
		}
		return null;
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
			if (++guidCharCount >= GUID_LENGTH)
			{
				return true;
			}
		}
		return false;
	}
	public static bool isGUIDChar(char value) { return isNumeric(value) || isLower(value); }
	public static Dictionary<string, string> getAllResourceMeta(string path)
	{
		string key = "guid: ";
		Dictionary<string, string> allFileMeta = new();
		foreach (string item in findFilesNonAlloc(path, ".meta"))
		{
			foreach (string lineItem in splitLine(File.ReadAllText(item)))
			{
				if (lineItem.StartsWith(key))
				{
					allFileMeta.add(lineItem.removeStartCount(key.Length), item);
					break;
				}
			}
		}
		return allFileMeta;
	}
	public static List<string> getTextureSuffixList()
	{
		return new(){ ".png", ".tga", ".jpg", ".jpeg", ".cubemap", ".exr", ".psd", ".tif" };
	}
	public static bool isTextureSuffix(string suffixWithDot)
	{
		foreach (string suffix in getTextureSuffixList())
		{
			if (suffixWithDot.endWith(suffix, false))
			{
				return true;
			}
		}
		return false;
	}
	public static Dictionary<string, List<FileGUIDLines>> getAllFileText(string path, string[] patterns = null)
	{
		List<string> supportPatterns = new() { ".prefab", ".shadergraph", ".shadersubgraph", ".unity", ".mat", ".asset", ".meta", ".controller", ".lighting", ".overrideController", ".terrainlayer" };
		supportPatterns.AddRange(patterns.safe());
		// key是后缀名,value是该后缀名的文件信息列表
		Dictionary<string, List<FileGUIDLines>> allFileText = new();
		foreach (string item in findFilesNonAlloc(path, supportPatterns))
		{
			if (isDirExist(item.removeEndString(".meta")))
			{
				continue;
			}
			allFileText.getOrAddNew(getFileSuffix(item).ToLower()).addNotNull(getGUIDsInFile(item));
		}
		return allFileText;
	}
	public static FileGUIDLines getGUIDsInFile(string path)
	{
		string suffixNoMeta = getFileSuffix(path).removeEndString(".meta");
		// 图片的meta中不会有任何文件的引用
		if (isTextureSuffix(suffixNoMeta))
		{
			return null;
		}
		HashSet<string> list = new();
		foreach (string lineItem in splitLine(File.ReadAllText(path)))
		{
			// 只将guid放到列表中,认为一行只有一个GUID,如果存在多个,则认为这一行不包含GUID,可能是其他的数据
			list.addNotEmpty(getGUIDString(lineItem));
		}
		FileGUIDLines fileGUIDLines = new();
		string fileName = fullPathToProjectPath(path.rightToLeft());
		fileGUIDLines.mProjectFileName = fileName;
		fileGUIDLines.mContainGUIDLines = list;
		if (path.endWith(".asset"))
		{
			// TerrainData不是文本格式的,所以只能加载为对象来访问
			fileGUIDLines.mObject = loadAsset(fileName) as TerrainData;
		}
		return fileGUIDLines;
	}
	// 根据后缀获取指定文件路径下的指定资源的所有GUID(filePath:查找路径, assetType : 后缀类型名, tipText : 查找类型提示,默认为空)
	public static Dictionary<string, string> getAllGUIDBySuffixInFilePath(string filePath, string suffix, string tipText = "")
	{
		Dictionary<string, string> allGUIDDic = new();
		List<string> files = findFilesNonAlloc(filePath, suffix);
		int fileCount = files.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			displayProgressBar("正在查找所有" + tipText + "资源", "进度:", i + 1, fileCount);
			string file = files[i];
			foreach (string line in openTxtFileLines(file))
			{
				if (line.Contains("guid: "))
				{
					allGUIDDic.Add(line.removeStartString("guid: "), file);
					break;
				}
			}
		}
		clearProgressBar();
		return allGUIDDic;
	}
	// 根据后缀获取指定文件路径下指定类型资源的所有GUID和spriteID(filePath:查找路径, assetType : 后缀类型名,tipText : 查找类型提示,默认为空)
	public static Dictionary<string, string> getAllGUIDAndSpriteIDBySuffixInFilePath(string filePath, string suffix, string tipText = "")
	{
		Dictionary<string, string> allGUIDDic = new();
		List<string> files = findFilesNonAlloc(filePath, suffix);
		int fileCount = files.Count;
		const string spritesArrMark = "TextureImporter:";
		for (int i = 0; i < fileCount; ++i)
		{
			EditorUtility.DisplayProgressBar("正在查找所有" + tipText + "资源", "进度:" + (i + 1) + "/" + fileCount, (float)(i + 1) / fileCount);
			string file = files[i];
			openTxtFileLines(file, out string[] lines);
			for (int j = 0; j < lines.Length - 1; ++j)
			{
				string line = lines[j];
				if (line.Contains("guid: "))
				{
					allGUIDDic.Add(line.removeStartString("guid: "), file);
					// 如果.meat文件中guid的下一行为"TextureImporter:"说明这个.meta文件为图集类型的.meta文件
					if (lines[j + 1].Contains(spritesArrMark))
					{
						continue;
					}
					break;
				}
				if (hasGUID(line) && line.Contains("spriteID: "))
				{
					int startIndex = line.findFirstSubstr("spriteID: ", 0, true);
					allGUIDDic.TryAdd(line.removeStartCount(startIndex), file);
				}
			}
		}
		clearProgressBar();
		return allGUIDDic;
	}
	// 获得文件中引用到了cs脚本的所在行
	public static Dictionary<string, FileGUIDLines> getScriptRefrenceFileText(string path)
	{
		// key是后缀名,value是该后缀名的文件信息列表
		Dictionary<string, FileGUIDLines> allFileText = new();
		List<string> files = findFilesNonAlloc(path, new List<string>() { ".prefab", ".unity" });
		int filesCounts = files.Count;
		int curFileIndex = 0;
		foreach (string item in files)
		{
			++curFileIndex;
			EditorUtility.DisplayProgressBar("查找所有脚本文件的引用", "进度:" + curFileIndex + "/" + filesCounts, (float)curFileIndex / filesCounts);
			FileGUIDLines fileGUIDLines = new();
			HashSet<string> list = new();
			foreach (string lineItem in openTxtFileLines(item))
			{
				if (hasGUID(lineItem) && lineItem.Contains("m_Script:"))
				{
					int startIndex = lineItem.findFirstSubstr("guid: ", 0, true);
					int endIndex = lineItem.findFirstSubstr(", ", startIndex);
					list.Add(lineItem.range(startIndex, endIndex));
				}
			}
			fileGUIDLines.mProjectFileName = fullPathToProjectPath(item.rightToLeft());
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
		}
		clearProgressBar();
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
			string line = lines[i];
			if (!hasGUID(line))
			{
				break;
			}
			if (line.Contains(BUILD_IN_GUID))
			{
				continue;
			}
			int subStartIndex = line.findFirstSubstr("guid: ", 0, true);
			int subEndIndex = line.findFirstSubstr(", ", subStartIndex);
			splitStr += "-" + line.range(subStartIndex, subEndIndex);
		}
		return splitStr;
	}
	// 获得文件中引用到了Material的所在行
	public static Dictionary<string, FileGUIDLines> getMaterialRefrenceFileText(string path)
	{
		// key是文件名,value是文件信息列表
		Dictionary<string, FileGUIDLines> allFileText = new();
		List<string> fileList = findFilesNonAlloc(path, new List<string> { ".prefab", ".unity" });
		int filesCounts = fileList.Count;
		int curFileIndex = 0;
		foreach (string item in fileList)
		{
			++curFileIndex;
			EditorUtility.DisplayProgressBar("查找所有材质的引用", "进度:" + curFileIndex + "/" + filesCounts, (float)curFileIndex / filesCounts);
			FileGUIDLines fileGUIDLines = new();
			openTxtFileLines(item, out string[] lines);
			HashSet<string> list = new();
			for (int i = 0; i < lines.Length; ++i)
			{
				if (lines[i].Contains("m_Materials:") && !getGUIDSplitStr(lines, i).isEmpty())
				{
					list.Add(getGUIDSplitStr(lines, i));
				}
			}
			fileGUIDLines.mProjectFileName = fullPathToProjectPath(item.rightToLeft());
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
		}
		clearProgressBar();
		return allFileText;
	}
	// 获得文件中具有引用的所在行
	public static Dictionary<string, FileGUIDLines> getAllRefrenceFileText(string path)
	{
		// key是后缀名,value是该后缀名的文件信息列表
		Dictionary<string, FileGUIDLines> allFileText = new();
		List<string> files = findFilesNonAlloc(path, new List<string>() { ".prefab", ".unity", ".mat", ".controller" });
		int filesCounts = files.Count;
		int curFileIndex = 0;
		foreach (string item in files)
		{
			++curFileIndex;
			EditorUtility.DisplayProgressBar("查找所有的引用", "进度:" + curFileIndex + "/" + filesCounts, (float)curFileIndex / filesCounts);
			HashSet<string> list = new();
			foreach (string lineItem in openTxtFileLines(item))
			{
				if (hasGUID(lineItem) && lineItem.Contains("guid: "))
				{
					list.Add(lineItem.rangeToFirst(lineItem.findFirstSubstr("guid: ", 0, true), ','));
				}
			}
			FileGUIDLines fileGUIDLines = new();
			fileGUIDLines.mProjectFileName = item.rightToLeft();
			fileGUIDLines.mContainGUIDLines = list;
			allFileText.Add(item, fileGUIDLines);
		}
		clearProgressBar();
		return allFileText;
	}
	public static Dictionary<string, string> checkAtlasNotExistSprite(string path)
	{
		Dictionary<string, string> notExistsprites = new();
		string atlasContent = openTxtFile(projectPathToFullPath(path + ".meta"), true);
		int startIndex = atlasContent.IndexOf("externalObjects: {}");
		foreach (var item in getSpriteGUIDs(path))
		{
			if (atlasContent.IndexOf(item.Value, startIndex) == -1)
			{
				notExistsprites.add(item);
			}
		}
		return notExistsprites;
	}
#if USE_GOOGLE_PLAY_ASSET_DELIVERY
	public static BuildResult buildGoogleAAB(string apkPath, BuildOptions buildOptions, AssetPackConfig packConfig = null)
	{
		// 没有传特定的配置,就使用默认的配置
		if (packConfig == null)
		{
			// 将所有AssetBundle文件备份到其他地方,然后再打包
			packConfig = new();
			// 查找当前的所有AssetBundle,因为在这之前已经将不需要打包的AssetBundle备份到了其他地方
			packConfig.AddAssetsFolder("assetbundles", PlatformBase.INSTALL_TIME_TEMP_PATH, AssetPackDeliveryMode.InstallTime);
		}
		BuildPlayerOptions options = new();
		options.scenes = new string[] { START_SCENE };
		options.locationPathName = apkPath;
		options.targetGroup = BuildTargetGroup.Android;
		options.target = BuildTarget.Android;
		options.options = buildOptions;
		BuildResult result = Bundletool.BuildBundle(options, packConfig, true) ? BuildResult.Succeeded : BuildResult.Failed;
		// 打包完成后删除其中无用的目录,等待10秒,否则可能会因为文件被占用而删除失败
		Thread.Sleep(10000);
		try
		{
			if (result == BuildResult.Succeeded)
			{
				deleteFolder(removeSuffix(apkPath) + "_BackUpThisFolder_ButDontShipItWithYourGame");
				deleteFolder(removeSuffix(apkPath) + "_BurstDebugInformation_DoNotShip");
			}
		}
		catch (Exception e)
		{
			Debug.LogError("删除文件夹失败:" + e.Message);
		}
		return result;
	}
#endif
	// apkPath是apk的绝对路径
	public static BuildResult buildAndroid(string apkPath, BuildOptions buildOptions)
	{
		createDir(getFilePath(apkPath));
		return buildGame(apkPath, BuildTarget.Android, BuildTargetGroup.Android, buildOptions);
	}
	// outputPath是index.html所在的目录
	public static BuildResult buildWebGL(string outputPath, BuildOptions buildOptions)
	{
		deleteFolder(outputPath);
		createDir(outputPath);
		return buildGame(outputPath, BuildTarget.WebGL, BuildTargetGroup.WebGL, buildOptions);
	}
	// outputPath是exe的绝对路径
	public static BuildResult buildWindows(string outputPath, BuildOptions buildOptions)
	{
		deleteFolder(getFilePath(outputPath));
		createDir(getFilePath(outputPath));
		return buildGame(outputPath, BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, buildOptions);
	}
	// outputPath是xcodeproj所在的目录
	public static BuildResult buildIOS(string outputPath, BuildOptions buildOptions)
	{
		deleteFolder(outputPath);
		return buildGame(outputPath, BuildTarget.iOS, BuildTargetGroup.iOS, buildOptions);
	}
	// outputPath是app文件输出的绝对路径
	public static BuildResult buildMacOS(string outputPath, BuildOptions buildOptions)
	{
		deleteFolder(getFilePath(outputPath));
		createDir(getFilePath(outputPath));
		return buildGame(outputPath, BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, buildOptions);
	}
	public static bool fixMeshOfMeshCollider(GameObject go)
	{
		if (go == null)
		{
			return false;
		}

		go.TryGetComponent<MeshCollider>(out var collider);
		go.TryGetComponent<MeshFilter>(out var meshFiliter);
		bool modified = collider != null && meshFiliter != null;
		if (modified)
		{
			collider.sharedMesh = meshFiliter.sharedMesh;
			collider.convex = false;
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
		EditorUtility.DisplayProgressBar(title, info + " " + (curCount + 1) + "/" + totalCount, (float)(curCount + 1) / totalCount);
	}
	public static void clearProgressBar()
	{
		EditorUtility.ClearProgressBar();
	}
	public static UObject loadAsset(string filePath)
	{
		if (filePath.isEmpty())
		{
			return null;
		}
		// 如果是绝对路径,需要转换为项目下的相对路径
		if (filePath.StartsWith(F_ASSETS_PATH))
		{
			filePath = fullPathToProjectPath(filePath);
		}
		return AssetDatabase.LoadAssetAtPath<UObject>(filePath);
	}
	public static T loadAsset<T>(string filePath) where T : UObject
	{
		if (filePath.isEmpty())
		{
			return null;
		}
		// 如果是绝对路径,需要转换为项目下的相对路径
		if (filePath.StartsWith(F_ASSETS_PATH))
		{
			filePath = fullPathToProjectPath(filePath);
		}
		return AssetDatabase.LoadAssetAtPath<T>(filePath);
	}
	public static T loadFirstSubAsset<T>(string filePath) where T : UObject
	{
		if (filePath.isEmpty())
		{
			return null;
		}
		// 如果是绝对路径,需要转换为项目下的相对路径
		if (filePath.StartsWith(F_ASSETS_PATH))
		{
			filePath = fullPathToProjectPath(filePath);
		}
		UObject[] assets = AssetDatabase.LoadAllAssetsAtPath(filePath);
		foreach (UObject asset in assets)
		{
			if (asset is T obj)
			{
				return obj;
			}
		}	
		return null;
	}
	public static GameObject loadGameObject(string filePath)
	{
		if (filePath.isEmpty())
		{
			return null;
		}
		// 如果是绝对路径,需要转换为项目下的相对路径
		if (filePath.StartsWith(F_ASSETS_PATH))
		{
			filePath = fullPathToProjectPath(filePath);
		}
		return AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
	}
	// 图片尺寸是否为2的n次方
	public static bool isSizePow2(Texture2D tex)
	{
		return isPow2(tex.width) && isPow2(tex.height);
	}
	// 是否忽略该文件
	public static bool isIgnoreFile(string filePath, IEnumerable<string> ignoreArr = null)
	{
		foreach (string str in getIgnoreScriptCheck())
		{
			if (filePath.Contains(str))
			{
				return true;
			}
		}
		foreach (string element in ignoreArr.safe())
		{
			if (filePath.Contains(element))
			{
				return true;
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
		string[] codeList = split(codeLine, ' ', '\t', ':');
		if (codeList == null)
		{
			return null;
		}
		for (int i = 0; i < codeList.Length; ++i)
		{
			if (codeList[i] == "enum")
			{
				if (i + 1 < codeList.Length)
				{
					return codeList[i + 1];
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
		codeLine = codeLine.removeEndEmpty();

		// 移除模板参数
		codeLine = codeLine.removeFirstBetweenPairChars('<', '>', out _, out _);

		// 先根据空格分割字符串
		string[] elements = split(codeLine, ' ', '\t');
		if (elements.count() < 2)
		{
			return null;
		}
		List<string> elementList = new(elements);
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
		string variableName = elementList[^1];
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
			startIndex = codeLine.findFirstSubstr(" struct ", 0, true);
		}
		else
		{
			startIndex = codeLine.findFirstSubstr(" class ", 0, true);
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
			return codeLine.range(startIndex, getMax(spaceIndex, colonIndex));
		}
		// 两个符号全部都找到
		return codeLine.range(startIndex, getMin(spaceIndex, colonIndex));
	}
	// 查找作用域(代码行数组, 类声明下标, 结束下标)
	public static bool findRegionBody(string[] lines, int classNameIndex, out int endIndex)
	{
		// 未配对的大括号数量
		int num = 0;
		for (int i = classNameIndex; i < lines.Length; ++i)
		{
			foreach (char item in lines[i])
			{
				if (item == '{')
				{
					++num;
				}
				else if (item == '}' && --num == 0)
				{
					endIndex = i;
					return true;
				}
			}
		}
		endIndex = -1;
		return false;
	}
	// 检测注释标准
	public static void doCheckCommentStandard(string filePath, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			// 如果是文件流,网址行,移除干扰字符串
			lines[i] = lines[i].removeStartCount(lines[i].findFirstSubstr("://", 0, true));
			// 注释下标
			int noteIndex = lines[i].findFirstSubstr("//", 0, true);
			if (noteIndex < 0)
			{
				continue;
			}
			string noteStr = lines[i].removeStartCount(noteIndex);
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
			noteStr = noteStr.removeAllEmpty();
			if (noteStr.Length == 0)
			{
				Debug.LogError("注释后没有内容, 应当移除注释" + addFileLine(filePath, i + 1));
				continue;
			}
		}
	}
	// 根据名称获取程序集
	public static Assembly getAssembly(string assemblyName)
	{
		// 获取Assembly集合
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (assembly.GetName().Name == assemblyName)
			{
				return assembly;
			}
		}
		return null;
	}
	// 加载热更程序集
	public static Assembly loadHotFixAssembly()
	{
#if USE_HYBRID_CLR
		string dllFileName = F_ASSET_BUNDLE_PATH + HOTFIX_BYTES_FILE;
#else
		string dllFileName = null;
#endif
		if (!isFileExist(dllFileName))
		{
			return null;
		}
		return Assembly.LoadFile(dllFileName);
	}
	public static Type findClass(Assembly assembly, string className)
	{
		// 获取到类型
		foreach (Type type in (assembly?.GetTypes()).safe())
		{
			if (type.Name == className)
			{
				return type;
			}
		}
		return null;
	}
	// 检测命令命名规范
	public static void doCheckCommandName(string filePath, string[] lines, Assembly csharpAssembly, Assembly hotfixAssembly)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
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
		if (!getFileNameWithSuffix(filePath).StartsWith(folder))
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
			// 包含'<'符号说明是泛型类
			className = className.rangeToFirst('<');
			// 判断类名是否与脚本文件名相同
			if (className != getFileNameNoSuffixNoDir(filePath))
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
			char code = codeLine[i];
			if (code == ' ')
			{
				continue;
			}
			if (startIndex < 0)
			{
				startIndex = i;
				continue;
			}
			if (code == ' ' || code == ',')
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
		return codeLine.range(startIndex, endIndex);
	}
	// 逐行检查脚本中的命名规范
	public static void doCheckScriptLineByLine(string filePath, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
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
			if (findBaseClassName(codeLine) == typeof(MonoBehaviour).Name && className.endWith("Debug"))
			{
				continue;
			}
			if (isIgnoreFile(filePath, getIgnoreFileCheckFunction()))
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
			foreach (string item in getIgnoreCheckClass())
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
		if (enumName.isEmpty())
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
		foreach (string item in getIgnoreCheckFunction())
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
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			// 设置忽略,忽略函数命名,函数命名行,有长字符串行,成员变量定义,委托定义
			if (findFunctionName(line, out _, out _) ||
				hasLongStr(line) ||
				findMemberVariableName(line) != null ||
				line.Contains(" delegate ") || 
				line.Contains("//"))
			{
				continue;
			}
			if (generateCharWidth(line) > 150)
			{
				Debug.LogError("单行代码太长,超出了150个字符宽度" + addFileLine(filePath, i + 1));
				findMemberVariableName(line);
			}
		}
	}
	// 检查分隔代码行宽度
	public static void doCheckCodeSeparateLineWidth(string filePath, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i];
			if (!line.Contains("//----------"))
			{
				continue;
			}
			if (line != "\t//------------------------------------------------------------------------------------------------------------------------------")
			{
				checkScriptTip("分隔行字符数应该为130,且以table键开头,当前为:" + (line.Length + 1), filePath, i + 1);
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
		foreach (char element in codeLine)
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
			string line = lines[i];
			foreach (string guid in missingList)
			{
				if (line.Contains(guid))
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
			string line = lines[i];
			if (refID == null && line.IndexOf('&') >= 0)
			{
				refID = line.removeStartCount(line.IndexOf('&') + 1);
			}
			if (!refID.isEmpty() && line.Contains("fileID: " + refID))
			{
				refIDIndex = i;
				break;
			}
		}
		for (int i = refIDIndex; i < scriptLineIndex; ++i)
		{
			string line = lines[i];
			if (line.Contains("m_Name: "))
			{
				return line.removeStartCount(line.IndexOf(": ") + 1);
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
			List<string> missingRefObjectsList = new();
			string assetPath = lineData.Key;
			setCheckRefObjectsList(missingRefObjectsList, assetPath, lineData.Value);
			UObject obj = loadAsset(assetPath);
			Debug.LogError("有" + missingType + "的引用丢失" + obj.name + "\n" + stringsToString(missingRefObjectsList, '\n'), obj);
			if (assetPath.endWith(".unity"))
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
		foreach (FileGUIDLines projectFile in fileDic.Values)
		{
			foreach (string guid in projectFile.mContainGUIDLines)
			{
				if (guidList.TryGetValue(guid, out string fileName))
				{
					errorRefAssetDic.getOrAddNew(projectFile.mProjectFileName).Add(guid, fileName);
				}
			}
		}
	}
	// 检查Protobuf的消息字段的顺序
	public static void doCheckProtoMemberOrder(string file, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + file);
			return;
		}
		int realOrder = 0;
		bool findProtoContract = false;
		for (int i = 0; i < lines.Length - 1; ++i)
		{
			string line = lines[i];
			if (line == "[ProtoContract]")
			{
				findProtoContract = true;
				continue;
			}
			if (!findProtoContract)
			{
				continue;
			}
			if (line.Contains("[ProtoMember("))
			{
				if (SToI(line.rangeToFirst(line.findFirstSubstr("[ProtoMember(", 0, true), ',')) - 1 != realOrder++)
				{
					Debug.LogError("Protobuf的消息字段顺序检测:有不符合规定的字段顺序." + addFileLine(file, i + 1));
				}
			}
		}
	}
	// 检查空行
	public static void doCheckEmptyLine(string file, string[] lines)
	{
		if (lines.count() <= 1)
		{
			Debug.LogError(file + "文件为空");
			return;
		}
		// 移除开始的空格和空行
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = lines[i].removeStartEmpty();
		}

		// 函数名的上一行不能为空行,需要保留空白字符进行检测
		for (int i = 1; i < lines.Length; ++i)
		{
			if (findFunctionName(lines[i], out _, out _) && lines[i - 1].Length == 0)
			{
				Debug.LogError("函数名的上一行发现空行." + addFileLine(file, i));
			}
		}
		// 文件第一行不能为空行,不过如果没有任何using,允许第一行空着
		if (lines[0].Length == 0)
		{
			bool hasCode = false;
			for (int i = 0; i < lines.Length; ++i)
			{
				// 跳过所有注释
				if (lines[i].StartsWith("//"))
				{
					continue;
				}
				if (!lines[i].Contains(" class ") && !lines[i].Contains(" struct "))
				{
					break;
				}
				hasCode = true;
				break;
			}
			if (hasCode)
			{
				Debug.LogError("文件第一行发现空行." + addFileLine(file, 1));
			}
		}

		// 先去除所有行的空白字符
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = lines[i].removeAllEmpty();
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
	}
	// 检查空格
	public static void doCheckSpace(string file, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + file);
			return;
		}
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
						Debug.LogError("运算符" + line.substr(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.substr(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
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
						Debug.LogError("运算符" + line.substr(j, 1) + "后面需要有空格" + addFileLine(file, i + 1));
					}
					if (expectedFrontSpacePos >= 0 && line[expectedFrontSpacePos] != ' ')
					{
						Debug.LogError("运算符" + line.substr(j, 1) + "前面需要有空格" + addFileLine(file, i + 1));
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
		if (lines.isEmpty())
		{
			Debug.LogError("fields 有问题 file：" + file);
			return;
		}
		if (allChildTrans.isEmpty())
		{
			Debug.LogError("prefab 有问题 file：" + file);
			return;
		}

		// 用于过滤UI变量
		string myUGUI = " myUGUI";
		// 用于存储变量行
		Dictionary<string, int> linesDic = new();
		bool finish = false;
		bool start = false;
		for (int i = 0; i < lines.Length; ++i)
		{
			// 遇到函数就可以停止遍历了
			string line = lines[i];
			if (line.IndexOf('(') >= 0)
			{
				break;
			}
			if (line.Contains(myUGUI))
			{
				if (finish)
				{
					Debug.LogError("检测UI变量名规范:  在布局:" + fileName + " 中请保持UI变量中不要混入其他变量." + addFileLine(file, i));
					return;
				}
				start = true;
				// 数组变量可以忽略
				if (line.Contains("[]"))
				{
					continue;
				}
				string variableStr = split(line, ' ')[^1];
				variableStr = variableStr.removeStartString("m");
				variableStr = variableStr.removeEndString(";");
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
		return codeLine.findFirstSubstr(" class ") >= 0;
	}
	// 判断一行字符串是不是函数名声明行,如果是,则返回是否为构造函数,函数名
	public static bool findFunctionName(string line, out bool isConstructor, out string functionName)
	{
		functionName = null;
		isConstructor = false;
		if (line.Contains("=>"))
		{
			return false;
		}
		// 移除注释和字符串
		line = removeComment(line);
		line = removeQuotedStrings(line);
		// 消除所有的尖括号内的字符
		line = line.removeFirstBetweenPairChars('<', '>', out _, out _);

		// 先根据空格分割字符串
		string[] elements = split(line, ' ', '\t');
		if (elements.count() < 2)
		{
			return false;
		}

		// 移除可能存在的where约束
		List<string> elementList = new(elements);
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
			if (firstString == "delegate" || firstString == "using")
			{
				return false;
			}
			if (firstString == "public" ||
				firstString == "protected" ||
				firstString == "internal" ||
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
		if (elementList.Count == 0)
		{
			return false;
		}
		string str0 = elementList[0];
		if (str0 == "if" ||
			str0 == "while" ||
			str0 == "else" ||
			str0 == "foreach" ||
			str0 == "for" ||
			str0 == "switch" ||
			str0.startWith("if(") ||
			str0.startWith("for(") ||
			str0.startWith("while(") ||
			str0.startWith("foreach(") ||
			str0.startWith("switch("))
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
		// '('前有两个元素（返回值，函数名）
		List<string> functionElements = new();
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
					functionElements.Add(newLine.substr(i + 1, lastElementIndex - i));
					lastElementIndex = -1;
				}
				else if (i == 0)
				{
					functionElements.Add(newLine.substr(i, lastElementIndex + 1));
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
			return newLine[^1] == ';';
		}

		// 函数名与函数定义不在同一行的
		if (newLine.IndexOf('{') < 0 && newLine.IndexOf('}') < 0)
		{
			// 函数名肯定是以括号结尾
			return newLine[^1] == ')';
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
		foreach (var item in checkAtlasNotExistSprite(path))
		{
			log("图集:" + path + "中的图片:" + item.Value + "不存在", Color.red, loadAsset(path));
		}
	}
	public static void doCheckTPAtlasRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		Dictionary<string, SpriteRefrenceInfo> refrenceList = new();
		searchSpriteRefrence(path, refrenceList, allFileText);
		foreach (var item in refrenceList)
		{
			log("图集:" + path + "被布局:" + item.Key + "所引用, sprite:" + item.Value.mSpriteName, item.Value.mObject);
		}
		Debug.Log("图集" + path + "被" + refrenceList.Count + "个布局引用");
	}
	public static void doCheckUnusedFile(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		Dictionary<string, UObject> refrenceMaterialList = new();
		searchFileRefrence(path, false, refrenceMaterialList, allFileText, true);
		if (refrenceMaterialList.Count == 0)
		{
			Debug.Log("资源未引用:" + path, loadAsset(path));
		}
	}
	public static void doSearchRefrence(string path, Dictionary<string, List<FileGUIDLines>> allFileText)
	{
		Dictionary<string, UObject> refrenceList = new();
		string fileName = getFileNameWithSuffix(path);
		DateTime start = DateTime.Now;
		Debug.Log("<<<<<<<开始查找" + fileName + "的引用.......");
		searchFileRefrence(path, false, refrenceList, allFileText, false);
		foreach (string item in refrenceList.Keys)
		{
			log(item, Color.green, loadAsset(item));
		}
		Debug.Log(">>>>>>>完成查找" + fileName + "的引用, 共有" + refrenceList.Count + "处引用, 耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒", loadAsset(path));
	}
	public static void doSearchResourceRefOther(string path, Dictionary<string, string> allMetaList)
	{
		Dictionary<string, UObject> refrenceList = new();
		string fileName = getFileNameWithSuffix(path);
		DateTime start = DateTime.Now;
		Debug.Log("<<<<<<<开始查找" + fileName + "引用的文件.......");
		searchFileRefrenceOther(path, false, refrenceList, allMetaList);
		foreach (string item in refrenceList.Keys)
		{
			log(item, Color.green, loadAsset(item));
		}
		Debug.Log(">>>>>>>完成查找" + fileName + "引用的文件, 共引用" + refrenceList.Count + "个文件, 耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒", loadAsset(path));
	}
	public static void doCheckSingleUsedFile(string path, Dictionary<string, List<FileGUIDLines>> allFileText, bool needMoveFile)
	{
		Dictionary<string, UObject> refrenceList = new();
		searchFileRefrence(path, false, refrenceList, allFileText, false);
		string refFilePath = null;
		foreach (string item in refrenceList.Keys)
		{
			if (refFilePath == null)
			{
				refFilePath = getFilePath(item);
			}
			else if (refFilePath != getFilePath(item))
			{
				return;
			}
		}
		if (refFilePath.rightToLeft() != getFilePath(path.rightToLeft()))
		{
			Debug.LogError(path + " 只被 " + refFilePath + "中的文件引用,但是没有在同一个目录", loadAsset(path));
			Debug.LogError("所在目录:" + refFilePath, loadAsset(refrenceList.firstKey()));
			if (needMoveFile)
			{
				moveFile(projectPathToFullPath(path), projectPathToFullPath(refFilePath + "/") + getFileNameWithSuffix(path));
			}
		}
	}
	// 确保路径为相对于Project的路径
	public static string ensureProjectPath(string filePath)
	{
		if (!filePath.startWith(P_ASSETS_PATH) && filePath.startWith(F_ASSETS_PATH))
		{
			// Assets资源路径
			filePath = fullPathToProjectPath(filePath);
		}
		return filePath;
	}
	// 检查代码提示
	public static void checkScriptTip(string tipInfo, string filePath, int lineNumber)
	{
		filePath = ensureProjectPath(filePath);
		// 非热更文件
		if (filePath.StartsWith(P_ASSETS_PATH))
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber), loadAsset(filePath));
		}
		else
		{
			Debug.LogError(tipInfo + addFileLine(filePath, lineNumber));
		}
	}
	// 检查是否全部节点都添加了缩放适配组件
	public static void doCheckUIHasScaleAnchor(string path)
	{
		GameObject uiPrefab = loadGameObject(path);
		if (uiPrefab == null)
		{
			return;
		}
		foreach (RectTransform item in uiPrefab.GetComponentsInChildren<RectTransform>())
		{
			if (!item.TryGetComponent<ScaleAnchor>(out _))
			{
				Debug.LogError("节点:" + item.name + " 没有添加缩放组件, prefab1:" + path, loadAsset(path));
			}
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
			if (fileList[i].endWith(".meta"))
			{
				fileList.RemoveAt(i--);
			}
		}
	}
	public static void roundTransformToInt(Transform transform)
	{
		if (transform == null)
		{
			return;
		}
		transform.localPosition = round(transform.localPosition);
		if (transform is RectTransform rectTrans)
		{
			setRectSize(rectTrans, round(rectTrans.rect.size));
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			roundTransformToInt(transform.GetChild(i));
		}
	}
	public static void doCheckResetProperty(Assembly assemly, string path)
	{
		// 遍历目录,存储所有文件名和对应文本内容
		Dictionary<string, ClassInfo> classInfoList = new();
		getCSharpFile(path, classInfoList);
		// 不需要检测的基类
		List<Type> ignoreBaseClass = new()
		{
			typeof(myUIObject),
			typeof(FrameSystem),
			typeof(LayoutScript),
			typeof(WindowShader),
			typeof(WindowObjectBase),
			typeof(OBJECT),
			typeof(ExcelData),
#if USE_SQLITE
			typeof(SQLiteData),
			typeof(SQLiteTable),
#endif
			typeof(SceneProcedure),
			typeof(NetPacketBit),
			typeof(NetPacketByte),
			typeof(NetPacketJson),
		};
		ignoreBaseClass.AddRange(getIgnoreResetPropertyClass());
		// 获取到类型
		foreach (Type type in assemly.GetTypes())
		{
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
			List<MemberInfo> fieldMembers = new();
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
			// `表示是模板类
			string className = type.Name.rangeToFirst('`');
			if (!classInfoList.TryGetValue(className, out ClassInfo info))
			{
				Debug.LogError("class:" + className + " 程序集中有此类,但是代码文件中找不到此类");
				continue;
			}
			// UI中的文件不需要检测
			if (info.mFilePath.Contains("/" + UI + "/"))
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
				if (index++ == 0)
				{
					startIndex = i;
				}
			}
			if (line.IndexOf('}') >= 0)
			{
				if (--index == 0)
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
		List<string> notResetMemberList = new();
		for (int i = 0; i < fieldMembers.Count; ++i)
		{
			notResetMemberList.Add(fieldMembers[i].Name);
		}

		List<string> resetFunctionLines = new();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			resetFunctionLines.Add(classInfo.mLines[startIndex + i]);
		}
		List<char> letters = new();
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
		foreach (string item in findFilesNonAlloc(path, ".cs"))
		{
			string[] fileLines = File.ReadAllLines(item);
			int classBeginIndex = -1;
			string nameSpace = EMPTY;
			for (int i = 0; i < fileLines.Length; ++i)
			{
				string line = fileLines[i];
				if (line.Contains("namespace "))
				{
					int startIndex = line.findFirstSubstr("namespace ", 0, true);
					if (line.Contains('{'))
					{
						nameSpace = line.rangeToFirst(startIndex, '{') + ".";
					}
					else
					{
						nameSpace = line.removeStartCount(startIndex) + ".";
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
		List<string> classLines = new();
		for (int i = 0; i < endIndex - startIndex + 1; ++i)
		{
			classLines.Add(fileLines[i + startIndex].removeAll('\t'));
		}
		string headLine = fileLines[startIndex];
		int nameStartIndex = headLine.findFirstSubstr("class ", 0, true);
		string className;
		if (headLine.Contains(':'))
		{
			className = nameSpace + headLine.rangeToFirst(nameStartIndex, ':').removeAll(' ');
		}
		else
		{
			className = nameSpace + headLine.removeStartCount(nameStartIndex).removeAll(' ');
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
			ClassInfo info = new();
			info.mLines.AddRange(classLines);
			info.mFilePath = path;
			info.mFunctionLine = startIndex + 1;
			fileInfos.Add(className, info);
		}
	}
	public static string findStackTrace()
	{
		// 找到UnityEditor.EditorWindow的assembly
		Assembly assembly_unity_editor = Assembly.GetAssembly(typeof(EditorWindow));
		if (assembly_unity_editor == null)
		{
			return null;
		}

		// 找到类UnityEditor.ConsoleWindow
		Type type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
		if (type_console_window == null)
		{
			return null;
		}
		// 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
		FieldInfo field_console_window = type_console_window.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
		if (field_console_window == null)
		{
			return null;
		}
		// 获取ms_ConsoleWindow的值
		object instance_console_window = field_console_window.GetValue(null);
		if (instance_console_window == null)
		{
			return null;
		}

		// 如果console窗口是焦点窗口的话，获取stacktrace信息
		if ((object)EditorWindow.focusedWindow == instance_console_window)
		{
			// 通过assembly获取类ListViewState
			Type type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
			if (type_list_view_state == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ListView
			FieldInfo field_list_view = type_console_window.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field_list_view == null)
			{
				return null;
			}

			// 获取m_ListView的值
			object value_list_view = field_list_view.GetValue(instance_console_window);
			if (value_list_view == null)
			{
				return null;
			}

			// 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
			FieldInfo field_active_text = type_console_window.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
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
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
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
					while (lines[checkLineNum].Contains("//") && !lines[checkLineNum].Contains("----"))
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
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
		for (int i = 0; i < lines.Length; ++i)
		{
			// 查找类名
			string className = findClassName(lines[i]);
			if (!className.isEmpty())
			{
				doCheckClassComment(filePath, lines, i, className);
				continue;
			}
			// 类名未找到的情况下查找枚举名
			string enumName = findEnumVariableName(lines[i]);
			if (!enumName.isEmpty())
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
			string lastLine = lines[lastIndex].removeAllEmpty();
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
		for (int i = index + 1; i < endIndex; ++i)
		{
			string line = lines[i];
			if (line.IndexOf('{') < 0 && !line.Contains("//") && !line.hasLowerLetter())
			{
				Debug.LogError("添加" + enumName + "." + line + "的注释!!!" + addFileLine(filePath, i + 1));
			}
			continue;
		}
	}
	public static void doCheckClassComment(string filePath, string[] lines, int index, string className)
	{
		if (index == 0)
		{
			Debug.LogError("类的上一行需要添加注释" + addFileLine(filePath, 0));
			return;
		}
		int lastIndex = index - 1;
		// 类名的上一行需要写对于该类的注释
		while (true)
		{
			string lastLine = lines[lastIndex].removeAllEmpty();
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
		if (className == typeof(FrameBaseHotFix).Name)
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
			if (!normalVarlable.isEmpty() && !lines[j].Contains("myUGUI") && !lines[j].Contains("//"))
			{
				Debug.LogError("需要在成员变量的后面增加注释!!!" + addFileLine(filePath, j + 1));
			}
		}
	}
	public static void doCheckSystemFunction(string filePath, string[] lines)
	{
		if (lines.isEmpty())
		{
			Debug.LogError("文件为空:" + filePath);
			return;
		}
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
			if (className.isEmpty())
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
			className = className.removeStartCount(namespaceIndex + 1);
		}
		// 一行字符串中会出现多个className
		while (true)
		{
			// 移除className之前的字符串。
			int firstIndex = codeLine.findFirstSubstr(className, 0, false);
			if (firstIndex < 0)
			{
				break;
			}

			// 过滤以className结尾的类 例如MyDebug
			if (firstIndex > 0 && isFunctionNameChar(codeLine[firstIndex - 1]))
			{
				// 移除这部分字符串
				codeLine = codeLine.removeStartCount(firstIndex + className.Length);
				continue;
			}

			// 找到一个有效的方法
			string functionString = findFirstFunctionString(codeLine, firstIndex + className.Length, out int resultIndex);
			if (functionString.isEmpty())
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
				codeLine = codeLine.removeStartCount((resultIndex + functionString.Length));
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
					return code.range(resultIndex, i);
				}
				if (i == code.Length - 1)
				{
					return code.removeStartCount(resultIndex);
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
					return code.range(resultIndex, lastIndex + 1);
				}
				if (i == 0)
				{
					resultIndex = 0;
					return code.range(resultIndex, lastIndex);
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
		return c == '_' || isNumeric(c) || isLetter(c);
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

			int index = code.findFirstSubstr(" guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点图片为空，filePath:" + filePath + ", 节点名:" + findGameObejctNameInPrefab(lines, i), loadAsset(filePath));
				continue;
			}
			code = code.removeStartCount(index);
			string assetPath = AssetDatabase.GUIDToAssetPath(code.rangeToFirst(','));
			// Resources下还有内置的资源，所有需要指定文件夹
			if (!assetPath.Contains(P_GAME_RESOURCES_PATH) &&
				!assetPath.Contains(P_RESOURCES_ATLAS_PATH) &&
				!assetPath.Contains(P_RESOURCES_TEXTURE_PATH))
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
			int index = code.findFirstSubstr(" guid: ", 0, true);
			if (index < 0)
			{
				Debug.LogError("UI节点字体为空，filePath:" + filePath + ", 节点名称:" + findGameObejctNameInPrefab(lines, i), loadAsset(filePath));
			}
			else
			{
				Debug.Log("Font assetPath:" + AssetDatabase.GUIDToAssetPath(code.rangeToFirst(index, ',')) + ", file:" + filePath, loadAsset(filePath));
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
				int nameIndex = lines[j].findFirstSubstr("m_Name: ", 0, true);
				if (nameIndex >= 0)
				{
					return lines[j].removeStartCount(nameIndex);
				}
			}
		}
		return null;
	}
	// 获取图片的引用信息 filePath 文件路径 assetType类型 tipText提示信息
	public static Dictionary<string, Dictionary<string, PrefabNodeItem>> getImageReferenceInfo(string filePath, string suffix, string tipText = "")
	{
		return getResourceReferenceInfo(filePath, suffix, "m_Sprite", tipText);
	}
	// 获取字体的引用信息 filePath 文件路径 assetType类型 tipText提示信息
	public static Dictionary<string, Dictionary<string, PrefabNodeItem>> getFontReferenceInfo(string filePath, string suffix, string tipText = "")
	{
		return getResourceReferenceInfo(filePath, suffix, "m_Font", tipText);
	}
	public string findSpriteGUID(string path, string name)
	{
		string nameLine = "name: " + name;
		string spriteIDKey = "spriteID: ";
		bool spriteStart = false;
		bool findName = false;
		foreach (string line in openTxtFileLines(projectPathToFullPath(P_GAME_RESOURCES_PATH + path + ".meta")))
		{
			if (!spriteStart && line.EndsWith("spriteSheet:"))
			{
				spriteStart = true;
				continue;
			}
			if (!spriteStart)
			{
				continue;
			}
			if (!findName && line.EndsWith(nameLine))
			{
				findName = true;
				continue;
			}
			if (!findName)
			{
				continue;
			}
			if (line.Contains(spriteIDKey))
			{
				return line.removeStartCount(line.IndexOf(spriteIDKey) + spriteIDKey.Length);
			}
		}
		return string.Empty;
	}
	public static BuildTarget getBuildTarget()
	{
#if UNITY_ANDROID
		return BuildTarget.Android;
#elif UNITY_WEBGL
		return BuildTarget.WebGL;
#elif UNITY_STANDALONE_WIN
		return BuildTarget.StandaloneWindows;
#elif UNITY_IOS
		return BuildTarget.iOS;
#elif UNITY_STANDALONE_OSX
		return BuildTarget.StandaloneOSX;
#endif
	}
	public static NamedBuildTarget getNameBuildTarget()
	{
		return NamedBuildTarget.FromBuildTargetGroup(getBuildTargetGroup());
	}
	public static BuildTargetGroup getBuildTargetGroup()
	{
		BuildTarget target = getBuildTarget();
		if (target == BuildTarget.Android)
		{
			return BuildTargetGroup.Android;
		}
		else if (target == BuildTarget.WebGL)
		{
			return BuildTargetGroup.WebGL;
		}
		else if (target == BuildTarget.StandaloneWindows ||
			target == BuildTarget.StandaloneWindows64 ||
			target == BuildTarget.StandaloneLinux64 ||
			target == BuildTarget.StandaloneOSX)
		{
			return BuildTargetGroup.Standalone;
		}
		else if (target == BuildTarget.iOS)
		{
			return BuildTargetGroup.iOS;
		}
		return BuildTargetGroup.Unknown;
	}
	public static string getAssetBundlePath(bool fullOrInProject, BuildTarget target = BuildTarget.NoTarget)
	{
		if (target == BuildTarget.NoTarget)
		{
			target = getBuildTarget();
		}
		if (target == BuildTarget.Android)
		{
			return fullOrInProject ? F_ASSET_BUNDLE_ANDROID_PATH : P_ASSET_BUNDLE_ANDROID_PATH;
		}
		else if (target == BuildTarget.WebGL)
		{
			return fullOrInProject ? F_ASSET_BUNDLE_WEBGL_PATH : P_ASSET_BUNDLE_WEBGL_PATH;
		}
		else if (target == BuildTarget.iOS)
		{
			return fullOrInProject ? F_ASSET_BUNDLE_IOS_PATH : P_ASSET_BUNDLE_IOS_PATH;
		}
		else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
		{
			return fullOrInProject ? F_ASSET_BUNDLE_WINDOWS_PATH : P_ASSET_BUNDLE_WINDOWS_PATH;
		}
		else if (target == BuildTarget.StandaloneOSX)
		{
			return fullOrInProject ? F_ASSET_BUNDLE_MACOS_PATH : P_ASSET_BUNDLE_MACOS_PATH;
		}
		return null;
	}
	public static bool multiSpriteToSpritePNG(Texture2D tex2D, string outputPath)
	{
		bool backupReadable = tex2D.isReadable;
		string texPath = AssetDatabase.GetAssetPath(tex2D);
		bool modified = false;
		var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
		TextureImporterCompression backupCompress = importer.textureCompression;
		if (!tex2D.isReadable)
		{
			importer.isReadable = true;
			modified = true;
		}
		if (tex2D.format != TextureFormat.RGBA32 && tex2D.format != TextureFormat.RGB24)
		{
			importer.textureCompression = TextureImporterCompression.Uncompressed;
			modified = true;
		}
		if (modified)
		{
			importer.SaveAndReimport();
		}
		validPath(ref outputPath);
		foreach (UObject obj in AssetDatabase.LoadAllAssetsAtPath(texPath))
		{
			if (obj is not Sprite sprite)
			{
				continue;
			}
			Texture2D output = new((int)sprite.rect.width, (int)sprite.rect.height);
			Rect r = sprite.textureRect;
			output.SetPixels(sprite.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height));
			output.Apply();
			output.name = sprite.name;
			byte[] bytes = output.EncodeToPNG();
			writeFile(outputPath + sprite.name + ".png", bytes, bytes.Length);
		}
		if (modified)
		{
			importer.isReadable = backupReadable;
			importer.textureCompression = backupCompress;
			importer.SaveAndReimport();
		}
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 获取字体的引用信息 filePath 文件路径 assetType类型 tipText提示信息
	protected static Dictionary<string, Dictionary<string, PrefabNodeItem>> getResourceReferenceInfo(string filePath, string suffix, string key, string tipText = "")
	{
		// 初始化和查找相应文件
		Dictionary<string, Dictionary<string, PrefabNodeItem>> allPrefabMap = new();
		List<string> files = findFilesNonAlloc(filePath, suffix);
		int fileCount = files.Count;
		string currentID = EMPTY;
		for (int i = 0; i < fileCount; ++i)
		{
			EditorUtility.DisplayProgressBar("正在查找所有" + tipText + "资源", "进度:" + (i + 1) + "/" + fileCount, (float)(i + 1) / fileCount);
			string file = files[i];
			Dictionary<string, PrefabNodeItem> currentItem = new();
			openTxtFileLines(file, out string[] lines);
			string gameObjectName = null;
			for (int j = 0; j < lines.Length - 1; ++j)
			{
				string line = lines[j];
				// 开始一个GameObject信息时需要清空一下名字
				if (line.StartsWith("GameObject:"))
				{
					string lastLine = lines[j - 1];
					string gameObjectID = lastLine.removeStartCount(lastLine.findFirstSubstr(" &", 0, true));
					currentItem.Add(gameObjectID, new());
					currentID = gameObjectID;
					gameObjectName = null;
				}
				// GameObject信息中的第一个名字就是GameObject的名字,后面出现的名字是组件名,需要忽略
				else if (line.Contains(" m_Name:"))
				{
					if (gameObjectName == null)
					{
						gameObjectName = line.removeStartCount(line.findFirstSubstr(": ", 0, true));
						currentItem.get(currentID).mGameObjectName = gameObjectName;
					}
				}
				// 对应资源的ID
				else if (line.Contains(" " + key + ":"))
				{
					// 部分预制体会存在两行显示的情况,导致索引位置不一致
					int startIndex = line.findFirstSubstr("guid: ", 0, true);
					if (startIndex >= 0)
					{
						currentItem.get(currentID).mResourceID = line.range(startIndex, line.IndexOf(", type:"));
					}
				}
			}
			allPrefabMap.Add(file, currentItem);
		}
		EditorUtility.ClearProgressBar();
		return allPrefabMap;
	}
	protected static BuildResult buildGame(string outputPath, BuildTarget target, BuildTargetGroup targetGroup, BuildOptions buildOptions)
	{
		BuildPlayerOptions options = new();
		options.scenes = new string[] { START_SCENE };
		options.locationPathName = outputPath;
		options.targetGroup = targetGroup;
		options.target = target;
		options.options = buildOptions;
		BuildReport report = BuildPipeline.BuildPlayer(options);
		AssetDatabase.Refresh();
		// 暂停10秒,等到打包处理完再进行删除
		Thread.Sleep(10000);
		try
		{
			if (report.summary.result == BuildResult.Succeeded)
			{
				List<string> dirList = new();
				findFolders(getFilePath(outputPath), dirList, null);
				foreach (string dir in dirList)
				{
					if (dir.Contains("_BackUpThisFolder_ButDontShipItWithYourGame") ||
						dir.Contains("_BurstDebugInformation_DoNotShip"))
					{
						deleteFolder(dir);
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError("删除文件夹失败:" + e.Message);
		}
		return report.summary.result;
	}
	[OnOpenAsset(1)]
	protected static bool OnOpenAsset(int instanceID, int line)
	{
		// 自定义函数，用来获取log中的stacktrace
		string stack_trace = findStackTrace();
		// 通过stacktrace来定位是否是我们自定义的log
		if (stack_trace.isEmpty())
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
		foreach (string infoLine in splitLine(stack_trace))
		{
			if (infoLine.startWith("File:"))
			{
				filePath = infoLine.removeStartString("File:");
			}
			else if (infoLine.startWith("Line:"))
			{
				fileLine = SToI(infoLine.removeStartString("Line:"));
			}
		}

		if (filePath == null)
		{
			return false;
		}
		// 如果以Asset开头打开Asset文件并定位到该行
		if (filePath.StartsWith(P_ASSETS_PATH))
		{
			UObject codeObject = loadAsset(filePath);
			if ((codeObject == null || codeObject.GetInstanceID() == instanceID) && fileLine == line)
			{
				return false;
			}
			AssetDatabase.OpenAsset(codeObject, fileLine);
		}
		else
		{
			// 如果以HotFix开头只打开文件不定位到行
			EditorUtility.OpenWithDefaultApp(F_PROJECT_PATH + filePath);
		}
		return true;
	}
}