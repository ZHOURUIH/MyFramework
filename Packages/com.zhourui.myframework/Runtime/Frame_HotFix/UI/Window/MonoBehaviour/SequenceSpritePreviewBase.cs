using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using static MathUtility;

// 预览一个Image或者SpriteRenderer的序列帧,不带位置偏移,不要直接将这个组件添加到GameObject上,应该需要添加派生出的子类
public abstract class SequenceSpritePreviewBase : MonoBehaviour
{
#if UNITY_EDITOR
    [Range(0, 1)]
    public float mSlider;
    [SerializeField] private Sprite[] mFrames;
    [SerializeField] private bool mLoop = true;
    [SerializeField] private float mFPS = 10.0f;
    [SerializeField] private bool mPreviewInEditor = true;
    private int mCurFrame;
    private float mTimer;
    private bool mPlaying;
    private double mLastEditorTime;
    public virtual void Awake()
    {
        enabled = !Application.isPlaying;
    }
    public virtual void Update()
    {
        if (Application.isPlaying || !mPreviewInEditor || this == null || !isActiveAndEnabled)
        {
            return;
        }

        double curTime = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(curTime - mLastEditorTime);
        mLastEditorTime = curTime;

        if (mPlaying && !mFrames.isEmpty())
        {
            float frameTime = 1.0f / Mathf.Max(1.0f, mFPS);
            mTimer += deltaTime;
            while (mTimer >= frameTime)
            {
                mTimer -= frameTime;
                ++mCurFrame;
                if (mCurFrame >= mFrames.Length)
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

                syncSliderByCurFrame();
                applyFrame(mCurFrame);
            }
        }
    }
    public void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        reloadFramesFromCurrentSprite();

        if (!mFrames.isEmpty())
        {
            refreshImage();
        }
    }
    public void Play()
    {
        if (mFrames.isEmpty())
        {
            reloadFramesFromCurrentSprite();
        }

        if (mFrames.isEmpty())
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
        if (mFrames.isEmpty())
        {
            reloadFramesFromCurrentSprite();
        }

        if (mFrames.isEmpty())
        {
            return;
        }

        mPlaying = true;
        mLastEditorTime = EditorApplication.timeSinceStartup;
    }
    public void EditorReloadFrames()
    {
        reloadFramesFromCurrentSprite();
        refreshImage();
    }
    public void EditorPreviousFrame()
    {
        if (mFrames.isEmpty())
        {
            reloadFramesFromCurrentSprite();
        }

        if (mFrames.isEmpty())
        {
            return;
        }

        mPlaying = false;
        mCurFrame = Mathf.Max(0, mCurFrame - 1);

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void EditorNextFrame()
    {
        if (mFrames.isEmpty())
        {
            reloadFramesFromCurrentSprite();
        }

        if (mFrames.isEmpty())
        {
            return;
        }

        mPlaying = false;
        mCurFrame = Mathf.Min(mFrames.Length - 1, mCurFrame + 1);

        syncSliderByCurFrame();
        applyFrame(mCurFrame);
    }
    public void EditorRefreshBySlider()
    {
        mPlaying = false;
        refreshImage();
    }
    public int EditorGetFrameCount()
    {
        return mFrames == null ? 0 : mFrames.Length;
    }
    public int EditorGetCurFrame()
    {
        return mCurFrame;
    }
    public bool EditorIsPlaying()
    {
        return mPlaying;
    }
    public static void setImage(Component component, Sprite sprite)
    {
        if (component == null)
        {
            return;
        }

        if (component is Image image)
        {
            image.sprite = sprite;
            EditorUtility.SetDirty(image);
        }
        else if (component is SpriteRenderer renderer)
        {
            renderer.sprite = sprite;
            EditorUtility.SetDirty(renderer);
        }
    }
    public static Sprite getImage(Component component)
    {
        if (component is Image image)
        {
            return image.sprite;
        }
        else if (component is SpriteRenderer renderer)
        {
            return renderer.sprite;
        }

        return null;
    }

    public static string getSpriteSetName(string spriteName)
    {
        if (spriteName.isEmpty())
        {
            return "";
        }

        int index = spriteName.LastIndexOf('_');
        if (index < 0)
        {
            return "";
        }

        return spriteName.startString(index);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected virtual Component getSpriteComponent() { return null; }
    protected void refreshImage()
    {
        if (mFrames.isEmpty())
        {
            reloadFramesFromCurrentSprite();
        }

        if (mFrames.isEmpty())
        {
            return;
        }

        int frameCount = mFrames.Length;
        int index = ceil(mSlider * frameCount) - 1;
        clamp(ref index, 0, frameCount - 1);

        mCurFrame = index;
        applyFrame(mCurFrame);
    }
    private void reloadFramesFromCurrentSprite()
    {
        Component image = getSpriteComponent();
        if (image == null)
        {
            return;
        }

        Sprite curSprite = getImage(image);
        if (curSprite == null)
        {
            return;
        }
        string path = AssetDatabase.GetAssetPath(curSprite.texture);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        string spriteSetName = getSpriteSetName(curSprite.name);
        if (spriteSetName.isEmpty())
        {
            mFrames = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .OrderBy(getSpriteFrameIndex)
                .ThenBy(s => s.name)
                .ToArray();
        }
        else
        {
            mFrames = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Sprite>()
                .Where(s => s.name.StartsWith(spriteSetName + "_"))
                .OrderBy(getSpriteFrameIndex)
                .ThenBy(s => s.name)
                .ToArray();
        }
        clamp(ref mCurFrame, 0, getMax(0, mFrames.Length - 1));
    }
    private void applyFrame(int index)
    {
        if (!TryGetComponent<ImageAtlasPath>(out _))
        {
            Debug.LogError("需要添加ImageAtlasPath组件, gameObject:" + name, this);
        }

        Component image = getSpriteComponent();
        if (image == null || mFrames.isEmpty())
        {
            return;
        }
        clamp(ref index, 0, mFrames.Length - 1);
        setImage(image, mFrames[index]);
        EditorUtility.SetDirty(this);
        SceneView.RepaintAll();
    }
    private void syncSliderByCurFrame()
    {
        if (mFrames.isEmpty() || mFrames.Length <= 1)
        {
            mSlider = 0.0f;
            return;
        }
        mSlider = mCurFrame / (float)(mFrames.Length - 1);
    }
    private static int getSpriteFrameIndex(Sprite sprite)
    {
        if (sprite == null || sprite.name.isEmpty())
        {
            return int.MaxValue;
        }

        string spriteName = sprite.name;
        int index = spriteName.LastIndexOf('_');
        if (index < 0 || index >= spriteName.Length - 1)
        {
            return int.MaxValue;
        }

        string indexString = spriteName.Substring(index + 1);
        if (int.TryParse(indexString, out int frameIndex))
        {
            return frameIndex;
        }
        return int.MaxValue;
    }
#endif
}