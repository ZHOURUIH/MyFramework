using System;

// 游戏枚举定义-----------------------------------------------------------------------------------------------

// 应用程序配置参数
public class APPLICATION_FLOAT
{
	public static int NONE = 0;
	public static int FULL_SCREEN = 1;              // 是否全屏,0为窗口模式,1为全屏,2为无边框窗口
	public static int SCREEN_WIDTH = 2;             // 分辨率的宽
	public static int SCREEN_HEIGHT = 3;            // 分辨率的高
	public static int FORCE_TOP = 4;                // 是否将窗口置顶
	public static int USE_FIXED_TIME = 5;           // 是否将每帧的时间固定下来
	public static int FIXED_TIME = 6;               // 每帧的固定时间,单位秒
	public static int VSYNC = 7;                    // 垂直同步,0为关闭垂直同步,1为开启垂直同步
}

// 框架配置参数
public class FRAME_FLOAT
{
	public static int NONE = 0;
	public static int LOAD_RESOURCES = 1;             // 游戏加载资源的路径,0代表在Resources中读取,1代表从AssetBundle中读取
	public static int LOG_LEVEL = 2;                  // 日志输出等级
}

// 关键帧ID定义,对应关键帧预设中的曲线ID
public class KEY_CURVE
{
	public static int NONE = 0;
	// 内置的曲线
	public static int BUILDIN_CURVE = 1;
	public static int ZERO_ONE = 2;
	public static int ZERO_ONE_ZERO = 3;
	public static int ONE_ZERO = 4;
	public static int ONE_ZERO_ONE = 5;
	public static int BACK_IN = 6;
	public static int BACK_OUT = 7;
	public static int BACK_IN_OUT = 8;
	public static int BOUNCE_IN = 9;
	public static int BOUNCE_OUT = 10;
	public static int BOUNCE_IN_OUT = 11;
	public static int CIRCLE_IN = 12;
	public static int CIRCLE_OUT = 13;
	public static int CIRCLE_IN_OUT = 14;
	public static int CUBIC_IN = 15;
	public static int CUBIC_OUT = 16;
	public static int CUBIC_IN_OUT = 17;
	public static int ELASTIC_IN = 18;
	public static int ELASTIC_OUT = 19;
	public static int ELASTIC_IN_OUT = 20;
	public static int EXPO_IN = 21;
	public static int EXPO_OUT = 22;
	public static int EXPO_IN_OUT = 23;
	public static int QUAD_IN = 24;
	public static int QUAD_OUT = 25;
	public static int QUAD_IN_OUT = 26;
	public static int QUART_IN = 27;
	public static int QUART_OUT = 28;
	public static int QUART_IN_OUT = 29;
	public static int QUINT_IN = 30;
	public static int QUINT_OUT = 31;
	public static int QUINT_IN_OUT = 32;
	public static int SINE_IN = 33;
	public static int SINE_OUT = 34;
	public static int SINE_IN_OUT = 35;
}

// 数字窗口中数字的停靠位置
public enum DOCKING_POSITION : byte
{
	LEFT,					// 横向排列时,向左停靠
	RIGHT,					// 横向排列时,向右停靠
	CENTER,					// 中间对齐
	TOP,					// 纵向排列时,向顶部停靠
	BOTTOM,					// 纵向排列时,向底部停靠
}

// 数字窗口中数字的排列方向
public enum NUMBER_DIRECTION : byte
{
	HORIZONTAL,				// 横向排列
	VERTICAL,				// 纵向排列
}

// 循环方式
public enum LOOP_MODE : byte
{
	ONCE,					// 单次不循环
	LOOP,					// 循环
	PING_PONG,				// 以来回的方式循环
}

// 播放状态
public enum PLAY_STATE : byte
{
	NONE,					// 无效状态
	PLAY,					// 正在播放
	PAUSE,					// 已暂停
	STOP,					// 已停止
}

// 加载状态
public enum LOAD_STATE : byte
{
	NONE,					// 无效状态
	UNLOAD,					// 已卸载
	WAIT_FOR_LOAD,			// 等待加载
	LOADING,				// 正在加载
	LOADED,					// 已加载
}

