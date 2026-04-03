using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using static FrameBaseUtility;
using static FrameUtility;
using static StringUtility;
using static UnityUtility;
using static MathUtility;

// 专用于显示大量伤害数字的组件,比ImageNumber和单纯使用Image来显示伤害数字的效率高很多
// 所有伤害数字使用同一个组件实例来渲染
// 使用图片来显示数字,暂时只支持TPAtlas,如果使用SpriteAtlas,纹理坐标会计算错误
[RequireComponent(typeof(CanvasRenderer))]
public class DamageNumberRenderer : MonoBehaviour
{
	public Sprite mImage;                       // 这个只是用来允许从面板上拖拽一个Sprite进去,用来确定图集
	public Material mMaterial;                  // 如果不设置材质,则使用默认的UI材质
	public CanvasRenderer mCanvasRenderer;      // CanvasRenderer
	public int mVertCount;						// 用于查看调试信息
	public int mDamageNumberCount;              // 用于查看调试信息

	protected SpriteData[] mNumberSpriteList = new SpriteData[10];	// 图片列表,需要所有图片都是相同大小的,下标就是对应的数字
	protected List<DamageNumberData> mDamageItems = new(1024);
	protected int mNumberHeight;
	protected int mNumberWidth;
	protected float mInterval;
	protected DOCKING_POSITION mDocking = DOCKING_POSITION.CENTER;
	protected Mesh mMesh;
	protected const int mMaxNumberCount = 10000;		// 最多只支持同时显示1万个数字,因为单个模型的索引数量限制在65535
	protected SpriteVertex[] mVertices = new SpriteVertex[mMaxNumberCount * 4];
	protected int[] mIndices = new int[mMaxNumberCount * 6];
	void Awake()
	{
		mCanvasRenderer = GetComponent<CanvasRenderer>();
		mMesh = new Mesh();
		mMesh.MarkDynamic();
		mCanvasRenderer.SetMaterial(mMaterial != null ? mMaterial : Canvas.GetDefaultCanvasMaterial(), null);
		// 由于索引都是相对固定的,所以可以预先计算好
		for (int i = 0; i < mIndices.Length / 6; ++i)
		{
			mIndices[i * 6 + 0] = i * 4 + 0;
			mIndices[i * 6 + 1] = i * 4 + 1;
			mIndices[i * 6 + 2] = i * 4 + 2;
			mIndices[i * 6 + 3] = i * 4 + 1;
			mIndices[i * 6 + 4] = i * 4 + 3;
			mIndices[i * 6 + 5] = i * 4 + 2;
		}
	}
	public void setNumberSpriteList(SpriteData[] spriteList)
	{
		using var a = new ListScope<SpriteData>(out var list, spriteList);
		setNumberSpriteList(list);
	}
	public void setNumberSpriteList(List<SpriteData> spriteList)
	{
		if (spriteList.Count > mNumberSpriteList.Length)
		{
			logError("图片的数量错误,不能超过" + IToS(mNumberSpriteList.Length) + "个");
			return;
		}
		mNumberSpriteList.setRange(spriteList);
		SpriteData firstSprite = mNumberSpriteList.first();
		int width = firstSprite.mWidth;
		int height = firstSprite.mHeight;
		if (isEditor() && mNumberSpriteList.find(sprite => sprite.mWidth != width || sprite.mHeight != height, out SpriteData sprite))
		{
			logError("设置的数字图片大小不一致!spriteName:" + mImage.name + ", sprite.width:" + sprite.mWidth + ", sprite.height:" + sprite.mHeight);
		}
		// 根据RectTransform的大小自动计算显示的宽度和高度
		if (transform as RectTransform == null)
		{
			logError("找不到RectTransform");
		}
		mNumberHeight = (int)(transform as RectTransform).rect.height;
		mNumberWidth = (int)(divide(width, height) * mNumberHeight);
		mCanvasRenderer.SetTexture(firstSprite.mTexture);
	}
	public SpriteData[] getSpriteList() { return mNumberSpriteList; }
	public void addDamageNumber(DamageNumberData damage)
	{
		mDamageItems.add(damage);
	}
	public void setDamageList(List<DamageNumberData> damageItems)
	{
		UN_CLASS_LIST(mDamageItems);
		mDamageItems.setRange(damageItems);
	}
	public void clearNumber()
	{
		UN_CLASS_LIST(mDamageItems);
	}
	public void cloneDamageList(List<DamageNumberData> damageItems)
	{
		UN_CLASS_LIST(mDamageItems);
		damageItems.For(item => item.cloneTo(mDamageItems.addClass()));
	}
	public float getNumberWidth() { return mNumberWidth; }
	public float getNumberHeight() { return mNumberHeight; }
	public List<DamageNumberData> getDamageItems() { return mDamageItems; }
	public DOCKING_POSITION getDocking() { return mDocking; }
	public void setDocking(DOCKING_POSITION docking) { mDocking = docking; }
	public void setInterval(float interval) { mInterval = interval; }
	public float getInterval() { return mInterval; }
	void LateUpdate()
	{
		mDamageNumberCount = mDamageItems.Count;
		{
			using var a = new ProfilerScope(0);
			float dt = Time.deltaTime;
			for (int i = 0; i < mDamageNumberCount; ++i)
			{
				DamageNumberData item = mDamageItems[i];
				item.mCurTime += dt * item.mSpeed;
				if (item.mCurTime < item.mKeyFrameMaxTime)
				{
					updateDamage(item);
				}
				else
				{
					UN_CLASS(mDamageItems.removeAt(i--));
					--mDamageNumberCount;
				}
			}
		}

		{
			using var a = new ProfilerScope(0);
			mVertCount = 0;
			float step = mNumberWidth + mInterval;
			for (int j = 0; j < mDamageNumberCount; ++j)
			{
				DamageNumberData item = mDamageItems[j];
				float scaleX = item.mScale.x;
				float scaleY = item.mScale.y;
				float posX = item.mPosition.x;
				float posY = item.mPosition.y;
				// 显示标记,要避免标记挡住数字的情况,所以都是在数字背后显示
				int flagCount = item.mExtraFlags.count();
				int count = item.mNumbers.count();
				if (mVertCount + ((flagCount + count) << 2) >= mVertices.Length)
				{
					logWarning("已经超出了顶点上限,最多只允许" + IToS(mVertices.Length) + "个顶点");
					return;
				}
				for (int i = 0; i < flagCount; ++i)
				{
					DamageNumberFlag flag = item.mExtraFlags[i];
					Vector2[] uvs = flag.mUVs;
					float width = flag.mSpriteWidth * scaleX;
					float height = flag.mSpriteHeight * scaleY;
					float leftX = posX + flag.mOffsetX * scaleX - width * 0.5f;
					float bottomY = posY + flag.mOffsetY * scaleY - height * 0.5f;

					int tempVert = mVertCount + (i << 2);
					ref SpriteVertex v0 = ref mVertices[tempVert + 0];
					ref SpriteVertex v1 = ref mVertices[tempVert + 1];
					ref SpriteVertex v2 = ref mVertices[tempVert + 2];
					ref SpriteVertex v3 = ref mVertices[tempVert + 3];

					v0.mPositionX = leftX;
					v0.mPositionY = bottomY + height;
					v0.mUV = uvs[0];

					v1.mPositionX = leftX + width;
					v1.mPositionY = bottomY + height;
					v1.mUV = uvs[1];

					v2.mPositionX = leftX;
					v2.mPositionY = bottomY;
					v2.mUV = uvs[2];

					v3.mPositionX = leftX + width;
					v3.mPositionY = bottomY;
					v3.mUV = uvs[3];
				}
				mVertCount += flagCount << 2;

				// 显示数字
				float step0 = step * scaleX;
				float numberWidth = mNumberWidth * scaleX;
				float numberHeight = mNumberHeight * scaleY;
				float startX = posX - item.mTotalWidth * scaleX * 0.5f;
				float itemBottomPosY = posY - numberHeight * 0.5f;
				for (int i = 0; i < count; ++i)
				{
					ref SpriteData spriteData = ref mNumberSpriteList[item.mNumbers[i]];
					Vector2[] uvs = spriteData.mUVs;
					int tempVert = mVertCount + (i << 2);
					float tempStartX = startX + i * step0;

					ref SpriteVertex v0 = ref mVertices[tempVert + 0];
					ref SpriteVertex v1 = ref mVertices[tempVert + 1];
					ref SpriteVertex v2 = ref mVertices[tempVert + 2];
					ref SpriteVertex v3 = ref mVertices[tempVert + 3];

					v0.mPositionX = tempStartX;
					v0.mPositionY = itemBottomPosY + numberHeight;
					v0.mUV = uvs[0];

					v1.mPositionX = tempStartX + numberWidth;
					v1.mPositionY = itemBottomPosY + numberHeight;
					v1.mUV = uvs[1];

					v2.mPositionX = tempStartX;
					v2.mPositionY = itemBottomPosY;
					v2.mUV = uvs[2];

					v3.mPositionX = tempStartX + numberWidth;
					v3.mPositionY = itemBottomPosY;
					v3.mUV = uvs[3];
				}
				mVertCount += count << 2;
			}
		}

		if (mMesh.vertexCount != mVertCount)
		{
			mMesh.Clear();
			mMesh.SetVertexBufferParams(mVertCount,
				new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
				new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
		}
		mMesh.SetIndices(mIndices, 0, mVertCount / 4 * 6, MeshTopology.Triangles, 0, false);
		mMesh.SetVertexBufferData(mVertices, 0, 0, mVertCount, 0,
			MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers);
		mCanvasRenderer.SetMesh(mMesh);
	}
	protected static void updateDamage(DamageNumberData data)
	{
		// 根据当前的时间找出位于哪两个点之间
		float curTime = data.mCurTime;

		// 计算位置
		if (!data.mPositionKeyFrames.isEmpty())
		{
			float[] posTimeList = data.mPositionTimeList;
			Vector3[] posList = data.mPositionList;
			int index0 = findValueIndex(posTimeList, curTime, data.mLastPositionKeyIndex);
			data.mLastPositionKeyIndex = index0;
			Vector3 startValue0 = posList[index0];
			if (index0 < data.mPositionKeyFrames.Count - 1)
			{
				startValue0 = lerpSimple(startValue0, posList[index0 + 1], inverseLerp(posTimeList[index0], posTimeList[index0 + 1], curTime));
			}
			data.mPosition = startValue0 + data.mPositionOffset;
		}

		// 计算缩放
		if (!data.mScaleKeyFrames.isEmpty())
		{
			float[] scaleTimeList = data.mScaleTimeList;
			Vector3[] scaleList = data.mScaleList;
			int index1 = findValueIndex(scaleTimeList, curTime, data.mLastScaleKeyIndex);
			data.mLastScaleKeyIndex = index1;
			Vector3 startValue1 = scaleList[index1];
			if (index1 < data.mScaleKeyFrames.Count - 1)
			{
				startValue1 = lerpSimple(startValue1, scaleList[index1 + 1], inverseLerp(scaleTimeList[index1], scaleTimeList[index1 + 1], curTime));
			}
			data.mScale = multiVector3(startValue1, data.mScaleOffset);
		}
	}
	// 快速地找到当前位于哪两个时间点之间,因为时间是单调递增的,所以比正常的查找要简化一些
	protected static int findValueIndex(float[] timeList, float curTime, int startIndex)
	{
		if (timeList[startIndex] > curTime)
		{
			return clampMin(startIndex - 1);
		}
		int count = timeList.Length;
		for (int i = startIndex + 1; i < count; ++i)
		{
			if (timeList[i] > curTime)
			{
				return i - 1;
			}
		}
		return count - 1;
	}
}