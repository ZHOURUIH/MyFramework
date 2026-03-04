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
	public Sprite mImage;                       // 这个只是用来允许从面板上拖拽一个Sprite进去
	public Material mMaterial;                  // 如果不设置材质,则使用默认的UI材质
	public CanvasRenderer mCanvasRenderer;      // CanvasRenderer
	public int mVertCount;						// 用于查看调试信息
	public int mDamageNumberCount;				// 用于查看调试信息

	protected DamageNumberSpriteData[] mSpriteList = new DamageNumberSpriteData[10];	// 图片列表,需要所有图片都是相同大小的,下标就是对应的数字
	protected List<DamageNumberData> mDamageItems = new(1024);
	protected int mNumberHeight;
	protected int mNumberWidth;
	protected float mInterval;
	protected DOCKING_POSITION mDocking = DOCKING_POSITION.CENTER;
	protected Mesh mMesh;
	protected const int mMaxNumberCount = 10000;		// 最多只支持同时显示1万个数字,因为单个模型的索引数量限制在65535
	protected DamageNumberVertex[] mVertices = new DamageNumberVertex[mMaxNumberCount * 4];
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
	public void setSpriteList(IList<DamageNumberSpriteData> spriteList)
	{
		if (spriteList.Count > mSpriteList.Length)
		{
			logError("图片的数量错误,不能超过" + IToS(mSpriteList.Length) + "个");
			return;
		}
		mSpriteList.setRange(spriteList);
		DamageNumberSpriteData firstSprite = mSpriteList.first();
		int width = firstSprite.mWidth;
		int height = firstSprite.mHeight;
		if (isEditor() && mSpriteList.contains(sprite => sprite.mWidth != width || sprite.mHeight != height))
		{
			logError("设置的数字图片大小不一致!spriteName:" + mImage.name);
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
	public DamageNumberSpriteData[] getSpriteList() { return mSpriteList; }
	// 添加伤害,pos是屏幕上的世界坐标
	public void addDamage(Vector3 pos, Vector3 scale, long number, float speed, Dictionary<float, Vector3> positionKeyFrames, Dictionary<float, Vector3> scaleKeyFrames)
	{
		DamageNumberData data = mDamageItems.addClass();
		splitNumber(number, LIST_PERSIST(out data.mNumbers));
		data.setPositionKeyframes(positionKeyFrames);
		data.setScaleKeyframes(scaleKeyFrames);
		data.mPositionOffset = pos;
		data.mScaleOffset = scale;
		data.mPosition = pos;
		data.mScale = scale;
		data.mCurTime = 0.0f;
		data.mSpeed = speed;
		data.mTotalWidth = data.mNumbers.Count * mNumberWidth + mInterval * (data.mNumbers.Count - 1);
		data.init();
		mDamageNumberCount = mDamageItems.Count;
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
	public List<DamageNumberData> getDamageItems() { return mDamageItems; }
	public DOCKING_POSITION getDocking() { return mDocking; }
	public void setDocking(DOCKING_POSITION docking) { mDocking = docking; }
	public void setInterval(float interval) { mInterval = interval; }
	public float getInterval() { return mInterval; }
	void LateUpdate()
	{
		int damageCount = mDamageItems.Count;
		{
			using var a = new ProfilerScope(0);
			float dt = Time.deltaTime;
			for (int i = 0; i < damageCount; ++i)
			{
				DamageNumberData item = mDamageItems[i];
				item.mCurTime += dt * item.mSpeed;
				if (item.mCurTime < item.mKeyFrameMaxTime)
				{
					DamageNumberMovement.updateDamage(item);
				}
				else
				{
					UN_CLASS(mDamageItems.removeAt(i--));
					--damageCount;
				}
			}
		}

		{
			using var a = new ProfilerScope(0);
			mVertCount = 0;
			float step = mNumberWidth + mInterval;
			for (int j = 0; j < damageCount; ++j)
			{
				DamageNumberData item = mDamageItems[j];
				float scaleX = item.mScale.x;
				float step0 = step * scaleX;
				float width = mNumberWidth * scaleX;
				float height = mNumberHeight * item.mScale.y;
				float startX = item.mPosition.x - item.mTotalWidth * scaleX * 0.5f;
				float itemBottomPosY = item.mPosition.y - height * 0.5f;
				int count = item.mNumbers.Count;
				for (int i = 0; i < count; ++i)
				{
					ref DamageNumberSpriteData spriteData = ref mSpriteList[item.mNumbers[i]];
					Vector2[] uvs = spriteData.mUVs;
					int tempVert = mVertCount + i * 4;
					float tempStartX = startX + i * step0;

					ref DamageNumberVertex v0 = ref mVertices[tempVert + 0];
					ref DamageNumberVertex v1 = ref mVertices[tempVert + 1];
					ref DamageNumberVertex v2 = ref mVertices[tempVert + 2];
					ref DamageNumberVertex v3 = ref mVertices[tempVert + 3];

					v0.mPositionX = tempStartX;
					v0.mPositionY = itemBottomPosY + height;
					v0.mUV = uvs[0];

					v1.mPositionX = tempStartX + width;
					v1.mPositionY = itemBottomPosY + height;
					v1.mUV = uvs[1];

					v2.mPositionX = tempStartX;
					v2.mPositionY = itemBottomPosY;
					v2.mUV = uvs[2];

					v3.mPositionX = tempStartX + width;
					v3.mPositionY = itemBottomPosY;
					v3.mUV = uvs[3];
				}
				startX += step0 * count * 4;
				mVertCount += count * 4;
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
}