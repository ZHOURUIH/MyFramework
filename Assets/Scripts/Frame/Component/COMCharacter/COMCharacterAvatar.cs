using System.Collections.Generic;
using UnityEngine;

public class COMCharacterAvatar : GameComponent
{
	protected Dictionary<string, float> mAnimationLengthList;   // 缓存动作时长的列表
	protected OnCharacterLoaded mCharacterLoadedCallback;       // 角色模型加载完毕的回调
	protected CreateObjectCallback mModelLoadCallback;          // 保存自身的回调函数,用于避免GC
	protected CharacterController mController;                  // 模型角色控制器组件,仅用于方便获取
	protected GameObject mObject;                               // 模型节点
	protected Transform mModelTransform;                        // 模型变换组件
	protected Animation mAnimation;                             // 模型动作组件,仅用于方便获取
	protected Character mCharacter;                             // 模型所属角色
	protected Rigidbody mRigidBody;                             // 模型刚体组件,仅用于方便获取
	protected Animator mAnimator;                               // 模型动画状态机组件,仅用于方便获取
	protected string mAnimatorControllerPath;                   // 角色动作状态机文件路径,用于在加载模型时使用其他路径的状态机
	protected string mModelPath;                                // 模型文件的相对路径,相对于GameResources
	protected object mUserData;                                 // 模型加载回调的自定义参数
	protected int mModelTag;                                    // 模型标签,用于资源池
	protected bool mDestroyReally;                              // 在销毁时是否真正销毁模型,如果不是真正销毁,则是缓存到资源池中
	protected AVATAR_RELATIONSHIP mRelationship;                // 模型节点与角色节点的关系
	protected TRANSFORM_SYNC_TIME mTransformSyncTime;           // 同步变换的时机
	protected TRANSFORM_ASYNC mPositionSync;                    // 位置同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_ASYNC mRotationSync;                    // 旋转同步方式,仅在mRelationship为AVATAR_ALONE时生效
	protected TRANSFORM_ASYNC mScaleSync;                       // 缩放同步方式,仅在mRelationship为AVATAR_ALONE时生效
	public COMCharacterAvatar()
	{
		mAnimationLengthList = new Dictionary<string, float>();
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.LATE_UPDATE;
		mPositionSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mModelLoadCallback = onModelLoaded;
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
		mModelTag = 0;
		mDestroyReally = false;
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mPositionSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mTransformSyncTime = TRANSFORM_SYNC_TIME.LATE_UPDATE;
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
		if (mAnimator == null || isEmpty(name))
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
	public void setPosition(Vector3 pos) { mModelTransform.localPosition = pos; }
	public void setRotation(Vector3 rot) { mModelTransform.localEulerAngles = rot; }
	public void setScale(Vector3 scale) { mModelTransform.localScale = scale; }
	public Vector3 getPosition() { return mModelTransform.localPosition; }
	public Vector3 getRotation() { return mModelTransform.localEulerAngles; }
	public Vector3 getScale() { return mModelTransform.localScale; }
	public CharacterController getCharacterController() { return mController; }
	public Animator getAnimator() { return mAnimator; }
	public Rigidbody getRigidBody() { return mRigidBody; }
	public Animation getAnimation() { return mAnimation; }
	public GameObject getModel() { return mObject; }
	public string getModelPath() { return mModelPath; }
	public bool isAvatarActive() { return mObject.activeSelf; }
	public void setModelPath(string modelPath) { mModelPath = modelPath; }
	public void setAvatarRelationship(AVATAR_RELATIONSHIP relationship) { mRelationship = relationship; }
	public void setTransformSync(TRANSFORM_ASYNC posSync, TRANSFORM_ASYNC rotSync, TRANSFORM_ASYNC scaleSync)
	{
		mPositionSync = posSync;
		mRotationSync = rotSync;
		mScaleSync = scaleSync;
	}
	public AVATAR_RELATIONSHIP getAvatarRelationship() { return mRelationship; }
	public TRANSFORM_ASYNC getPositionSync() { return mPositionSync; }
	public TRANSFORM_ASYNC getRotationSync() { return mRotationSync; }
	public TRANSFORM_ASYNC getScaleSync() { return mScaleSync; }
	public void setDestroyReally(bool destroyReally) { mDestroyReally = destroyReally; }
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
		mObjectPool.destroyObject(ref mObject, mDestroyReally);
		mModelTransform = null;
		mController = null;
		mAnimator = null;
		mAnimation = null;
		mModelPath = null;
		mRigidBody = null;
	}
	public void activeController(bool active) { mController.enabled = active; }
	//------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void syncTransform()
	{
		if (mObject == null)
		{
			return;
		}
		if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_ALONE)
		{
			// 角色节点与模型节点的同步
			if (mPositionSync == TRANSFORM_ASYNC.USE_AVATAR)
			{
				Vector3 pos = getPosition();
				if (!isVectorEqual(mCharacter.getPosition(), pos))
				{
					mCharacter.setPosition(pos);
				}
			}
			else if (mPositionSync == TRANSFORM_ASYNC.USE_CHARACTER)
			{
				Vector3 pos = mCharacter.getPosition();
				if (!isVectorEqual(getPosition(), pos))
				{
					setPosition(pos);
				}
			}
			if (mRotationSync == TRANSFORM_ASYNC.USE_AVATAR)
			{
				Vector3 rot = getRotation();
				if (!isVectorEqual(mCharacter.getRotation(), rot))
				{
					mCharacter.setRotation(rot);
				}
			}
			else if (mRotationSync == TRANSFORM_ASYNC.USE_CHARACTER)
			{
				Vector3 rot = mCharacter.getRotation();
				if (!isVectorEqual(getRotation(), rot))
				{
					setRotation(rot);
				}
			}
			if (mScaleSync == TRANSFORM_ASYNC.USE_AVATAR)
			{
				Vector3 scale = getScale();
				if (!isVectorEqual(mCharacter.getScale(), scale))
				{
					mCharacter.setScale(scale);
				}
			}
			else if (mScaleSync == TRANSFORM_ASYNC.USE_CHARACTER)
			{
				Vector3 scale = mCharacter.getScale();
				if (!isVectorEqual(getScale(), scale))
				{
					setScale(scale);
				}
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
			mCharacter.setObject(go, true);
			// 将外部节点设置为角色节点后,角色在销毁时就不能自动销毁节点,否则会出错
			mCharacter.setDestroyObject(false);
			mCharacter.setParent(mCharacterManager.getObject());
			FT.MOVE(mCharacter, lastPosition);
			FT.ROTATE(mCharacter, lastRotation);
			FT.SCALE(mCharacter, lastScale);
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
		mAnimator = mObject.GetComponent<Animator>();
		// 如果根节点找不到,则在第一级子节点中查找
		if (mAnimator == null)
		{
			int childCount = mModelTransform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				mAnimator = mModelTransform.GetChild(i).GetComponent<Animator>();
				if (mAnimator != null)
				{
					break;
				}
			}
		}
		mAnimation = mObject.GetComponent<Animation>();
		// 如果根节点找不到,则在第一级子节点中查找
		if (mAnimation == null)
		{
			int childCount = mModelTransform.childCount;
			for (int i = 0; i < childCount; ++i)
			{
				mAnimation = mModelTransform.GetChild(i).GetComponent<Animation>();
				if (mAnimation != null)
				{
					break;
				}
			}
		}
	}
}