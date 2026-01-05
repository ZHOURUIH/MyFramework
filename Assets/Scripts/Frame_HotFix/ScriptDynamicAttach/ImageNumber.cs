using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static FrameBaseUtility;
using static UnityUtility;
using static MathUtility;

// 使用图片来显示数字,暂时只支持TPAtlas,如果使用SpriteAtlas,纹理坐标会计算错误
public class ImageNumber : Image
{
	protected Dictionary<char, Sprite> mSpriteList = new(); // 图片列表,需要所有图片都是相同大小的
	protected List<UIVertex> mVertextCache = new();			// 顶点数据列表缓存
	protected List<int> mIndices = new();					// 缓存的顶点索引数据
	protected string mNumber;                               // 要显示的数字内容
	protected int mNumberWidth;								// 单个数字显示的宽度
	protected int mNumberHeight;                            // 单个数字显示的高度
	protected int mInterval;                                // 数字显示间隔
	protected DOCKING_POSITION mDockingPosition = DOCKING_POSITION.CENTER;            // 数字停靠方式
	protected bool mDirty;									// 是否需要刷新渲染数据
	public void setSpriteList(Dictionary<char, Sprite> spriteList)
	{
		mSpriteList.addRange(spriteList);
		Sprite firstSprite = mSpriteList.firstValue();
		int width = (int)firstSprite.rect.width;
		int height = (int)firstSprite.rect.height;
		if (isEditor())
		{
			foreach (Sprite sprite in mSpriteList.Values)
			{
				if ((int)sprite.rect.width != width || (int)sprite.rect.height != height)
				{
					logError("设置的数字图片大小不一致!spriteName:" + sprite.name);
				}
			}
		}
		// 根据RectTracsform的大小自动计算显示的宽度和高度
		if (rectTransform == null)
		{
			logError("找不到rectTransform");
		}
		mNumberHeight = (int)rectTransform.rect.height;
		mNumberWidth = (int)(divide(width, height) * mNumberHeight);
		setDirty();
	}
	public void clearNumber()
	{
		mNumber = null;
		setDirty();
	}
	public void setInterval(int interval) 
	{
		mInterval = interval;
		setDirty();
	}
	public void setDockingPosition(DOCKING_POSITION docking)
	{
		mDockingPosition = docking;
		setDirty();
	}
	public void setNumber(string number) 
	{
		mNumber = number;
		// 检查是否包含无法显示的数字
		if (isEditor() && !mNumber.isEmpty())
		{
			foreach (char c in mNumber)
			{
				if (!mSpriteList.ContainsKey(c))
				{
					logError("设置的数字内容无法显示:" + mNumber);
					return;
				}
			}
		}
		setDirty();
	}
	// 获得内容横向排列时的实际显示内容总宽度
	public int getContentWidth()
	{
		int count = mNumber.length();
		if (count == 0)
		{
			return 0;
		}
		return mNumberWidth * count + mInterval * (count - 1);
	}
	public int getInterval() { return mInterval; }
	public DOCKING_POSITION getDockingPosition() { return mDockingPosition; }
	public string getNumber() { return mNumber; }
	public Dictionary<char, Sprite> getSpriteList() { return mSpriteList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void setDirty()
	{
		mDirty = true;
		SetVerticesDirty();
	}
	protected void refreshPoints()
	{
		mVertextCache.Clear();
		mIndices.Clear();
		int numberLength = mNumber.length();
		if (numberLength == 0)
		{
			return;
		}
		Texture texture = mSpriteList.firstValue().texture;
		// 计算顶点,纹理坐标,颜色
		int verticeCount = numberLength << 2;
		mVertextCache.Capacity = verticeCount;
		mIndices.Capacity = (numberLength << 1) * 3;
		// mVertices
		//0---------1
		//|	       /|
		//|       / |
		//|      /  |
		//|     /   |
		//|    /    |
		//|   /     |
		//|  /      |
		//| /       |
		//|/        |
		//2---------3
		int halfTotalWidth = (numberLength * mNumberWidth + (numberLength - 1) * mInterval) >> 1;
		int halfHeight = mNumberHeight >> 1;
		float inverseTextureWidth = 1.0f / texture.width;
		float inverseTextureHeight = 1.0f / texture.height;
		for (int i = 0; i < numberLength; ++i)
		{
			Rect spriteRect = mSpriteList.get(mNumber[i]).rect;
			float posX0 = i * (mNumberWidth + mInterval) - halfTotalWidth;
			mVertextCache.add(new()
			{
				color = color,
				position = new(posX0, halfHeight, 0.0f),
				uv0 = new(spriteRect.x * inverseTextureWidth, (spriteRect.y + spriteRect.height) * inverseTextureHeight)
			});
			mVertextCache.add(new()
			{
				color = color,
				position = new(posX0 + mNumberWidth, halfHeight, 0.0f),
				uv0 = new((spriteRect.x + spriteRect.width) * inverseTextureWidth, (spriteRect.y + spriteRect.height) * inverseTextureHeight)
			});
			mVertextCache.add(new()
			{
				color = color,
				position = new(posX0, -halfHeight, 0.0f),
				uv0 = new(spriteRect.x * inverseTextureWidth, spriteRect.y * inverseTextureHeight)
			});
			mVertextCache.add(new()
			{
				color = color,
				position = new(posX0 + mNumberWidth, -halfHeight, 0.0f),
				uv0 = new((spriteRect.x + spriteRect.width) * inverseTextureWidth, spriteRect.y * inverseTextureHeight)
			});

			// 三角形顶点索引
			int indexValue = i << 2;
			mIndices.add(indexValue + 0);
			mIndices.add(indexValue + 1);
			mIndices.add(indexValue + 2);
			mIndices.add(indexValue + 1);
			mIndices.add(indexValue + 3);
			mIndices.add(indexValue + 2);
		}
		// 默认为居中停靠,所以只额外处理左停靠和右停靠
		if (mDockingPosition == DOCKING_POSITION.LEFT)
		{
			for (int i = 0; i < verticeCount; ++i)
			{
				UIVertex vert = mVertextCache[i];
				vert.position.x += halfTotalWidth;
				mVertextCache[i] = vert;
			}
		}
		else if (mDockingPosition == DOCKING_POSITION.RIGHT)
		{
			for (int i = 0; i < verticeCount; ++i)
			{
				UIVertex vert = mVertextCache[i];
				vert.position.x -= halfTotalWidth;
				mVertextCache[i] = vert;
			}
		}
	}
	// 此函数由UGUI自动调用
	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		// 编辑器下非play状态时,使用Image组件原始的渲染,其他时候使用实际的渲染逻辑
		if (isEditor() && !isPlaying())
		{
			base.OnPopulateMesh(toFill);
		}
		else
		{
			toFill.Clear();
			if (mDirty)
			{
				refreshPoints();
			}
			toFill.AddUIVertexStream(mVertextCache, mIndices);
		}
	}
}