using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static BinaryUtility;
using static MathUtility;
using static FrameDefine;

// 制作角色的动画状态机时需要注意
// 当动作为非循环动作时,需要勾选CanTransitionToSelf,且添加一个Dirty参数,类型为Trigger
// 当动作为循环动作时,则不勾选CanTransitionToSelf,也不需要添加Dirty
// 每个动作都有对应的Param参数,用于跳转动画
public class COMCharacterAvatar : GameComponent
{
	protected Dictionary<string, float> mAnimationLengthList = new();	// 缓存动作时长的列表
	protected CharacterCallback mCharacterLoadedCallback;				// 角色模型加载完毕的回调
	protected CharacterController mController;							// 模型角色控制器组件,仅用于方便获取
	protected GameObject mObject;										// 模型节点
	protected Transform mModelTransform;								// 模型变换组件
	protected Character mCharacter;										// 模型所属角色
	protected Rigidbody mRigidBody;										// 模型刚体组件,仅用于方便获取
	protected Animator mAnimator;										// 模型动画状态机组件,仅用于方便获取
	protected string mAnimatorControllerPath;							// 角色动作状态机文件路径,用于在加载模型时使用其他路径的状态机
	protected string mModelPath;										// 模型文件的相对路径,相对于GameResources
	protected float[] mAnimationSpeed;									// 当前动作的播放速度,数组的每一个元素对应每一个动画层
	protected int[] mAnimationParam;									// 当前的播放动作的参数,对应了一个动作,数组的每一个元素对应每一个动画层
	protected bool[] mLayerParamDirty;									// 动画层参数是否有修改未应用
	protected int mDefaultLayer;										// 初始的层
	protected int mModelTag;											// 模型标签,用于资源池
	protected bool mDestroyReally;										// 在销毁时是否真正销毁模型,如果不是真正销毁,则是缓存到资源池中
	protected AVATAR_RELATIONSHIP mRelationship;						// 模型节点与角色节点的关系
	protected TRANSFORM_SYNC_TIME mTransformSyncTime;					// 同步变换的时机
	protected TRANSFORM_SYNC mPositionSync;								// 位置同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_SYNC mRotationSync;								// 旋转同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_SYNC mScaleSync;								// 缩放同步方式,仅在mRelationship为AVATAR_ALONE时生效
	public COMCharacterAvatar()
	{
		mAnimationSpeed = new float[ANIMATION_LAYER_COUNT] { 1.0f, 1.0f};
		mAnimationParam = new int[ANIMATION_LAYER_COUNT];
		mLayerParamDirty = new bool[ANIMATION_LAYER_COUNT];
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.UPDATE;
		mPositionSync = TRANSFORM_SYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_SYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_SYNC.USE_CHARACTER;
	}
	public override void destroy()
	{
		destroyModel();
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mAnimationLengthList.Clear();
		mCharacterLoadedCallback = null;
		mController = null;
		mObject = null;
		mModelTransform = null;
		mCharacter = null;
		mRigidBody = null;
		mAnimator = null;
		mAnimatorControllerPath = null;
		mModelPath = null;
		memset(mAnimationSpeed, 1.0f);
		memset(mAnimationParam, 0);
		memset(mLayerParamDirty, false);
		mDefaultLayer = 0;
		mModelTag = 0;
		mDestroyReally = false;
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.UPDATE;
		mPositionSync = TRANSFORM_SYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_SYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_SYNC.USE_CHARACTER;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mCharacter = mComponentOwner as Character;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mTransformSyncTime == TRANSFORM_SYNC_TIME.UPDATE)
		{
			syncTransform();
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		if (mTransformSyncTime == TRANSFORM_SYNC_TIME.LATE_UPDATE)
		{
			syncTransform();
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		if (mTransformSyncTime == TRANSFORM_SYNC_TIME.FIXED_UPDATE)
		{
			syncTransform();
		}
	}
	public float getAnimationLength(string name)
	{
		if (mAnimator == null || mAnimator.runtimeAnimatorController == null || name.isEmpty())
		{
			return 0.0f;
		}
		if (mAnimationLengthList.TryGetValue(name, out float length))
		{
			return length;
		}
		return mAnimationLengthList.add(name, UnityUtility.getAnimationLength(mAnimator, name));
	}
	// 播放动作
	public void playAnimation(int anim, float speed = 1.0f)
	{
		playAnimation(anim, 0, speed);
	}
	// 播放动作
	public void playAnimation(int anim, int layer, float speed = 1.0f)
	{
		mAnimationParam[layer] = anim;
		mAnimationSpeed[layer] = speed;
		mLayerParamDirty[layer] = mAnimator == null;
		if (mAnimator != null)
		{
			mAnimator.speed = mAnimationSpeed[layer];
			mAnimator.SetInteger(ANIMATOR_STATE[layer], mAnimationParam[layer]);
			mAnimator.SetTrigger(ANIMATOR_DIRTY[layer]);
		}
	}
	public void addLoadedCallback(CharacterCallback callback)
	{
		if (mObject != null)
		{
			callback?.Invoke(mCharacter);
		}
		else
		{
			mCharacterLoadedCallback += callback;
		}
	}
	// 异步加载模型
	public CustomAsyncOperation loadModelAsync(string modelPath, CharacterCallback callback = null, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimatorControllerPath = animationControllerPath;
		mCharacterLoadedCallback += callback;
		if (mModelPath.isEmpty())
		{
			return null;
		}
		return mPrefabPoolManager.createObjectAsyncSafe(this, mModelPath, mModelTag, true, true, (GameObject go) =>
		{
			onModelLoaded(go);
		});
	}
	// 同步加载模型
	public void loadModel(string modelPath, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimatorControllerPath = animationControllerPath;
		mCharacterLoadedCallback = null;
		if (!mModelPath.isEmpty())
		{
			onModelLoaded(mPrefabPoolManager.createObject(mModelPath, mModelTag, true, true));
		}
	}
	public void setTransformSync(TRANSFORM_SYNC posSync, TRANSFORM_SYNC rotSync, TRANSFORM_SYNC scaleSync)
	{
		mPositionSync = posSync;
		mRotationSync = rotSync;
		mScaleSync = scaleSync;
	}
	public override void setIgnoreTimeScale(bool ignore)
	{
		base.setIgnoreTimeScale(ignore);
		if (mAnimator != null)
		{
			mAnimator.updateMode = ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
		}
	}
	public override void setActive(bool active)
	{
		if (mObject != null && mObject.activeSelf != active)
		{
			mObject.SetActive(active);
		}
		base.setActive(active);
	}
	public virtual void destroyModel()
	{
		if (mObject == null)
		{
			return;
		}
		// 需要重置对模型可能做出的所有修改
		resetLayer();
		mPrefabPoolManager.destroyObject(ref mObject, mDestroyReally);
		// 如果模型节点是角色节点,则需要将角色节点也清空
		if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_AS_CHARACTER)
		{
			mCharacter.setObject(null);
		}
		mModelTransform = null;
		mController = null;
		mAnimator = null;
		mModelPath = null;
		mRigidBody = null;
		// 动作也要清空一下
		memset(mAnimationSpeed, 1.0f);
		memset(mAnimationParam, 0);
	}
	public void syncTransform()
	{
		if (mObject == null)
		{
			return;
		}
		if (mRelationship != AVATAR_RELATIONSHIP.AVATAR_ALONE)
		{
			return;
		}
		// 角色节点与模型节点的同步
		if (mPositionSync == TRANSFORM_SYNC.USE_AVATAR)
		{
			Vector3 pos = getPosition();
			if (!isVectorEqual(mCharacter.getPosition(), pos))
			{
				mCharacter.setPosition(pos);
			}
		}
		else if (mPositionSync == TRANSFORM_SYNC.USE_CHARACTER)
		{
			Vector3 pos = mCharacter.getPosition();
			if (!isVectorEqual(getPosition(), pos))
			{
				setPosition(pos);
			}
		}
		if (mRotationSync == TRANSFORM_SYNC.USE_AVATAR)
		{
			Quaternion rot = getRotationQuaternion();
			if (!isQuaternionEqual(mCharacter.getRotationQuaternion(), rot))
			{
				mCharacter.setRotation(rot);
			}
		}
		else if (mRotationSync == TRANSFORM_SYNC.USE_CHARACTER)
		{
			Quaternion rot = mCharacter.getRotationQuaternion();
			if (!isQuaternionEqual(getRotationQuaternion(), rot))
			{
				setRotation(rot);
			}
		}
		if (mScaleSync == TRANSFORM_SYNC.USE_AVATAR)
		{
			Vector3 scale = getScale();
			if (!isVectorEqual(mCharacter.getScale(), scale))
			{
				mCharacter.setScale(scale);
			}
		}
		else if (mScaleSync == TRANSFORM_SYNC.USE_CHARACTER)
		{
			Vector3 scale = mCharacter.getScale();
			if (!isVectorEqual(getScale(), scale))
			{
				setScale(scale);
			}
		}
	}
	public void setRootMotion(bool applyRootMotion)
	{
		if (mAnimator != null)
		{
			mAnimator.applyRootMotion = applyRootMotion;
		}
	}
	public void setModelParent(GameObject parent)
	{
		if (mRelationship != AVATAR_RELATIONSHIP.AVATAR_ALONE)
		{
			return;
		}
		if (mObject.transform.parent != parent.transform)
		{
			mObject.transform.SetParent(parent.transform);
		}
	}
	public void resetLayer()											{ setGameObjectLayer(mObject, mDefaultLayer); }
	public void setLayer(int layer)										{ setGameObjectLayer(mObject, layer); }
	public void setModelTag(int tag)									{ mModelTag = tag; }
	public void activeController(bool active)							{ mController.enabled = active; }
	public void setAvatarRelationship(AVATAR_RELATIONSHIP relationship)	{ mRelationship = relationship; }
	public void setDestroyReally(bool destroyReally)					{ mDestroyReally = destroyReally; }
	public virtual void setPosition(Vector3 pos)						{ mModelTransform.localPosition = pos; }
	public void setRotation(Vector3 rot)								{ mModelTransform.localEulerAngles = rot; }
	public void setRotation(Quaternion rot)								{ mModelTransform.localRotation = rot; }
	public void setScale(Vector3 scale)									{ mModelTransform.localScale = scale; }
	public void setTransformSyncTime(TRANSFORM_SYNC_TIME syncTime)		{ mTransformSyncTime = syncTime; }
	public Vector3 getPosition()										{ return mModelTransform.localPosition; }
	public Vector3 getRotation()										{ return mModelTransform.localEulerAngles; }
	public Quaternion getRotationQuaternion()							{ return mModelTransform.localRotation; }
	public Vector3 getScale()											{ return mModelTransform.localScale; }
	public CharacterController getCharacterController()					{ return mController; }
	public Animator getAnimator()										{ return mAnimator; }
	public Rigidbody getRigidBody()										{ return mRigidBody; }
	public GameObject getModel()										{ return mObject; }
	public string getModelPath()										{ return mModelPath; }
	public bool isAvatarActive()										{ return mObject.activeSelf; }
	public AVATAR_RELATIONSHIP getAvatarRelationship()					{ return mRelationship; }
	public TRANSFORM_SYNC_TIME getTransformSyncTime()					{ return mTransformSyncTime; }
	public TRANSFORM_SYNC getPositionSync()								{ return mPositionSync; }
	public TRANSFORM_SYNC getRotationSync()								{ return mRotationSync; }
	public TRANSFORM_SYNC getScaleSync()								{ return mScaleSync; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onModelLoaded(GameObject go)
	{
		if (mCharacter.isDestroy())
		{
			mPrefabPoolManager.destroyObject(ref go, false);
			return;
		}
		Vector3 lastPosition = mCharacter.getPosition();
		Vector3 lastRotation = mCharacter.getRotation();
		Vector3 lastScale = mCharacter.getScale();
		// 先销毁旧的模型
		destroyModel();

		// 将模型节点作为角色节点
		if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_AS_CHARACTER)
		{
			// 设置角色节点之前,先确认当前的角色节点已经销毁
			// 因为角色节点默认都是从GameObjectPool中创建的,所以此处的销毁方式也使用GameObjectPool
			if (mCharacter.getObject() != null)
			{
				mGameObjectPool.destroyObject(mCharacter.getObject(), true);
				mCharacter.setObject(null);
			}
			mCharacter.setObject(go);
			setNormalProperty(go, mCharacterManager.getObject(), null, lastScale, lastRotation, lastPosition);
		}
		// 将模型节点挂接在角色节点下
		else if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_AS_CHILD)
		{
			setNormalProperty(go, mCharacter.getObject());
		}
		else
		{
			Transform transform = go.transform;
			transform.localPosition = lastPosition;
			transform.localEulerAngles = lastRotation;
			transform.localScale = lastScale;
		}
		setModel(go);
		if (mAnimator != null && !mAnimatorControllerPath.isEmpty())
		{
			mAnimator.runtimeAnimatorController = mResourceManager.loadGameResource<RuntimeAnimatorController>(mAnimatorControllerPath);
		}
		// 回调顺序是先通知组件的子类,再通知所属角色,最后执行异步加载的回调
		postModelLoaded();
		mCharacter.notifyModelLoaded();

		mCharacterLoadedCallback?.Invoke(mCharacter);
		mCharacterLoadedCallback = null;
	}
	protected virtual void postModelLoaded() { }
	protected void setModel(GameObject model)
	{
		if (mObject != null)
		{
			logError("model is not null! can not set again!");
			return;
		}
		mObject = model;
		if (mObject == null)
		{
			return;
		}
		if (mObject.activeSelf != mActive)
		{
			mObject.SetActive(mActive);
		}
		mObject.TryGetComponent(out mController);
		mObject.TryGetComponent(out mModelTransform);
		mObject.TryGetComponent(out mRigidBody);
		mAnimator = mObject.GetComponentInChildren<Animator>();
		mDefaultLayer = mObject.layer;
		// 模型加载完毕时,如果已经需要播放动作了,则播放指定的动作
		for(int i = 0; i < mLayerParamDirty.Length; ++i)
		{
			if (mLayerParamDirty[i])
			{
				playAnimation(mAnimationParam[i], i, mAnimationSpeed[i]);
			}
		}
	}
}