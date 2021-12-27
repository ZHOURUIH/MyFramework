using System.Collections.Generic;
using UnityEngine;

// 制作角色的动画状态机时需要注意
// 当动作为非循环动作时,需要勾选CanTransitionToSelf,且添加一个Dirty参数,类型为Trigger
// 当动作为循环动作时,则不勾选CanTransitionToSelf,也不需要添加Dirty
// 每个动作都有对应的Param参数,用于跳转动画
public class COMCharacterAvatar : GameComponent
{
	protected Dictionary<string, float> mAnimationLengthList;	// 缓存动作时长的列表
	protected OnCharacterLoaded mCharacterLoadedCallback;		// 角色模型加载完毕的回调
	protected CreateObjectCallback mModelLoadCallback;			// 保存自身的回调函数,用于避免GC
	protected CharacterController mController;					// 模型角色控制器组件,仅用于方便获取
	protected GameObject mObject;								// 模型节点
	protected Transform mModelTransform;						// 模型变换组件
	protected Animation mAnimation;								// 模型动作组件,仅用于方便获取
	protected Character mCharacter;								// 模型所属角色
	protected Rigidbody mRigidBody;								// 模型刚体组件,仅用于方便获取
	protected Animator mAnimator;								// 模型动画状态机组件,仅用于方便获取
	protected string mAnimatorControllerPath;					// 角色动作状态机文件路径,用于在加载模型时使用其他路径的状态机
	protected string mModelPath;								// 模型文件的相对路径,相对于GameResources
	protected object mUserData;									// 模型加载回调的自定义参数
	protected float[] mAnimationSpeed;							// 当前动作的播放速度,数组的每一个元素对应每一个动画层
	protected int[] mAnimationParam;							// 当前的播放动作的参数,对应了一个动作,数组的每一个元素对应每一个动画层
	protected int mDefaultLayer;								// 初始的层
	protected int mModelTag;									// 模型标签,用于资源池
	protected bool mDestroyReally;								// 在销毁时是否真正销毁模型,如果不是真正销毁,则是缓存到资源池中
	protected AVATAR_RELATIONSHIP mRelationship;				// 模型节点与角色节点的关系
	protected TRANSFORM_SYNC_TIME mTransformSyncTime;			// 同步变换的时机
	protected TRANSFORM_SYNC mPositionSync;						// 位置同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_SYNC mRotationSync;						// 旋转同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_SYNC mScaleSync;						// 缩放同步方式,仅在mRelationship为AVATAR_ALONE时生效
	public COMCharacterAvatar()
	{
		mAnimationLengthList = new Dictionary<string, float>();
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.LATE_UPDATE;
		mPositionSync = TRANSFORM_SYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_SYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_SYNC.USE_CHARACTER;
		mModelLoadCallback = onModelLoaded;
		mAnimationSpeed = new float[FrameDefine.ANIMATION_LAYER_COUNT] { 1.0f, 1.0f};
		mAnimationParam = new int[FrameDefine.ANIMATION_LAYER_COUNT];
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
		mModelTransform = null;
		mAnimator = null;
		mAnimation = null;
		mObject = null;
		mModelPath = null;
		mController = null;
		mRigidBody = null;
		mCharacterLoadedCallback = null;
		mAnimatorControllerPath = null;
		mUserData = null;
		mCharacter = null;
		mModelTag = 0;
		mDestroyReally = false;
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mPositionSync = TRANSFORM_SYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_SYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_SYNC.USE_CHARACTER;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.LATE_UPDATE;
		memset(mAnimationSpeed, 1.0f);
		memset(mAnimationParam, 0);
		mDefaultLayer = 0;
		// mModelLoadCallback不重置
		// mModelLoadCallback = null;
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
		if (mAnimator == null || mAnimator.runtimeAnimatorController == null || isEmpty(name))
		{
			return 0.0f;
		}
		if (mAnimationLengthList.TryGetValue(name, out float length))
		{
			return length;
		}
		length = getAnimationLength(mAnimator, name);
		mAnimationLengthList.Add(name, length);
		return length;
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
		if (mAnimator != null)
		{
			mAnimator.speed = mAnimationSpeed[layer];
			mAnimator.SetInteger(FrameDefine.ANIMATOR_STATE[layer], mAnimationParam[layer]);
			mAnimator.SetTrigger(FrameDefine.ANIMATOR_DIRTY[layer]);
		}
	}
	// 异步加载模型
	public void loadModelAsync(string modelPath, OnCharacterLoaded callback = null, object userData = null, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimatorControllerPath = animationControllerPath;
		mCharacterLoadedCallback = callback;
		mUserData = userData;
		if (!isEmpty(mModelPath))
		{
			mObjectPool.createObjectAsync(mModelPath, mModelLoadCallback, mModelTag);
		}
	}
	// 同步加载模型
	public void loadModel(string modelPath, string animationControllerPath = null)
	{
		mModelPath = modelPath;
		mAnimatorControllerPath = animationControllerPath;
		mCharacterLoadedCallback = null;
		mUserData = null;
		if (!isEmpty(mModelPath))
		{
			onModelLoaded(mObjectPool.createObject(mModelPath, mModelTag), null);
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
		mAnimator.updateMode = ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
	}
	public override void setActive(bool active)
	{
		base.setActive(active);
		mObject?.SetActive(active);
	}
	public void destroyModel()
	{
		// 需要重置对模型可能做出的所有修改
		resetLayer();
		setRootMotion(false);
		mObjectPool.destroyObject(ref mObject, mDestroyReally);
		// 如果模型节点是角色节点,则需要将角色节点也清空
		if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_AS_CHARACTER)
		{
			mCharacter.setObject(null);
		}
		mModelTransform = null;
		mController = null;
		mAnimator = null;
		mAnimation = null;
		mModelPath = null;
		mRigidBody = null;
		// 动作也要清空一下
		memset(mAnimationSpeed, 1.0f);
		memset(mAnimationParam, 0);
	}
	public void setRootMotion(bool applyRootMotion)
	{
		if (mAnimator != null)
		{
			mAnimator.applyRootMotion = applyRootMotion;
		}
	}
	public void resetLayer()											{ setGameObjectLayer(mObject, mDefaultLayer); }
	public void setLayer(int layer)										{ setGameObjectLayer(mObject, layer); }
	public void setModelTag(int tag)									{ mModelTag = tag; }
	public void activeController(bool active)							{ mController.enabled = active; }
	public void setAvatarRelationship(AVATAR_RELATIONSHIP relationship)	{ mRelationship = relationship; }
	public void setDestroyReally(bool destroyReally)					{ mDestroyReally = destroyReally; }
	public void setPosition(Vector3 pos)								{ mModelTransform.localPosition = pos; }
	public void setRotation(Vector3 rot)								{ mModelTransform.localEulerAngles = rot; }
	public void setScale(Vector3 scale)									{ mModelTransform.localScale = scale; }
	public Vector3 getPosition()										{ return mModelTransform.localPosition; }
	public Vector3 getRotation()										{ return mModelTransform.localEulerAngles; }
	public Vector3 getScale()											{ return mModelTransform.localScale; }
	public CharacterController getCharacterController()					{ return mController; }
	public Animator getAnimator()										{ return mAnimator; }
	public Rigidbody getRigidBody()										{ return mRigidBody; }
	public Animation getAnimation()										{ return mAnimation; }
	public GameObject getModel()										{ return mObject; }
	public string getModelPath()										{ return mModelPath; }
	public bool isAvatarActive()										{ return mObject.activeSelf; }
	public AVATAR_RELATIONSHIP getAvatarRelationship()					{ return mRelationship; }
	public TRANSFORM_SYNC getPositionSync()							{ return mPositionSync; }
	public TRANSFORM_SYNC getRotationSync()							{ return mRotationSync; }
	public TRANSFORM_SYNC getScaleSync()								{ return mScaleSync; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void syncTransform()
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
			Vector3 rot = getRotation();
			if (!isVectorEqual(mCharacter.getRotation(), rot))
			{
				mCharacter.setRotation(rot);
			}
		}
		else if (mRotationSync == TRANSFORM_SYNC.USE_CHARACTER)
		{
			Vector3 rot = mCharacter.getRotation();
			if (!isVectorEqual(getRotation(), rot))
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
	protected void onModelLoaded(GameObject go, object userData)
	{
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
				mGameObjectPool.destroyObject(mCharacter.getObject());
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
		if (!isEmpty(mAnimatorControllerPath))
		{
			mAnimator.runtimeAnimatorController = mResourceManager.loadResource<RuntimeAnimatorController>(mAnimatorControllerPath);
		}
		mCharacter.notifyModelLoaded();

		mCharacterLoadedCallback?.Invoke(mCharacter, mUserData);
		mCharacterLoadedCallback = null;
		mUserData = null;
	}
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
		mObject.SetActive(mActive);
		mController = mObject.GetComponent<CharacterController>();
		mModelTransform = mObject.GetComponent<Transform>();
		mRigidBody = mObject.GetComponent<Rigidbody>();
		mAnimator = mObject.GetComponentInChildren<Animator>();
		mAnimation = mObject.GetComponentInChildren<Animation>();
		mDefaultLayer = mObject.layer;
		// 模型加载完毕时,如果已经需要播放动作了,则播放指定的动作
		for(int i = 0; i < mAnimationParam.Length; ++i)
		{
			playAnimation(mAnimationParam[i], i, mAnimationSpeed[i]);
		}
	}
}