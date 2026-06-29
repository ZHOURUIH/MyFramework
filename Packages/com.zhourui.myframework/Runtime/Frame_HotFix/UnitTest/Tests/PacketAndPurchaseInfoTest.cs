using static TestAssert;

public static class PacketAndPurchaseInfoTest
{
	public static void Run()
	{
		testPacketReceiveInfoConstructor();
		testPacketSendInfoConstructor();
		testProductStructsAndPurchaseInfo();
	}
	private static void testPacketReceiveInfoConstructor()
	{
		byte[] data = { 1, 2, 3 };
		PacketReceiveInfo info = new(data, 0xF0UL, 3, 99U, 12, true);
		assertEqual(data, info.mPacketData);
		assertEqual(0xF0UL, info.mFieldFlag);
		assertEqual(3, info.mPacketSize);
		assertEqual(99U, info.mSequence);
		assertEqual((ushort)12, info.mType);
		assertTrue(info.mHasSign);
	}
	private static void testPacketSendInfoConstructor()
	{
		byte[] data = { 7, 8 };
		PacketSendInfo info = new(data, 2, true, 1001);
		assertEqual(data, info.mData);
		assertEqual(2, info.mDataSize);
		assertTrue(info.mDataNeedDestroy);
		assertEqual(1001, info.mPacketType);
	}
	private static void testProductStructsAndPurchaseInfo()
	{
		ProductInfo product = new(){ productId = "coin_1", price = "100", currency = "JPY" };
		assertEqual("coin_1", product.productId);
		assertEqual("100", product.price);
		assertEqual("JPY", product.currency);
		ProductReceipt receipt = new(){ Payload = "payload", Store = "GooglePlay", TransactionID = "tx" };
		assertEqual("payload", receipt.Payload);
		assertEqual("GooglePlay", receipt.Store);
		assertEqual("tx", receipt.TransactionID);
		PurchaseInfo purchase = new(){ purchaseToken = "token", orderId = "order", productId = "coin_1", state = 1 };
		assertEqual("token", purchase.purchaseToken);
		assertEqual("order", purchase.orderId);
		assertEqual("coin_1", purchase.productId);
		assertEqual(1, purchase.state);
	}
}