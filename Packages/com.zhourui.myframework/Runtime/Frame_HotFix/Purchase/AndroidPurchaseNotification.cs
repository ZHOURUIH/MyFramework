using Newtonsoft.Json;
#if USE_OBFUZ
using Obfuz;
#endif
using UnityEngine;
using static UnityUtility;

public class AndroidPurchaseNotification : MonoBehaviour
{
	protected String2Callback mSuccessedCallback;
	protected StringCallback mFailedCallback;
#if USE_OBFUZ
    [ObfuzIgnore]
#endif
	public void purchaseSuccess(string infoStr)
    {
		log("支付成功:" + infoStr);
		var info = JsonConvert.DeserializeObject<PurchaseInfo>(infoStr);
		mSuccessedCallback?.Invoke(info.productId, info.purchaseToken);
	}
#if USE_OBFUZ
    [ObfuzIgnore]
#endif
	public void purchaseCancel(string infoStr)
	{
		log("支付已取消");
		mFailedCallback?.Invoke(infoStr);
	}
#if USE_OBFUZ
    [ObfuzIgnore]
#endif
	public void purchaseFailed(string infoStr)
	{
		log("支付失败:" + infoStr);
		mFailedCallback?.Invoke(infoStr);
	}
	public void setSuccessedCallback(String2Callback callback) { mSuccessedCallback = callback; }
	public void setFailedCallback(StringCallback callback) { mFailedCallback = callback; }
}