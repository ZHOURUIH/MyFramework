using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using static FrameDefine;
using static StringUtility;
using static MathUtility;
using static FileUtility;

[ExecuteAlways]
[RequireComponent(typeof(RawImageAnimPath))]
[RequireComponent(typeof(RawImage))]
public class SequenceRawImagePreview : MonoBehaviour
{
#if UNITY_EDITOR
    [Range(0, 1)]
    public float mSlider;
    [SerializeField] protected bool mLoop = true;
    [SerializeField] protected float mFPS = 10.0f;
    [SerializeField] protected bool mPreviewInEditor = true;
    protected RawImage mRawImage;
    protected RawImageAnimPath mAnimPath;
    protected List<Texture> mTextureList = new();
    protected int mCurFrame;
    protected float mTimer;
    protected bool mPlaying;
    protected double mLastEditorTime;
    public void Awake()
    {
        enabled = !Application.isPlaying;
        initComponent();
    }
    public void OnEnable()
    {
        initComponent();
        refresh();
        mLastEditorTime = EditorApplication.timeSinceStartup;
    }
    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        initComponent();
        refresh();

        if (mTextureList.Count > 0)
        {
            refreshImageBySlider();
        }
    }
    public void Update()
    {
        if (Application.isPlaying || !mPreviewInEditor || this == null || !isActiveAndEnabled)
        {
            return;
        }

        initComponent();

        double curTime = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(curTime - mLastEditorTime);
        mLastEditorTime = curTime;

        if (mPlaying && mTextureList.Count > 0)
        {
            float frameTime = 1.0f / Mathf.Max(1.0f, mFPS);
            mTimer += deltaTime;

            while (mTimer >= frameTime)
            {
                mTimer -= frameTime;
                ++mCurFrame;
                if (mCurFrame >= mTextureList.Count)
                {
                    if (mLoop)
                    {
                        mCurFrame = 0;
                    }
                    else
                    {
                        mCurFrame = mTextureList.Count - 1;
                        mPlaying = false;
                    }
                }
                syncSliderByCurFrame();
                applyFrame(mCurFrame);
            }
        }
    }
    public void Play()
    {
        initComponent();

        if (mTextureList.Count == 0)
        {
            refresh();
        }
        if (mTextureList.Count == 0)
        {
            return;
        }
        mPlaying = true;
        mCurFrame = 0;
        mTimer = 0.0f;
        mLastEditorTime = EditorApplication.timeSinceStartup;

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void Stop()
    {
        mPlaying = false;
        mCurFrame = 0;
        mTimer = 0.0f;

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void Pause()
    {
        mPlaying = false;
    }
    public void Resume()
    {
        if (mTextureList.Count == 0)
        {
            refresh();
        }

        if (mTextureList.Count == 0)
        {
            return;
        }

        mPlaying = true;
        mLastEditorTime = EditorApplication.timeSinceStartup;
    }
    public void EditorRefresh()
    {
        initComponent();
        refresh();
        refreshImageBySlider();
    }
    public void EditorPreviousFrame()
    {
        if (mTextureList.Count == 0)
        {
            refresh();
        }

        if (mTextureList.Count == 0)
        {
            return;
        }

        mPlaying = false;

        --mCurFrame;
        clamp(ref mCurFrame, 0, mTextureList.Count - 1);

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void EditorNextFrame()
    {
        if (mTextureList.Count == 0)
        {
            refresh();
        }

        if (mTextureList.Count == 0)
        {
            return;
        }

        mPlaying = false;

        ++mCurFrame;
        clamp(ref mCurFrame, 0, mTextureList.Count - 1);

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void EditorRefreshBySlider()
    {
        mPlaying = false;
        refreshImageBySlider();
    }
    public int EditorGetFrameCount()
    {
        return mTextureList.Count;
    }
    public int EditorGetCurFrame()
    {
        return mCurFrame;
    }
    public bool EditorIsPlaying()
    {
        return mPlaying;
    }
    //---------------------------------------------------------------------------------------------------------------------------
    protected void initComponent()
    {
        if (mRawImage == null)
        {
            mRawImage = GetComponentInChildren<RawImage>();
        }
        if (mAnimPath == null)
        {
            mAnimPath = GetComponent<RawImageAnimPath>();
        }
    }
    protected void refresh()
    {
        mTextureList.Clear();

        if (mAnimPath == null)
        {
            Debug.LogError("需要添加RawImageAnimPath组件以后才能预览", this);
            return;
        }
        if (mRawImage == null)
        {
            Debug.LogError("需要添加RawImage组件以后才能预览", this);
            return;
        }
        if (mRawImage.texture == null && mAnimPath.mTexturePath.isEmpty())
        {
            return;
        }

        for (int i = 0; i < 1000; ++i)
        {
            string fileName = F_GAME_RESOURCES_PATH + mAnimPath.mTexturePath + "/" + mAnimPath.mTextureName + "_" + IToS(i) + ".png";
            if (!isFileExist(fileName))
            {
                break;
            }
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(fullPathToProjectPath(fileName));
            if (texture == null)
            {
                break;
            }
            mTextureList.add(texture);
        }
        clamp(ref mCurFrame, 0, Mathf.Max(0, mTextureList.Count - 1));
    }
    protected void refreshImageBySlider()
    {
        if (mTextureList.Count == 0)
        {
            refresh();
        }

        int frameCount = mTextureList.Count;
        if (frameCount <= 0)
        {
            return;
        }
        mCurFrame = ceil(mSlider * frameCount) - 1;
        clamp(ref mCurFrame, 0, frameCount - 1);
        applyFrame(mCurFrame);
    }
    protected void applyFrame(int index)
    {
        if (mRawImage == null)
        {
            initComponent();
        }
        if (mRawImage == null || mTextureList.Count == 0)
        {
            return;
        }
        clamp(ref index, 0, mTextureList.Count - 1);
        mRawImage.texture = mTextureList[index];

        EditorUtility.SetDirty(mRawImage);
        EditorUtility.SetDirty(this);
        SceneView.RepaintAll();
    }
    protected void syncSliderByCurFrame()
    {
        if (mTextureList.Count <= 1)
        {
            mSlider = 0.0f;
            return;
        }
        mSlider = mCurFrame / (float)(mTextureList.Count - 1);
    }
#endif
}