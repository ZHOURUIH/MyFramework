using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static FrameDefine;
using static EditorDefine;
using static FileUtility;
using static StringUtility;
using static EditorCommonUtility;

public class AssetsImport : AssetPostprocessor
{
	//所有的资源的导入，删除，移动，都会调用此方法，注意，这个方法是static的
	public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		// 检查是否有文件名带中文的空格
		foreach (string fullPath in findFilesNonAlloc(F_GAME_RESOURCES_PATH))
		{
			if (fullPath.Contains('　'))
			{
				Debug.LogError("文件名带中文的空格,可能会引起打包报错,file:" + fullPath);
			}
		}

		// 检查是否有场景目录中的文件夹名与场景文件名相同
		List<string> tempList = new();
		foreach (string sceneFullPath in findFilesNonAlloc(F_GAME_RESOURCES_PATH, ".unity"))
		{
			tempList.Clear();
			findFolders(getFilePath(sceneFullPath), tempList, null, false);
			if (tempList.Contains(removeSuffix(sceneFullPath)))
			{
				Debug.LogError("场景文件所在目录中有文件夹与场景重名,会导致打包失败,场景:" + sceneFullPath, loadAsset(fullPathToProjectPath(sceneFullPath)));
			}
		}
	}
	// 图片的导入
	public void OnPostprocessTexture(Texture2D texture)
	{
		var textureImporter = assetImporter as TextureImporter;
		// 是否启用mipmaps
#if !PROJECT_2D
		bool needMipmaps = true;
		foreach (string path in getNoMipmapsPath())
		{
			if (textureImporter.assetPath.StartsWith(path))
			{
				needMipmaps = false;
				break;
			}
		}
		textureImporter.mipmapEnabled = needMipmaps;
#else
		textureImporter.mipmapEnabled = false;
#endif
	}
	// 导入音频,由编辑器自动在导入音频资源时调用
	public void OnPostprocessAudio(AudioClip clip)
	{
#if !UNITY_WEBGL
		var audioImporter = assetImporter as AudioImporter;
		AudioImporterSampleSettings settings = new();
		// 根据音频时长设置属性
		// 音频时间长度小于 1 秒
		if (clip.length < 1)
		{
			audioImporter.loadInBackground = false;
			settings.loadType = AudioClipLoadType.DecompressOnLoad;
			settings.compressionFormat = AudioCompressionFormat.ADPCM;
		}
		// 音频时间长度大于 1 秒 ，小于 3 秒
		else if (clip.length < 3)
		{
			audioImporter.loadInBackground = false;
			settings.loadType = AudioClipLoadType.CompressedInMemory;
			settings.compressionFormat = AudioCompressionFormat.ADPCM;
		}
		// 音频时间长度大于 3 秒 
		else
		{
			audioImporter.loadInBackground = true;
			settings.loadType = AudioClipLoadType.Streaming;
			settings.compressionFormat = AudioCompressionFormat.Vorbis;
			settings.quality = 0.7f;
		}
		settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
		settings.sampleRateOverride = 22050;
		settings.preloadAudioData = false;
		// 进行设置
		audioImporter.defaultSampleSettings = settings;
		audioImporter.forceToMono = true;
#endif
	}
	// 导入模型前调用
	public void OnPreprocessModel()
	{
		var modelImporter = assetImporter as ModelImporter;
		modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
		modelImporter.sortHierarchyByName = true;
		modelImporter.importCameras = false;
		modelImporter.importLights = false;
		modelImporter.importBlendShapes = false;
		modelImporter.importVisibility = false;
		modelImporter.preserveHierarchy = true;
		modelImporter.meshCompression = ModelImporterMeshCompression.High;
		modelImporter.weldVertices = true;
		modelImporter.importBlendShapeNormals = ModelImporterNormals.None;
	}
}