﻿using UnityEngine;

// 游戏枚举定义-----------------------------------------------------------------------------------------------

// 关键帧ID定义,对应关键帧预设中的曲线ID
public class KEY_CURVE
{
	public static int NONE = 0;
	// 内置的曲线
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
	public static int MAX_BUILDIN_CURVE = 100;
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
	NONE,					// 无效值
	WAIT_FOR_LOAD,			// 等待加载
	DOWNLOADING,			// 正在下载
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

// 鼠标按键枚举,同时也是作为鼠标的触点ID,因为鼠标和触点不会同时出现,所以不会冲突
public enum MOUSE_BUTTON : byte
{
	LEFT,					// 左键
	RIGHT,					// 右键
	MIDDLE,					// 中键
}

// 可用的组合键
public enum COMBINATION_KEY : byte
{ 
	NONE,				// 无效值
	CTRL = 1 << 0,		// Ctrl键掩码
	SHIFT = 1 << 1,		// Shift键掩码
	ALT = 1 << 2,		// Alt键掩码
}

// 输入事件的状态掩码,初衷是为了区分当前是正在输入状态还是普通状态下按的按键,避免正在输入时触发了快捷键
public enum FOCUS_MASK : ushort
{
	NONE = 0,				// 无掩码
	SCENE = 1 << 1,			// 可处理场景中的输入
	UI = 1 << 2,			// 输入框正在输入
}

// 滚动窗口的状态
public enum SCROLL_STATE : byte
{
	NONE,					// 无状态
	SCROLL_TO_TARGET,		// 匀减速滚动到目标点
	LERP_SCROLL_TO_TARGET,	// 插值滚动到目标点
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
	OVERLAP_LAYER,			// 会进行状态叠加的通知,但是同一时间只会保留一个状态,且层数要么清零,要么增加,层数不会出现逐渐减少的情况,因为持续时间只有一个,可以在状态内自己添加根据一定条件减少层数的逻辑
}

// 同一状态组中的状态互斥选项
public enum GROUP_MUTEX : byte
{
	COEXIST,				// 各状态可完全共存
	REMOVE_OTHERS,			// 添加新状态时移除组中的其他所有状态
	NO_NEW,					// 状态组中有状态时不允许添加新状态
	MUTEX_WITH_MAIN,		// 仅与主状态互斥,添加主状态时移除其他所有状态,有主状态时不可添加其他状态,没有主状态时可任意添加其他状态
	MUTEX_WITH_MAIN_ONLY,	// 仅与主状态互斥,添加主状态时移除其他所有状态,无论是否有主状态都可以添加其他状态
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
	HMS_2,                  // 以Hour:Minute:Second形式显示,并且每个数都显示为2位数
	MS_2,					// 以Minute:Second形式显示,并且每个数都显示为2位数
	DHMS_ZH,                // 以Day天Hour小时Minute分Second秒的形式显示,获取当前时间时将不会显示天数
	DHM_ZH,					// 以Day天Hour小时Minute分的形式显示,获取当前时间时将不会显示天数
	YMD_ZH,                 // 以Year年Month月Day天的形式显示,只适用于DateTime
	YMDHM_ZH,               // 以Year年Month月Day天Hour时Minute分的形式显示,只适用于DateTime
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

// 横向的方向
public enum HORIZONTAL_DIRECTION : byte
{
	NONE,			// 无效值
	LEFT,			// 向左并且位于父节点内停靠
	RIGHT,			// 向右并且位于父节点内停靠
	CENTER,         // 以中间停靠
}

// 纵向的方向
public enum VERTICAL_DIRECTION : byte
{
	NONE,			// 无效值
	TOP,			// 向上并且位于父节点内停靠
	BOTTOM,			// 向下并且位于父节点内停靠
	CENTER,         // 以中间停靠
}

// UI适配时的停靠类型
public enum ANCHOR_MODE : byte
{
	[EnumLabel("无")]
	NONE,                       // 无效值
	[EnumLabel("在父节点中停靠"), Tooltip("停靠父节点的指定边界,并且大小不改变")]
	PADDING_PARENT_SIDE,        // 停靠父节点的指定边界,并且大小不改变,0,1,2,3表示左上右下
	[EnumLabel("父节点拉伸"), Tooltip("将锚点设置到距离相对于父节点最近的边,并且各边界到父节点对应边界的距离固定不变")]
	STRETCH_TO_PARENT_SIDE,		// 将锚点设置到距离相对于父节点最近的边,并且各边界到父节点对应边界的距离固定不变
}

// 当mAnchorMode的值为STRETCH_TO_PARENT_SIDE时,x方向上要停靠的边界
public enum HORIZONTAL_PADDING : sbyte
{
	NONE = -1,		// 无效值
	LEFT_IN,		// 向左并且位于父节点内停靠
	LEFT_OUT,		// 向左并且位于父节点外停靠
	RIGHT_IN,		// 向右并且位于父节点内停靠
	RIGHT_OUT,		// 向右并且位于父节点外停靠
	CENTER,			// 以中间停靠
}

// 当mAnchorMode的值为STRETCH_TO_PARENT_SIDE时,y方向上要停靠的边界
public enum VERTICAL_PADDING : sbyte
{
	NONE = -1,		// 无效值
	TOP_IN,         // 向上并且位于父节点内停靠
	TOP_OUT,        // 向上并且位于父节点外停靠
	BOTTOM_IN,      // 向下并且位于父节点内停靠
	BOTTOM_OUT,     // 向下并且位于父节点外停靠
	CENTER,			// 以中间停靠
}

// 缩放比例的计算方式
public enum ASPECT_BASE : byte
{
	[EnumLabel("根据屏幕宽度进行缩放")]
	USE_WIDTH_SCALE,            // 使用宽的缩放值来缩放控件
	[EnumLabel("根据屏幕高度进行缩放")]
	USE_HEIGHT_SCALE,           // 使用高的缩放值来缩放控件
	[EnumLabel("根据屏幕宽高中缩放最小的进行缩放")]
	AUTO,                       // 取宽高缩放值中最小的,保证缩放以后不会超出屏幕范围
	[EnumLabel("根据屏幕宽高中缩放最大的进行缩放")]
	INVERSE_AUTO,               // 取宽高缩放值中最大的,保证缩放以后不会在屏幕范围留出空白
	[EnumLabel("不缩放")]
	NONE,						// 无效值
}

// 模型节点与角色节点的关系
public enum AVATAR_RELATIONSHIP : byte
{
	AVATAR_AS_CHARACTER,		// 模型节点就是角色节点
	AVATAR_ALONE,				// 模型节点是单独的节点,不挂接在角色节点下,不会自动同步两个节点的变换
	AVATAR_AS_CHILD,			// 模型节点是角色节点的子节点
}

// 模型节点与角色节点的同步方式
public enum TRANSFORM_SYNC : byte
{
	USE_AVATAR,					// 使用模型节点的值同步到角色节点
	USE_CHARACTER,				// 使用角色节点的值同步到模型节点
}

// 同步位置的时机
public enum TRANSFORM_SYNC_TIME : byte
{
	UPDATE,						// 执行Update时更新
	LATE_UPDATE,				// 执行LateUpdate时更新
	FIXED_UPDATE,				// 执行FixedUpdate时更新
}

// 摄像机连接器的更新时机
public enum LINKER_UPDATE : byte
{
	UPDATE,						// 执行Update时更新
	LATE_UPDATE,				// 执行LateUpdate时更新
	FIXED_UPDATE,				// 执行FixedUpdate时更新
}

// 角的类型
public enum CORNER : byte
{
	LEFT_TOP,					// 左上角
	LEFT_BOTTOM,				// 左下角
	RIGHT_TOP,					// 右上角
	RIGHT_BOTTOM,				// 右下角
}

// 布局的生命周期定义
public enum LAYOUT_LIFE_CYCLE : byte
{
	NONE,                       // 无效值
	PERSIST,					// 全局常驻
	PART_USE,					// 只在一些流程中使用
}

// 调整Content的子节点的方式
public enum CONTENT_ADJUST : byte
{ 
	NONE,					// 无效值,不改变子节点的位置
	SINGLE_COLUMN_OR_LINE,	// 自动按照单行或者单列进行排列子节点
	FIXED_WIDTH_OR_HEIGHT,	// 按照固定的父节点宽度或宽度,顺序排列子节点
}

// 摄像机碰撞检测类型
public enum CAMERA_COLLISION : byte
{
	NONE,                   // 无效值
	CHECK_MODEL_BETWEEN,    // 检查摄像机与角色之间是否有模型挡住
	CHECK_MODEL_INTERSECT,  // 检查摄像机是否与模型有
}

// http请求的方式
public enum HTTP_METHOD : byte
{
	NONE,       // 无效值
	POST,       // 以post方式
	GET,        // 以get方式
}

// 版本号比较结果
public enum VERSION_COMPARE : byte
{
	EQUAL,          // 版本号相同
	REMOTE_LOWER,   // 远端版本号更小
	LOCAL_LOWER,	// 本地版本号更小
}

// 多指操作手势的类型
public enum MULTI_TOUCH_GESTURE : byte
{
	NONE,                   // 无效值
	TWO_FINGER_SCALE,       // 双指缩放
	TWO_FINGER_MOVE,        // 双指移动
	TWO_FINGER_ROTATE,      // 双指在屏幕二维平面旋转
}

// 用于选择渐变方向
public enum GRADIENT_DIRECTION
{
	NONE,				// 无效值
	HORIZONTAL,         // 横向渐变
	VERTICAL            // 纵向渐变
}

// 用于选择颜色混合模式
public enum GRADIENT_BLEND
{
	NONE,				// 无效值
	OVERRIDE,           // 覆盖
	ADD,                // 相加
	MULTIPLY            // 相乘
}

// 安卓平台下的屏幕旋转方式
public enum ANDROID_ORIENTATION : sbyte
{
	NONE = -1,
	SCREEN_ORIENTATION_LANDSCAPE = 0,
	SCREEN_ORIENTATION_PORTRAIT = 1,
	SCREEN_ORIENTATION_USER = 2,
	SCREEN_ORIENTATION_BEHIND = 3,
	SCREEN_ORIENTATION_SENSOR = 4,
	SCREEN_ORIENTATION_NOSENSOR = 5,
	SCREEN_ORIENTATION_SENSOR_LANDSCAPE = 6,
	SCREEN_ORIENTATION_SENSOR_PORTRAIT = 7,
	SCREEN_ORIENTATION_REVERSE_LANDSCAPE = 8,
	SCREEN_ORIENTATION_REVERSE_PORTRAIT = 9,
	SCREEN_ORIENTATION_FULL_SENSOR = 10,
	SCREEN_ORIENTATION_USER_LANDSCAPE = 11,
	SCREEN_ORIENTATION_USER_PORTRAIT = 12,
	SCREEN_ORIENTATION_FULL_USER = 13,
	SCREEN_ORIENTATION_LOCKED = 14,
}

// 更新游戏执行的阶段
public enum PROGRESS_TYPE : byte
{
	CHECKING_UPDATE,    // 检查更新
	DELETE_FILE,        // 删除本地无用文件
	DOWNLOAD_RESOURCE,  // 正在下载资源文件
	FINISH,             // 更新完毕
	DOWNLOAD_APK,       // 正在下载安装包
	DOWNLOADED_APK,     // 安装包下载完毕
	INSTALL_APK,        // 正在安装apk
}

public enum DOWNLOAD_TIP : byte
{
	NONE,                       // 无效值
	CHECKING_UPDATE,            // 正在检查更新
	DOWNLOAD_FAILED,            // 文件下载失败
	NOT_IN_REMOTE_FILE_LIST,    // 已经下载的文件不存在于远端的文件列表中,一般不会有这个错误
	VERIFY_FAILED,              // 文件校验失败
}