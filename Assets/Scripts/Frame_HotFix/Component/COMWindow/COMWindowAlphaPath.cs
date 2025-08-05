
// 使UI按指定的透明度关键帧列表进行变化的组件
public class COMWindowAlphaPath : ComponentPathAlpha, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		var obj = mComponentOwner as myUGUIObject;
		obj.setAlpha(value, false);
	}
}