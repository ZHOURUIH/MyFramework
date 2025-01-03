using UnityEngine;
using static MathUtility;

// 物体的抛物线移动组件
public class COMTransformableMoveParabola : ComponentKeyFrame, IComponentModifyPosition
{
	protected Vector3 mStartPos;    // 移动开始时的位置
	protected Vector3 mTargetPos;   // 移动目标位置
	protected float mDistanceHori;	// 起点到终点的水平方向上的距离
	protected float mTopHeight;     // 抛物线的最高点高度,相对于起点
	protected float mFactorA;       // mFactorA越大,抛物线的顶点越高
	protected float mFactorB;       // 根据mFactorA和起点,终点计算出的抛物线公式中的参数
	public override void resetProperty()
	{
		base.resetProperty();
		mStartPos = Vector3.zero;
		mTargetPos = Vector3.zero;
		mDistanceHori = 0.0f;
		mTopHeight = 0.0f;
		mFactorA = 0.0f;
		mFactorB = 0.0f;
	}
	public void setTargetPos(Vector3 pos) { mTargetPos = pos; }
	public void setStartPos(Vector3 pos) { mStartPos = pos; }
	public void setTopHeight(float top) { mTopHeight = abs(top); }
	public override void play(int keyframe, bool loop, float onceLength, float offset)
	{
		base.play(keyframe, loop, onceLength, offset);
		mDistanceHori = getLength(resetY(mTargetPos - mStartPos));
		generateParabola(mTopHeight, mStartPos, mTargetPos, out mFactorA, out mFactorB);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		Vector3 curPos = lerpSimple(mStartPos, mTargetPos, value);
		// 根据插值计算x,再代入抛物线方程计算y
		float x = value * mDistanceHori;
		curPos.y = mStartPos.y + mFactorA * x * x + mFactorB * x;
		(mComponentOwner as Transformable).setPosition(curPos);
	}
}