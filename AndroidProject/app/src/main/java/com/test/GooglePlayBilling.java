//package 包名;
//
//import com.android.billingclient.api.AcknowledgePurchaseParams;
//import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
//import com.android.billingclient.api.BillingClient;
//import com.android.billingclient.api.BillingClientStateListener;
//import com.android.billingclient.api.BillingFlowParams;
//import com.android.billingclient.api.BillingResult;
//import com.android.billingclient.api.ProductDetails;
//import com.android.billingclient.api.Purchase;
//import com.android.billingclient.api.PurchasesUpdatedListener;
//import com.android.billingclient.api.QueryProductDetailsParams;
//import android.app.Activity;
//import com.google.gson.Gson;
//import com.unity3d.player.UnityPlayer;
//
//import java.util.ArrayList;
//import java.util.Arrays;
//import java.util.List;
//import androidx.annotation.NonNull;
//import androidx.annotation.Nullable;
//
//public class GooglePlayBilling implements PurchasesUpdatedListener {
//    private BillingClient billingClient;
//    private Activity activity;
//    private static String mAllProductInfos;
//    public GooglePlayBilling(Activity activity) {
//        this.activity = activity;
//        initializeBillingClient();
//    }
//    private void initializeBillingClient() {
//        billingClient = BillingClient.newBuilder(activity)
//                .setListener(this)
//                .enablePendingPurchases()  // 仍然需要此方法
//                .build();
//        startBillingConnection();
//    }
//    public static String getAllProducts() { return mAllProductInfos; }
//    private void startBillingConnection()
//    {
//        billingClient.startConnection(new BillingClientStateListener()
//        {
//            @Override
//            public void onBillingSetupFinished(BillingResult billingResult)
//            {
//                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK)
//                {
//                    MainClass.unityLog("Billing Client connected.");
//                    List<QueryProductDetailsParams.Product> productList = new ArrayList<>();
// 这里填写的是测试的ProductID,从PlayConsole中商店配置获取
//                    List<String> productIDs = Arrays.asList("charge_0.99",
//                                                            "charge_19.99",
//                                                            "charge_3.99",
//                                                            "charge_49.99",
//                                                            "charge_9.99",
//                                                            "charge_99.99");
//                    for (String id : productIDs)
//                    {
//                        productList.add(QueryProductDetailsParams.Product.newBuilder()
//                                .setProductId(id)
//                                .setProductType(BillingClient.ProductType.INAPP)
//                                .build());
//                    }
//                    QueryProductDetailsParams params = QueryProductDetailsParams.newBuilder()
//                            .setProductList(productList)
//                            .build();
//
//                    billingClient.queryProductDetailsAsync(params, (result, productDetailsList) ->
//                    {
//                        if (result.getResponseCode() == BillingClient.BillingResponseCode.OK && productDetailsList != null)
//                        {
//                            mAllProductInfos = "[";
//                            for (ProductDetails details : productDetailsList)
//                            {
//                                String productId = details.getProductId();
//                                String price = details.getOneTimePurchaseOfferDetails().getFormattedPrice();
//                                String currency = details.getOneTimePurchaseOfferDetails().getPriceCurrencyCode();
//                                mAllProductInfos += "{\"productId\":\"" + productId + "\",";
//                                mAllProductInfos += "\"price\":\"" + price + "\",";
//                                mAllProductInfos += "\"currency\":\"" + currency + "\"},";
//                            }
//                            mAllProductInfos = mAllProductInfos.substring(0, mAllProductInfos.length() - 1);
//                            mAllProductInfos += "]";
//                        }
//                        else
//                        {
//                            MainClass.unityLog("Query failed: " + result.getDebugMessage());
//                        }
//                    });
//                }
//            }
//
//            @Override
//            public void onBillingServiceDisconnected() {
//                MainClass.unityLog("Billing Client disconnected.");
//            }
//        });
//    }
//    public void purchaseItem(String productId) {
//        MainClass.unityLog("start purchase item : " + productId);
//        List<QueryProductDetailsParams.Product> productList = new ArrayList<>();
//        productList.add(QueryProductDetailsParams.Product.newBuilder()
//                .setProductId(productId)
//                .setProductType(BillingClient.ProductType.INAPP)  // 设置为 INAPP 或 SUBS
//                .build());
//
//        QueryProductDetailsParams params = QueryProductDetailsParams.newBuilder()
//                .setProductList(productList)
//                .build();
//
//        billingClient.queryProductDetailsAsync(params, (billingResult, productDetailsList) -> {
//            if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK && productDetailsList != null) {
//                for (ProductDetails productDetails : productDetailsList) {
//                    if (productDetails.getProductId().equals(productId)) {
//                        BillingFlowParams billingFlowParams = BillingFlowParams.newBuilder()
//                                .setProductDetailsParamsList(
//                                        List.of(BillingFlowParams.ProductDetailsParams.newBuilder()
//                                                .setProductDetails(productDetails)
//                                                .build()))
//                                .build();
//                        billingClient.launchBillingFlow(activity, billingFlowParams);
//                        break;
//                    }
//                }
//            } else {
//                MainClass.unityError("Failed to query product details: " + billingResult.getDebugMessage());
//            }
//        });
//    }
//    @Override
//    public void onPurchasesUpdated(@NonNull BillingResult billingResult, @Nullable List<Purchase> list)
//    {
//        if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK && list != null)
//        {
//            for (Purchase purchase : list)
//            {
//                PurchaseInfo info = new PurchaseInfo();
//                info.purchaseToken = purchase.getPurchaseToken();
//                info.orderId = purchase.getOrderId();
//                if (!purchase.getProducts().isEmpty())
//                {
//                    info.productId = purchase.getProducts().get(0);
//                }
//                else
//                {
//                    MainClass.unityLog("no product in purchase.getProducts()");
//                }
//                info.state = purchase.getPurchaseState();
//                Gson gson = new Gson();
//                String jsonStr = gson.toJson(info);
//                MainClass.unityLog("purchase success:" + jsonStr);
//                UnityPlayer.UnitySendMessage("AndroidPurchasing", "purchaseSuccess", jsonStr);
//            }
//        }
//        else if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.USER_CANCELED)
//        {
//            MainClass.unityLog("purchase canceled");
//            UnityPlayer.UnitySendMessage("AndroidPurchasing", "purchaseCancel", "cancel");
//        }
//        else
//        {
//            MainClass.unityLog("purchase failed," + billingResult.getResponseCode());
//            UnityPlayer.UnitySendMessage("AndroidPurchasing", "purchaseFailed", "code:" + billingResult.getResponseCode());
//        }
//    }
//    public void notifyServerProcessed(String purchaseToken)
//    {
//        AcknowledgePurchaseParams acknowledgePurchaseParams =
//                AcknowledgePurchaseParams.newBuilder()
//                        .setPurchaseToken(purchaseToken)
//                        .build();
//        billingClient.acknowledgePurchase(acknowledgePurchaseParams, new AcknowledgePurchaseResponseListener()
//        {
//            @Override
//            public void onAcknowledgePurchaseResponse(BillingResult billingResult)
//            {
//                if (billingResult.getResponseCode() == BillingClient.BillingResponseCode.OK)
//                {
//                    // 确认成功，继续后续操作
//                    MainClass.unityLog("Purchase acknowledged successfully.");
//                }
//                else
//                {
//                    // 处理确认失败的情况
//                    MainClass.unityLog("Failed to acknowledge purchase: " + billingResult.getDebugMessage());
//                }
//            }
//        });
//    }
//}