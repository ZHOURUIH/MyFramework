
// 事件参数对象,每一个事件都会有对应的参数定义
public class GameEvent : ClassObject
{
	public long mCharacterGUID;		// 角色唯一ID
	public override void resetProperty()
	{
		base.resetProperty();
		mCharacterGUID = 0;
	}
}