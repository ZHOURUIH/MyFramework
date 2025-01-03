using System.Net;

public class NetManager : FrameSystem
{
	protected NetConnectTCPBit mServerConnect;
	public override void init()
	{
		base.init();
		mServerConnect = new();
		mServerConnect.setName("Server");
		mServerConnect.init(IPAddress.Parse("000.000.000.000"), 0);
	}
	public void disconnect()
	{
		mServerConnect.disconnect();
	}
	public void connect()
	{
		mServerConnect.startConnect(null);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mServerConnect.update(elapsedTime);
	}
	public override void destroy()
	{
		mServerConnect.destroy();
	}
	public void sendPacket(NetPacketBit packet)
	{
		mServerConnect.sendNetPacket(packet);
	}
	public int getPing() { return mServerConnect.getPing(); }
	public NetConnectTCPBit getConnect() { return mServerConnect; }
}