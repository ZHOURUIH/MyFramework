using System;

// Frame需要的枚举值,每个项目的值都不同,但是Frame中需要使用
// 界面布局定义
public class LAYOUT_ILR
{
	public static int NONE = 1000;
	public static int LOGIN = 1001;
	public static int GAMING = 1002;
};

// 关键帧ID定义,对应关键帧预设中的曲线ID
public class KEY_CURVE_ILR
{
	// 自定义曲线,从101开始
	public static int ONE_ZERO_ONE_CURVE = 101;
	public static int QUADRATIC_CURVE = 102;
	public static int ZERO_ONE_ZERO_CURV = 103;
	public static int SIN_CURVE = 014;
	public static int YE_MAN_CHONG_ZHUANG = 105;
	public static int STRICK_BACK = 106;
	public static int BUFF_TIP = 107;
}

// 音效定义
public class SOUND_DEFINE_ILR
{
	public static int NONE = 0;
}

// 游戏配置参数,浮点数
public class GAME_FLOAT
{
	public static int NONE = 0;
}

// 游戏配置参数,字符串
public class GAME_STRING
{
	public static int NONE = 0;
}