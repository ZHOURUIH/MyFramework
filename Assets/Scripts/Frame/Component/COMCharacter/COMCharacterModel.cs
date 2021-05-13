using UnityEngine;

public class COMCharacterModel : GameComponent
{
	protected GameObject mObject;
	protected Transform mModelTransform;
	protected Animation mAnimation;
	protected Animator mAnimator;
	protected string mModelPath;
	protected bool mDestroyReally;
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
		mDestroyReally = false;
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
	public Animator getAnimator() { return mAnimator; }
	public Animation getAnimation() { return mAnimation; }
	public GameObject getModel() { return mObject; }
	public string getModelPath() { return mModelPath; }
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
		mAnimator = null;
		mAnimation = null;
		mModelPath = null;
	}
}