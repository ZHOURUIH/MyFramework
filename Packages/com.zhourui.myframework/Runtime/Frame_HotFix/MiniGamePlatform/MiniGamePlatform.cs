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
	protected int mCallbackVersion;
	public override void init()
	{
		base.init();
		mDestroying = false;
		++mCallbackVersion;
	}
	public void showReward(Action success, Action failure)
	{
		if (mADManager == null)
		{
			failure?.Invoke();
			return;
		}
		mADManager.showReward(success, failure);
	}
	public void showInterstitial(Action success, Action failure)
	{
		if (mADManager == null)
		{
			failure?.Invoke();
			return;
		}
		mADManager.showInterstitial(success, failure);
	}
	public void cancelAd()
	{
		mADManager?.cancelAd();
	}
	public bool isAdRequestInProgress()
	{
		return mADManager != null && mADManager.isAdRequestInProgress();
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
	public override void willDestroy()
	{
		mDestroying = true;
		++mCallbackVersion;
		base.willDestroy();
	}
	public override void destroy()
	{
		mDestroying = true;
		++mCallbackVersion;
		base.destroy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
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
