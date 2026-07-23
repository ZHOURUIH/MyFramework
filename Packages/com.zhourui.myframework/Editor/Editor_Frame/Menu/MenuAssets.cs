using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using static EditorCommonUtility;
using static FileUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static FrameDefine;
using static StringUtility;
using UObject = UnityEngine.Object;

public class TpSheetSprite
{
	public string name;
	public int x;
	public int y;
	public int width;
	public int height;
	public Vector2 pivot;
	public Vector4 border;
}

public class TpSheetData
{
	public string textureFileName;
	public int width;
	public int height;
	public readonly List<TpSheetSprite> sprites = new();
}

public class MenuAssets
{
	public const string mMenuName = "Assets/";
	[MenuItem(mMenuName + "将图集还原为散图")]
	public static void doMultiSpriteToSpritePNG()
	{
		foreach (UObject obj in Selection.objects)
		{
			var tex = obj as Texture2D;
			if (tex == null)
			{
				Debug.LogError("当前在Project中没有选中任何图集文件");
				return;
			}
			string assetPath = getAssetPath(obj);
			string outputPath = projectPathToFullPath(getFilePath(assetPath, true) + tex.name);
			if (multiSpriteToSpritePNG(tex, outputPath))
			{
				Debug.Log("已输出图片到" + outputPath);
			}
			else
			{
				Debug.LogError("生成散图失败");
			}
		}
		AssetDatabase.Refresh();
	}
	// 创建SpriteAtlasV2，并将指定文件夹添加到Objects for Packing
	[MenuItem(mMenuName + "生成图集文件")]
	public static void generateSpriteAtlasV2()
	{
		foreach (UObject obj in Selection.objects)
		{
			string assetPath = getAssetPath(obj);
			if (!isDirExist(assetPath))
			{
				continue;
			}
			string folderName = getFileNameNoSuffixNoDir(assetPath);
			string atlasPath = assetPath + "/" + folderName + ".spriteatlasv2";
			SpriteAtlasAsset atlasAsset = SpriteAtlasAsset.Load(atlasPath);
			if (atlasAsset == null)
			{
				DefaultAsset folder = loadAssetAtPath<DefaultAsset>(assetPath);
				if (folder == null)
				{
					logErrorBase("无法加载文件夹:" + assetPath);
					return;
				}
				atlasAsset = new SpriteAtlasAsset();
				atlasAsset.Add(new UObject[] { folder });
				SpriteAtlasAsset.Save(atlasAsset, atlasPath);
				AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport);
			}

			var importer = AssetImporter.GetAtPath(atlasPath) as SpriteAtlasImporter;
			if (importer == null)
			{
				logErrorBase("无法获取SpriteAtlasImporter:" + atlasPath);
				return;
			}

			SpriteAtlasPackingSettings packingSettings = importer.packingSettings;
			packingSettings.enableRotation = false;
			packingSettings.enableTightPacking = false;
			importer.packingSettings = packingSettings;
			importer.SaveAndReimport();

			logBase("已创建SpriteAtlasV2:" + atlasPath);
		}
	}
	[MenuItem(mMenuName + "精简TMP字体大小,但是精简完以后无法再替换材质")]
	public static void extractTexture()
	{
		string fontPath = getAssetPath(Selection.activeObject).rightToLeft();
		string texturePath = fontPath.Replace(".asset", ".png");
		var targeFontAsset = Selection.activeObject as TMP_FontAsset;
		Texture2D texture2D = new(targeFontAsset.atlasTexture.width, targeFontAsset.atlasTexture.height, TextureFormat.ASTC_6x6, false);
		Graphics.CopyTexture(targeFontAsset.atlasTexture, texture2D);
		byte[] dataBytes = texture2D.EncodeToPNG();
		FileStream fs = File.Open(texturePath, FileMode.OpenOrCreate);
		fs.Write(dataBytes, 0, dataBytes.Length);
		fs.Flush();
		fs.Close();
		AssetDatabase.Refresh();
		Texture2D atlas = loadAssetAtPath<Texture2D>(texturePath.Replace(Application.dataPath, "Assets"));
		AssetDatabase.RemoveObjectFromAsset(targeFontAsset.atlasTexture);
		targeFontAsset.atlasTextures[0] = atlas;
		targeFontAsset.material.mainTexture = atlas;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	[MenuItem(mMenuName + "删除所有空文件夹", false, 32)]
	public static void deleteAllEmptyFolder()
	{
		UObject obj = Selection.GetFiltered(typeof(UObject), SelectionMode.Assets).first();
		if (obj != null)
		{
			string path = getAssetPath(obj);
			if (Directory.Exists(path))
			{
				deleteEmptyFolder(projectPathToFullPath(path));
			}
			else
			{
				Debug.LogError("选中的不是文件夹");
			}
		}
		AssetDatabase.Refresh();
	}
	[MenuItem(mMenuName + "修正引用了Border的sprite但是没有使用Slice的组件", false, 133)]
	public static void fixNeedUseSliceImage()
	{
		Debug.Log("开始修正引用了Border的sprite但是没有使用Slice的组件...");
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() == 0)
		{
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = getAssetPath(obj);
				if (Directory.Exists(path))
				{
					foreach (string file in findFilesNonAlloc(projectPathToFullPath(path), ".prefab"))
					{
						GameObject prefab = loadAsset<GameObject>(file);
						if (doFixSingleNeedUseSliceImage(prefab))
						{
							PrefabUtility.SaveAsPrefabAsset(prefab, fullPathToProjectPath(file));
						}
					}
				}
				else if (isFileExist(path))
				{
					GameObject prefab = loadAsset<GameObject>(path);
					if (doFixSingleNeedUseSliceImage(prefab))
					{
						PrefabUtility.SaveAsPrefabAsset(prefab, path);
					}
				}
			}
		}
	}
	[MenuItem(mMenuName + "检查引用了ImageType为Slice的组件引用了没有Border的节点", false, 133)]
	public static void checkSliceImageUseNoBorderSprite()
	{
		Debug.Log("开始检查引用了ImageType为Slice的组件引用了没有Border的节点...");
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() == 0)
		{
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = getAssetPath(obj);
				if (Directory.Exists(path))
				{
					foreach (string file in findFilesNonAlloc(projectPathToFullPath(path), ".prefab"))
					{
						doCheckSliceImageButNoBorder(loadAsset<GameObject>(file));
					}
				}
				else if (isFileExist(path))
				{
					doCheckSliceImageButNoBorder(loadAsset<GameObject>(path));
				}
			}
		}
	}
	[MenuItem(mMenuName + "修正ImageAtlasPath记录的图集路径", false, 133)]
	public static void fixImageAtlasPath()
	{
		Debug.Log("开始修正...");
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() == 0)
		{
			Dictionary<string, SpriteAtlas> atlasListCache = new();
			List<string> atlasFiles = new();
			findFiles(F_ASSETS_PATH, atlasFiles, SPRITE_ATLAS_SUFFIX);
			foreach (string file in atlasFiles)
			{
				string path = fullPathToProjectPath(file);
				atlasListCache.add(path, loadAssetAtPath<SpriteAtlas>(path));
			}
			foreach (GameObject go in Selection.gameObjects.safe())
			{
				if (doFixImageAtlasPath(go, atlasListCache))
				{
					EditorUtility.SetDirty(go);
				}
			}
			List<string> prefabFiles = new();
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = getAssetPath(obj);
				if (Directory.Exists(path))
				{
					prefabFiles.Clear();
					findFiles(projectPathToFullPath(path), prefabFiles, ".prefab");
					foreach (string file in prefabFiles)
					{
						GameObject prefab = loadAsset<GameObject>(file);
						if (doFixImageAtlasPath(prefab, atlasListCache))
						{
							PrefabUtility.SaveAsPrefabAsset(prefab, fullPathToProjectPath(file));
						}
					}
				}
				else if (isFileExist(path))
				{
					GameObject prefab = loadAsset<GameObject>(path);
					if (doFixImageAtlasPath(prefab, atlasListCache))
					{
						PrefabUtility.SaveAsPrefabAsset(prefab, path);
					}
				}
			}
		}
	}
	[MenuItem(mMenuName + "将文件夹中所有图片导入为PixelUnit为100的Sprite格式", false, 133)]
	public static void importTextureAsSpritePixelUnit100()
	{
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() > 0)
		{
			return;
		}
		AssetDatabase.StartAssetEditing();
		foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
		{
			string path = getAssetPath(obj);
			if (Directory.Exists(path))
			{
				List<string> files = new();
				findFiles(projectPathToFullPath(path), files, mTextureSuffixList);
				for (int i = 0; i < files.Count; ++i)
				{
					setPixelPerUnit(fullPathToProjectPath(files[i]), 1);
					displayProgressBar("正在处理导入格式", "进度:", i, files.Count);
				}
			}
			else if (isFileExist(path))
			{
				setPixelPerUnit(path, 1);
			}
		}
		clearProgressBar();
		AssetDatabase.StopAssetEditing();
		AssetDatabase.Refresh();
	}
	[MenuItem(mMenuName + "将文件夹中所有图片导入为PixelUnit为1的Sprite格式", false, 133)]
	public static void importTextureAsSpritePixelUnit1()
	{
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() > 0)
		{
			return;
		}
		List<string> allFiles = new();
		foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
		{
			string path = getAssetPath(obj);
			if (Directory.Exists(path))
			{
				findFiles(projectPathToFullPath(path), allFiles, mTextureSuffixList);
			}
			else if (isFileExist(path))
			{
				allFiles.add(projectPathToFullPath(path));
			}
		}

		int batchSize = 1000;
		int batchCount = allFiles.Count / batchSize;
		if (batchSize * batchCount < allFiles.Count)
		{
			++batchCount;
		}
		for (int i = 0; i < batchCount; ++i)
		{
			AssetDatabase.StartAssetEditing();
			int startIndex = i * batchSize;
			for (int j = 0; j < batchSize; ++j)
			{
				int curIndex = startIndex + j;
				if (curIndex >= allFiles.Count)
				{
					break;
				}
				setPixelPerUnit(fullPathToProjectPath(allFiles[curIndex]), 100);
				displayProgressBar("正在处理导入格式", "进度:", curIndex, allFiles.Count);
			}
			clearProgressBar();
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
		}
	}
	[MenuItem(mMenuName + "给文件夹内所有Prefab的所有节点添加RectTransform", false, 133)]
	public static void addRectTransformToAllNode()
	{
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() == 0)
		{
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = getAssetPath(obj);
				if (Directory.Exists(path))
				{
					findFilesNonAlloc(projectPathToFullPath(path), ".prefab").For(file => doPrefab(fullPathToProjectPath(file)));
				}
				else if (isFileExist(path))
				{
					doPrefab(path);
				}
			}
		}
	}
	[MenuItem(mMenuName + "查找文件引用", false, 134)]
	public static void searchReference()
	{
		MenuCheckResources.searchReference();
	}
	[MenuItem(mMenuName + "手动导入TPSheet", false, 2000)]
	private static void importTPSheet()
	{
		List<string> sheetAssetPaths = new();
		foreach (UObject selectedObject in Selection.objects)
		{
			string assetPath = AssetDatabase.GetAssetPath(selectedObject);
			if (string.Equals(Path.GetExtension(assetPath), ".tpsheet", StringComparison.OrdinalIgnoreCase))
			{
				sheetAssetPaths.Add(assetPath);
			}
		}
		if (sheetAssetPaths.Count == 0)
		{
			EditorUtility.DisplayDialog("TPSheet导入", "请在Project窗口中选中一个或多个.tpsheet文件。", "确定");
			return;
		}

		int successCount = 0;
		int errorsCount = 0;
		foreach (string sheetAssetPath in sheetAssetPaths)
		{
			try
			{
				string sheetFilePath = assetPathToFilePath(sheetAssetPath);
				TpSheetData sheetData = parseSheet(sheetFilePath);
				string textureAssetPath = getTextureAssetPath(sheetFilePath, sheetData.textureFileName);
				if (!File.Exists(assetPathToFilePath(textureAssetPath)))
				{
					Debug.LogError("找不到图集图片:" + textureAssetPath);
				}

				// 先强制刷新PNG像素，再覆盖Sprite切割数据。
				AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(textureAssetPath);
				if (texture == null)
				{
					Debug.LogError("无法加载图集图片:" + textureAssetPath);
				}
				if (texture.width != sheetData.width || texture.height != sheetData.height)
				{
					Debug.LogError("PNG尺寸与tpsheet不一致。PNG:" + texture.width + "x" + texture.height +
									"，tpsheet:" + sheetData.width + "x" + sheetData.height);
				}

				var textureImporter = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
				if (textureImporter == null)
				{
					Debug.LogError("无法获取TextureImporter:" + textureAssetPath);
				}

				textureImporter.textureType = TextureImporterType.Sprite;
				textureImporter.spriteImportMode = SpriteImportMode.Multiple;
				applyWithTextureImporter(textureImporter, sheetData);
				Debug.Log("TPSheet导入成功:" + sheetAssetPath + "，纹理:" + textureAssetPath + "，Sprite数量:" + sheetData.sprites.Count);
				++successCount;
			}
			catch (Exception exception)
			{
				++errorsCount;
				Debug.LogError("TPSheet导入失败: " + sheetAssetPath + "\n" + exception.Message);
			}
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		if (errorsCount == 0)
		{
			Debug.Log("导入完成，成功导入" + successCount + "个图集。");
		}
		else
		{
			Debug.LogError("成功:" + successCount + "，失败:" + errorsCount + "。\n详细错误请查看Console。");
		}
	}
	[MenuItem(mMenuName + "手动导入TPSheet", true)]
	private static bool validateImportSelectedMenu()
	{
		foreach (UObject selectedObject in Selection.objects)
		{
			string assetPath = AssetDatabase.GetAssetPath(selectedObject);
			if (string.Equals(Path.GetExtension(assetPath), ".tpsheet", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void doPrefab(string prefabPath)
	{
		GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
		doAddRectTransform(prefab);
		PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
	}
	protected static void doAddRectTransform(GameObject current)
	{
		if (current.GetComponent<RectTransform>() == null)
		{
			Transform oldTransform = current.transform;
			oldTransform.GetLocalPositionAndRotation(out Vector3 pos, out Quaternion rot);
			Vector3 scale = oldTransform.localScale;

			var rt = current.AddComponent<RectTransform>();
			rt.localPosition = pos;
			rt.localRotation = rot;
			rt.localScale = scale;

			// 修复布局
			if (current.TryGetComponent<LayoutElement>(out _))
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
			}
		}

		// 递归处理子对象
		foreach (Transform child in current.transform)
		{
			doAddRectTransform(child.gameObject);
		}
	}
	protected static void setPixelPerUnit(string assetPath, int pixelUnit)
	{
		try
		{
			var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
			if (importer == null)
			{
				Debug.LogError("找不到文件:" + assetPath);
				return;
			}
			// multiple可能是之前已经打好的图集,不处理
			if (importer.spriteImportMode == SpriteImportMode.Multiple)
			{
				return;
			}
			// 仅当需要修改时才更新设置
			if (importer.spritePixelsPerUnit != pixelUnit ||
				importer.textureType != TextureImporterType.Sprite ||
				importer.spriteImportMode != SpriteImportMode.Single)
			{
				importer.textureType = TextureImporterType.Sprite;
				importer.spriteImportMode = SpriteImportMode.Single;
				importer.spritePixelsPerUnit = pixelUnit;
				importer.SaveAndReimport();
			}
		}
		catch(Exception e)
		{
			Debug.LogError("exception:" + e.Message);
		}
	}
	protected static bool doFixSingleNeedUseSliceImage(GameObject prefab)
	{
		bool modified = false;
		foreach (var img in prefab.GetComponentsInChildren<Image>(true))
		{
			// 跳过未分配Sprite的节点
			if (img.sprite == null)
			{
				continue;
			}
			bool hasBorder = img.sprite.border.x > 0 ||
							 img.sprite.border.y > 0 ||
							 img.sprite.border.z > 0 ||
							 img.sprite.border.w > 0;
			// 检查条件：有边框但未使用Slice模式
			if (hasBorder && img.type != Image.Type.Sliced)
			{
				img.type = Image.Type.Sliced;
				modified = true;
				Debug.Log("已修复引用的图片有border但是没有使用slice模式,GameObject:" + img.gameObject.name, prefab);
			}
		}
		return modified;
	}
	// 检查引用了Border的sprite但是没有使用Slice的组件,但不修改Prefab,仅修改内存中的对象,适用于当前选中的是场景中的对象
	protected static void doCheckSliceImageButNoBorder(GameObject prefab)
	{
		foreach (var img in prefab.GetComponentsInChildren<Image>(true))
		{
			// 跳过未分配Sprite的节点
			if (img.sprite == null)
			{
				continue;
			}
			bool hasBorder = img.sprite.border.x > 0 ||
							 img.sprite.border.y > 0 ||
							 img.sprite.border.z > 0 ||
							 img.sprite.border.w > 0;
			// 检查条件：有边框但未使用Slice模式
			if (!hasBorder && img.type == Image.Type.Sliced)
			{
				Debug.LogError("ImageType为slice,但是引用的图片没有border,GameObject:" + img.gameObject.name + ", prefab:" + prefab.name + ", 图片:" + img.sprite.name, img.sprite.texture);
			}
		}
	}
	protected static bool doFixImageAtlasPath(GameObject prefab, Dictionary<string, SpriteAtlas> atlasListCache)
	{
		bool modified = false;
		foreach (var img in prefab.GetComponentsInChildren<ImageAtlasPath>(true))
		{
			if (img.refresh(true, atlasListCache))
			{
				modified = true;
				Debug.Log("已修复ImageAtlasPath,GameObject:" + img.gameObject.name, prefab);
			}
		}
		return modified;
	}
	private static void applyWithTextureImporter(TextureImporter textureImporter, TpSheetData sheetData)
	{
		SpriteMetaData[] spriteMetaData = new SpriteMetaData[sheetData.sprites.Count];
		for (int i = 0; i < sheetData.sprites.Count; ++i)
		{
			TpSheetSprite sprite = sheetData.sprites[i];
			SpriteMetaData metadata = new();
			metadata.name = sprite.name;
			metadata.rect = new Rect(sprite.x, sprite.y, sprite.width, sprite.height);
			metadata.pivot = sprite.pivot;
			metadata.alignment = (int)SpriteAlignment.Custom;
			metadata.border = sprite.border;
			spriteMetaData[i] = metadata;
		}
		textureImporter.spritesheet = spriteMetaData;
		textureImporter.SaveAndReimport();
	}
	private static TpSheetData parseSheet(string sheetFilePath)
	{
		TpSheetData data = new();
		HashSet<string> spriteNames = new(StringComparer.Ordinal);
		bool formatValid = false;
		string[] lines = File.ReadAllLines(sheetFilePath, Encoding.UTF8);
		for (int lineIndex = 0; lineIndex < lines.Length; ++lineIndex)
		{
			string line = lines[lineIndex].Trim();
			if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
			{
				continue;
			}
			if (line.StartsWith(":format=", StringComparison.Ordinal))
			{
				formatValid = line[8..].Trim() == "40300";
				continue;
			}
			if (line.StartsWith(":texture=", StringComparison.Ordinal))
			{
				data.textureFileName = line[9..].Trim();
				continue;
			}
			if (line.StartsWith(":size=", StringComparison.Ordinal))
			{
				parseSize(line[6..].Trim(), data, lineIndex + 1);
				continue;
			}
			if (line[0] == ':')
			{
				continue;
			}

			TpSheetSprite sprite = parseSprite(line, lineIndex + 1);
			if (!spriteNames.Add(sprite.name))
			{
				Debug.LogError("第" + (lineIndex + 1) + "行存在重复Sprite名称:" + sprite.name);
			}
			data.sprites.Add(sprite);
		}

		if (!formatValid)
		{
			Debug.LogError("只支持:format=40300的tpsheet文件。");
		}
		if (string.IsNullOrEmpty(data.textureFileName))
		{
			Debug.LogError("tpsheet中缺少:texture字段。");
		}
		if (data.width <= 0 || data.height <= 0)
		{
			Debug.LogError("tpsheet中缺少有效的:size字段。");
		}
		if (data.sprites.Count == 0)
		{
			Debug.LogError("tpsheet中没有Sprite数据。");
		}
		return data;
	}
	private static void parseSize(string value, TpSheetData data, int lineNumber)
	{
		string[] values = value.Split('x');
		if (values.Length != 2 ||
			!int.TryParse(values[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out data.width) ||
			!int.TryParse(values[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out data.height))
		{
			Debug.LogError("第" + lineNumber + "行的:size格式错误:" + value);
		}
	}
	private static TpSheetSprite parseSprite(string line, int lineNumber)
	{
		string[] values = line.Split(';');
		if (values.Length < 7)
		{
			Debug.LogError("第" + lineNumber + "行的Sprite数据字段不足。");
		}

		TpSheetSprite sprite = new();
		sprite.name = values[0].Trim();
		if (sprite.name.Length == 0)
		{
			Debug.LogError("第" + lineNumber + "行的Sprite名称为空。");
		}

		sprite.x = parseInt(values[1], lineNumber, "x");
		sprite.y = parseInt(values[2], lineNumber, "y");
		sprite.width = parseInt(values[3], lineNumber, "width");
		sprite.height = parseInt(values[4], lineNumber, "height");
		if (sprite.x < 0 || sprite.y < 0 || sprite.width <= 0 || sprite.height <= 0)
		{
			Debug.LogError("第" + lineNumber + "行的Sprite矩形无效。");
		}

		float pivotX = parseFloat(values[5], lineNumber, "pivotX");
		float pivotY = parseFloat(values[6], lineNumber, "pivotY");
		sprite.pivot = new Vector2(pivotX, pivotY);
		sprite.border = Vector4.zero;
		if (values.Length >= 11)
		{
			sprite.border = new Vector4(parseFloat(values[7], lineNumber, "borderLeft"), parseFloat(values[8], lineNumber, "borderBottom"),
										parseFloat(values[9], lineNumber, "borderRight"), parseFloat(values[10], lineNumber, "borderTop"));
		}
		return sprite;
	}
	private static int parseInt(string value, int lineNumber, string fieldName)
	{
		if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
		{
			Debug.LogError("第" + lineNumber + "行的" + fieldName + "不是有效整数:" + value);
		}
		return result;
	}
	private static float parseFloat(string value, int lineNumber, string fieldName)
	{
		if (!float.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
		{
			Debug.LogError("第" + lineNumber + "行的" + fieldName + "不是有效数字:" + value);
		}
		return result;
	}
	private static string getTextureAssetPath(string sheetFilePath, string textureFileName)
	{
		string sheetDirectory = Path.GetDirectoryName(sheetFilePath);
		string textureFilePath = Path.GetFullPath(Path.Combine(sheetDirectory, textureFileName));
		string projectDirectory = Path.GetFullPath(Path.GetDirectoryName(Application.dataPath));
		string fullPath = Path.GetFullPath(textureFilePath);
		string projectPrefix = projectDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
		if (!fullPath.startWith(projectPrefix))
		{
			Debug.LogError("纹理文件不在当前Unity工程中:" + fullPath);
		}

		string assetPath = fullPath[projectPrefix.Length..].Replace('\\', '/');
		if (!assetPath.startWith(P_ASSETS_PATH))
		{
			Debug.LogError("纹理文件不在Assets目录中:" + fullPath);
		}
		return assetPath;
	}
	private static string assetPathToFilePath(string assetPath)
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath));
	}
}