using System;
using static FrameBaseHotFix;
using static UnityUtility;
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
#elif (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
using QGMiniGame;
#endif

public class MiniGamePlatform : FrameSystem
{
	public const float LOAD_TIMEOUT = 30.0f;
	protected Action mSuccess;
	protected Action mFailure;
	protected int mRequestID;
	protected int mCallbackVersion;
	protected bool mPending;
	protected bool mShowRequested;
	protected bool mShowing;
	protected float mLoadTime;
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
		++mCallbackVersion;
	}
	public void showReward(Action success, Action failure)
	{
		showAd(true, success, failure);
	}
	public void showInterstitial(Action success, Action failure)
	{
		showAd(false, success, failure);
	}
	public void cancelAd()
	{
		finish(mRequestID, false);
	}
	public bool isAdRequestInProgress()
	{
		return mPending;
	}
	public bool isShareSupported()
	{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
	public bool isShortcutSupported()
	{
#if (BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
	public bool isSidebarSupported()
	{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
		return true;
#else
		return false;
#endif
	}
	public void shareAppMessage(bool isInvite, BoolCallback callback = null)
	{
		if (mDestroying)
		{
			callback?.Invoke(false);
			return;
		}
		BoolCallback complete = createCallback(callback);
		try
		{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
			JsonData data = new();
			data["channel"] = isInvite ? "invite" : "share";
			TT.ShareAppMessage(data, response => complete(true), error => complete(false), () => complete(false));
#else
			complete(false);
#endif
		}
		catch (Exception e)
		{
			logWarning("分享失败:" + e.Message);
			complete(false);
		}
	}
	public void addShortcut(BoolCallback callback = null)
	{
		if (mDestroying)
		{
			callback?.Invoke(false);
			return;
		}
		BoolCallback complete = createCallback(callback);
		try
		{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
			TT.AddShortcut(success => complete(success));
#elif OPPO_MINI_GAME && UNITY_WEBGL && !UNITY_EDITOR
			QG.InstallShortcut(response => complete(true), error => complete(false));
#elif VIVO_MINI_GAME && UNITY_WEBGL && !UNITY_EDITOR
			QG.InstallShortcut("", response => complete(true), error => complete(false));
#else
			complete(false);
#endif
		}
		catch (Exception e)
		{
			logWarning("添加桌面快捷方式失败:" + e.Message);
			complete(false);
		}
	}
	public bool checkLaunchFromSidebar(out string clickID)
	{
		clickID = "";
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
		if (!mDestroying)
		{
			try
			{
				LaunchOption option = TT.GetLaunchOptionsSync();
				clickID = option.Query.get("clickid") ?? "";
				return option.Scene == "sidebar";
			}
			catch (Exception e)
			{
				logWarning("读取启动来源失败:" + e.Message);
			}
		}
#endif
		return false;
	}
	public void navigateToSideBar(BoolCallback callback = null)
	{
		if (mDestroying)
		{
			callback?.Invoke(false);
			return;
		}
		BoolCallback complete = createCallback(callback);
		try
		{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
			JsonData data = new();
			data["scene"] = "sidebar";
			TT.NavigateToScene(data, () => complete(true), null, (code, error) => complete(false));
#else
			complete(false);
#endif
		}
		catch (Exception e)
		{
			logWarning("跳转侧边栏失败:" + e.Message);
			complete(false);
		}
	}
	// 平台接口只检查入口可用性，奖励领取资格由玩家存档判断。
	public void checkSideBar(BoolCallback callback)
	{
		if (mDestroying)
		{
			callback?.Invoke(false);
			return;
		}
		BoolCallback complete = createCallback(callback);
		try
		{
#if BYTE_DANCE && UNITY_WEBGL && !UNITY_EDITOR
			TT.CheckScene(TTSideBar.SceneEnum.SideBar, success => complete(success), null, (code, error) => complete(false));
#else
			complete(false);
#endif
		}
		catch (Exception e)
		{
			logWarning("检查侧边栏失败:" + e.Message);
			complete(false);
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (!mPending || mShowing)
		{
			return;
		}
		mLoadTime += mGameFrameworkHotFix.getUnscaledTime();
		if (mLoadTime >= LOAD_TIMEOUT)
		{
			onAdError(mRequestID, "广告加载或展示超时");
		}
	}
	public override void willDestroy()
	{
		mDestroying = true;
		++mCallbackVersion;
		cancelAd();
		base.willDestroy();
	}
	public override void destroy()
	{
		mDestroying = true;
		++mCallbackVersion;
		cancelAd();
		base.destroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void showAd(bool rewarded, Action success, Action failure)
	{
		if (mDestroying || mPending)
		{
			failure?.Invoke();
			return;
		}
#if BYTE_DANCE || OPPO_MINI_GAME || VIVO_MINI_GAME
		string adUnitID = rewarded ? PlatformConfig.getRewardedVideoID() : PlatformConfig.getInterstitialID();
		if (string.IsNullOrWhiteSpace(adUnitID))
		{
			logWarning("未配置当前平台的" + (rewarded ? "激励视频" : "插屏") + "广告位 ID");
			failure?.Invoke();
			return;
		}
#if UNITY_WEBGL && !UNITY_EDITOR
		mSuccess = success;
		mFailure = failure;
		mPending = true;
		mShowing = false;
		mShowRequested = false;
		mLoadTime = 0.0f;
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
		logWarning("广告需要在对应平台的真机环境验证");
		failure?.Invoke();
#endif
#else
		success?.Invoke();
#endif
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
			mRewardedVideo.OnClose += (isEnded, count) => finish(requestID, isEnded);
			mRewardedVideo.Load();
		}
		else
		{
			mInterstitial = TT.CreateInterstitialAd(new CreateInterstitialAdParam { InterstitialAdId = adUnitID });
			mInterstitial.OnLoad += () => onLoaded(requestID);
			mInterstitial.OnError += (code, error) => onAdError(requestID, code + " " + error);
			mInterstitial.OnClose += () => finish(requestID, true);
			mInterstitial.Load();
		}
#else
		if (rewarded)
		{
			mRewardedVideo = QG.CreateRewardedVideoAd(new QGCommonAdParam { adUnitId = adUnitID });
			mRewardedVideo.OnLoad(() => onLoaded(requestID));
			mRewardedVideo.OnError(error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
			mRewardedVideo.OnClose((QGRewardedVideoResponse response) => finish(requestID, response != null && response.isEnded));
			mRewardedVideo.Load(response => onLoaded(requestID), error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
		}
		else
		{
			mInterstitial = QG.CreateInterstitialAd(new QGCommonAdParam { adUnitId = adUnitID });
			mInterstitial.OnLoad(() => onLoaded(requestID));
			mInterstitial.OnError(error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
#if VIVO_MINI_GAME
			mInterstitial.OnClose(() => finish(requestID, true));
#else
			mInterstitial.OnClose(response => finish(requestID, true));
			mInterstitial.Load(response => onLoaded(requestID), error => onAdError(requestID, error?.errCode + " " + error?.errMsg));
#endif
		}
#endif
	}
	protected void onLoaded(int requestID)
	{
		if (!mPending || requestID != mRequestID || mShowRequested)
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
		if (mPending && requestID == mRequestID)
		{
			mShowing = true;
		}
	}
#endif
	protected void onAdError(int requestID, string error)
	{
		if (!mPending || requestID != mRequestID)
		{
			return;
		}
		logWarning("广告失败:" + error);
		finish(requestID, false);
	}
	protected void finish(int requestID, bool success)
	{
		if (!mPending || requestID != mRequestID)
		{
			return;
		}
		mPending = false;
		mShowing = false;
		++mRequestID;
		Action callback = success ? mSuccess : mFailure;
		mSuccess = null;
		mFailure = null;
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
		callback?.Invoke();
	}
	protected BoolCallback createCallback(BoolCallback callback)
	{
		int version = mCallbackVersion;
		bool settled = false;
		return success =>
		{
			if (settled || mDestroying || version != mCallbackVersion)
			{
				return;
			}
			settled = true;
			callback?.Invoke(success);
		};
	}
}
