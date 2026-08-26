using Framework;
using Spine;
using Spine.Unity;
using UnityEngine;
using static SpineDynamicAnimation;
using static Spine40AnimationFileNameUtility;

public class Sample
{
    public static bool loadAnimation(SkeletonAnimation skeletonAnimation, string name)
    {
        if (skeletonAnimation == null)
        {
            return false;
        }
        if (hasAnimation(skeletonAnimation, name))
        {
            return true;
        }
        Spine40AnimationCommonData commonData = getCommonData(skeletonAnimation.Skeleton.Data);
        if (commonData == null)
        {
            string commonAssetName = getCommonAssetName(skeletonAnimation.skeletonDataAsset.name);
            var commonAsset = ResourcesManager.Instance.LoadAsset<TextAsset>(commonAssetName);
            if (commonAsset != null)
            {
                setCommonData(skeletonAnimation.Skeleton.Data, commonAsset.bytes);
            }
        }
        string assetName = getAnimationAssetName(skeletonAnimation.skeletonDataAsset.name, name);
        var asset = ResourcesManager.Instance.LoadAsset<TextAsset>(assetName, true, false);
        if (asset != null)
        {
            return addAnimation(skeletonAnimation, asset.bytes) != null;
        }
        return false;
    }
    public static bool loadAnimation(SkeletonGraphic skeletonGraphic, string name)
    {
        if (skeletonGraphic == null)
        {
            return false;
        }
        if (hasAnimation(skeletonGraphic, name))
        {
            return true;
        }
        Spine40AnimationCommonData commonData = getCommonData(skeletonGraphic.Skeleton.Data);
        if (commonData == null)
        {
            string commonAssetName = getCommonAssetName(skeletonGraphic.skeletonDataAsset.name);
            var commonAsset = ResourcesManager.Instance.LoadAsset<TextAsset>(commonAssetName);
            if (commonAsset != null)
            {
                setCommonData(skeletonGraphic.Skeleton.Data, commonAsset.bytes);
            }
        }

        string assetName = getAnimationAssetName(skeletonGraphic.skeletonDataAsset.name, name);
        var asset = ResourcesManager.Instance.LoadAsset<TextAsset>(assetName, true, false);
        if (asset != null)
        {
            return addAnimation(skeletonGraphic, asset.bytes) != null;
        }
        return false;
    }
    public static TrackEntry playSpineAnimation(SkeletonAnimation skeletonAnimation, string name, bool loop = true, int trackIndex = 0)
    {
        if (skeletonAnimation == null)
        {
            return null;
        }
        loadAnimation(skeletonAnimation, name);
        // 由于使用了拆分动画,所以就不能调用旧的AnimationState.SetAnimation,需要使用专用的方法
        return playAnimation(skeletonAnimation, trackIndex, name, loop);
    }
    public static TrackEntry playSpineAnimation(SkeletonGraphic skeletonGraphic, string name, bool loop = true, int trackIndex = 0)
    {
        if (skeletonGraphic == null)
        {
            return null;
        }
        loadAnimation(skeletonGraphic, name);
        // 由于使用了拆分动画,所以就不能调用旧的AnimationState.SetAnimation,需要使用专用的方法
        return playAnimation(skeletonGraphic, trackIndex, name, loop);
    }
}