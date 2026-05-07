using UnityEngine;
using UnityEngine.UI;
using System.Linq;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class ResSpriteSequence : MonoBehaviour
{
	[SerializeField] private Sprite[] mFrames;
	[SerializeField] private bool mPlayOnEnable = true;
	[SerializeField] private bool mLoop = true;
	[SerializeField] private float mFPS = 10.0f;
	[SerializeField] private bool mPreviewInEditor = true;
	private Image mImage;
	private int mCurFrame;
	private float mTimer;
	private bool mPlaying;
#if UNITY_EDITOR
	private double mLastEditorTime;
#endif
	private void Awake()
	{
		mImage = GetComponent<Image>();
	}
	private void OnEnable()
	{
		if (mPlayOnEnable)
		{
			Play();
		}

#if UNITY_EDITOR
		mLastEditorTime = EditorApplication.timeSinceStartup;
		EditorApplication.update -= EditorUpdate;
		EditorApplication.update += EditorUpdate;
#endif
	}
	private void OnDisable()
	{
#if UNITY_EDITOR
		EditorApplication.update -= EditorUpdate;
#endif
	}
	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Tick(Time.deltaTime);
	}
#if UNITY_EDITOR
	private void EditorUpdate()
	{
		if (Application.isPlaying || !mPreviewInEditor || this == null || !isActiveAndEnabled)
		{
			return;
		}
		double curTime = EditorApplication.timeSinceStartup;
		float deltaTime = (float)(curTime - mLastEditorTime);
		mLastEditorTime = curTime;
		Tick(deltaTime);
		if (mPlaying)
		{
			InternalEditorUtility.RepaintAllViews();
		}
	}
#endif
	private void Tick(float deltaTime)
	{
		if (!mPlaying || mFrames == null || mFrames.Length == 0)
		{
			return;
		}

		float frameTime = 1.0f / Mathf.Max(1.0f, mFPS);
		mTimer += deltaTime;
		while (mTimer >= frameTime)
		{
			mTimer -= frameTime;
			if (++mCurFrame >= mFrames.Length)
			{
				if (mLoop)
				{
					mCurFrame = 0;
				}
				else
				{
					mCurFrame = mFrames.Length - 1;
					mPlaying = false;
				}
			}
			ApplyFrame();
		}
	}
	public void Play()
	{
		if (mFrames == null || mFrames.Length == 0)
		{
			return;
		}
		mPlaying = true;
		mCurFrame = 0;
		mTimer = 0.0f;
		ApplyFrame();
#if UNITY_EDITOR
		mLastEditorTime = EditorApplication.timeSinceStartup;
#endif
	}
	public void Stop()
	{
		mPlaying = false;
	}
	public void Pause()
	{
		mPlaying = false;
	}
	public void Resume()
	{
		if (mFrames != null && mFrames.Length > 0)
		{
			mPlaying = true;
#if UNITY_EDITOR
			mLastEditorTime = EditorApplication.timeSinceStartup;
#endif
		}
	}
	protected void ApplyFrame()
	{
		if (mImage != null && mCurFrame >= 0 && mCurFrame < mFrames.Length)
		{
			mImage.sprite = mFrames[mCurFrame];
		}
	}
#if UNITY_EDITOR
	protected void OnValidate()
	{
		mImage = GetComponent<Image>();
		if (mImage == null || mImage.sprite == null)
		{
			return;
		}
		string path = AssetDatabase.GetAssetPath(mImage.sprite.texture);
		if (string.IsNullOrEmpty(path))
		{
			return;
		}
		mFrames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToArray();
		if (mFrames.Length > 0)
		{
			mImage.sprite = mFrames[0];
		}
	}
#endif
}