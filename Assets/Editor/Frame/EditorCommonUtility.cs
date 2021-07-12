using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using LitJson;
using UnityEditor.Build.Reporting;

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

public class EditorCommonUtility : FrameUtility
{
	protected static char[] mHexUpperChar;
	protected static char[] mHexLowerChar;
	protected static string mHexString = "ABCDEFabcdef0123456789";
	protected const int GUID_LENGTH = 32;
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
	// 图片尺寸是否为2的n次方
	protected static bool isSizePow2(Texture2D tex)
	{
		return isPow2(tex.width) && isPow2(tex.height);
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
					refrenceList.Add(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file));
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
			if (pattern.StartsWith("*"))
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
								refrenceList.Add(curFile, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(curFile));
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
		string[] lines = split(shaderContent, true, "\n");
		int propertyLine = -1;
		int propertyStartLine = -1;
		int propertyEndLine = -1;
		string texturePropertyKey = texturePropertyName + "(";
		int[] texturePropertyNextIndex;
		generateNextIndex(texturePropertyKey, out texturePropertyNextIndex);
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = removeAll(lines[i], "\r");
			lines[i] = removeAll(lines[i], "\t");
			lines[i] = removeAll(lines[i], " ");
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
		string materialContent = openTxtFile(projectPathToFullPath(path), true);
		string[] materialLines = split(materialContent, true, "\r\n");
		for (int i = 0; i < materialLines.Length; ++i)
		{
			materialLines[i] = removeAll(materialLines[i], " ");
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
				if (line.StartsWith("-"))
				{
					string textureLine = materialLines[i + 1];
					if (KMPSearch(textureLine, textureStr, textureNextIndex) < 0)
					{
						Debug.LogError("material texture property error, " + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
						return;
					}
					if (KMPSearch(textureLine, guidKey, guidNextIndex) >= 0)
					{
						int startIndex = textureLine.IndexOf(guidKey) + guidKey.Length;
						int endIndex = textureLine.IndexOf(",", startIndex);
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
							Debug.LogError("在GameResource中找不到材质引用的资源 : " + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
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
		string[] materialLines = split(materialContent, true, "\r\n");
		foreach (var item in materialLines)
		{
			string line = removeAll(item, " ");
			if (line.StartsWith("m_Shader:"))
			{
				string key = "guid:";
				int startIndex = line.IndexOf(key) + key.Length;
				int endIndex = line.IndexOf(",", startIndex);
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
			Debug.LogError("材质使用了内置shader,无法找到shader文件", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
			return;
		}
		string shaderFile = findAsset(shaderGUID, ".shader", true, allFileText);
		if (shaderFile == EMPTY)
		{
			Debug.LogWarning("can not find material shader:" + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
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
			string line = removeAll(item, " ");
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
				Debug.LogError("材质中使用了无效的贴图属性:" + path, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
				materialValid = false;
				break;
			}
		}
		if (materialValid)
		{
			Debug.Log("材质贴图属性正常:" + path);
		}
	}
	// 根据GUID查找资源文件
	private static string findAsset(string guid, string suffix, bool showAsset, Dictionary<string, List<FileGUIDLines>> allFileText)
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
					Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file));
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
						Debug.Log(curFile, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(curFile));
					}
					return curFile;
				}
			}
		}
		return EMPTY;
	}
	public static Dictionary<string, string> getSpriteGUIDs(string path)
	{
		Dictionary<string, string> spriteGUIDs = new Dictionary<string, string>();
		string atlasContent = openTxtFile(projectPathToFullPath(path + ".meta"), true);
		string[] atlasLines = split(atlasContent, true, "\n");
		bool spriteStart = false;
		foreach (var item in atlasLines)
		{
			string line = removeAll(item, " ");
			line = removeAll(line, "\r");
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
			string[] elem = split(line, true, ":");
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
						info.mObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
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
			string metaContent = openTxtFile(projectPathToFullPath(path + ".meta"), false);
			string[] lines = split(metaContent, true, "\n");
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
	public static bool isGUIDChar(char value)
	{
		return value >= '0' && value <= '9' || value >= 'a' && value <= 'z';
	}
	public static Dictionary<string, List<FileGUIDLines>> getAllFileText(string path, string[] patterns)
	{
		string[] supportPatterns = new string[] { ".prefab", ".unity", ".mat", ".asset", ".meta", ".controller", ".overrideController" };
		// key是后缀名,value是该后缀名的文件信息列表
		Dictionary<string, List<FileGUIDLines>> allFileText = new Dictionary<string, List<FileGUIDLines>>();
		string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
		foreach (var item in files)
		{
			if (matchPattern(item, supportPatterns) && matchPattern(item, patterns))
			{
				string curSuffix = getFileSuffix(item);
				if (!allFileText.TryGetValue(curSuffix, out List<FileGUIDLines> guidLines))
				{
					guidLines = new List<FileGUIDLines>();
					allFileText.Add(curSuffix, guidLines);
				}
				FileGUIDLines fileGUIDLines = new FileGUIDLines();
				string fileContent = File.ReadAllText(item);
				string[] lines = split(fileContent, true, "\n");
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
		}
		return allFileText;
	}
	public static bool matchPattern(string fileName, string[] patterns)
	{
		if (patterns == null)
		{
			return true;
		}
		if (fileName.EndsWith(".meta"))
		{
			return true;
		}
		foreach (var item in patterns)
		{
			if (fileName.EndsWith(item))
			{
				return true;
			}
		}
		return false;
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
	public static void generateColliderMain()
	{
		var mSelectGameObjects = Selection.gameObjects;
		if (mSelectGameObjects == null)
		{
			EditorUtility.DisplayDialog("Error", "请先在选中需要设置的物体对象", "确定");
			return;
		}

		// 对选中的所有对象遍历生成角色控制器
		foreach (var item in mSelectGameObjects)
		{
			var mRenderers = item.GetComponentsInChildren<Renderer>();
			if (mRenderers == null)
			{
				continue;
			}

			//计算中心位置
			var transforms = item.GetComponentsInChildren<Transform>();
			int posCount = 0;
			Vector3 centerPos = Vector3.zero;
			foreach (var tf in transforms)
			{
				if (tf == item.transform)
				{
					continue;
				}
				centerPos += tf.position;
				posCount++;
			}
			centerPos = centerPos / posCount;

			// 计算包围盒
			Bounds mBounds = new Bounds(centerPos, Vector3.zero);
			foreach (var render in mRenderers)
			{
				Bounds bounds = render.bounds;
				bounds.size = multiVector3(render.bounds.size, render.transform.localScale);
				bounds.center = render.bounds.center;
				mBounds.Encapsulate(bounds);
			}

			// 创建角色控制器并设置参数
			makeCharacterCollider(item, mBounds);
			//激活物体MeshCollider
			activeMeshCollider(item);

		}
	}
	static void makeCharacterCollider(GameObject obj, Bounds mBounds)
	{
		var characterController = obj.GetComponent<CharacterController>();
		if (characterController == null)
		{
			characterController = obj.AddComponent<CharacterController>();
		}
		characterController.enabled = true;
		characterController.radius = clamp(mBounds.size.x * 0.5f, 0.001f, mBounds.size.z * 0.5f);
		characterController.height = clampMin(mBounds.size.y, characterController.radius * 2.0f);
		characterController.center = new Vector3(0.0f, characterController.height * 0.5f, 0.0f);
		characterController.stepOffset = getMin(characterController.height, 0.3f);
		characterController.enabled = false;
	}
	static void activeMeshCollider(GameObject obj)
    {
		var meshCollider = obj.GetComponent<MeshCollider>();
        if (meshCollider==null)
        {
			meshCollider = obj.AddComponent<MeshCollider>();
        }
		meshCollider.enabled = true;
		meshCollider.convex = true;
    }
	public static void generateFloderConfig()
	{
		string filePath = EditorUtility.OpenFilePanel("选择保存路径", "", "json");
		if (isEmpty(filePath))
		{
			return;
		}
		Dictionary<string, List<string>> saveObjects = new Dictionary<string, List<string>>();
		saveObjects["ItemNameConfig"] = new List<string>();
		var mSelectGameObjects = Selection.gameObjects;
		if (mSelectGameObjects == null)
		{
			EditorUtility.DisplayDialog("Error", "请先在选中需要设置的物体对象", "确定");
			return;
		}
		foreach (var item in mSelectGameObjects)
		{
			saveObjects["ItemNameConfig"].Add(item.name);
		}
		string saveConfigStr = JsonMapper.ToJson(saveObjects);
		writeTxtFile(filePath, saveConfigStr);
		EditorUtility.DisplayDialog("Success", "生成成功", "确定");
	}
	public static BuildReport buildAndroidGame(string apkPath)
	{
		BuildPlayerOptions options = new BuildPlayerOptions();
		options.scenes = new string[] { FrameDefine.START_SCENE };
		options.locationPathName = apkPath;
		options.targetGroup = BuildTargetGroup.Android;
		options.target = BuildTarget.Android;
		return BuildPipeline.BuildPlayer(options);
	}
}