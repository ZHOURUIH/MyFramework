using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FrameBaseUtility;
using static EditorCommonUtility;
using static MathUtility;
using static FileUtility;
using static StringUtility;
using UObject = UnityEngine.Object;

// 图集矩形放置策略,打包时会依次尝试所有策略，从中寻找能够容纳全部图片的最小POT图集
public enum PackHeuristic
{
	BestShortSideFit,       // 优先选择放入后短边剩余空间最小的位置
	BestAreaFit,            // 优先选择放入后剩余面积最小的位置
	BottomLeft,             // 优先选择位置更靠下、其次更靠左的位置
}

// 单个待打包PNG的数据,同时保存原始像素、打包后的图集区域以及生成MultiSprite时需要的切片信息
public class AtlasSprite
{
	public string name;         // Sprite名称，使用PNG文件名且不包含扩展名
	public RectInt rect;        // Sprite在图集中的区域，打包前仅使用width和height
	public Vector2 pivot;       // Sprite归一化轴心点
	public Vector4 border;      // Sprite九宫格边界，顺序为左、下、右、上
	public Color32[] pixels;    // PNG解码后的原始像素数据
}

// Unity编辑器MultiSprite图集生成工具。
// 在Project窗口中选中一个文件夹后，可通过右键菜单将该目录第一层的所有PNG打包为一张POT图集，
// 并自动把生成的PNG配置为Sprite Multiple以及写入对应的切片信息。
public class MultiSpriteAtlasGenerator
{
	private const string MENU_PATH = "Assets/生成MultiSprite图集";      // Project窗口右键菜单路径。
	private const int MAX_ATLAS_SIZE = 4096;                            // 图集允许的最大宽度和高度。
	private const int BORDER_PADDING = 1;                               // Sprite与图集边缘之间保留的像素间距。
	private const int SHAPE_PADDING = 1;                                // 相邻Sprite区域之间保留的像素间距。
	private const float SPRITE_PIXELS_PER_UNIT = 100.0f;                // 输出图集中所有Sprite统一使用的Pixels Per Unit。
	// 根据Project窗口当前选中的文件夹执行完整的图集生成流程,生成的PNG保存在选中文件夹外，文件名为“文件夹名.png”
	public static void generateMultiSprite(string folderAssetPath)
	{
		if (!AssetDatabase.IsValidFolder(folderAssetPath))
		{
			EditorUtility.DisplayDialog("生成MultiSprite图集", "请在Project窗口中选中一个文件夹。", "确定");
			return;
		}

		try
		{
			string outputAssetPath = folderAssetPath + ".png";
			displayProgressBar("生成MultiSprite图集", "读取文件夹中的PNG", 0.1f);
			List<AtlasSprite> sprites = loadSprites(folderAssetPath, outputAssetPath);
			if (sprites.Count == 0)
			{
				Debug.LogError("选中文件夹中没有可打包的PNG文件。");
			}

			displayProgressBar("生成MultiSprite图集", "计算图集布局", 0.55f);
			if (!findBestLayout(sprites, MAX_ATLAS_SIZE, BORDER_PADDING, SHAPE_PADDING, out int atlasWidth, out int atlasHeight))
			{
				Debug.LogError("所有Sprite无法放入单张POT图集，最大尺寸为" + MAX_ATLAS_SIZE + "x" + MAX_ATLAS_SIZE + "。");
			}

			displayProgressBar("生成MultiSprite图集", "写入图集PNG", 0.7f);
			writeAtlasPng(outputAssetPath, sprites, atlasWidth, atlasHeight);

			displayProgressBar("生成MultiSprite图集", "设置MultiSprite切割数据", 0.9f);
			configureAtlasImporter(outputAssetPath, sprites, atlasWidth, atlasHeight);

			UObject atlasAsset = loadMainAssetAtPath(outputAssetPath);
			Selection.activeObject = atlasAsset;
			EditorGUIUtility.PingObject(atlasAsset);
			Debug.Log("MultiSprite图集生成成功:" + outputAssetPath + "，Sprite数量:" + sprites.Count +
				"，图集尺寸:" + atlasWidth + "x" + atlasHeight, atlasAsset);
		}
		catch (Exception exception)
		{
			Debug.LogError("MultiSprite图集生成失败:" + exception);
		}
		finally
		{
			clearProgressBar();
		}
	}
	// 读取指定文件夹第一层中的全部PNG文件，并创建待打包的Sprite数据。
	// 源PNG不要求在Unity中导入为Sprite，也不要求开启Read/Write。
	// outputAssetPath用于排除上一次生成的图集，避免把输出文件再次打包进去。
	private static List<AtlasSprite> loadSprites(string folderAssetPath, string outputAssetPath)
	{
		string folderFilePath = projectPathToFullPath(folderAssetPath);
		List<string> filePaths = findFilesNonAlloc(folderFilePath, ".png", false);
		filePaths.Sort(StringComparer.OrdinalIgnoreCase);

		List<AtlasSprite> sprites = new();
		foreach (string filePath in filePaths)
		{
			string assetPath = fullPathToProjectPath(filePath);
			if (assetPath == outputAssetPath)
			{
				continue;
			}

			Texture2D texture = new(2, 2, TextureFormat.RGBA32, false, false);
			try
			{
				byte[] pngBytes = openFileSync(filePath, true);
				if (!texture.LoadImage(pngBytes, false))
				{
					Debug.LogError("PNG读取失败:" + assetPath);
				}
				if (texture.width <= 0 || texture.height <= 0)
				{
					Debug.LogError("PNG尺寸无效:" + assetPath);
				}

				AtlasSprite atlasSprite = new();
				atlasSprite.name = getFileNameNoSuffixNoDir(filePath);
				atlasSprite.rect.width = texture.width;
				atlasSprite.rect.height = texture.height;
				atlasSprite.pivot = new Vector2(0.5f, 0.5f);
				atlasSprite.border = Vector4.zero;
				atlasSprite.pixels = texture.GetPixels32();
				sprites.Add(atlasSprite);
			}
			finally
			{
				UObject.DestroyImmediate(texture);
			}
		}
		sprites.Sort(delegate (AtlasSprite left, AtlasSprite right)
		{
			return string.Compare(left.name, right.name, StringComparison.Ordinal);
		});
		return sprites;
	}
	// 在不超过maxSize的POT尺寸中寻找能够容纳全部Sprite的最小图集。
	// 会按照图集面积从小到大测试候选尺寸，并对每个尺寸尝试所有矩形放置策略。
	private static bool findBestLayout(List<AtlasSprite> sprites, int maxSize, int borderPadding, int shapePadding, out int atlasWidth, out int atlasHeight)
	{
		atlasWidth = 0;
		atlasHeight = 0;
		int maxPowerOfTwo = getGreaterPow2(maxSize);
		if (maxPowerOfTwo <= 0)
		{
			return false;
		}
		if (maxPowerOfTwo != maxSize)
		{
			maxPowerOfTwo /= 2;
		}

		int minimumWidth = 1;
		int minimumHeight = 1;
		long totalArea = 0;
		foreach (AtlasSprite sprite in sprites)
		{
			RectInt rect = sprite.rect;
			minimumWidth = getMax(minimumWidth, rect.width + borderPadding * 2);
			minimumHeight = getMax(minimumHeight, rect.height + borderPadding * 2);
			totalArea += (long)(rect.width + shapePadding) * (rect.height + shapePadding);
		}

		List<int> powerValues = new();
		for (int value = 1; value <= maxPowerOfTwo; value *= 2)
		{
			powerValues.Add(value);
			if (value > maxPowerOfTwo / 2)
			{
				break;
			}
		}

		List<Vector2Int> candidates = new();
		foreach (int width in powerValues)
		{
			if (width < minimumWidth)
			{
				continue;
			}
			foreach (int height in powerValues)
			{
				if (height < minimumHeight || (long)width * height < totalArea)
				{
					continue;
				}
				candidates.Add(new(width, height));
			}
		}

		candidates.Sort(delegate (Vector2Int left, Vector2Int right)
		{
			long leftArea = (long)left.x * left.y;
			long rightArea = (long)right.x * right.y;
			if (leftArea != rightArea)
			{
				return leftArea < rightArea ? -1 : 1;
			}
			int leftLongSide = getMax(left.x, left.y);
			int rightLongSide = getMax(right.x, right.y);
			if (leftLongSide != rightLongSide)
			{
				return leftLongSide.CompareTo(rightLongSide);
			}
			return left.x.CompareTo(right.x);
		});

		PackHeuristic[] heuristics =
		{
			PackHeuristic.BestShortSideFit,
			PackHeuristic.BestAreaFit,
			PackHeuristic.BottomLeft,
		};
		foreach (Vector2Int candidate in candidates)
		{
			foreach (PackHeuristic heuristic in heuristics)
			{
				if (!tryPack(sprites, candidate.x, candidate.y, borderPadding, shapePadding, heuristic))
				{
					continue;
				}
				atlasWidth = candidate.x;
				atlasHeight = candidate.y;
				return true;
			}
		}
		return false;
	}
	// 使用指定图集尺寸、边距和放置策略尝试打包全部Sprite。
	// 打包成功时会把每个Sprite在图集中的坐标写入其rect，失败时返回false。
	private static bool tryPack(List<AtlasSprite> sprites, int atlasWidth, int atlasHeight, int borderPadding, int shapePadding, PackHeuristic heuristic)
	{
		int virtualWidth = atlasWidth - borderPadding * 2 + shapePadding;
		int virtualHeight = atlasHeight - borderPadding * 2 + shapePadding;
		if (virtualWidth <= 0 || virtualHeight <= 0)
		{
			return false;
		}

		List<RectInt> freeRects = new();
		freeRects.Add(new(0, 0, virtualWidth, virtualHeight));
		List<int> unplaced = new();
		for (int i = 0; i < sprites.Count; ++i)
		{
			unplaced.Add(i);
			sprites[i].rect.x = -1;
			sprites[i].rect.y = -1;
		}

		while (unplaced.Count > 0)
		{
			int bestUnplacedIndex = -1;
			RectInt bestNode = new();
			long bestScore1 = long.MaxValue;
			long bestScore2 = long.MaxValue;
			for (int i = 0; i < unplaced.Count; ++i)
			{
				AtlasSprite sprite = sprites[unplaced[i]];
				if (!findPosition(freeRects, sprite.rect.width + shapePadding, sprite.rect.height + shapePadding, heuristic, out RectInt node, out long score1, out long score2))
				{
					continue;
				}

				if (bestUnplacedIndex < 0 ||
					score1 < bestScore1 ||
					(score1 == bestScore1 && score2 < bestScore2))
				{
					bestUnplacedIndex = i;
					bestNode = node;
					bestScore1 = score1;
					bestScore2 = score2;
				}
			}

			if (bestUnplacedIndex < 0)
			{
				return false;
			}

			int spriteIndex = unplaced[bestUnplacedIndex];
			sprites[spriteIndex].rect.x = bestNode.x + borderPadding;
			sprites[spriteIndex].rect.y = bestNode.y + borderPadding;
			// 使用已占用矩形切割当前所有空闲矩形，并删除切割后无效或被其他矩形包含的区域
			List<RectInt> splitRects = new(freeRects.Count * 2);
			foreach (RectInt freeRect in freeRects)
			{
				splitFreeRectangle(freeRect, bestNode, splitRects);
			}
			freeRects.Clear();
			freeRects.AddRange(splitRects);

			// 清理空闲矩形列表，删除宽高无效的矩形以及完全包含在其他空闲矩形中的冗余矩形
			for (int i = 0; i < freeRects.Count; ++i)
			{
				if (freeRects[i].width <= 0 || freeRects[i].height <= 0)
				{
					freeRects.RemoveAt(i--);
					continue;
				}
				for (int j = 0; j < freeRects.Count; ++j)
				{
					if (i != j && contains(freeRects[j], freeRects[i]))
					{
						freeRects.RemoveAt(i--);
						break;
					}
				}
			}
			unplaced.RemoveAt(bestUnplacedIndex);
		}
		return true;
	}
	// 从当前空闲矩形列表中查找能够放入指定宽高的最佳位置。
	// score1和score2是当前放置策略使用的主评分与次评分，数值越小表示位置越合适。
	private static bool findPosition(List<RectInt> freeRects, int width, int height, PackHeuristic heuristic, out RectInt result, out long score1, out long score2)
	{
		bool found = false;
		result = new RectInt();
		score1 = long.MaxValue;
		score2 = long.MaxValue;
		foreach (RectInt freeRect in freeRects)
		{
			if (width > freeRect.width || height > freeRect.height)
			{
				continue;
			}

			int leftWidth = freeRect.width - width;
			int leftHeight = freeRect.height - height;
			int shortSide = getMin(leftWidth, leftHeight);
			int longSide = getMax(leftWidth, leftHeight);
			long currentScore1;
			long currentScore2;
			switch (heuristic)
			{
				case PackHeuristic.BestAreaFit:
					currentScore1 = (long)freeRect.width * freeRect.height - (long)width * height;
					currentScore2 = shortSide;
					break;
				case PackHeuristic.BottomLeft:
					currentScore1 = (long)freeRect.y + height;
					currentScore2 = freeRect.x;
					break;
				default:
					currentScore1 = shortSide;
					currentScore2 = longSide;
					break;
			}

			if (!found ||
				currentScore1 < score1 ||
				(currentScore1 == score1 && currentScore2 < score2))
			{
				found = true;
				score1 = currentScore1;
				score2 = currentScore2;
				result = new RectInt(freeRect.x, freeRect.y, width, height);
			}
		}
		return found;
	}
	// 从一个空闲矩形中扣除已占用区域，把剩余的左、右、下、上区域加入结果列表。
	// 如果两个矩形不相交，则直接保留原空闲矩形。
	private static void splitFreeRectangle(RectInt freeRect, RectInt usedRect, List<RectInt> result)
	{
		if (!intersects(freeRect, usedRect))
		{
			result.Add(freeRect);
			return;
		}

		int freeRight = freeRect.x + freeRect.width;
		int freeTop = freeRect.y + freeRect.height;
		int usedRight = usedRect.x + usedRect.width;
		int usedTop = usedRect.y + usedRect.height;
		if (usedRect.x > freeRect.x && usedRect.x < freeRight)
		{
			result.Add(new RectInt(freeRect.x, freeRect.y, usedRect.x - freeRect.x, freeRect.height));
		}
		if (usedRight < freeRight && usedRight > freeRect.x)
		{
			result.Add(new RectInt(usedRight, freeRect.y, freeRight - usedRight, freeRect.height));
		}
		if (usedRect.y > freeRect.y && usedRect.y < freeTop)
		{
			result.Add(new RectInt(freeRect.x, freeRect.y, freeRect.width, usedRect.y - freeRect.y));
		}
		if (usedTop < freeTop && usedTop > freeRect.y)
		{
			result.Add(new RectInt(freeRect.x, usedTop, freeRect.width, freeTop - usedTop));
		}
	}
	// 判断两个矩形是否存在实际面积上的相交，只有边界接触不视为相交。
	private static bool intersects(RectInt left, RectInt right)
	{
		return left.x < right.x + right.width &&
			   left.x + left.width > right.x &&
			   left.y < right.y + right.height &&
			   left.y + left.height > right.y;
	}
	// 判断outer矩形是否完整包含inner矩形，边界重合也视为包含。
	private static bool contains(RectInt outer, RectInt inner)
	{
		return inner.x >= outer.x &&
			   inner.y >= outer.y &&
			   inner.x + inner.width <= outer.x + outer.width &&
			   inner.y + inner.height <= outer.y + outer.height;
	}
	// 根据计算完成的布局创建透明RGBA图集，把所有Sprite像素写入图集并保存为PNG。
	// 保存完成后强制Unity同步导入输出文件，供后续写入MultiSprite切片信息。
	private static void writeAtlasPng(string outputAssetPath, List<AtlasSprite> sprites, int atlasWidth, int atlasHeight)
	{
		Texture2D atlas = new(atlasWidth, atlasHeight, TextureFormat.RGBA32, false, false);
		try
		{
			atlas.SetPixels32(new Color32[atlasWidth * atlasHeight]);
			foreach (AtlasSprite sprite in sprites)
			{
				atlas.SetPixels32(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height, sprite.pixels);
			}
			atlas.Apply(false, false);
			byte[] pngBytes = atlas.EncodeToPNG();
			if (pngBytes.isEmpty())
			{
				Debug.LogError("图集PNG编码失败。");
			}
			writeFile(projectPathToFullPath(outputAssetPath), pngBytes);
		}
		finally
		{
			UObject.DestroyImmediate(atlas);
		}
		AssetDatabase.ImportAsset(outputAssetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
	}
	// 配置输出PNG的TextureImporter，将其设置为Sprite Multiple并写入所有Sprite切片数据。
	private static void configureAtlasImporter(string outputAssetPath, List<AtlasSprite> sprites, int atlasWidth, int atlasHeight)
	{
		var textureImporter = AssetImporter.GetAtPath(outputAssetPath) as TextureImporter;
		if (textureImporter == null)
		{
			Debug.LogError("无法获取输出图集的TextureImporter:" + outputAssetPath);
		}

		textureImporter.textureType = TextureImporterType.Sprite;
		textureImporter.spriteImportMode = SpriteImportMode.Multiple;
		textureImporter.spritePixelsPerUnit = SPRITE_PIXELS_PER_UNIT;
		textureImporter.alphaIsTransparency = true;
		textureImporter.mipmapEnabled = false;
		textureImporter.wrapMode = TextureWrapMode.Clamp;
		textureImporter.maxTextureSize = getMax(32, getMax(atlasWidth, atlasHeight));

		SpriteMetaData[] spriteMetaData = new SpriteMetaData[sprites.Count];
		for (int i = 0; i < sprites.Count; ++i)
		{
			AtlasSprite sprite = sprites[i];
			SpriteMetaData metadata = new();
			metadata.name = sprite.name;
			metadata.rect = new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height);
			metadata.pivot = sprite.pivot;
			metadata.alignment = (int)SpriteAlignment.Custom;
			metadata.border = sprite.border;
			spriteMetaData[i] = metadata;
		}
		textureImporter.spritesheet = spriteMetaData;
		textureImporter.SaveAndReimport();
	}
}