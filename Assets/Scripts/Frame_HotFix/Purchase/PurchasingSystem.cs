#if USE_PURCHASING
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using static UnityUtility;
using static FrameEditorUtility;

// 支付系统,暂时只有iOS和GooglePlay的支付,但是GooglePlay总是会出现无法获得商品列表的问题,所以GooglePlay使用AndroidPurchaseSystem
public class PurchasingSystem : FrameSystem, IDetailedStoreListener
{
	protected IStoreController mStoreController;
	protected IExtensionProvider mExtensionProvider;
	protected IAppleExtensions mAppleExtension;
	protected String2Callback mSuccessedCallback;
	protected StringCallback mFailedCallback;
	protected Product mPendingProduct;					// 正在处理支付的商品对象,ios
	// 初始化商品,建议在游戏初始化完成的时候就去初始化商品
	public async void initPurchase(ICollection<string> goodsIDList, bool isSandbox, String2Callback successedCallback, StringCallback failedCallback)
	{
		mSuccessedCallback = successedCallback;
		mFailedCallback = failedCallback;
		await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(isSandbox ? "sandbox" : "production"));
		AppStore storeType = isIOS() ? AppStore.AppleAppStore : AppStore.GooglePlay;
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance(storeType));
		foreach (string id in goodsIDList)
		{
			builder.AddProduct(id, ProductType.Consumable);
		}
		UnityPurchasing.Initialize(this, builder);
	}
	// 发起内购
	public void purchase(string productId)
	{
		if (productId == null)
		{
			return;
		}
		if (mStoreController == null || mExtensionProvider == null)
		{
			onFailedCallback("Not initialized.");
			return;
		}
		Product product = mStoreController.products.WithID(productId);
		if (product == null || !product.availableToPurchase)
		{
			onFailedCallback("Either is not found or is not available for purchase");
			return;
		}

		log("purchase " + product.metadata.localizedTitle);
		mStoreController.InitiatePurchase(product);
	}
	// 服务器处理完毕后,需要通知IAP支付处理完成,无论服务器处理成功还是失败都需要调用
	public void notifyReceiptProcessDone()
	{
		mStoreController.ConfirmPendingPurchase(mPendingProduct);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// Interface
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		mStoreController = controller;
		mExtensionProvider = extensions;
		mAppleExtension = extensions.GetExtension<IAppleExtensions>();
	}
	public void OnInitializeFailed(InitializationFailureReason error)
	{
		logWarning("OnInitializeFailed Reason:" + error);
	}
	public void OnInitializeFailed(InitializationFailureReason error, string message)
	{
		logWarning("OnInitializeFailed Reason:" + error + ", " + message);
	}
	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		logWarning("OnPurchaseFailedproduct:" + product.transactionID + "  failureReason:" + failureReason);
	}
	public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
	{
		logWarning("OnPurchaseFailedproduct:" + product.transactionID + "  failureReason:" + failureDescription.message);
	}
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
	{
		mPendingProduct = purchaseEvent.purchasedProduct;
		var info = JsonConvert.DeserializeObject<ProductReceipt>(purchaseEvent.purchasedProduct.receipt);
		log("支付成功:" + purchaseEvent.purchasedProduct.definition.id);
		mSuccessedCallback?.Invoke(info.Payload, purchaseEvent.purchasedProduct.definition.id);
		// 需要服务器校验的支付只能返回Pending
		return PurchaseProcessingResult.Pending;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// IOS恢复内购,Google会在删除应用后，第一次安装是自动恢复
	protected void IosRestore(Action<bool, string> restoreCallback)
	{
		if (mAppleExtension != null)
		{
			mAppleExtension.RestoreTransactions(restoreCallback);
		}
		else
		{
			logWarning("IAppleExtensions is null");
			restoreCallback(false, null);
		}
	}
	protected void onFailedCallback(string reason)
	{
		log("支付失败:" + reason);
		mFailedCallback?.Invoke(reason);
	}
}
#else
public class PurchasingSystem : FrameSystem
{}
#endif