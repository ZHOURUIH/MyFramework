using System;

[Serializable]
// 购买信息,包含商品ID和数量
public class PurchaseInfo
{
	public string purchaseToken;
	public string orderId;
	public string productId;
	public int state;
}