// 拖拽滑动操作相关的方向类型
public enum DRAG_DIRECTION : byte
{
	HORIZONTAL,				// 只能横向滑动
	VERTICAL,				// 只能纵向滑动
	FREE,					// 可自由滑动
}

// 滑动组件滑动区域限制类型
public enum CLAMP_TYPE : byte
{
	CENTER_IN_RECT,			// 限制中心点不能超过父窗口的区域
	EDGE_IN_RECT,			// 限制边界不能超过父窗口的区域
}

// 网络状态
public enum NET_STATE : byte
{
	NONE,					// 无效值
	CONNECTING,				// 正在连接
	CONNECTED,				// 已连接
	SERVER_CLOSE,			// 服务器已关闭
	NET_CLOSE,				// 网络已断开
}

// 网络消息解析结果
public enum PARSE_RESULT : byte
{
	SUCCESS,				// 解析成功
	ERROR,					// 解析错误
	NOT_ENOUGH,				// 数据量不足
}

// 日志等级
public enum LOG_LEVEL : byte
{
	FORCE,					// 强制显示
	HIGH,					// 高
	NORMAL,					// 正常
	LOW,					// 低
	MAX,					// 无效值
}

// 摄像机碰撞的检测方向
public enum CHECK_DIRECTION : byte
{
	DOWN,					// 向下碰撞检测,检测到碰撞后,摄像机向上移动
	UP,						// 向上检测
	LEFT,					// 向左检测
	RIGHT,					// 向右检测
	FORWARD,				// 向前检测
	BACK,					// 向后检测
}

// 鼠标按键
public enum MOUSE_BUTTON : byte
{
	LEFT,					// 左键
	RIGHT,					// 右键
	MIDDLE,					// 中键
}

// 输入事件的状态掩码
public enum FOCUS_MASK : ushort
{
	NONE = 0,				// 无掩码
	SCENE = 1 << 1,			// 可处理场景中的输入
	UI = 1 << 2,			// 可处理UI的输入
	OTHER = 1 << 3,			// 可处理其他输入
}

// 滚动窗口的状态
public enum SCROLL_STATE : byte
{
	NONE,					// 无状态
	SCROLL_TARGET,			// 自动匀速滚动到目标点
	DRAGING,				// 鼠标拖动
	SCROLL_TO_STOP,			// 鼠标抬起后自动减速到停止
}

// 滑动条实现方式的类型
public enum SLIDER_MODE : byte
{
	FILL,					// 通过调整图片填充来实现滑动条
	SIZING,					// 通过调整窗口大小来实现滑动条
}

// 添加同一状态时的操作选项
public enum STATE_MUTEX : byte
{
	NO_NEW,					// 不可添加相同的新状态
	REMOVE_OLD,				// 添加新状态,移除互斥的旧状态
	COEXIST,				// 新旧状态可共存
	KEEP_HIGH_PRIORITY,		// 保留新旧状态中优先级最高的
}

// 同一状态组中的状态互斥选项
public enum GROUP_MUTEX : byte
{
	COEXIST,				// 各状态可完全共存
	REMOVE_OTHERS,			// 添加新状态时移除组中的其他所有状态
	NO_NEW,					// 状态组中有状态时不允许添加新状态
	MUTEX_WITH_MAIN,		// 仅与主状态互斥,添加主状态时移除其他所有状态,有主状态时不可添加其他状态,没有主状态时可任意添加其他状态
	MUTEX_WITH_MAIN_ONLY,   // 仅与主状态互斥,添加主状态时移除其他所有状态,无论是否有主状态都可以添加其他状态
	MUTEX_INVERSE_MAIN,		// 主状态反向互斥,有其他状态时,不允许添加主状态,添加其他状态时,立即将主状态移除
}

// 命令执行状态
public enum EXECUTE_STATE : byte
{
	NOT_EXECUTE,			// 未执行
	EXECUTING,				// 正在执行
	EXECUTED,				// 已执行
}

// 寻路中的节点状态
public enum NODE_STATE : byte
{
	NONE,					// 无状态
	OPEN,					// 在开启列表中
	CLOSE,					// 在关闭列表中
}

