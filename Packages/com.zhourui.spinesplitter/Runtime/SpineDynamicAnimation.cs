#if SPINE_RUNTIME_43 || SPINE_RUNTIME_42 || SPINE_RUNTIME_41 || SPINE_RUNTIME_40
using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Debug = UnityEngine.Debug;
#if SPINE_RUNTIME_43
using SpineAnimationFileVersion = Spine43AnimationFile;
using SpineAnimationCommonDataVersion = Spine43AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine43SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine43AnimationBinaryReader;
#elif SPINE_RUNTIME_42
using SpineAnimationFileVersion = Spine42AnimationFile;
using SpineAnimationCommonDataVersion = Spine42AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine42SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine42AnimationBinaryReader;
#elif SPINE_RUNTIME_41
using SpineAnimationFileVersion = Spine41AnimationFile;
using SpineAnimationCommonDataVersion = Spine41AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine41SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine41AnimationBinaryReader;
#elif SPINE_RUNTIME_40
using SpineAnimationFileVersion = Spine40AnimationFile;
using SpineAnimationCommonDataVersion = Spine40AnimationCommonData;
using SpineSingleAnimationDataVersion = Spine40SingleAnimationData;
using SpineAnimationBinaryReaderVersion = Spine40AnimationBinaryReader;
#endif

