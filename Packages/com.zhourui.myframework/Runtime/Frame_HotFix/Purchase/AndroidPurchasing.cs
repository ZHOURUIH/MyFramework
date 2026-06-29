using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using static FrameBaseUtility;

public class AndroidPurchasing : FrameSystem
{
#if USE_PURCHASING
	protected AndroidJavaObject mGooglePlayBilling;
	protected AndroidPurchaseNotification mNotification;
	protected Dictionary<string, ProductInfo> mAllInfo;
	public AndroidPurchasing()
	{
		if (!isEditor() && isAndroid())
		{
			mCreateObject = true;
		}
	}
	public override void init()
	{
		base.init();
		if (!isEditor() && isAndroid())
		{
			mNotification = mObject.AddComponent<AndroidPurchaseNotification>();
		}
		// 初始化 Java 插件
		mGooglePlayBilling = new AndroidJavaObject(AndroidPluginManager.getPackageName() + ".GooglePlayBilling", AndroidPluginManager.getMainActivity());
	}
	// 参数分别为:productID,purchaseToken
	public void setSuccessCallback(String2Callback callback) 
	{
		if (mNotification != null)
		{
			mNotification.setSuccessedCallback(callback);
		}
	}
	public void setFailedCallback(StringCallback callback) 
	{
		if (mNotification != null)
		{
			mNotification.setFailedCallback(callback);
		}
	}
	// 拉起支付界面,会通过设置的回调通知购买结果
	public void purchaseItem(string productId)
	{
		mGooglePlayBilling?.Call("purchaseItem", productId);
	}
	// 在游戏服务器处理完毕后调用,无论服务器校验成功失败都需要调用
	public void notifyServerProcessed(string purchaseToken)
	{
		mGooglePlayBilling?.Call("notifyServerProcessed", purchaseToken);
	}
	public ProductInfo getProductInfo(string productID)
	{
		return getAllProducts().get(productID);
	}
	public Dictionary<string, ProductInfo> getAllProducts()
	{
		if (mAllInfo == null)
		{
			mAllInfo = new();
			string allInfo = mGooglePlayBilling?.CallStatic<string>("getAllProducts");
			if (allInfo.isEmpty())
			{
				return mAllInfo;
			}
			var allInfoList = JsonConvert.DeserializeObject<List<ProductInfo>>(allInfo);
			foreach (ProductInfo info in allInfoList.safe())
			{
				mAllInfo.add(info.productId, info);
			}
		}
		return mAllInfo;
	}
#endif
}