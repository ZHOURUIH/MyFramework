using UnityEngine;
using static TestAssert;

// 补充覆盖内购回调组件、PurchaseInfo 与 CurveInfo
public static class PurchaseAndCurveInfoTest
{
	public static void Run()
	{
		testPurchaseInfoStoresValues();
		testAndroidPurchaseNotificationCallbacks();
		testCurveInfoConstructor();
	}

	private static void testPurchaseInfoStoresValues()
	{
		PurchaseInfo info = new()
		{
			productId = "product",
			purchaseToken = "token",
			orderId = "order",
			state = 2,
		};
		assertEqual("product", info.productId);
		assertEqual("token", info.purchaseToken);
		assertEqual("order", info.orderId);
		assertEqual(2, info.state);
	}

	private static void testAndroidPurchaseNotificationCallbacks()
	{
		GameObject go = new("AndroidPurchaseNotificationTest");
		try
		{
			AndroidPurchaseNotification notification = go.AddComponent<AndroidPurchaseNotification>();
			string successProduct = null;
			string successToken = null;
			string failedInfo = null;
			notification.setSuccessedCallback((product, token) =>
			{
				successProduct = product;
				successToken = token;
			});
			notification.setFailedCallback(info => failedInfo = info);

			notification.purchaseSuccess("{\"productId\":\"p1\",\"purchaseToken\":\"t1\"}");
			assertEqual("p1", successProduct, "成功回调 productId 错误");
			assertEqual("t1", successToken, "成功回调 token 错误");

			notification.purchaseCancel("cancel");
			assertEqual("cancel", failedInfo, "取消回调错误");
			notification.purchaseFailed("failed");
			assertEqual("failed", failedInfo, "失败回调错误");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}

	private static void testCurveInfoConstructor()
	{
		AnimationCurve curve = AnimationCurve.Linear(0, 1, 2, 3);
		CurveInfo info = new(10, "move", curve);
		assertEqual(10, info.mID);
		assertEqual("move", info.mName);
		assertEqual(curve, info.mCurve);
	}
}