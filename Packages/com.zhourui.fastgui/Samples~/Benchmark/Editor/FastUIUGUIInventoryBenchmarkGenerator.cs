using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FastUIUGUIInventoryBenchmarkGenerator
{
	protected const string GENERATED_ROOT = "Assets/FastUIBenchmarkGenerated";
	protected const string INVENTORY_ROOT = GENERATED_ROOT + "/Inventory";
	protected const string TEXTURE_ROOT = INVENTORY_ROOT + "/Textures";
	protected const string ATLAS_ROOT = INVENTORY_ROOT + "/Atlas";
	protected const string MATERIAL_ROOT = INVENTORY_ROOT + "/Materials";
	protected const string SCENE_ROOT = INVENTORY_ROOT + "/Scenes";
	protected const string TEXTURE_A_PATH = TEXTURE_ROOT + "/InventoryBenchmark_A.png";
	protected const string TEXTURE_B_PATH = TEXTURE_ROOT + "/InventoryBenchmark_B.png";
	protected const string ATLAS_TEXTURE_PATH = ATLAS_ROOT + "/InventorySpriteAtlas.png";
	protected const string ATLAS_LEGACY_ASSET_PATH = ATLAS_ROOT + "/InventorySpriteAtlas.asset";
	protected const int ATLAS_CELL_SIZE = 64;
	protected const int ATLAS_COLUMNS = 8;
	protected const int ATLAS_ROWS = 4;
	protected const int ATLAS_ICON_COUNT = 24;
	protected const string MATERIAL_PATH = MATERIAL_ROOT + "/InventoryBenchmark_UI_Default.mat";
	protected const string SCENE_PATH = SCENE_ROOT + "/FastUI_vs_UGUI_Inventory.unity";
	protected const string BENCHMARK_OBJECT_NAME = "FastUI_vs_UGUI_Inventory_Benchmark";
	protected const string CAMERA_OBJECT_NAME = "FastUI_Benchmark_Camera";
	protected const string FORMAL_BUILD_DEFINE = "FASTGUI_FORMAL_BENCHMARK_MASTER";
	protected const string FORMAL_BUILD_OUTPUT = "Output/FastGUI.exe";
	protected static readonly char[] REQUIRED_CHINESE_CHARACTERS = { '背', '包', '道', '具', '数', '量', '等', '级', '分', '类', '属', '性', '使', '用', '说', '明', '测', '试', '已', '全', '部', '完', '成', '可', '以', '关', '闭', '程', '序', '终', '止' };
	[MenuItem("Tools/FastGUI Benchmark/Generate Inventory Benchmark", false, 100)]
	public static void generateInventoryBenchmark()
	{
		if (!validateRuntimeTypes() || !validateTMPSettings())
		{
			return;
		}
		TMP_FontAsset font = findBenchmarkFont();
		if (font == null)
		{
			EditorUtility.DisplayDialog("FastUI Benchmark", "没有找到同时支持FastUI与UGUI测试所需中文字符的TMP_FontAsset。\n\n请先在项目中准备一个支持中文的TMP_FontAsset，再重新执行生成菜单。", "确定");
			Debug.LogError("[FastUI Benchmark Generator] 未找到支持Benchmark中文字符的TMP_FontAsset，停止生成。建议使用项目现有中文字体，例如simsun对应的TMP_FontAsset.");
			return;
		}
		try
		{
			AssetDatabase.StartAssetEditing();
			ensureFolders();
			generateTexture(TEXTURE_A_PATH, false);
			generateTexture(TEXTURE_B_PATH, true);
			generateInventoryAtlasTexture();
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}
		configureRawTextureImporter(TEXTURE_A_PATH);
		configureRawTextureImporter(TEXTURE_B_PATH);
		configureInventoryAtlasImporter();
		FastUIInventoryBenchmarkAtlas atlas = loadInventoryAtlasFromTexture();
		Material material = generateSharedMaterial();
		Texture textureA = AssetDatabase.LoadAssetAtPath<Texture>(TEXTURE_A_PATH);
		Texture textureB = AssetDatabase.LoadAssetAtPath<Texture>(TEXTURE_B_PATH);
		if (textureA == null || textureB == null || material == null || !isInventoryAtlasValid(atlas))
		{
			Debug.LogError("[FastUI Benchmark Generator] Benchmark资源生成失败，Texture或Material为空，停止创建场景.");
			return;
		}
		createBenchmarkScene(textureA, textureB, material, font, atlas);
		addSceneToBuildSettings(SCENE_PATH);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
		Debug.Log("[FastUI Benchmark Generator] Inventory Benchmark生成完成.\n" +
			"Scene:" + SCENE_PATH + "\n" +
			"TextureA:" + TEXTURE_A_PATH + "\n" +
			"TextureB:" + TEXTURE_B_PATH + "\n" +
			"SpriteAtlas(Multiple):" + ATLAS_TEXTURE_PATH + "\n" +
			"Material:" + MATERIAL_PATH + "\n" +
			"Font:" + AssetDatabase.GetAssetPath(font) + "\n" +
			"Editor参考测试可直接Play；正式Player测试请执行Tools/FastGUI Benchmark/Build Formal Windows Benchmark.");
	}
	[MenuItem("Tools/FastGUI Benchmark/Open Inventory Benchmark Scene", false, 101)]
	public static void openInventoryBenchmarkScene()
	{
		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
		if (sceneAsset == null)
		{
			EditorUtility.DisplayDialog("FastUI Benchmark", "尚未生成Inventory Benchmark。请先执行：\nTools/FastGUI Benchmark/Generate Inventory Benchmark", "确定");
			return;
		}
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			return;
		}
		EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
	}
	[MenuItem("Tools/FastGUI Benchmark/Build Formal Windows Benchmark", false, 102)]
	public static void buildFormalWindowsBenchmark()
	{
		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
		FastUIInventoryBenchmarkAtlas atlas = loadInventoryAtlasFromTexture();
		if (sceneAsset == null || !isInventoryAtlasValid(atlas))
		{
			generateInventoryBenchmark();
			sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
			atlas = loadInventoryAtlasFromTexture();
			if (sceneAsset == null || !isInventoryAtlasValid(atlas))
			{
				EditorUtility.DisplayDialog("FastGUI Benchmark", "Inventory Benchmark生成失败，无法构建。", "确定");
				return;
			}
		}
		if (!validateTMPSettings() || !validateFastGUIUnsafeSetting())
		{
			return;
		}
		if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 &&
			!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
		{
			Debug.LogError("[FastGUI Benchmark Generator] 无法切换到StandaloneWindows64，停止正式Benchmark构建.");
			return;
		}
		NamedBuildTarget namedBuildTarget = NamedBuildTarget.Standalone;
		PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.IL2CPP);
		PlayerSettings.SetIl2CppCompilerConfiguration(namedBuildTarget, Il2CppCompilerConfiguration.Master);
		PlayerSettings.SetIl2CppCodeGeneration(namedBuildTarget, Il2CppCodeGeneration.OptimizeSpeed);
		if (PlayerSettings.GetScriptingBackend(namedBuildTarget) != ScriptingImplementation.IL2CPP ||
			PlayerSettings.GetIl2CppCompilerConfiguration(namedBuildTarget) != Il2CppCompilerConfiguration.Master ||
			PlayerSettings.GetIl2CppCodeGeneration(namedBuildTarget) != Il2CppCodeGeneration.OptimizeSpeed)
		{
			Debug.LogError("[FastGUI Benchmark Generator] 正式Benchmark构建配置设置失败，停止构建.");
			return;
		}
		string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
		if (string.IsNullOrEmpty(projectRoot))
		{
			Debug.LogError("[FastGUI Benchmark Generator] 无法获得Unity工程根目录，停止构建.");
			return;
		}
		string outputPath = Path.Combine(projectRoot, FORMAL_BUILD_OUTPUT);
		string outputDirectory = Path.GetDirectoryName(outputPath);
		if (!string.IsNullOrEmpty(outputDirectory))
		{
			Directory.CreateDirectory(outputDirectory);
		}
		BuildPlayerOptions options = new()
		{
			scenes = new[] { SCENE_PATH },
			locationPathName = outputPath,
			target = BuildTarget.StandaloneWindows64,
			options = BuildOptions.AutoRunPlayer,
			extraScriptingDefines = new[] { FORMAL_BUILD_DEFINE },
		};
		bool originalFrameTimingStats = PlayerSettings.enableFrameTimingStats;
		BuildReport report = null;
		try
		{
			PlayerSettings.enableFrameTimingStats = true;
			if (!PlayerSettings.enableFrameTimingStats)
			{
				Debug.LogError("[FastGUI Benchmark Generator] 无法启用Frame Timing Stats，停止正式Benchmark构建.");
				return;
			}
			Debug.Log("[FastGUI Benchmark Generator] 开始正式Windows Benchmark构建:\n" +
				"ScriptingBackend=IL2CPP\nCppConfiguration=Master\nIl2CppCodeGeneration=OptimizeSpeed\nDevelopmentBuild=False\n" +
				"FrameTimingStats=True\nEasyECSUnsafe=Required\nOutput=" + outputPath);
			report = BuildPipeline.BuildPlayer(options);
		}
		finally
		{
			PlayerSettings.enableFrameTimingStats = originalFrameTimingStats;
		}
		if (report == null || report.summary.result != BuildResult.Succeeded)
		{
			Debug.LogError("[FastGUI Benchmark Generator] 正式Benchmark构建失败:" + (report != null ? report.summary.result.ToString() : "BuildReport为空"));
			return;
		}
		Debug.Log("[FastGUI Benchmark Generator] 正式Benchmark构建完成:" + outputPath + " | FrameTimingStats=True");
	}
	[MenuItem("Tools/FastGUI Benchmark/Delete Generated Inventory Benchmark", false, 120)]
	public static void deleteGeneratedInventoryBenchmark()
	{
		if (!AssetDatabase.IsValidFolder(INVENTORY_ROOT))
		{
			Debug.Log("[FastUI Benchmark Generator] 没有需要删除的Inventory Benchmark生成资源.");
			return;
		}
		if (!EditorUtility.DisplayDialog("FastUI Benchmark", "确定删除自动生成的Inventory Benchmark资源？\n\n" + INVENTORY_ROOT, "删除", "取消"))
		{
			return;
		}
		removeSceneFromBuildSettings(SCENE_PATH);
		AssetDatabase.DeleteAsset(INVENTORY_ROOT);
		AssetDatabase.Refresh();
		Debug.Log("[FastUI Benchmark Generator] 已删除:" + INVENTORY_ROOT);
	}
	protected static bool validateRuntimeTypes()
	{
		if (typeof(FastUIUGUIInventoryBenchmark) == null || typeof(FastCanvas) == null || typeof(FastRawImage) == null || typeof(FastImage) == null || typeof(FastText) == null)
		{
			Debug.LogError("[FastUI Benchmark Generator] FastUI Benchmark运行时代码未正确编译，停止生成.");
			return false;
		}
		return true;
	}
	protected static bool validateTMPSettings()
	{
		if (TMP_Settings.instance != null)
		{
			return true;
		}
		EditorUtility.DisplayDialog("FastGUI Benchmark", "TextMesh Pro Essential Resources尚未导入。\n\n请先执行：\nWindow/TextMeshPro/Import TMP Essential Resources", "确定");
		Debug.LogError("[FastGUI Benchmark Generator] TMP Settings不存在，停止操作。请先导入TextMesh Pro Essential Resources.");
		return false;
	}
	protected static bool validateFastGUIUnsafeSetting()
	{
		const string asmdefPath = "Packages/com.zhourui.fastgui/Runtime/FastGUI.Runtime.asmdef";
		string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
		if (string.IsNullOrEmpty(projectRoot))
		{
			Debug.LogError("[FastGUI Benchmark Generator] 无法获得Unity工程根目录.");
			return false;
		}
		string absolutePath = Path.Combine(projectRoot, asmdefPath);
		if (!File.Exists(absolutePath))
		{
			Debug.LogError("[FastGUI Benchmark Generator] 找不到FastGUI.Runtime.asmdef:" + asmdefPath);
			return false;
		}
		string json = File.ReadAllText(absolutePath);
		if (json.IndexOf("\"allowUnsafeCode\": true", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			return true;
		}
		Debug.LogError("[FastGUI Benchmark Generator] FastGUI.Runtime.asmdef未开启Allow Unsafe Code，停止正式Benchmark构建.");
		return false;
	}
	protected static void ensureFolders()
	{
		ensureFolder("Assets", "FastUIBenchmarkGenerated");
		ensureFolder(GENERATED_ROOT, "Inventory");
		ensureFolder(INVENTORY_ROOT, "Textures");
		ensureFolder(INVENTORY_ROOT, "Atlas");
		ensureFolder(INVENTORY_ROOT, "Materials");
		ensureFolder(INVENTORY_ROOT, "Scenes");
	}
	protected static void ensureFolder(string parent, string folderName)
	{
		string path = parent + "/" + folderName;
		if (!AssetDatabase.IsValidFolder(path))
		{
			AssetDatabase.CreateFolder(parent, folderName);
		}
	}
	protected static void generateTexture(string assetPath, bool second)
	{
		const int size = 64;
		Texture2D texture = new(size, size, TextureFormat.RGBA32, false, false);
		Color32[] pixels = new Color32[size * size];
		for (int y = 0; y < size; ++y)
		{
			for (int x = 0; x < size; ++x)
			{
				float nx = (x + 0.5f) / size * 2.0f - 1.0f;
				float ny = (y + 0.5f) / size * 2.0f - 1.0f;
				float radius = Mathf.Sqrt(nx * nx + ny * ny);
				bool checker = ((x >> 3) + (y >> 3)) % 2 == 0;
				Color color;
				if (!second)
				{
					float edge = Mathf.Clamp01(1.0f - radius * 0.72f);
					color = checker ? new Color(0.24f, 0.52f, 0.92f, 1.0f) : new Color(0.16f, 0.30f, 0.62f, 1.0f);
					color *= 0.72f + edge * 0.28f;
					color.a = 1.0f;
				}
				else
				{
					float diamond = Mathf.Clamp01(1.0f - (Mathf.Abs(nx) + Mathf.Abs(ny)) * 0.72f);
					color = checker ? new Color(0.92f, 0.50f, 0.20f, 1.0f) : new Color(0.62f, 0.24f, 0.12f, 1.0f);
					color *= 0.72f + diamond * 0.28f;
					color.a = 1.0f;
				}
				pixels[y * size + x] = color;
			}
		}
		texture.SetPixels32(pixels);
		texture.Apply(false, false);
		byte[] png = texture.EncodeToPNG();
		UnityEngine.Object.DestroyImmediate(texture);
		string absolutePath = getAbsoluteAssetPath(assetPath);
		File.WriteAllBytes(absolutePath, png);
	}
	protected static void generateInventoryAtlasTexture()
	{
		int width = ATLAS_COLUMNS * ATLAS_CELL_SIZE;
		int height = ATLAS_ROWS * ATLAS_CELL_SIZE;
		Texture2D texture = new(width, height, TextureFormat.RGBA32, false, false);
		Color32[] pixels = new Color32[width * height];
		for (int cell = 0; cell < ATLAS_COLUMNS * ATLAS_ROWS; ++cell)
		{
			int startX = (cell % ATLAS_COLUMNS) * ATLAS_CELL_SIZE;
			int startY = (cell / ATLAS_COLUMNS) * ATLAS_CELL_SIZE;
			for (int y = 0; y < ATLAS_CELL_SIZE; ++y)
			{
				for (int x = 0; x < ATLAS_CELL_SIZE; ++x)
				{
					pixels[(startY + y) * width + startX + x] = getInventoryAtlasPixel(cell, x, y);
				}
			}
		}
		texture.SetPixels32(pixels);
		texture.Apply(false, false);
		File.WriteAllBytes(getAbsoluteAssetPath(ATLAS_TEXTURE_PATH), texture.EncodeToPNG());
		UnityEngine.Object.DestroyImmediate(texture);
	}
	protected static Color32 getInventoryAtlasPixel(int cell, int x, int y)
	{
		float nx = (x + 0.5f) / ATLAS_CELL_SIZE * 2.0f - 1.0f;
		float ny = (y + 0.5f) / ATLAS_CELL_SIZE * 2.0f - 1.0f;
		bool edge4 = x < 4 || y < 4 || x >= ATLAS_CELL_SIZE - 4 || y >= ATLAS_CELL_SIZE - 4;
		bool edge8 = x < 8 || y < 8 || x >= ATLAS_CELL_SIZE - 8 || y >= ATLAS_CELL_SIZE - 8;
		if (cell == 0) return edge8 ? new Color(0.30f, 0.34f, 0.42f, 1.0f) : new Color(0.09f, 0.11f, 0.15f, 1.0f);
		if (cell == 1) return edge4 ? Color.white : Color.clear;
		if (cell == 2)
		{
			bool chain = Mathf.Abs(nx + ny) < 0.18f || Mathf.Abs(nx + ny - 0.55f) < 0.14f;
			return chain && nx * nx + ny * ny < 0.85f ? new Color(0.30f, 0.82f, 1.0f, 1.0f) : Color.clear;
		}
		if (cell == 3)
		{
			bool body = Mathf.Abs(nx) < 0.42f && ny < 0.32f && ny > -0.58f;
			float lockRadius = nx * nx / 0.18f + (ny - 0.25f) * (ny - 0.25f) / 0.28f;
			bool arch = lockRadius > 0.52f && lockRadius < 1.0f && ny > 0.05f;
			return body || arch ? new Color(1.0f, 0.68f, 0.24f, 1.0f) : Color.clear;
		}
		if (cell == 4) return edge4 ? new Color(1.0f, 0.84f, 0.18f, 0.92f) : Color.clear;
		if (cell == 5) return edge8 ? new Color(0.22f, 0.28f, 0.42f, 1.0f) : new Color(0.10f, 0.12f, 0.18f, 1.0f);
		if (cell == 6) return edge8 ? new Color(0.40f, 0.52f, 0.82f, 1.0f) : new Color(0.18f, 0.21f, 0.30f, 1.0f);
		if (cell == 7) return edge8 ? new Color(0.28f, 0.30f, 0.38f, 1.0f) : new Color(0.11f, 0.12f, 0.16f, 1.0f);
		int iconIndex = cell - 8;
		float hue = Mathf.Repeat(iconIndex * 0.137f, 1.0f);
		Color baseColor = Color.HSVToRGB(hue, 0.62f, 0.96f);
		float radius = Mathf.Sqrt(nx * nx + ny * ny);
		bool diamond = Mathf.Abs(nx) + Mathf.Abs(ny) < 0.72f;
		bool ring = radius > 0.35f && radius < 0.68f;
		bool cross = (Mathf.Abs(nx) < 0.16f || Mathf.Abs(ny) < 0.16f) && radius < 0.72f;
		bool shape = iconIndex % 3 == 0 ? diamond : iconIndex % 3 == 1 ? ring : cross;
		if (!shape) return new Color(0.03f, 0.04f, 0.06f, 0.0f);
		float highlight = Mathf.Clamp01(1.12f - radius * 0.40f);
		baseColor *= 0.72f + highlight * 0.28f;
		baseColor.a = 1.0f;
		return baseColor;
	}
	protected static void configureInventoryAtlasImporter()
	{
		if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ATLAS_LEGACY_ASSET_PATH) != null)
		{
			AssetDatabase.DeleteAsset(ATLAS_LEGACY_ASSET_PATH);
		}
		TextureImporter importer = AssetImporter.GetAtPath(ATLAS_TEXTURE_PATH) as TextureImporter;
		if (importer == null)
		{
			throw new InvalidOperationException("无法获得Inventory MultiSprite TextureImporter:" + ATLAS_TEXTURE_PATH);
		}
		importer.textureType = TextureImporterType.Sprite;
		importer.spriteImportMode = SpriteImportMode.Multiple;
		importer.spritePixelsPerUnit = 100.0f;
		importer.sRGBTexture = true;
		importer.alphaIsTransparency = true;
		importer.mipmapEnabled = false;
		importer.wrapMode = TextureWrapMode.Clamp;
		importer.filterMode = FilterMode.Bilinear;
		importer.npotScale = TextureImporterNPOTScale.None;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.crunchedCompression = false;
		importer.isReadable = false;
		TextureImporterSettings settings = new();
		importer.ReadTextureSettings(settings);
		settings.spriteMeshType = SpriteMeshType.FullRect;
		importer.SetTextureSettings(settings);
		SpriteMetaData[] sprites = new SpriteMetaData[ATLAS_COLUMNS * ATLAS_ROWS];
		Vector4 slicedBorder = new(8.0f, 8.0f, 8.0f, 8.0f);
		sprites[0] = createSpriteMetaData("SlotBackground", 0, slicedBorder);
		sprites[1] = createSpriteMetaData("QualityFrame", 1, Vector4.zero);
		sprites[2] = createSpriteMetaData("BindIcon", 2, Vector4.zero);
		sprites[3] = createSpriteMetaData("LockIcon", 3, Vector4.zero);
		sprites[4] = createSpriteMetaData("Selected", 4, Vector4.zero);
		sprites[5] = createSpriteMetaData("HeaderBackground", 5, slicedBorder);
		sprites[6] = createSpriteMetaData("TabBackground", 6, slicedBorder);
		sprites[7] = createSpriteMetaData("DetailBackground", 7, slicedBorder);
		for (int i = 0; i < ATLAS_ICON_COUNT; ++i)
		{
			sprites[8 + i] = createSpriteMetaData("ItemIcon_" + i.ToString("00"), 8 + i, Vector4.zero);
		}
		var spritesheetProperty = typeof(TextureImporter).GetProperty("spritesheet");
		if (spritesheetProperty == null)
		{
			throw new InvalidOperationException("当前Unity版本无法通过TextureImporter配置Multiple Sprite数据。");
		}
		spritesheetProperty.SetValue(importer, sprites);
		importer.SaveAndReimport();
	}
	protected static SpriteMetaData createSpriteMetaData(string name, int cellIndex, Vector4 border)
	{
		int x = (cellIndex % ATLAS_COLUMNS) * ATLAS_CELL_SIZE;
		int y = (cellIndex / ATLAS_COLUMNS) * ATLAS_CELL_SIZE;
		return new SpriteMetaData
		{
			name = name,
			rect = new Rect(x, y, ATLAS_CELL_SIZE, ATLAS_CELL_SIZE),
			alignment = (int)SpriteAlignment.Center,
			pivot = new Vector2(0.5f, 0.5f),
			border = border,
		};
	}
	public static FastUIInventoryBenchmarkAtlas loadInventoryAtlasFromTexture()
	{
		Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ATLAS_TEXTURE_PATH);
		if (texture == null)
		{
			return null;
		}
		UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(ATLAS_TEXTURE_PATH);
		FastUIInventoryBenchmarkAtlas atlas = new()
		{
			mTexture = texture,
			mSlotBackground = findSprite(assets, "SlotBackground"),
			mQualityFrame = findSprite(assets, "QualityFrame"),
			mBindIcon = findSprite(assets, "BindIcon"),
			mLockIcon = findSprite(assets, "LockIcon"),
			mSelected = findSprite(assets, "Selected"),
			mHeaderBackground = findSprite(assets, "HeaderBackground"),
			mTabBackground = findSprite(assets, "TabBackground"),
			mDetailBackground = findSprite(assets, "DetailBackground"),
			mItemIcons = new Sprite[ATLAS_ICON_COUNT],
		};
		for (int i = 0; i < atlas.mItemIcons.Length; ++i)
		{
			atlas.mItemIcons[i] = findSprite(assets, "ItemIcon_" + i.ToString("00"));
		}
		return atlas;
	}
	protected static Sprite findSprite(UnityEngine.Object[] assets, string name)
	{
		for (int i = 0; i < assets.Length; ++i)
		{
			if (assets[i] is Sprite sprite && sprite.name == name)
			{
				return sprite;
			}
		}
		return null;
	}
	public static bool isInventoryAtlasValid(FastUIInventoryBenchmarkAtlas atlas)
	{
		if (atlas == null || atlas.mTexture == null || atlas.mSlotBackground == null || atlas.mQualityFrame == null ||
			atlas.mBindIcon == null || atlas.mLockIcon == null || atlas.mSelected == null || atlas.mHeaderBackground == null ||
			atlas.mTabBackground == null || atlas.mDetailBackground == null || atlas.mItemIcons == null || atlas.mItemIcons.Length != ATLAS_ICON_COUNT)
		{
			return false;
		}
		for (int i = 0; i < atlas.mItemIcons.Length; ++i)
		{
			if (atlas.mItemIcons[i] == null || atlas.mItemIcons[i].texture != atlas.mTexture)
			{
				return false;
			}
		}
		TextureImporter importer = AssetImporter.GetAtPath(ATLAS_TEXTURE_PATH) as TextureImporter;
		return importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Multiple;
	}
	protected static string getAbsoluteAssetPath(string assetPath)
	{
		if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
		{
			throw new ArgumentException("必须是Assets下的路径:" + assetPath);
		}
		string relative = assetPath.Substring("Assets/".Length);
		return Path.Combine(Application.dataPath, relative);
	}
	protected static void configureRawTextureImporter(string assetPath)
	{
		TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
		if (importer == null)
		{
			Debug.LogError("[FastUI Benchmark Generator] 无法获得TextureImporter:" + assetPath);
			return;
		}
		importer.textureType = TextureImporterType.Default;
		importer.sRGBTexture = true;
		importer.alphaIsTransparency = true;
		importer.mipmapEnabled = false;
		importer.wrapMode = TextureWrapMode.Clamp;
		importer.filterMode = FilterMode.Bilinear;
		importer.npotScale = TextureImporterNPOTScale.None;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.crunchedCompression = false;
		importer.isReadable = false;
		importer.SaveAndReimport();
	}
	protected static Material generateSharedMaterial()
	{
		Material material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
		Shader shader = Shader.Find("UI/Default");
		if (shader == null)
		{
			Material defaultCanvasMaterial = Canvas.GetDefaultCanvasMaterial();
			shader = defaultCanvasMaterial != null ? defaultCanvasMaterial.shader : null;
		}
		if (shader == null)
		{
			Debug.LogError("[FastUI Benchmark Generator] 找不到UI/Default Shader，无法生成共享Benchmark Material.");
			return null;
		}
		if (material == null)
		{
			material = new Material(shader) { name = "InventoryBenchmark_UI_Default" };
			AssetDatabase.CreateAsset(material, MATERIAL_PATH);
		}
		else if (material.shader != shader)
		{
			material.shader = shader;
			EditorUtility.SetDirty(material);
		}
		return material;
	}
	protected static TMP_FontAsset findBenchmarkFont()
	{
		TMP_FontAsset preferred = findFontByName("simsun");
		if (isFontSupported(preferred))
		{
			return preferred;
		}
		string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
		Array.Sort(guids, StringComparer.Ordinal);
		for (int i = 0; i < guids.Length; ++i)
		{
			string path = AssetDatabase.GUIDToAssetPath(guids[i]);
			TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
			if (isFontSupported(font))
			{
				return font;
			}
		}
		TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
		return isFontSupported(defaultFont) ? defaultFont : null;
	}
	protected static TMP_FontAsset findFontByName(string name)
	{
		string[] guids = AssetDatabase.FindAssets(name + " t:TMP_FontAsset");
		Array.Sort(guids, StringComparer.Ordinal);
		for (int i = 0; i < guids.Length; ++i)
		{
			TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[i]));
			if (font != null)
			{
				return font;
			}
		}
		return null;
	}
	protected static bool isFontSupported(TMP_FontAsset font)
	{
		if (font == null)
		{
			return false;
		}
		for (int i = 0; i < REQUIRED_CHINESE_CHARACTERS.Length; ++i)
		{
			if (!font.HasCharacter(REQUIRED_CHINESE_CHARACTERS[i]))
			{
				return false;
			}
		}
		return true;
	}
	protected static void createBenchmarkScene(Texture textureA, Texture textureB, Material material, TMP_FontAsset font, FastUIInventoryBenchmarkAtlas atlas)
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
		{
			throw new OperationCanceledException("用户取消保存当前场景，Benchmark生成中止.");
		}
		Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		createCamera();
		GameObject benchmarkObject = new(BENCHMARK_OBJECT_NAME);
		FastUIUGUIInventoryBenchmark benchmark = benchmarkObject.AddComponent<FastUIUGUIInventoryBenchmark>();
		benchmark.mTextureA = textureA;
		benchmark.mTextureB = textureB;
		benchmark.mImageMaterial = material;
		benchmark.mFont = font;
		benchmark.mInventoryAtlas = atlas;
		benchmark.mItemCount = 300;
		benchmark.mColumnCount = 10;
		benchmark.mVisibleRefreshCount = 60;
		benchmark.mBatchRefreshCount = 100;
		benchmark.mItemSize = new Vector2(72.0f, 72.0f);
		benchmark.mItemInterval = new Vector2(76.0f, 76.0f);
		benchmark.mViewportSize = new Vector2(800.0f, 900.0f);
		benchmark.mWindowSize = new Vector2(1180.0f, 1080.0f);
		benchmark.mScrollStep = 13.0f;
		benchmark.mStartDelay = 1.0f;
		benchmark.mWarmupTime = 0.25f;
		benchmark.mSampleTime = 0.75f;
		benchmark.mAutoBenchmark = true;
		benchmark.mFastEnableAdjacentSOAMerge = true;
		benchmark.mCppCompilerConfigurationTag = "Master";
		EditorUtility.SetDirty(benchmark);
		EditorSceneManager.MarkSceneDirty(scene);
		EditorSceneManager.SaveScene(scene, SCENE_PATH, false);
		Selection.activeGameObject = benchmarkObject;
	}
	protected static void createCamera()
	{
		GameObject cameraObject = new(CAMERA_OBJECT_NAME, typeof(Camera));
		cameraObject.tag = "MainCamera";
		Camera camera = cameraObject.GetComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 700.0f;
		camera.nearClipPlane = 0.01f;
		camera.farClipPlane = 100.0f;
		camera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = new Color(0.035f, 0.035f, 0.045f, 1.0f);
	}
	protected static void addSceneToBuildSettings(string scenePath)
	{
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		List<EditorBuildSettingsScene> newScenes = new(scenes.Length + 1)
		{
			new EditorBuildSettingsScene(scenePath, true),
		};
		for (int i = 0; i < scenes.Length; ++i)
		{
			if (scenes[i].path != scenePath)
			{
				newScenes.Add(scenes[i]);
			}
		}
		EditorBuildSettings.scenes = newScenes.ToArray();
	}
	protected static void removeSceneFromBuildSettings(string scenePath)
	{
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		List<EditorBuildSettingsScene> newScenes = new(scenes.Length);
		bool changed = false;
		for (int i = 0; i < scenes.Length; ++i)
		{
			if (scenes[i].path == scenePath)
			{
				changed = true;
				continue;
			}
			newScenes.Add(scenes[i]);
		}
		if (changed)
		{
			EditorBuildSettings.scenes = newScenes.ToArray();
		}
	}
}
