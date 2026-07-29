using System;

[Serializable]
// 结构体,商品收据信息,包含收据数据和签名
public struct ProductReceipt
{
	public string Payload;
	public string Store;
	public string TransactionID;
}