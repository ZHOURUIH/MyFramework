using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static FrameBaseHotFix;
using static TimeUtility;
using static UnityUtility;
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
using TTSDK;
#elif (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
using QGMiniGame;
#endif

public class ADManager : FrameSystem
{
	public const float LOAD_TIMEOUT = 30.0f;
	protected int mRequestID;
	protected int mConfigRequestID;
	protected long mSessionStartTimeMS;
	protected long mLastInterstitialTimeMS;
	protected long mNextConfigRefreshTimeMS;
	protected bool mConfigRequestPending;
	protected bool mInterstitialConfigLoaded;
	protected bool mInterstitialReviewPassed;
	protected bool mHasShownInterstitial;
	protected bool mAdRequestPending;
	protected bool mShowing;
	protected bool mShowRequested;
	protected bool mCurrentAdIsInterstitial;
	protected float mLoadTime;
	protected float mPreviousTimeScale;
	protected bool mPreviousAudioPause;
	protected double mInterstitialFirstDelay;
	protected double mInterstitialInterval;
	protected Action mSuccess;
	protected Action mFailure;
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
	protected TTRewardedVideoAd mRewardedVideo;
	protected TTInterstitialAd mInterstitial;
#elif (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
	protected QGRewardedVideoAd mRewardedVideo;
	protected QGInterstitialAd mInterstitial;
#endif
	public override void init()
	{
		base.init();
		mDestroying = false;
		mSessionStartTimeMS = getNowUTCTimeStampMS();
		refreshInterstitialConfig();
	}
	public void showReward(Action success, Action failure)
	{
		showAd(true, success, failure);
	}
	public void showInterstitial(Action success, Action failure)
	{
		showAd(false, success, failure);
	}
	public bool isAdRequestInProgress()
	{
		return mAdRequestPending;
	}
	public bool canShowInterstitial()
	{
#if UNITY_EDITOR
		return true;
#else
		string switchID = MiniGameSettings.getInterstitialSwitchID();
		if (string.IsNullOrWhiteSpace(switchID))
		{
			return true;
		}
		long now = getNowUTCTimeStampMS();
		return mInterstitialConfigLoaded && mInterstitialReviewPassed &&
			(now - mSessionStartTimeMS) / 1000.0 >= mInterstitialFirstDelay &&
			(!mHasShownInterstitial || (now - mLastInterstitialTimeMS) / 1000.0 >= mInterstitialInterval);
#endif
	}
	public void cancelAd()
	{
		finishAd(mRequestID, false);
	}
	public void refreshInterstitialConfig()
	{
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		MiniGameSettings settings = MiniGameSettings.get();
		string switchID = MiniGameSettings.getInterstitialSwitchID();
		if (mDestroying || mConfigRequestPending || string.IsNullOrWhiteSpace(settings.InterstitialConfigURL) || string.IsNullOrWhiteSpace(switchID))
		{
			return;
		}
		mConfigRequestPending = true;
		long refreshMS = (long)(Mathf.Max(1.0f, settings.InterstitialConfigRefreshSeconds) * 1000.0f);
		mNextConfigRefreshTimeMS = getNowUTCTimeStampMS() + refreshMS;
		int requestID = ++mConfigRequestID;
		try
		{
			JObject request = new();
			request["switchIdList"] = new JArray(switchID);
			request["gameId"] = MiniGameSettings.getAppID();
			request["pkgName"] = MiniGameSettings.getPackageName();
#if OPPO_MINI_GAME
			request["platform"] = "oppo";
#elif VIVO_MINI_GAME
			request["platform"] = "vivo";
#endif
			request["version"] = Application.version;
			HttpUtility.httpPostAsyncWebGL(settings.InterstitialConfigURL, request.ToString(Formatting.None),
				(result, status, code) => onConfigReceived(requestID, result, status, code));
		}
		catch (Exception e)
		{
			mConfigRequestPending = false;
			logWarning("获取插屏配置失败:" + e.Message);
		}
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mAdRequestPending && !mShowing)
		{
			mLoadTime += mGameFrameworkHotFix.getUnscaledTime();
			if (mLoadTime >= LOAD_TIMEOUT)
			{
				onAdError(mRequestID, "广告加载或展示超时");
			}
		}
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		if (!mDestroying && !string.IsNullOrWhiteSpace(MiniGameSettings.get().InterstitialConfigURL) &&
			!string.IsNullOrWhiteSpace(MiniGameSettings.getInterstitialSwitchID()) && getNowUTCTimeStampMS() >= mNextConfigRefreshTimeMS)
		{
			mConfigRequestPending = false;
			refreshInterstitialConfig();
		}
#endif
	}
	public override void willDestroy()
	{
		mDestroying = true;
		++mConfigRequestID;
		mConfigRequestPending = false;
		cancelAd();
		base.willDestroy();
	}
	public override void destroy()
	{
		mDestroying = true;
		++mConfigRequestID;
		mConfigRequestPending = false;
		cancelAd();
		base.destroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void showAd(bool rewarded, Action success, Action failure)
	{
		if (mDestroying || isAdRequestInProgress())
		{
			failure?.Invoke();
			return;
		}
#if UNITY_EDITOR
		success?.Invoke();
#else
		if (!rewarded && !canShowInterstitial())
		{
			success?.Invoke();
			return;
		}
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		string adUnitID = rewarded ? MiniGameSettings.getRewardedVideoID() : MiniGameSettings.getInterstitialID();
		if (string.IsNullOrWhiteSpace(adUnitID))
		{
			logWarning("未配置当前平台的" + (rewarded ? "激励视频" : "插屏") + "广告位 ID");
			failure?.Invoke();
			return;
		}
		mAdRequestPending = true;
		mCurrentAdIsInterstitial = !rewarded;
		mShowing = false;
		mShowRequested = false;
		mLoadTime = 0.0f;
		mSuccess = success;
		mFailure = failure;
		mPreviousTimeScale = Time.timeScale;
		mPreviousAudioPause = AudioListener.pause;
		CmdTimeManagerScaleTime.execute(0.0f, 0.0f, 0.0f, 0.0f, 0, false, null, null);
		AudioListener.pause = true;
		int requestID = ++mRequestID;
		try
		{
			createAd(requestID, rewarded, adUnitID);
		}
		catch (Exception e)
		{
			onAdError(requestID, e.Message);
		}
#else
		success?.Invoke();
#endif
#endif
	}
	protected void finishAd(int requestID, bool success)
	{
		if (!mAdRequestPending || requestID != mRequestID)
		{
			return;
		}
		mAdRequestPending = false;
		mShowing = false;
		++mRequestID;
		if (success && mCurrentAdIsInterstitial)
		{
			mLastInterstitialTimeMS = getNowUTCTimeStampMS();
			mHasShownInterstitial = true;
		}
		mCurrentAdIsInterstitial = false;
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		var rewardedVideo = mRewardedVideo;
		var interstitial = mInterstitial;
		mRewardedVideo = null;
		mInterstitial = null;
		try
		{
			rewardedVideo?.Destroy();
			interstitial?.Destroy();
		}
		catch (Exception e)
		{
			logWarning("释放广告失败:" + e.Message);
		}
#endif
		CmdTimeManagerScaleTime.execute(mPreviousTimeScale, mPreviousTimeScale, 0.0f, 0.0f, 0, false, null, null);
		AudioListener.pause = mPreviousAudioPause;
		Action callback = success ? mSuccess : mFailure;
		mSuccess = null;
		mFailure = null;
		callback?.Invoke();
	}
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
	protected void createAd(int requestID, bool rewarded, string adUnitID)
	{
#if BYTE_DANCE
		if (rewarded)
		{
			mRewardedVideo = TT.CreateRewardedVideoAd(new CreateRewardedVideoAdParam { AdUnitId = adUnitID });
			mRewardedVideo.OnLoad += () => onLoaded(requestID);
			mRewardedVideo.OnError += (code, error) => onAdError(requestID, code + " " + error);
			mRewardedVideo.OnClose += (isEnded, count) => finishAd(requestID, isEnded);
			mRewardedVideo.Load();
		}
		else
		{
			mInterstitial = TT.CreateInterstitialAd(new CreateInterstitialAdParam { InterstitialAdId = adUnitID });
			mInterstitial.OnLoad += () => onLoaded(requestID);
			mInterstitial.OnError += (code, error) => onAdError(requestID, code + " " + error);
			mInterstitial.OnClose += () => finishAd(requestID, true);
			mInterstitial.Load();
		}
#else
		if (rewarded)
		{
			mRewardedVideo = QG.CreateRewardedVideoAd(new QGCommonAdParam { adUnitId = adUnitID });
			mRewardedVideo.OnLoad(() => onLoaded(requestID));
			mRewardedVideo.OnError(error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
			mRewardedVideo.OnClose((QGRewardedVideoResponse response) => finishAd(requestID, response != null && response.isEnded));
			mRewardedVideo.Load(response => onLoaded(requestID), error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
		}
		else
		{
			mInterstitial = QG.CreateInterstitialAd(new QGCommonAdParam { adUnitId = adUnitID });
			mInterstitial.OnLoad(() => onLoaded(requestID));
			mInterstitial.OnError(error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
#if VIVO_MINI_GAME
			mInterstitial.OnClose(() => finishAd(requestID, true));
#else
			mInterstitial.OnClose(response => finishAd(requestID, true));
			mInterstitial.Load(response => onLoaded(requestID), error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
#endif
		}
#endif
	}
	protected void onLoaded(int requestID)
	{
		if (!mAdRequestPending || requestID != mRequestID || mShowRequested)
		{
			return;
		}
		mShowRequested = true;
		try
		{
#if BYTE_DANCE
			if (mRewardedVideo != null)
			{
				mRewardedVideo.Show();
			}
			else
			{
				mInterstitial.Show();
			}
			onShown(requestID);
#else
			QGBaseAd ad = mRewardedVideo != null ? mRewardedVideo : (QGBaseAd)mInterstitial;
			ad.Show(response => onShown(requestID), error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
#endif
		}
		catch (Exception e)
		{
			onAdError(requestID, e.Message);
		}
	}
	protected void onShown(int requestID)
	{
		if (mAdRequestPending && requestID == mRequestID)
		{
			mShowing = true;
		}
	}
#endif
	protected void onAdError(int requestID, string error)
	{
		if (!mAdRequestPending || requestID != mRequestID)
		{
			return;
		}
		logWarning("广告失败:" + error);
		finishAd(requestID, false);
	}
	protected void onConfigReceived(int requestID, string result, UnityWebRequest.Result status, long code)
	{
		if (mDestroying || !mConfigRequestPending || requestID != mConfigRequestID)
		{
			return;
		}
		mConfigRequestPending = false;
		if (status != UnityWebRequest.Result.Success || code < 200 || code >= 300 || !tryApplyInterstitialConfig(result))
		{
			logWarning("插屏配置无效，尚无有效配置时跳过插屏");
		}
	}
	protected bool tryApplyInterstitialConfig(string result)
	{
		try
		{
			JObject response = JObject.Parse(result);
			JObject meta = response["meta"] as JObject;
			JArray data = response["data"] as JArray;
			if (meta == null || meta["errCode"]?.Type != JTokenType.Integer || meta.Value<int>("errCode") != 0 ||
				data == null || data.Count != 1 || data[0]?.Type != JTokenType.String)
			{
				return false;
			}
			string value = data[0].Value<string>();
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
			Vector3 config = value.SToV3();
			if (config.x < 0.0f || config.y < 0.0f || config.z != 0.0f && config.z != 1.0f)
			{
				return false;
			}
			mInterstitialFirstDelay = config.x;
			mInterstitialInterval = config.y;
			mInterstitialReviewPassed = config.z == 1.0f;
			mInterstitialConfigLoaded = true;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