// 加载资源的来源
public enum LOAD_SOURCE : byte
{
	RESOURCES,				// 从Resources加载
	ASSET_BUNDLE,			// 从AssetBundle加载
}

// 状态的buff类型
public enum BUFF_STATE_TYPE : byte
{
	NONE,					// 既不属于buff,也不属于debuff
	BUFF,					// 属于buff
	DEBUFF,					// 属于debuff
}

// 时间字符串显示格式
public enum TIME_DISPLAY : byte
{
	HMSM,					// 以Hour:Minute:Second:Millisecond形式显示,并且不补0
	HMS_2,					// 以Hour:Minute:Second形式显示,并且每个数都显示为2位数
	DHMS_ZH,				// 以Day天Hour小时Minute分Second秒的形式显示
	YMD_ZH,					// 以Year年Month月Day天的形式显示
}

// 布局渲染顺序的计算类型
public enum LAYOUT_ORDER : byte
{
	ALWAYS_TOP,				// 总在最上层,并且需要自己指定渲染顺序
	ALWAYS_TOP_AUTO,		// 总在最上层,并且自动计算渲染顺序,设置为最上层中的最大深度
	AUTO,					// 自动计算,布局显示时设置为除开所有总在最上层的布局中的最大深度
	FIXED,					// 固定渲染顺序,并且不可以超过总在最上层的布局
}

// 每一帧校正图片位置时的选项
public enum EFFECT_ALIGN : byte
{
	NONE,					// 特效中心对齐父节点中心
	PARENT_BOTTOM,			// 特效底部对齐父节点底部
	POSITION_LIST,			// 使用位置列表对每一帧进行校正
}

// 角度的单位
public enum ANGLE : byte
{ 
	RADIAN,					// 弧度制
	DEGREE,					// 角度制
}

public enum ANCHOR_MODE : byte
{
	NONE,						// 无效值
	PADDING_PARENT_SIDE,		// 停靠父节点的指定边界,并且大小不改变,0,1,2,3表示左上右下
	STRETCH_TO_PARENT_SIDE,		// 将锚点设置到距离相对于父节点最近的边,并且各边界到父节点对应边界的距离固定不变
}

// 当mAnchorMode的值为STRETCH_TO_PARENT_SIDE时,x方向上要停靠的边界
public enum HORIZONTAL_PADDING : sbyte
{
	NONE = -1,
	LEFT,
	RIGHT,
	CENTER,
}

// 当mAnchorMode的值为STRETCH_TO_PARENT_SIDE时,y方向上要停靠的边界
public enum VERTICAL_PADDING : sbyte
{
	NONE = -1,
	TOP,
	BOTTOM,
	CENTER,
}

// 缩放比例的计算方式
public enum ASPECT_BASE : byte
{
	USE_WIDTH_SCALE,			// 使用宽的缩放值来缩放控件
	USE_HEIGHT_SCALE,			// 使用高的缩放值来缩放控件
	AUTO,						// 取宽高缩放值中最小的,保证缩放以后不会超出屏幕范围
	INVERSE_AUTO,				// 取宽高缩放值中最大的,保证缩放以后不会在屏幕范围留出空白
	NONE,
}

// 模型节点与角色节点的关系
public enum AVATAR_RELATIONSHIP : byte
{
	AVATAR_AS_CHARACTER,    // 模型节点就是角色节点
	AVATAR_ALONE,           // 模型节点是单独的节点,不挂接在角色节点下,不会自动同步两个节点的变换
	AVATAR_AS_CHILD,        // 模型节点是角色节点的子节点
}

// 模型节点与角色节点的同步方式
public enum TRANSFORM_ASYNC : byte
{
	USE_AVATAR,             // 使用模型节点的值同步到角色节点
	USE_CHARACTER,          // 使用角色节点的值同步到模型节点
}

// 摄像机连接器的更新时机
public enum LINKER_UPDATE : byte
{
	UPDATE,                 // 执行Update时更新
	LATE_UPDATE,            // 执行LateUpdate时更新
	FIXED_UPDATE,           // 执行FixedUpdate时更新
}