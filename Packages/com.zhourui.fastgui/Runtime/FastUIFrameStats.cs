// FastCanvas最近一次完整更新的轻量帧级摘要。
// 细粒度阶段分析使用Unity Profiler Marker，避免在生产热路径维护额外细分计数。

public struct FastUIFrameStats
{
	public int mFrame;
	public double mCPUTimeMS;
	public int mRebuiltVertexCount;
	public int mVertexUploadCallCount;
	public int mUploadedVertexCount;
	public int mIndexUploadCallCount;
	public int mUploadedIndexCount;
	public int mBatchCount;
	public int mRenderElementCount;
	public int mActiveRenderElementCount;
	public int mClippedElementCount;
	public int mSOAGroupCount;
	public bool mIndexRebuilt;
	public bool mBatchRebuilt;
	public bool mVisibilityRebuilt;
	public bool mVertexBufferResized;
	public bool mIndexBufferResized;
}
