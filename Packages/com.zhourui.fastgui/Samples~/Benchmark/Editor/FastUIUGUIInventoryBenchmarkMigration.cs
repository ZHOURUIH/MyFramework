#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FastUIUGUIInventoryBenchmarkMigration
{
	private const string SCENE_PATH = "Assets/FastUIBenchmarkGenerated/Inventory/Scenes/FastUI_vs_UGUI_Inventory.unity";
	private const string LEGACY_ATLAS_ASSET_PATH = "Assets/FastUIBenchmarkGenerated/Inventory/Atlas/InventorySpriteAtlas.asset";
	static FastUIUGUIInventoryBenchmarkMigration()
	{
		EditorApplication.delayCall += ensureNormalSpriteInventory;
	}
	private static void ensureNormalSpriteInventory()
	{
		if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
		{
			return;
		}
		SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_PATH);
		FastUIInventoryBenchmarkAtlas atlas = FastUIUGUIInventoryBenchmarkGenerator.loadInventoryAtlasFromTexture();
		bool legacyAtlasExists = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(LEGACY_ATLAS_ASSET_PATH) != null;
		if (scene != null && !legacyAtlasExists && FastUIUGUIInventoryBenchmarkGenerator.isInventoryAtlasValid(atlas))
		{
			return;
		}
		var activeScene = EditorSceneManager.GetActiveScene();
		if (activeScene.isDirty && !string.IsNullOrEmpty(activeScene.path) && !activeScene.path.StartsWith("Assets/FastUIBenchmarkGenerated/"))
		{
			Debug.LogWarning("[FastGUI Benchmark] 主背包测试资源需要升级为正常MultiSprite Atlas，但当前非Benchmark场景有未保存修改。保存场景后执行 Tools/FastGUI Benchmark/Generate Inventory Benchmark 即可重新生成。");
			return;
		}
		Debug.Log("[FastGUI Benchmark] 检测到旧背包图集或旧测试场景，正在重建正常MultiSprite + FastImage/Image背包测试资源。");
		FastUIUGUIInventoryBenchmarkGenerator.generateInventoryBenchmark();
	}
}
#endif
