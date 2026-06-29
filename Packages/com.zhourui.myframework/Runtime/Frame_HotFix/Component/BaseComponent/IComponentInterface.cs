
// 会修改位置的组件
public interface IComponentModifyPosition
{ }

// 会修改旋转的组件
public interface IComponentModifyRotation
{ }

// 会修改缩放的组件
public interface IComponentModifyScale
{ }

// 会修改透明度的组件
public interface IComponentModifyAlpha
{ }

// 会修改颜色的组件
public interface IComponentModifyColor
{ }

// 能够被其他组件中断的组件
public interface IComponentBreakable
{
	void notifyBreak();
}