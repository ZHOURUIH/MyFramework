#if USE_TMP
using TMPro;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;
using System;
using System.IO;
using System.Collections.Generic;
using static EditorCommonUtility;
using static StringUtility;
using static FileUtility;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseUtility;
using UObject = UnityEngine.Object;

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
			string assetPath = AssetDatabase.GetAssetPath(obj);
			string outputPath = projectPathToFullPath(getFilePath(assetPath, true) + getFolderName(assetPath));
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
	[MenuItem(mMenuName + "根据文件夹创建图集文件")]
	public static void createSpriteAtlasByFolder()
	{
		foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
		{
			string path = AssetDatabase.GetAssetPath(obj);
			if (!isDirExist(path))
			{
				Debug.LogError("选择的不是文件夹");
				return;
			}
			List<string> textureList = new();
			findFiles(projectPathToFullPath(path), textureList, ".png", false);
			if (textureList.Count == 0)
			{
				Debug.Log("选择的文件夹中没有图片资源");
				return;
			}
			foreach (string file in textureList)
			{
				var sprite = loadFirstSubAsset<Sprite>(file);
				if (sprite == null)
				{
					Debug.LogError("图片导入方式不是Sprite, 无法生成图集");
					return;
				}
			}
			SpriteAtlas atlas = new();
			atlas.SetIncludeInBuild(true);
			atlas.SetIsVariant(false);
			// 打包设置
			SpriteAtlasPackingSettings packing = new()
			{
				enableRotation = false,
				enableTightPacking = false,
				padding = 4
			};
			atlas.SetPackingSettings(packing);
			// 纹理设置
			SpriteAtlasTextureSettings texture = new()
			{
				readable = false,
				generateMipMaps = false,
				filterMode = FilterMode.Bilinear,
				anisoLevel = 1
			};
			atlas.SetTextureSettings(texture);
			atlas.Add(new[] { obj });

			// 保存文件
			string savePath = path + "/" + getFolderName(path) + SPRITE_ATLAS_SUFFIX;
			if (isFileExist(savePath))
			{
				AssetDatabase.DeleteAsset(savePath);
			}
			AssetDatabase.CreateAsset(atlas, savePath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Debug.Log("已创建图集：" + savePath + ",sprite数量:" + textureList.Count);
		}
	}
#if USE_TMP
	[MenuItem(mMenuName + "精简TMP字体大小,但是精简完以后无法再替换材质")]
	public static void extractTexture()
	{
		string fontPath = AssetDatabase.GetAssetPath(Selection.activeObject).rightToLeft();
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
		Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath.Replace(Application.dataPath, "Assets"));
		AssetDatabase.RemoveObjectFromAsset(targeFontAsset.atlasTexture);
		targeFontAsset.atlasTextures[0] = atlas;
		targeFontAsset.material.mainTexture = atlas;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
#endif
	[MenuItem(mMenuName + "删除所有空文件夹", false, 32)]
	public static void deleteAllEmptyFolder()
	{
		foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
		{
			string path = AssetDatabase.GetAssetPath(obj);
			if (Directory.Exists(path))
			{
				deleteEmptyFolder(projectPathToFullPath(path));
			}
			else
			{
				Debug.LogError("选中的不是文件夹");
			}
			break;
		}
		AssetDatabase.Refresh();
	}
	[MenuItem(mMenuName + "修正引用了Boader的sprite但是没有使用Slice的组件", false, 133)]
	public static void fixNeedUseSliceImage()
	{
		// 无法通过Selection.gameObjects来判断,只能使用Selection.transforms来判断
		// 选中的是文件或者文件夹
		if (Selection.transforms.count() == 0)
		{
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = AssetDatabase.GetAssetPath(obj);
				if (Directory.Exists(path))
				{
					List<string> files = new();
					findFiles(projectPathToFullPath(path), files, ".prefab");
					foreach (string file in files)
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
	[MenuItem(mMenuName + "修正ImageAtlasPath记录的图集路径", false, 133)]
	public static void fixImageAtlasPath()
	{
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
				string path = AssetDatabase.GetAssetPath(obj);
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
			string path = AssetDatabase.GetAssetPath(obj);
			if (Directory.Exists(path))
			{
				List<string> files = new();
				findFiles(projectPathToFullPath(path), files, getTextureSuffixList());
				for (int i = 0; i < files.Count; ++i)
				{
					setPixelPerUnit(fullPathToProjectPath(files[i]), 1);
					EditorUtility.DisplayProgressBar("正在处理导入格式", "进度:" + (i + 1) + "/" + files.Count, (float)(i + 1) / files.Count);
				}
			}
			else if (isFileExist(path))
			{
				setPixelPerUnit(path, 1);
			}
		}
		EditorUtility.ClearProgressBar();
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
			string path = AssetDatabase.GetAssetPath(obj);
			if (Directory.Exists(path))
			{
				findFiles(projectPathToFullPath(path), allFiles, getTextureSuffixList());
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
				EditorUtility.DisplayProgressBar("正在处理导入格式", "进度:" + (curIndex + 1) + "/" + allFiles.Count, (float)(curIndex + 1) / allFiles.Count);
			}
			EditorUtility.ClearProgressBar();
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
			List<string> prefabFiles = new();
			foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
			{
				string path = AssetDatabase.GetAssetPath(obj);
				if (Directory.Exists(path))
				{
					prefabFiles.Clear();
					findFiles(projectPathToFullPath(path), prefabFiles, ".prefab");
					foreach (string file in prefabFiles)
					{
						doPrefab(fullPathToProjectPath(file));
					}
				}
				else if (isFileExist(path))
				{
					doPrefab(path);
				}
			}
		}
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
			Vector3 pos = oldTransform.localPosition;
			Quaternion rot = oldTransform.localRotation;
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
			bool hasBoader = img.sprite.border.x > 0 ||
							 img.sprite.border.y > 0 ||
							 img.sprite.border.z > 0 ||
							 img.sprite.border.w > 0;
			// 检查条件：有边框但未使用Slice模式
			if (hasBoader && img.type != Image.Type.Sliced)
			{
				img.type = Image.Type.Sliced;
				modified = true;
				Debug.Log("已修复引用的图片有boader但是没有使用slice模式,GameObjcet:" + img.gameObject.name, prefab);
			}
		}
		return modified;
	}
	protected static bool doFixImageAtlasPath(GameObject prefab, Dictionary<string, SpriteAtlas> atlasListCache)
	{
		bool modified = false;
		foreach (var img in prefab.GetComponentsInChildren<ImageAtlasPath>(true))
		{
			if (img.refresh(true, atlasListCache))
			{
				modified = true;
				Debug.Log("已修复ImageAtlasPath,GameObjcet:" + img.gameObject.name, prefab);
			}
		}
		return modified;
	}
}