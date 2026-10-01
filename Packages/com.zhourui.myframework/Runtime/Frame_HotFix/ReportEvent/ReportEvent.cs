using Newtonsoft.Json;
using static TimeUtility;

public class ReportDataBody<T> where T : IResetProperty, new()
{
	public string game_id;
	public string event_name;
	public string user_id;
	public long timestamp;
	public T properties = new();
	public void resetProperty()
	{
		game_id = null;
		event_name = null;
		user_id = null;
		timestamp = 0;
		properties.resetProperty();
	}
}
public class ReportSendData<T> where T : IResetProperty, new()
{
	public ReportDataBody<T>[] logs = new ReportDataBody<T>[1];
	public ReportSendData()
	{
		logs[0] = new();
	}
	public void resetProperty()
	{
		foreach (ReportDataBody<T> item in logs)
		{
			item.resetProperty();
		}
	}
}

// 埋点对象基类
public class ReportEvent<T> : ClassObject, IReportEvent where T : IResetProperty, new()
{
	protected ReportSendData<T> mSendData = new();
	public string write(string gameID) 
	{
		mSendData.logs[0].game_id = gameID;
		mSendData.logs[0].event_name = getEventName();
		//mSendData.logs[0].user_id = mClientSystem.getComUserData().mUserID;
		mSendData.logs[0].timestamp = getNowUTCTimeStamp();
		return JsonConvert.SerializeObject(mSendData); 
	}
	public T data() { return mSendData.logs[0].properties; }
	public virtual string getEventName() { return null; }
	public override void resetProperty()
	{
		base.resetProperty();
		mSendData.resetProperty();
	}
}