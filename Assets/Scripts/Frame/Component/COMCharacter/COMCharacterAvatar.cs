using UnityEngine;

public class COMCharacterAvatar : GameComponent
{
	protected CharacterController mController;
	protected GameObject mObject;
	protected Transform mModelTransform;
	protected Animation mAnimation;
	protected Character mCharacter;
	protected Animator mAnimator;
	protected string mModelPath;
	protected bool mDestroyReally;
	protected AVATAR_RELATIONSHIP mRelationship;
	protected TRANSFORM_ASYNC mPositionSync;
	protected TRANSFORM_ASYNC mRotationSync;
	protected TRANSFORM_ASYNC mScaleSync;
	public COMCharacterAvatar()
	{
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mPositionSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_ASYNC.USE_CHARACTER;
	}
	public override void destroy()
	{
		destroyModel();
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mModelTransform = null;
		mAnimator = null;
		mAnimation = null;
		mObject = null;
		mModelPath = null;
		mController = null;
		mDestroyReally = false;
		mRelationship = AVATAR_RELATIONSHIP.AVATAR_AS_CHILD;
		mPositionSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mRotationSync = TRANSFORM_ASYNC.USE_CHARACTER;
		mScaleSync = TRANSFORM_ASYNC.USE_CHARACTER;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mCharacter = mComponentOwner as Character;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mObject == null)
		{
			return;
		}
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
	public void notifyModelLoaded(GameObject go, string animationControllerPath)
	{
		// 将模型节点作为角色节点
		if (mRelationship == AVATAR_RELATIONSHIP.AVATAR_AS_CHARACTER)
		{
			Vector3 lastPosition = getPosition();
			Vector3 lastRotation = getRotation();
			Vector3 lastScale = getScale();
			mCharacter.setObject(go, true);
			// 将外部节点设置为角色节点后,角色在销毁时就不能自动销毁节点,否则会出错
			mCharacter.setDestroyObject(false);
			mCharacter.setParent(mCharacterManager.getObject());
			FT.MOVE(mCharacter, lastPosition);
			FT.ROTATE(mCharacter, lastRotation);
			FT.SCALE(mCharacter, lastScale);
		}
		// 将模型节点挂接在角色节点下
		else if (getAvatarRelationship() == AVATAR_RELATIONSHIP.AVATAR_AS_CHILD)
		{
			setNormalProperty(go, mObject);
		}

		setModel(go, mModelPath);
		if (!isEmpty(animationControllerPath))
		{
			mAnimator.runtimeAnimatorController = mResourceManager.loadResource<RuntimeAnimatorController>(animationControllerPath);
		}
	}
	public void setModel(GameObject model, string modelPath)
	{
		if (mObject != null)
		{
			logError("model is not null! can not set again!");
			return;
		}
		mModelPath = modelPath;
		mObject = model;
		if (mObject == null)
		{
			return;
		}
		mObject.SetActive(mActive);
		mController = mObject.GetComponent<CharacterController>();
		mModelTransform = mObject.GetComponent<Transform>();
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
	public void setPosition(Vector3 pos) { mModelTransform.localPosition = pos; }
	public void setRotation(Vector3 rot) { mModelTransform.localEulerAngles = rot; }
	public void setScale(Vector3 scale) { mModelTransform.localScale = scale; }
	public Vector3 getPosition() { return mModelTransform.localPosition; }
	public Vector3 getRotation() { return mModelTransform.localEulerAngles; }
	public Vector3 getScale() { return mModelTransform.localScale; }
	public CharacterController getCharacterController() { return mController; }
	public Animator getAnimator() { return mAnimator; }
	public Animation getAnimation() { return mAnimation; }
	public GameObject getModel() { return mObject; }
	public string getModelPath() { return mModelPath; }
	public void setAvatarRelationship(AVATAR_RELATIONSHIP relationship) { mRelationship = relationship; }
	public void setPositionSync(TRANSFORM_ASYNC sync) { mPositionSync = sync; }
	public void setRotationSync(TRANSFORM_ASYNC sync) { mRotationSync = sync; }
	public void setScaleSync(TRANSFORM_ASYNC sync) { mScaleSync = sync; }
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
	}
}