// Spine动态动画通用接口,资源由外部管理。支持ZeroCopy加载、共享AnimationState安全卸载和事件驱动LRU；动态动画默认至少驻留60秒，且只管理本类动态加入的动画。
public static class SpineDynamicAnimation
{
	private sealed class DynamicAnimationCacheEntry
	{
		public string mAnimationName;
		public long mAccessOrder;
		public double mLastUseTime;
		public bool mPinned;
	}
	private sealed class DynamicAnimationCache
	{
		public int mLimit = -1;
		public double mMinResidentSeconds = 60.0;
		public readonly Dictionary<string, DynamicAnimationCacheEntry> mEntries = new Dictionary<string, DynamicAnimationCacheEntry>(StringComparer.Ordinal);
	}
	private sealed class AnimationStateRegistration
	{
		public WeakReference mAnimationState;
		public WeakReference mSkeletonData;
		public AnimationState.TrackEntryDelegate mDisposeHandler;
	}
	private static readonly Dictionary<string, SpineAnimationCommonDataVersion> mCommonData = new Dictionary<string, SpineAnimationCommonDataVersion>(StringComparer.Ordinal);
	private static readonly Dictionary<string, List<AnimationStateRegistration>> mAnimationStates = new Dictionary<string, List<AnimationStateRegistration>>(StringComparer.Ordinal);
	private static readonly Dictionary<SkeletonData, DynamicAnimationCache> mDynamicAnimationCaches = new Dictionary<SkeletonData, DynamicAnimationCache>();
	private static long mAnimationAccessOrder;
	public static bool hasAnimation(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (skeletonAnimation == null || skeletonAnimation.Skeleton == null || skeletonAnimation.Skeleton.Data == null)
		{
			return false;
		}
		registerAnimationState(skeletonAnimation.Skeleton.Data, skeletonAnimation.AnimationState);
		return skeletonAnimation.Skeleton.Data.FindAnimation(animationName) != null;
	}
	public static bool hasAnimation(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (skeletonGraphic == null || skeletonGraphic.Skeleton == null || skeletonGraphic.Skeleton.Data == null)
		{
			return false;
		}
		AnimationState animationState = getAnimationState(skeletonGraphic);
		if (animationState != null)
		{
			registerAnimationState(skeletonGraphic.Skeleton.Data, animationState);
		}
		return skeletonGraphic.Skeleton.Data.FindAnimation(animationName) != null;
	}
	public static bool removeAnimation(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return false;
		}
		return removeAnimationIfUnused(skeletonAnimation.Skeleton.Data, animationName);
	}
	public static bool removeAnimation(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return false;
		}
		return removeAnimationIfUnused(skeletonGraphic.Skeleton.Data, animationName);
	}
	public static bool forceRemoveAnimation(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return false;
		}
		return removeAnimationFromSkeletonData(skeletonAnimation.Skeleton.Data, animationName);
	}
	public static bool forceRemoveAnimation(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return false;
		}
		return removeAnimationFromSkeletonData(skeletonGraphic.Skeleton.Data, animationName);
	}
	public static bool isAnimationInUse(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return false;
		}
		return isAnimationInUse(skeletonAnimation.Skeleton.Data, animationName);
	}
	public static bool isAnimationInUse(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return false;
		}
		return isAnimationInUse(skeletonGraphic.Skeleton.Data, animationName);
	}
	public static bool registerAnimationState(SkeletonAnimation skeletonAnimation)
	{
		if (skeletonAnimation == null || skeletonAnimation.Skeleton == null || skeletonAnimation.Skeleton.Data == null || skeletonAnimation.AnimationState == null)
		{
			return false;
		}
		registerAnimationState(skeletonAnimation.Skeleton.Data, skeletonAnimation.AnimationState);
		return true;
	}
	public static bool registerAnimationState(SkeletonGraphic skeletonGraphic)
	{
		if (skeletonGraphic == null || skeletonGraphic.Skeleton == null || skeletonGraphic.Skeleton.Data == null)
		{
			return false;
		}
		AnimationState animationState = getAnimationState(skeletonGraphic);
		if (animationState == null)
		{
			return false;
		}
		registerAnimationState(skeletonGraphic.Skeleton.Data, animationState);
		return true;
	}
	public static bool unregisterAnimationState(SkeletonAnimation skeletonAnimation)
	{
		if (skeletonAnimation == null || skeletonAnimation.Skeleton == null || skeletonAnimation.Skeleton.Data == null || skeletonAnimation.AnimationState == null)
		{
			return false;
		}
		return unregisterAnimationState(skeletonAnimation.Skeleton.Data, skeletonAnimation.AnimationState);
	}
	public static bool unregisterAnimationState(SkeletonGraphic skeletonGraphic)
	{
		if (skeletonGraphic == null || skeletonGraphic.Skeleton == null || skeletonGraphic.Skeleton.Data == null)
		{
			return false;
		}
		AnimationState animationState = getAnimationState(skeletonGraphic);
		return animationState != null && unregisterAnimationState(skeletonGraphic.Skeleton.Data, animationState);
	}
	public static int getRegisteredAnimationStateCount(SkeletonData skeletonData)
	{
		if (skeletonData == null)
		{
			return 0;
		}
		return cleanupAnimationStates(getSkeletonKey(skeletonData));
	}
	public static bool clearAnimationStates(SkeletonData skeletonData)
	{
		if (skeletonData == null)
		{
			return false;
		}
		string key = getSkeletonKey(skeletonData);
		if (!mAnimationStates.TryGetValue(key, out List<AnimationStateRegistration> states))
		{
			return false;
		}
		for (int i = 0; i < states.Count; ++i)
		{
			unsubscribeAnimationState(states[i]);
		}
		mAnimationStates.Remove(key);
		return true;
	}
	public static bool setDynamicAnimationCacheLimit(SkeletonAnimation skeletonAnimation, int maxCount)
	{
		if (!validateSkeletonAnimation(skeletonAnimation) || maxCount < 0)
		{
			return false;
		}
		DynamicAnimationCache cache = getOrCreateDynamicAnimationCache(skeletonAnimation.Skeleton.Data);
		cache.mLimit = maxCount;
		trimDynamicAnimations(skeletonAnimation.Skeleton.Data);
		return true;
	}
	public static bool setDynamicAnimationCacheLimit(SkeletonGraphic skeletonGraphic, int maxCount)
	{
		if (!validateSkeletonGraphic(skeletonGraphic) || maxCount < 0)
		{
			return false;
		}
		DynamicAnimationCache cache = getOrCreateDynamicAnimationCache(skeletonGraphic.Skeleton.Data);
		cache.mLimit = maxCount;
		trimDynamicAnimations(skeletonGraphic.Skeleton.Data);
		return true;
	}
	public static bool setDynamicAnimationMinResidentTime(SkeletonAnimation skeletonAnimation, double seconds)
	{
		if (!validateSkeletonAnimation(skeletonAnimation) || seconds < 0.0)
		{
			return false;
		}
		DynamicAnimationCache cache = getOrCreateDynamicAnimationCache(skeletonAnimation.Skeleton.Data);
		cache.mMinResidentSeconds = seconds;
		trimDynamicAnimations(skeletonAnimation.Skeleton.Data);
		return true;
	}
	public static bool setDynamicAnimationMinResidentTime(SkeletonGraphic skeletonGraphic, double seconds)
	{
		if (!validateSkeletonGraphic(skeletonGraphic) || seconds < 0.0)
		{
			return false;
		}
		DynamicAnimationCache cache = getOrCreateDynamicAnimationCache(skeletonGraphic.Skeleton.Data);
		cache.mMinResidentSeconds = seconds;
		trimDynamicAnimations(skeletonGraphic.Skeleton.Data);
		return true;
	}
	public static double getDynamicAnimationMinResidentTime(SkeletonData skeletonData)
	{
		if (skeletonData == null)
		{
			return 60.0;
		}
		if (!mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return 60.0;
		}
		return cache.mMinResidentSeconds;
	}
	public static bool disableDynamicAnimationCacheLimit(SkeletonData skeletonData)
	{
		if (skeletonData == null || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return false;
		}
		cache.mLimit = -1;
		return true;
	}
	public static int getDynamicAnimationCacheLimit(SkeletonData skeletonData)
	{
		if (skeletonData == null || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return -1;
		}
		return cache.mLimit;
	}
	public static int getDynamicAnimationCount(SkeletonData skeletonData)
	{
		if (skeletonData == null || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return 0;
		}
		cleanupDynamicAnimationCache(skeletonData, cache);
		return cache.mEntries.Count;
	}
	public static bool pinAnimation(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return false;
		}
		return setAnimationPinned(skeletonAnimation.Skeleton.Data, animationName, true);
	}
	public static bool pinAnimation(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return false;
		}
		return setAnimationPinned(skeletonGraphic.Skeleton.Data, animationName, true);
	}
	public static bool unpinAnimation(SkeletonAnimation skeletonAnimation, string animationName)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return false;
		}
		bool result = setAnimationPinned(skeletonAnimation.Skeleton.Data, animationName, false);
		trimDynamicAnimations(skeletonAnimation.Skeleton.Data);
		return result;
	}
	public static bool unpinAnimation(SkeletonGraphic skeletonGraphic, string animationName)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return false;
		}
		bool result = setAnimationPinned(skeletonGraphic.Skeleton.Data, animationName, false);
		trimDynamicAnimations(skeletonGraphic.Skeleton.Data);
		return result;
	}
	public static int trimDynamicAnimations(SkeletonAnimation skeletonAnimation)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return 0;
		}
		return trimDynamicAnimations(skeletonAnimation.Skeleton.Data);
	}
	public static int trimDynamicAnimations(SkeletonGraphic skeletonGraphic)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return 0;
		}
		return trimDynamicAnimations(skeletonGraphic.Skeleton.Data);
	}
	public static bool clearDynamicAnimationCache(SkeletonData skeletonData)
	{
		if (skeletonData == null)
		{
			return false;
		}
		return mDynamicAnimationCaches.Remove(skeletonData);
	}
	public static Spine.Animation addAnimation(SkeletonAnimation skeletonAnimation, byte[] animationFileBytes)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return null;
		}
		return addAnimation(skeletonAnimation.Skeleton.Data, skeletonAnimation.skeletonDataAsset, animationFileBytes, skeletonAnimation);
	}
	public static Spine.Animation addAnimation(SkeletonGraphic skeletonGraphic, byte[] animationFileBytes)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return null;
		}
		return addAnimation(skeletonGraphic.Skeleton.Data, skeletonGraphic.skeletonDataAsset, animationFileBytes, skeletonGraphic);
	}
	public static TrackEntry playAnimation(SkeletonAnimation skeletonAnimation, string animationName, bool isLoop)
	{
		return playAnimation(skeletonAnimation, 0, animationName, isLoop);
	}
	public static TrackEntry playAnimation(SkeletonAnimation skeletonAnimation, int trackIndex, string animationName, bool isLoop)
	{
		if (!validateSkeletonAnimation(skeletonAnimation))
		{
			return null;
		}
		Spine.Animation animation = skeletonAnimation.Skeleton.Data.FindAnimation(animationName);
		if (animation == null)
		{
			Debug.LogWarning("Spine动画不存在:" + animationName + ",SkeletonDataAsset:" + skeletonAnimation.skeletonDataAsset.name, skeletonAnimation);
			return null;
		}
		touchDynamicAnimation(skeletonAnimation.Skeleton.Data, animationName);
		TrackEntry entry = skeletonAnimation.AnimationState.SetAnimation(trackIndex, animation, isLoop);
		trimDynamicAnimations(skeletonAnimation.Skeleton.Data);
		return entry;
	}
	public static TrackEntry playAnimation(SkeletonGraphic skeletonGraphic, string animationName, bool isLoop)
	{
		return playAnimation(skeletonGraphic, 0, animationName, isLoop);
	}
	public static TrackEntry playAnimation(SkeletonGraphic skeletonGraphic, int trackIndex, string animationName, bool isLoop)
	{
		if (!validateSkeletonGraphic(skeletonGraphic))
		{
			return null;
		}
		Spine.Animation animation = skeletonGraphic.Skeleton.Data.FindAnimation(animationName);
		if (animation == null)
		{
			Debug.LogWarning("Spine动画不存在:" + animationName + ",SkeletonDataAsset:" + skeletonGraphic.skeletonDataAsset.name, skeletonGraphic);
			return null;
		}
		AnimationState animationState = getAnimationState(skeletonGraphic);
		if (animationState == null)
		{
			Debug.LogError("SkeletonGraphic没有可用的SkeletonAnimation动画组件", skeletonGraphic);
			return null;
		}
		touchDynamicAnimation(skeletonGraphic.Skeleton.Data, animationName);
		TrackEntry entry = animationState.SetAnimation(trackIndex, animation, isLoop);
		trimDynamicAnimations(skeletonGraphic.Skeleton.Data);
		return entry;
	}
	public static void setCommonData(SkeletonData skeletonData, byte[] commonFileBytes)
	{
		if (commonFileBytes == null || commonFileBytes.Length == 0)
		{
			return;
		}
		string key = getSkeletonKey(skeletonData);
		if (mCommonData.ContainsKey(key))
		{
			return;
		}
		SpineAnimationCommonDataVersion commonData = SpineAnimationFileVersion.readCommon(commonFileBytes);
		validateCommonData(skeletonData, commonData);
		mCommonData.Add(key, commonData);
	}
	public static SpineAnimationCommonDataVersion getCommonData(SkeletonData skeletonData)
	{
		mCommonData.TryGetValue(getSkeletonKey(skeletonData), out SpineAnimationCommonDataVersion commonData);
		return commonData;
	}
	public static bool removeCommonData(SkeletonData skeletonData)
	{
		if (skeletonData == null)
		{
			return false;
		}
		return mCommonData.Remove(getSkeletonKey(skeletonData));
	}
	public static void clearCommonData()
	{
		mCommonData.Clear();
	}
	public static void clearRuntimeData()
	{
		foreach (KeyValuePair<string, List<AnimationStateRegistration>> pair in mAnimationStates)
		{
			List<AnimationStateRegistration> states = pair.Value;
			for (int i = 0; i < states.Count; ++i)
			{
				unsubscribeAnimationState(states[i]);
			}
		}
		mCommonData.Clear();
		mAnimationStates.Clear();
		mDynamicAnimationCaches.Clear();
		mAnimationAccessOrder = 0L;
	}
	//---------------------------------------------------------------------------------------------------------------------------
	private static bool removeAnimationIfUnused(SkeletonData skeletonData, string animationName)
	{
		if (string.IsNullOrEmpty(animationName))
		{
			return false;
		}
		Spine.Animation animation = skeletonData.FindAnimation(animationName);
		if (animation == null || isAnimationInUse(skeletonData, animation))
		{
			return false;
		}
		bool removed = skeletonData.Animations.Remove(animation);
		if (removed)
		{
			unregisterDynamicAnimation(skeletonData, animationName);
		}
		return removed;
	}
	private static bool removeAnimationFromSkeletonData(SkeletonData skeletonData, string animationName)
	{
		if (string.IsNullOrEmpty(animationName))
		{
			return false;
		}
		Spine.Animation animation = skeletonData.FindAnimation(animationName);
		if (animation == null)
		{
			return false;
		}
		bool removed = skeletonData.Animations.Remove(animation);
		if (removed)
		{
			unregisterDynamicAnimation(skeletonData, animationName);
		}
		return removed;
	}
	private static bool isAnimationInUse(SkeletonData skeletonData, string animationName)
	{
		if (string.IsNullOrEmpty(animationName))
		{
			return false;
		}
		Spine.Animation animation = skeletonData.FindAnimation(animationName);
		return animation != null && isAnimationInUse(skeletonData, animation);
	}
	private static DynamicAnimationCache getOrCreateDynamicAnimationCache(SkeletonData skeletonData)
	{
		if (!mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			cache = new DynamicAnimationCache();
			mDynamicAnimationCaches.Add(skeletonData, cache);
		}
		return cache;
	}
	private static void registerDynamicAnimation(SkeletonData skeletonData, string animationName)
	{
		DynamicAnimationCache cache = getOrCreateDynamicAnimationCache(skeletonData);
		if (!cache.mEntries.TryGetValue(animationName, out DynamicAnimationCacheEntry entry))
		{
			entry = new DynamicAnimationCacheEntry();
			entry.mAnimationName = animationName;
			cache.mEntries.Add(animationName, entry);
		}
		entry.mAccessOrder = nextAnimationAccessOrder();
		entry.mLastUseTime = getCurrentTime();
	}
	private static void unregisterDynamicAnimation(SkeletonData skeletonData, string animationName)
	{
		if (mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			cache.mEntries.Remove(animationName);
		}
	}
	private static void touchDynamicAnimation(SkeletonData skeletonData, string animationName)
	{
		if (skeletonData == null || string.IsNullOrEmpty(animationName) || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return;
		}
		if (cache.mEntries.TryGetValue(animationName, out DynamicAnimationCacheEntry entry))
		{
			entry.mAccessOrder = nextAnimationAccessOrder();
			entry.mLastUseTime = getCurrentTime();
		}
	}
	private static bool setAnimationPinned(SkeletonData skeletonData, string animationName, bool pinned)
	{
		if (skeletonData == null || string.IsNullOrEmpty(animationName) || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache))
		{
			return false;
		}
		if (!cache.mEntries.TryGetValue(animationName, out DynamicAnimationCacheEntry entry))
		{
			return false;
		}
		entry.mPinned = pinned;
		return true;
	}
	private static int trimDynamicAnimations(SkeletonData skeletonData)
	{
		if (skeletonData == null || !mDynamicAnimationCaches.TryGetValue(skeletonData, out DynamicAnimationCache cache) || cache.mLimit < 0)
		{
			return 0;
		}
		cleanupDynamicAnimationCache(skeletonData, cache);
		double currentTime = getCurrentTime();
		int removedCount = 0;
		while (cache.mEntries.Count > cache.mLimit)
		{
			DynamicAnimationCacheEntry candidate = null;
			Spine.Animation candidateAnimation = null;
			foreach (KeyValuePair<string, DynamicAnimationCacheEntry> pair in cache.mEntries)
			{
				DynamicAnimationCacheEntry entry = pair.Value;
				if (entry.mPinned || currentTime - entry.mLastUseTime < cache.mMinResidentSeconds)
				{
					continue;
				}
				Spine.Animation animation = skeletonData.FindAnimation(entry.mAnimationName);
				if (animation == null || isAnimationInUse(skeletonData, animation))
				{
					continue;
				}
				if (candidate == null || entry.mAccessOrder < candidate.mAccessOrder)
				{
					candidate = entry;
					candidateAnimation = animation;
				}
			}
			if (candidate == null)
			{
				break;
			}
			if (candidateAnimation != null)
			{
				skeletonData.Animations.Remove(candidateAnimation);
			}
			cache.mEntries.Remove(candidate.mAnimationName);
			++removedCount;
		}
		return removedCount;
	}
	private static void cleanupDynamicAnimationCache(SkeletonData skeletonData, DynamicAnimationCache cache)
	{
		if (cache.mEntries.Count == 0)
		{
			return;
		}
		List<string> invalidNames = null;
		foreach (KeyValuePair<string, DynamicAnimationCacheEntry> pair in cache.mEntries)
		{
			if (skeletonData.FindAnimation(pair.Key) != null)
			{
				continue;
			}
			if (invalidNames == null)
			{
				invalidNames = new List<string>();
			}
			invalidNames.Add(pair.Key);
		}
		if (invalidNames == null)
		{
			return;
		}
		for (int i = 0; i < invalidNames.Count; ++i)
		{
			cache.mEntries.Remove(invalidNames[i]);
		}
	}
	private static double getCurrentTime()
	{
		return UnityEngine.Time.realtimeSinceStartup;
	}
	private static long nextAnimationAccessOrder()
	{
		++mAnimationAccessOrder;
		if (mAnimationAccessOrder <= 0L)
		{
			mAnimationAccessOrder = 1L;
			foreach (KeyValuePair<SkeletonData, DynamicAnimationCache> cachePair in mDynamicAnimationCaches)
			{
				foreach (KeyValuePair<string, DynamicAnimationCacheEntry> entryPair in cachePair.Value.mEntries)
				{
					entryPair.Value.mAccessOrder = 0L;
				}
			}
		}
		return mAnimationAccessOrder;
	}
	private static bool isAnimationInUse(SkeletonData skeletonData, Spine.Animation animation)
	{
		if (skeletonData == null || animation == null)
		{
			return false;
		}
		string key = getSkeletonKey(skeletonData);
		if (!mAnimationStates.TryGetValue(key, out List<AnimationStateRegistration> states))
		{
			return false;
		}
		for (int i = states.Count - 1; i >= 0; --i)
		{
			AnimationStateRegistration registration = states[i];
			AnimationState animationState = registration.mAnimationState.Target as AnimationState;
			if (animationState == null)
			{
				states.RemoveAt(i);
				continue;
			}
			if (isAnimationInUse(animationState, animation))
			{
				return true;
			}
		}
		if (states.Count == 0)
		{
			mAnimationStates.Remove(key);
		}
		return false;
	}
	private static bool isAnimationInUse(AnimationState animationState, Spine.Animation animation)
	{
		if (animationState == null || animation == null)
		{
			return false;
		}
		ExposedList<TrackEntry> tracks = animationState.Tracks;
		TrackEntry[] trackItems = tracks.Items;
		for (int i = 0; i < tracks.Count; ++i)
		{
			if (isTrackEntryUsingAnimation(trackItems[i], animation))
			{
				return true;
			}
		}
		return false;
	}
	private static bool isTrackEntryUsingAnimation(TrackEntry entry, Spine.Animation animation)
	{
		for (TrackEntry queued = entry; queued != null; queued = queued.Next)
		{
			for (TrackEntry mixing = queued; mixing != null; mixing = mixing.MixingFrom)
			{
				if (ReferenceEquals(mixing.Animation, animation))
				{
					return true;
				}
			}
		}
		return false;
	}
	private static void registerAnimationState(SkeletonData skeletonData, AnimationState animationState)
	{
		if (skeletonData == null || animationState == null)
		{
			return;
		}
		string key = getSkeletonKey(skeletonData);
		if (!mAnimationStates.TryGetValue(key, out List<AnimationStateRegistration> states))
		{
			states = new List<AnimationStateRegistration>();
			mAnimationStates.Add(key, states);
		}
		for (int i = states.Count - 1; i >= 0; --i)
		{
			AnimationStateRegistration registration = states[i];
			AnimationState target = registration.mAnimationState.Target as AnimationState;
			if (target == null)
			{
				states.RemoveAt(i);
				continue;
			}
			if (ReferenceEquals(target, animationState))
			{
				return;
			}
		}
		WeakReference skeletonReference = new WeakReference(skeletonData);
		AnimationState.TrackEntryDelegate disposeHandler = delegate
		{
			SkeletonData targetSkeletonData = skeletonReference.Target as SkeletonData;
			if (targetSkeletonData != null)
			{
				trimDynamicAnimations(targetSkeletonData);
			}
		};
		animationState.Dispose += disposeHandler;
		AnimationStateRegistration newRegistration = new AnimationStateRegistration();
		newRegistration.mAnimationState = new WeakReference(animationState);
		newRegistration.mSkeletonData = skeletonReference;
		newRegistration.mDisposeHandler = disposeHandler;
		states.Add(newRegistration);
	}
	private static bool unregisterAnimationState(SkeletonData skeletonData, AnimationState animationState)
	{
		if (skeletonData == null || animationState == null)
		{
			return false;
		}
		string key = getSkeletonKey(skeletonData);
		if (!mAnimationStates.TryGetValue(key, out List<AnimationStateRegistration> states))
		{
			return false;
		}
		bool removed = false;
		for (int i = states.Count - 1; i >= 0; --i)
		{
			AnimationStateRegistration registration = states[i];
			AnimationState target = registration.mAnimationState.Target as AnimationState;
			if (target == null || ReferenceEquals(target, animationState))
			{
				if (target != null)
				{
					unsubscribeAnimationState(registration);
					removed = true;
				}
				states.RemoveAt(i);
			}
		}
		if (states.Count == 0)
		{
			mAnimationStates.Remove(key);
		}
		return removed;
	}
	private static int cleanupAnimationStates(string key)
	{
		if (!mAnimationStates.TryGetValue(key, out List<AnimationStateRegistration> states))
		{
			return 0;
		}
		for (int i = states.Count - 1; i >= 0; --i)
		{
			if (!(states[i].mAnimationState.Target is AnimationState))
			{
				states.RemoveAt(i);
			}
		}
		if (states.Count == 0)
		{
			mAnimationStates.Remove(key);
			return 0;
		}
		return states.Count;
	}
	private static void unsubscribeAnimationState(AnimationStateRegistration registration)
	{
		if (registration == null || registration.mDisposeHandler == null)
		{
			return;
		}
		AnimationState animationState = registration.mAnimationState.Target as AnimationState;
		if (animationState != null)
		{
			animationState.Dispose -= registration.mDisposeHandler;
		}
		registration.mDisposeHandler = null;
	}
	private static Spine.Animation addAnimation(SkeletonData skeletonData, SkeletonDataAsset skeletonDataAsset, byte[] animationFileBytes, UnityEngine.Object context)
	{
		if (!validateSkeleton(skeletonData, skeletonDataAsset, context))
		{
			return null;
		}
		if (animationFileBytes == null || animationFileBytes.Length == 0)
		{
			Debug.LogError("添加Spine动画失败,动画文件字节为空", context);
			return null;
		}
		try
		{
			SpineSingleAnimationDataVersion animationData = SpineAnimationFileVersion.readAnimationNoCopy(animationFileBytes);
			Spine.Animation existingAnimation = skeletonData.FindAnimation(animationData.mAnimationName);
			if (existingAnimation != null)
			{
				touchDynamicAnimation(skeletonData, animationData.mAnimationName);
				return existingAnimation;
			}
			SpineAnimationCommonDataVersion commonData = getCommonData(skeletonData);
			if (commonData == null)
			{
				Debug.LogError("添加Spine动画失败,尚未添加公共动画数据:" + animationData.mAnimationName, context);
				return null;
			}
			validateAnimationData(animationData, commonData);
			SpineAnimationBinaryReaderVersion reader = new SpineAnimationBinaryReaderVersion();
			Spine.Animation animation = reader.readAnimation(animationData.mBinarySourceData, animationData.mBinaryOffset, animationData.mBinaryLength, commonData.mStrings, skeletonData, skeletonDataAsset.scale, animationData.mAnimationName);
			animationData.mBinarySourceData = null;
			skeletonData.Animations.Add(animation);
			registerDynamicAnimation(skeletonData, animationData.mAnimationName);
			trimDynamicAnimations(skeletonData);
			return animation;
		}
		catch (Exception exception)
		{
			Debug.LogError("添加Spine动画失败,原因:" + exception.Message, context);
			Debug.LogException(exception);
			return null;
		}
	}
	private static string getSkeletonKey(SkeletonData skeletonData)
	{
		return skeletonData.Version + "|" + skeletonData.Hash;
	}
	// Spine 4.3将SkeletonGraphic的动画职责拆到独立SkeletonAnimation组件。
	// 新建4.3工程应由SkeletonGraphic.Animation关联SkeletonAnimation；本插件不负责旧工程升级或自动补组件。
	private static AnimationState getAnimationState(SkeletonGraphic skeletonGraphic)
	{
		if (skeletonGraphic == null)
		{
			return null;
		}
#if SPINE_RUNTIME_43
		SkeletonAnimation skeletonAnimation = skeletonGraphic.Animation as SkeletonAnimation;
		return skeletonAnimation != null ? skeletonAnimation.AnimationState : null;
#else
        return skeletonGraphic.AnimationState;
#endif
	}
	private static bool validateSkeletonAnimation(SkeletonAnimation skeletonAnimation)
	{
		if (skeletonAnimation == null || skeletonAnimation.Skeleton == null || skeletonAnimation.Skeleton.Data == null || skeletonAnimation.AnimationState == null || skeletonAnimation.skeletonDataAsset == null)
		{
			Debug.LogError("SkeletonAnimation尚未初始化", skeletonAnimation);
			return false;
		}
		registerAnimationState(skeletonAnimation.Skeleton.Data, skeletonAnimation.AnimationState);
		return true;
	}
	private static bool validateSkeletonGraphic(SkeletonGraphic skeletonGraphic)
	{
		if (skeletonGraphic == null || skeletonGraphic.Skeleton == null || skeletonGraphic.Skeleton.Data == null || skeletonGraphic.skeletonDataAsset == null)
		{
			Debug.LogError("SkeletonGraphic尚未初始化", skeletonGraphic);
			return false;
		}
		AnimationState animationState = getAnimationState(skeletonGraphic);
		if (animationState == null)
		{
			Debug.LogError("SkeletonGraphic没有可用的SkeletonAnimation动画组件", skeletonGraphic);
			return false;
		}
		registerAnimationState(skeletonGraphic.Skeleton.Data, animationState);
		return true;
	}
	private static bool validateSkeleton(SkeletonData skeletonData, SkeletonDataAsset skeletonDataAsset, UnityEngine.Object context)
	{
		if (skeletonData == null || skeletonDataAsset == null)
		{
			Debug.LogError("添加Spine动画失败,Skeleton数据为空", context);
			return false;
		}
		return true;
	}
	private static void validateCommonData(SkeletonData skeletonData, SpineAnimationCommonDataVersion commonData)
	{
		if (!string.Equals(commonData.mSpineVersion, skeletonData.Version, StringComparison.Ordinal))
		{
			Debug.LogError("Spine版本不一致,公共文件:" + commonData.mSpineVersion + ",Skeleton:" + skeletonData.Version);
			return;
		}
		long skeletonHash = SpineSkeletonHashUtility.getStableHash(skeletonData.Hash);
		if (commonData.mSkeletonHash != skeletonHash)
		{
			Debug.LogError("Skeleton Hash不一致,公共文件:" + commonData.mSkeletonHash + ",Skeleton:" + skeletonHash);
			return;
		}
	}
	private static void validateAnimationData(SpineSingleAnimationDataVersion animationData, SpineAnimationCommonDataVersion commonData)
	{
		if (!string.Equals(animationData.mSpineVersion, commonData.mSpineVersion, StringComparison.Ordinal))
		{
			Debug.LogError("单动画文件Spine版本不一致:" + animationData.mAnimationName);
		}
		if (animationData.mSkeletonHash != commonData.mSkeletonHash)
		{
			Debug.LogError("单动画文件Skeleton Hash不一致:" + animationData.mAnimationName);
		}
	}
}
#endif