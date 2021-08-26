using System;

// 加载一个布局,一般由LT调用
public class CmdLayoutManagerLoad : Command 
{
	public LayoutAsyncDone mCallback;		// 异步加载时的完成回调
	public LAYOUT_ORDER mOrderType;			// 布局的顺序类型
	public string mParam;					// 布局显示或隐藏时要传递的参数
	public int mRenderOrder;				// 渲染顺序,与mOrderType配合使用
	public int mLayoutID;					// 布局ID
	public bool mImmediatelyShow;			// 是否跳过显示动效立即显示
	public bool mVisible;					// 加载完毕后是否显示
	public bool mIsScene;					// 是否不挂接到UGUIRoot下,不挂接在UGUIRoot下将由其他摄像机进行渲染
	public bool mAsync;						// 是否异步加载
	public override void resetProperty()
	{
		base.resetProperty();
		mCallback = null;
		mLayoutID = LAYOUT.NONE;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mParam = null;
		mVisible = true;
		mAsync = false;
		mImmediatelyShow = false;
		mIsScene = false;
		mRenderOrder = 0;
	}
	public override void execute()
	{
		LayoutInfo info = new LayoutInfo();
		info.mID = mLayoutID;
		info.mRenderOrder = mRenderOrder;
		info.mOrderType = mOrderType;
		info.mIsScene = mIsScene;
		info.mCallback = mCallback;
		GameLayout layout = mLayoutManager.createLayout(info, mAsync);
		if (layout == null)
		{
			return;
		}
		// 计算实际的渲染顺序
		int renderOrder = mLayoutManager.generateRenderOrder(layout, mRenderOrder, mOrderType);
		// 顺序有改变,则设置最新的顺序
		if (layout.getRenderOrder() != renderOrder)
		{
			CMD(out CmdLayoutManagerRenderOrder cmd);
			cmd.mLayoutID = mLayoutID;
			cmd.mRenderOrder = renderOrder;
			pushCommand(cmd, mLayoutManager);
		}
		if (mVisible)
		{
			layout.setVisible(mVisible, mImmediatelyShow, mParam);
		}
		// 隐藏时需要设置强制隐藏,不通知脚本,因为通常这种情况只是想后台加载布局
		else
		{
			layout.setVisibleForce(mVisible);
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(mVisible, layout);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mLayoutID:", mLayoutID).
				append(", mVisible:", mVisible).
				append(", mRenderOrder:", mRenderOrder).
				append(", mAsync:", mAsync).
				append(", mParam:", mParam);
	}
}