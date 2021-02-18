using UnityEngine;

public class CharacterComponentModel : GameComponent
{
	protected Transform mModelTransform;
	protected Animator mAnimator;
	protected Animation mAnimation;
	protected GameObject mModel;
	protected string mModelPath;
	protected bool mDestroyReally;
	public override void destroy()
	{
		destroyModel();
		base.destroy();
	}
	public void setModel(GameObject model, string modelPath)
	{
		if(mModel != null)
		{
			logError("model is not null! can not set again!");
			return;
		}
		mModelPath = modelPath;
		mModel = model;
		if (mModel == null)
		{
			return;
		}
		mModel.SetActive(mActive);
		mModelTransform = mModel.GetComponent<Transform>();
		mAnimator = mModel.GetComponent<Animator>();
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
		mAnimation = mModel.GetComponent<Animation>();
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
	public GameObject getModel(){return mModel;}
	public string getModelPath(){return mModelPath;}
	public void setDestroyReally(bool destroyReally) { mDestroyReally = destroyReally; }
	public override void setIgnoreTimeScale(bool ignore) 
	{
		base.setIgnoreTimeScale(ignore);
		mAnimator.updateMode = ignore ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
	}
	public override void setActive(bool active)
	{
		base.setActive(active);
		mModel?.SetActive(active);
	}
	public void destroyModel()
	{
		mObjectPool.destroyObject(ref mModel, mDestroyReally);
		mModelTransform = null;
		mAnimator = null;
		mAnimation = null;
		mModelPath = null;
	}
}